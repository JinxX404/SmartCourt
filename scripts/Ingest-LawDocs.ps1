$ErrorActionPreference = "Stop"

# --- Configuration ---
$BaseDir = "d:\ITI 9 Month\Graduation Project\SmartCourt"
$ConfigPath = Join-Path $BaseDir "SmartCourt\appsettings.Development.json"
$DocsDir = Get-ChildItem -Path (Join-Path $BaseDir "docs\Egyptian law") -Directory | Where-Object { $_.Name -notmatch "manshurat" } | Select-Object -First 1 | Select-Object -ExpandProperty FullName
$StateFile = Join-Path $BaseDir "scripts\ingest_state.json"

Write-Host "Loading configuration from $ConfigPath"
$Config = Get-Content $ConfigPath -Raw | ConvertFrom-Json

$QdrantHost = $Config.Qdrant.Host
$QdrantPort = 6333
$QdrantApiKey = $Config.Qdrant.ApiKey
$QdrantCollection = $Config.Qdrant.CollectionName

$GeminiApiKey = $Config.GeminiEmbedding.ApiKey
$GeminiModel = $Config.GeminiEmbedding.Model
$GeminiDim = $Config.GeminiEmbedding.Dimensions
$GeminiBaseUrl = $Config.GeminiEmbedding.BaseUrl

$MaxChunkTokens = if ($null -ne $Config.Chunking.MaxChunkTokens) { $Config.Chunking.MaxChunkTokens } else { 512 }
$OverlapTokens = if ($null -ne $Config.Chunking.OverlapTokens) { $Config.Chunking.OverlapTokens } else { 64 }
$MinChunkTokens = if ($null -ne $Config.Chunking.MinChunkTokens) { $Config.Chunking.MinChunkTokens } else { 50 }

if (Test-Path $StateFile) {
    $State = Get-Content $StateFile -Raw -Encoding UTF8 | ConvertFrom-Json
} else {
    $State = @{ IngestedFiles = @() }
}
function Is-FileIngested([string]$filePath) {
    return $State.IngestedFiles -contains $filePath
}
function Mark-FileIngested([string]$filePath) {
    if (-not (Is-FileIngested $filePath)) {
        $State.IngestedFiles += $filePath
        $State | ConvertTo-Json -Depth 10 | Set-Content $StateFile -Encoding UTF8
    }
}

Write-Host "Compiling C# Chunker..."
$csPath = Join-Path $BaseDir "scripts\LegalDocumentChunker.cs"
Add-Type -Path $csPath

$Chunker = New-Object LegalDocumentChunker($MaxChunkTokens, $OverlapTokens, $MinChunkTokens)

$QdrantHeaders = @{
    "api-key" = $QdrantApiKey
    "Content-Type" = "application/json"
}
$QdrantBase = "https://${QdrantHost}:${QdrantPort}/collections"

try {
    $existsResp = Invoke-RestMethod -Uri "$QdrantBase/$QdrantCollection" -Headers $QdrantHeaders -ErrorAction Stop
} catch {
    Write-Host "Collection doesn't exist. Creating..."
    $createBody = @{
        vectors = @{
            size = $GeminiDim
            distance = "Cosine"
        }
    }
    Invoke-RestMethod -Uri "$QdrantBase/$QdrantCollection" -Method Put -Headers $QdrantHeaders -Body ($createBody | ConvertTo-Json -Depth 5) | Out-Null
    
    $indexes = @("document_id", "document_title", "language", "category", "part", "chapter", "article_number", "version")
    foreach ($idx in $indexes) {
        $type = if ($idx -eq "article_number" -or $idx -eq "version") { "integer" } else { "keyword" }
        $idxBody = @{ field_name = $idx; field_schema = $type }
        Invoke-RestMethod -Uri "$QdrantBase/$QdrantCollection/index" -Method Put -Headers $QdrantHeaders -Body ($idxBody | ConvertTo-Json -Depth 5) | Out-Null
    }
    Write-Host "Created collection and indexes."
}

$Files = Get-ChildItem -Path $DocsDir -Filter "*.md" -Recurse
Write-Host "Found $($Files.Count) markdown files in $DocsDir"

$TotalProcessed = 0
$TotalSkipped = 0
$TotalChunks = 0

