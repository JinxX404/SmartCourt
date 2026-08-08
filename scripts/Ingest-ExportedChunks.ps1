$ErrorActionPreference = "Stop"

# --- Configuration ---
$BaseDir = "d:\ITI 9 Month\Graduation Project\SmartCourt"
$ConfigPath = Join-Path $BaseDir "SmartCourt\appsettings.Development.json"
$ExportsDir = "d:\ITI 9 Month\Graduation Project\exports"

Write-Host "Loading configuration from $ConfigPath"
$Config = Get-Content $ConfigPath -Raw | ConvertFrom-Json

$QdrantHost = $Config.Qdrant.Host
$QdrantPort = 6333
$QdrantApiKey = $Config.Qdrant.ApiKey
$QdrantCollection = "legal_chunks_imported" # Or $Config.Qdrant.CollectionName if you want to use the same one

$GeminiApiKey = $Config.GeminiEmbedding.ApiKey
$GeminiModel = $Config.GeminiEmbedding.Model
$GeminiDim = $Config.GeminiEmbedding.Dimensions
$GeminiBaseUrl = $Config.GeminiEmbedding.BaseUrl

$QdrantHeaders = @{
    "api-key" = $QdrantApiKey
    "Content-Type" = "application/json"
}
$QdrantBase = "https://${QdrantHost}:${QdrantPort}/collections"

# --- 1. Ensure Collection Exists ---
try {
    $existsResp = Invoke-RestMethod -Uri "$QdrantBase/$QdrantCollection" -Headers $QdrantHeaders -ErrorAction Stop
} catch {
    Write-Host "Collection '$QdrantCollection' doesn't exist. Creating..."
    $createBody = @{
        vectors = @{
            size = $GeminiDim
            distance = "Cosine"
        }
    }
    Invoke-RestMethod -Uri "$QdrantBase/$QdrantCollection" -Method Put -Headers $QdrantHeaders -Body ($createBody | ConvertTo-Json -Depth 5) | Out-Null
    
    # Optional: Setup indexes if needed based on the JSON payload properties
    $indexes = @("law_name", "law_category", "article_number", "document_id")
    foreach ($idx in $indexes) {
        $idxBody = @{ field_name = $idx; field_schema = "keyword" }
        Invoke-RestMethod -Uri "$QdrantBase/$QdrantCollection/index" -Method Put -Headers $QdrantHeaders -Body ($idxBody | ConvertTo-Json -Depth 5) | Out-Null
    }
    Write-Host "Created collection and indexes."
}

# --- 2. Process JSONL files ---
$JsonlFiles = Get-ChildItem -Path $ExportsDir -Filter "legal-chunks-*.jsonl" | Sort-Object Name
Write-Host "Found $($JsonlFiles.Count) JSONL files in $ExportsDir"

$TotalProcessedChunks = 0

foreach ($File in $JsonlFiles) {
    Write-Host "Processing: $($File.Name)"
    
    $Lines = Get-Content -Path $File.FullName -Encoding UTF8
    
    $BatchSize = 5
    for ($i = 0; $i -lt $Lines.Count; $i += $BatchSize) {
        $BatchLines = $Lines | Select-Object -Skip $i -First $BatchSize
        $BatchObjects = $BatchLines | ConvertFrom-Json

        # --- A. Generate Embeddings using Gemini ---
        $GeminiUrl = "${GeminiBaseUrl}models/${GeminiModel}:batchEmbedContents?key=${GeminiApiKey}"
        $GemBody = @{ requests = @() }
        
        foreach ($obj in $BatchObjects) {
            # Use 'embedding_text' if available, otherwise fallback to 'text'
            $textToEmbed = if ($obj.embedding_text) { $obj.embedding_text } else { $obj.text }
            $GemBody.requests += @{
                model = "models/${GeminiModel}"
                content = @{ parts = @( @{ text = $textToEmbed } ) }
                outputDimensionality = $GeminiDim
            }
        }
        
        # Call Gemini API with retries for rate limits
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
                    Write-Host "Rate limit hit. Waiting $($delayMs / 1000) seconds... ($retries retries left)"
                    Start-Sleep -Milliseconds $delayMs
                    $delayMs = [math]::Min($delayMs * 2, 60000)
                    $retries--
                } else {
                    Write-Host "Gemini API Error: $($_.Exception.Message)" -ForegroundColor Red
                    throw $_
                }
            }
        }

        if (-not $vectors) {
            throw "Failed embeddings."
        }

        # --- B. Upload to Qdrant ---
        $QdPoints = @()
        for ($j = 0; $j -lt $BatchObjects.Length; $j++) {
            $obj = $BatchObjects[$j]
            $v = $vectors[$j].values
            
            # Convert chunk_id (32 chars) to a valid GUID format for Qdrant ID
            # e.g., "b214b80df02428d094ee3980a2276649" -> "b214b80d-f024-28d0-94ee-3980a2276649"
            $guidStr = $obj.chunk_id.Insert(8,"-").Insert(13,"-").Insert(18,"-").Insert(23,"-")
            $pointId = [Guid]::Parse($guidStr).ToString()
            
            $QdPoints += @{
                id = $pointId
                vector = $v
                payload = $obj
            }
        }
        
        $QdBody = @{ points = $QdPoints }
        try {
            $qdJsonStr = $QdBody | ConvertTo-Json -Depth 10 -Compress
            $qdJsonBytes = [System.Text.Encoding]::UTF8.GetBytes($qdJsonStr)
            Invoke-RestMethod -Method Put -Uri "$QdrantBase/$QdrantCollection/points?wait=true" -Headers $QdrantHeaders -Body $qdJsonBytes | Out-Null
        } catch {
            Write-Host "Qdrant API Error: $($_.Exception.Message)" -ForegroundColor Red
            throw $_
        }
        
        $TotalProcessedChunks += $BatchObjects.Length
        Write-Progress -Activity "Uploading Chunks" -Status "Processed $TotalProcessedChunks chunks"
        
        # Respect Gemini rate limits
        Start-Sleep -Seconds 4
    }
}

Write-Host "Ingestion Complete. Total chunks processed: $TotalProcessedChunks"
