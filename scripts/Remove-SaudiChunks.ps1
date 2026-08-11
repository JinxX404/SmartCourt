$ErrorActionPreference = "Stop"

$BaseDir = "d:\ITI 9 Month\Graduation Project\SmartCourt"
$ConfigPath = Join-Path $BaseDir "SmartCourt\appsettings.Development.json"
$JsonlFile = Join-Path $BaseDir "scripts\cleaned_legal_chunks.jsonl"
$OutputFile = Join-Path $BaseDir "scripts\cleaned_legal_chunks_filtered.jsonl"

Write-Host "Loading configuration from $ConfigPath"
$Config = Get-Content $ConfigPath -Raw | ConvertFrom-Json

$QdrantHost = $Config.Qdrant.Host
$QdrantPort = 6334
$QdrantApiKey = $Config.Qdrant.ApiKey
$QdrantCollection = $Config.Qdrant.CollectionName

$QdrantHeaders = @{
    "api-key" = $QdrantApiKey
    "Content-Type" = "application/json"
}
$QdrantBase = "https://${QdrantHost}:${QdrantPort}/collections"

Write-Host "Auditing $JsonlFile for non-Egyptian law..."

$SaudiKeywords = @(
    "حقوق العاملين في النظام السعودي",
    "نظام العمل السعودي",
    "هيئة تسوية الخلافات العمالية"
)

$Lines = Get-Content $JsonlFile -Encoding UTF8
$FilteredLines = @()
$ChunksToDelete = @()

$deletedCount = 0
foreach ($line in $Lines) {
    if ([string]::IsNullOrWhiteSpace($line)) { continue }
    
    $obj = $line | ConvertFrom-Json
    $isSaudi = $false
    
    foreach ($kw in $SaudiKeywords) {
        if (($obj.law_name -match $kw) -or ($obj.text -match $kw)) {
            $isSaudi = $true
            break
        }
    }
    
    if ($isSaudi) {
        $deletedCount++
        $pointId = $obj.chunk_id
        if ($pointId -notmatch "-") {
            try {
                $guidStr = $pointId.Insert(8,"-").Insert(13,"-").Insert(18,"-").Insert(23,"-")
                $pointId = [Guid]::Parse($guidStr).ToString()
            } catch {
                # if it fails, just use it as is if it might be valid, or log
                Write-Host "Failed to parse chunk_id: $($obj.chunk_id)"
            }
        }
        $ChunksToDelete += $pointId
        Write-Host "Found non-Egyptian chunk: $($obj.law_name) (ID: $pointId)"
    } else {
        $FilteredLines += $line
    }
}

Write-Host "Found $deletedCount non-Egyptian chunks to delete."

if ($ChunksToDelete.Count -gt 0) {
    Write-Host "Deleting vectors from Qdrant collection '$QdrantCollection'..."
    $deleteBody = @{
        points = $ChunksToDelete
    }
    
    try {
        Invoke-RestMethod -Uri "$QdrantBase/$QdrantCollection/points/delete?wait=true" -Method Post -Headers $QdrantHeaders -Body ($deleteBody | ConvertTo-Json -Depth 5) | Out-Null
        Write-Host "Successfully deleted $($ChunksToDelete.Count) vectors from Qdrant."
    } catch {
        Write-Host "Error deleting from Qdrant: $_" -ForegroundColor Red
    }
    
    Write-Host "Saving filtered chunks to $OutputFile..."
    $FilteredLines | Set-Content $OutputFile -Encoding UTF8
    
    Write-Host "Replacing original file..."
    Remove-Item $JsonlFile -Force
    Rename-Item $OutputFile $JsonlFile
}

Write-Host "Done!"