foreach ($File in $Files) {
    $RelPath = $File.FullName.Substring($DocsDir.Length).TrimStart('\')
    
    if (Is-FileIngested $RelPath) {
        $TotalSkipped++
        continue
    }

    Write-Host "Processing: $RelPath"
    
    try {
        $Text = [System.IO.File]::ReadAllText("\\?\" + $File.FullName, [System.Text.Encoding]::UTF8)
    } catch {
        Write-Error "Failed to read file $($File.FullName). Error: $($_.Exception.Message)"
        continue
    }
    
    $Chunks = $Chunker.ChunkText($Text, "ar") | Where-Object { -not [string]::IsNullOrWhiteSpace($_.Text) }
    if (-not $Chunks -or $Chunks.Count -eq 0) {
        Write-Host "Skipping empty file."
        Mark-FileIngested $RelPath
        continue
    }

    $BatchSize = 5
    for ($i = 0; $i -lt $Chunks.Count; $i += $BatchSize) {
        $Batch = $Chunks | Select-Object -Skip $i -First $BatchSize
        
        $GeminiUrl = "${GeminiBaseUrl}models/${GeminiModel}:batchEmbedContents?key=${GeminiApiKey}"
        $GemBody = @{ requests = @() }
        foreach ($c in $Batch) {
            $GemBody.requests += @{
                model = "models/${GeminiModel}"
                content = @{ parts = @( @{ text = $c.Text } ) }
                outputDimensionality = $GeminiDim
            }
        }
        
        $retries = 10
        $delayMs = 5000
        $vectors = $null
        
        while ($retries -gt 0) {
            try {
                $jsonStr = $GemBody | ConvertTo-Json -Depth 10 -Compress
                $jsonBytes = [System.Text.Encoding]::UTF8.GetBytes($jsonStr)
                $gemResp = Invoke-RestMethod -Method Post -Uri $GeminiUrl -ContentType "application/json; charset=utf-8" -Body $jsonBytes
                $vectors = $gemResp.embeddings
                break
            } catch {
                if ($_.Exception.Response.StatusCode -eq 429) {
                    if ($_.Exception.Response) {
                        $stream = $_.Exception.Response.GetResponseStream()
                        if ($stream) {
                            $reader = New-Object System.IO.StreamReader($stream)
                            $errBody = $reader.ReadToEnd()
                            Write-Host "Rate limit error details: $errBody" -ForegroundColor Yellow
                        }
                    }
                    Write-Host "Rate limit hit. Waiting $($delayMs / 1000) seconds... ($retries retries left)"
                    Start-Sleep -Milliseconds $delayMs
                    $delayMs = [math]::Min($delayMs * 2, 60000) # Max 60 seconds
                    $retries--
                } else {
                    Write-Host "Gemini API Error: $($_.Exception.Message)" -ForegroundColor Red
                    if ($_.ErrorDetails) { Write-Host $_.ErrorDetails.Message -ForegroundColor Red }
                    if ($_.Exception.Response) {
                        $stream = $_.Exception.Response.GetResponseStream()
                        $reader = New-Object System.IO.StreamReader($stream)
                        $errBody = $reader.ReadToEnd()
                        Write-Host "Body: $errBody" -ForegroundColor Red
                    }
                    throw $_
                }
            }
        }

        if (-not $vectors) {
            throw "Failed embeddings."
        }

        $QdPoints = @()
        for ($j = 0; $j -lt $Batch.Length; $j++) {
            $c = $Batch[$j]
            $v = $vectors[$j].values
            
            $docId = [LegalDocumentChunker]::GenerateDeterministicGuid($RelPath)
            $pointId = [LegalDocumentChunker]::GenerateDeterministicGuid("${RelPath}_$($c.ChunkIndex)")
            
            $cat = if ($RelPath -match '^[^\\]+') { $matches[0] } else { 'none' }
            
            $payload = @{
                document_id = $docId
                document_title = $File.BaseName
                language = "ar"
                category = $cat
                part = $c.Part
                chapter = $c.Chapter
                section = $c.Section
                article_number = $c.Article
                chunk_index = $c.ChunkIndex
                chunk_text = $c.Text
                version = 1
                source = "local_markdown"
            }
            
            $QdPoints += @{
                id = $pointId
                vector = $v
                payload = $payload
            }
        }
        
        $QdBody = @{ points = $QdPoints }
        try {
            $qdJsonStr = $QdBody | ConvertTo-Json -Depth 10 -Compress
            $qdJsonBytes = [System.Text.Encoding]::UTF8.GetBytes($qdJsonStr)
            Invoke-RestMethod -Method Put -Uri "$QdrantBase/$QdrantCollection/points?wait=true" -Headers $QdrantHeaders -Body $qdJsonBytes | Out-Null
        } catch {
            Write-Host "Qdrant API Error: $($_.Exception.Message)" -ForegroundColor Red
            if ($_.Exception.Response) {
                $stream = $_.Exception.Response.GetResponseStream()
                $reader = New-Object System.IO.StreamReader($stream)
                $errBody = $reader.ReadToEnd()
                Write-Host "Qdrant Body: $errBody" -ForegroundColor Red
            }
            throw $_
        }
        $TotalChunks += $Batch.Length
        Start-Sleep -Seconds 4
    }

    Mark-FileIngested $RelPath
    $TotalProcessed++
    Write-Host "Ingested $RelPath"
}

Write-Host "Ingestion Complete"
