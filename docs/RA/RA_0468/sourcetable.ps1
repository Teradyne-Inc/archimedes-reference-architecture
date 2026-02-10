# Get the path of the script itself
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition

# Locate the first 'source' folder
$sourcePath = Get-ChildItem -Path $scriptDir -Recurse -Directory -Filter "source" | Select-Object -First 1

if (-not $sourcePath) {
    Write-Warning "No 'source' folder found."
    exit 1
}

Write-Host "Source directory found: $($sourcePath.FullName)"

# Step 1: Clean unwanted folders inside 'source'
$unwantedDirs = @("bin", "obj", ".vs")
foreach ($dirName in $unwantedDirs) {
    Get-ChildItem -Path $sourcePath.FullName -Recurse -Directory -Filter $dirName | ForEach-Object {
        Write-Host "Removing: $($_.FullName)"
        Remove-Item -Recurse -Force -Path $_.FullName
    }
}

# Step 2: Generate Markdown content
$md = @()
$md += "| File | Path |"
$md += "|------|------|"

Get-ChildItem -Path $sourcePath.FullName -Recurse -File | ForEach-Object {
    $relativePath = $_.FullName.Replace($scriptDir + "\", "").Replace("\", "/")
    $fileName = $_.Name
    $folder = $_.Directory.Name
    $md += "| [$fileName]($relativePath) | $folder |"
}

# Step 3: Generate allsource.zip inside the source directory
$zipPath = Join-Path $sourcePath.FullName "allsource.zip"
if (Test-Path $zipPath) {
    Write-Host "Removing existing zip: $zipPath"
    Remove-Item -Path $zipPath -Force
}

Write-Host "Creating new allsource.zip..."
Compress-Archive -Path "$($sourcePath.FullName)\*" -DestinationPath $zipPath

# Step 4: Add download link to the Markdown
$zipRelative = $zipPath.Replace($scriptDir + "\", "").Replace("\", "/")
$md += "`nYou can also [download all the source files as a zip archive]($zipRelative)."

# Step 5: Save the Markdown
$outputFile = Join-Path $scriptDir "file-table.md"
$md | Out-File -FilePath $outputFile -Encoding utf8

Write-Host "`nDone! Markdown file and zip archive ready."
