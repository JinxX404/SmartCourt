$ErrorActionPreference = "Stop"
$BaseDir = "d:\ITI 9 Month\Graduation Project\SmartCourt"
$DocsDir = Join-Path $BaseDir "docs\Egyptian law\القوانين"
$csPath = Join-Path $BaseDir "scripts\LegalDocumentChunker.cs"

Add-Type -Path $csPath
$Chunker = New-Object LegalDocumentChunker(512, 64, 50)
$Files = Get-ChildItem -Path $DocsDir -Filter "*.md" -Recurse

$TotalChunks = 0
foreach ($File in $Files) {
    $Text = [System.IO.File]::ReadAllText("\\?\" + $File.FullName, [System.Text.Encoding]::UTF8)
    $Chunks = $Chunker.ChunkText($Text, "ar") | Where-Object { -not [string]::IsNullOrWhiteSpace($_.Text) }
    if ($Chunks) {
        $TotalChunks += $Chunks.Count
    }
}
Write-Host "Total files: $($Files.Count)"
Write-Host "Total chunks: $TotalChunks"
