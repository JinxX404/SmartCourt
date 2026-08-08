$files = Get-ChildItem -Path "p:\Projects\Smart Court\SmartCourt.Tests" -Filter *.cs -Recurse

$pattern = '(?s)new SmartCourt\.Entities\.Case\(\s*([^,]+?),\s*([^,]+?),\s*([^,]+?),\s*([^,]+?),\s*([^,]+?),\s*([^)]+?)\)\s*(?:\{\s*Status\s*=\s*([^}]+?)\s*\})?'

foreach ($f in $files) {
    $content = [System.IO.File]::ReadAllText($f.FullName)
    
    if ($content -match $pattern) {
        $newContent = [regex]::Replace($content, $pattern, {
            param($match)
            $id = $match.Groups[1].Value
            $clientId = $match.Groups[2].Value
            $title = $match.Groups[3].Value
            $desc = $match.Groups[4].Value
            $city = $match.Groups[5].Value
            $date = $match.Groups[6].Value
            $status = $match.Groups[7].Value
            
            $res = "new SmartCourt.Entities.Case { Id = $id, ClientId = $clientId, Title = $title, Description = $desc, City = $city, SubmittedAt = $date"
            if ($status -ne "") {
                $res += ", Status = $status"
            }
            $res += " }"
            return $res
        })
        [System.IO.File]::WriteAllText($f.FullName, $newContent)
    }
}
