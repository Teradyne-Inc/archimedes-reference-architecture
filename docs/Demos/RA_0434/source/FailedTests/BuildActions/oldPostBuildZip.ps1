param (
    [string]$buildConfig = "Debug",
    [string]$outputFolder = "$PSScriptRoot\bin\Debug",
    [string]$projectName = "DefaultProject"
)

# ✅ Get the solution directory (parent directory of the project)
$solutionDir = Split-Path -Path $PSScriptRoot -Parent
$publishFolder = "$solutionDir\..\..\Publish\FAARCH-434"

# ✅ Display Debug Information
Write-Host "----------------------------------"
Write-Host "🔍 Build Configuration: $buildConfig"
Write-Host "📂 Output Folder: $outputFolder"
Write-Host "📛 Project Name: $projectName"
Write-Host "📦 Publish Folder: $publishFolder"
Write-Host "----------------------------------"

# ✅ Only run if in Release mode
if ($buildConfig -ne "Release") {
    Write-Host "⚠️ Skipping ZIP operation, not in Release mode."
    exit 0
}

# ✅ Ensure the output folder exists
if (!(Test-Path $outputFolder)) {
    Write-Host "❌ ERROR: Output folder does not exist: $outputFolder"
    exit 1
}

# ✅ Ensure the publish folder exists
if (!(Test-Path $publishFolder)) {
    Write-Host "📂 Creating Publish folder: $publishFolder"
    New-Item -ItemType Directory -Path $publishFolder | Out-Null
}

# ✅ Create "Marketing" folder under PublishFolder
$marketingFolder = "$publishFolder\Marketing"
if (!(Test-Path $marketingFolder)) {
    Write-Host "📂 Creating Marketing folder under PublishFolder"
    New-Item -ItemType Directory -Path $marketingFolder | Out-Null
}

# ✅ Copy all *.xml files into "Marketing"
Write-Host "📄 Copying XML files to Marketing folder..."
Get-ChildItem -Path $outputFolder\Config -Filter "*.*" -File -Recurse | ForEach-Object {
    Copy-Item -Path $_.FullName -Destination $marketingFolder -Force
}
Write-Host "✅ XML files copied successfully."

# ✅ Copy TestPrograms recursively into OutputFolder\TestPrograms
$testProgramsSource = "$publishFolder\..\TestPrograms"
$testProgramsDest = "$outputFolder\TestPrograms"

if (Test-Path $testProgramsSource) {
    Write-Host "📂 Copying TestPrograms from $testProgramsSource to $testProgramsDest..."
    Copy-Item -Path "$testProgramsSource\*" -Destination $testProgramsDest -Recurse -Force
    Write-Host "✅ TestPrograms copied successfully."
} else {
    Write-Host "⚠️ WARNING: TestPrograms source folder does not exist, skipping copy."
}

# ✅ Define the ZIP file name dynamically based on the project name
$zipDestination = "$publishFolder\FAARCH-434.zip"

# ✅ Check if an existing ZIP file is present and delete it
if (Test-Path $zipDestination) {
    Write-Host "🗑️ Removing existing ZIP file: $zipDestination"
    Remove-Item -Path $zipDestination -Force
}

# ✅ Compress the output folder into a ZIP
Write-Host "🗜️ Compressing files from $outputFolder to $zipDestination..."
Compress-Archive -Path "$outputFolder\*" -DestinationPath $zipDestination -Force

# ✅ Display Success Message
Write-Host "✅ Build folder zipped successfully!"
Write-Host "📦 ZIP Location: $zipDestination"
Write-Host "----------------------------------"
