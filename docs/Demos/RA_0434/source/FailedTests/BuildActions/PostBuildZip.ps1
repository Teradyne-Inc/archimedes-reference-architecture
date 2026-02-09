<# 
.SYNOPSIS
    Validates input parameters and paths for a PowerShell build process.
    Cleans and sets up necessary folders for the build process.
    Copies necessary files for deployment, including binaries.
.NOTES
    Author: Laurent Bonneval
    Disclaimer: Use at your own risk.
#>

param (
    [string]$buildConfig,
    [string]$outputFolder,
    [string]$projectName,
    [string]$projectid,
    [string]$solutionPath
)

# Function to validate input parameters
function Validate-Parameters {
    # Check if any parameter is empty
    if (-not $buildConfig -or -not $outputFolder -or -not $projectName -or -not $projectid -or -not $solutionPath) {
        Write-Host "ERROR: One or more required parameters are missing." -ForegroundColor Red
        Exit 1
    }

    # Ensure that Build Configuration is Release
    if ($buildConfig -ne "Release") {
        Write-Host "Build Configuration is not 'Release'. Exiting script."
        Exit 0
    }

    # Ensure that outputFolder exists
    if (-not (Test-Path -Path $outputFolder -PathType Container)) {
        Write-Host "ERROR: Output folder does not exist: $outputFolder" -ForegroundColor Red
        Exit 1
    }

    # Ensure that solutionPath exists
    if (-not (Test-Path -Path $solutionPath -PathType Leaf)) {
        Write-Host "ERROR: Solution file not found: $solutionPath" -ForegroundColor Red
        Exit 1
    }

    # Ensure that source marketing file exists
    if (-not (Test-Path -Path $sourcemarketingFile -PathType Leaf)) {
        Write-Host "ERROR: Source marketing file not found: $sourcemarketingFile" -ForegroundColor Red
        Exit 1
    }

    # Ensure that instruction file exists
    if (-not (Test-Path -Path $sourceinstructionFile -PathType Leaf)) {
        Write-Host "ERROR: Instruction file not found: $sourceinstructionFile" -ForegroundColor Red
        Exit 1
    }

    Write-Host "All parameters and paths validated successfully." -ForegroundColor Green
}

# Function to clean and set up necessary folders
function Setup-Folders {
    Write-Host "Starting Phase 1: Cleaning and Setting Up Folders..."

    # Step 1: Clean the entire Publish folder
    if (Test-Path -Path $rootpublishFolder) {
        Write-Host "Cleaning Publish folder: $rootpublishFolder"
        Remove-Item -Path $rootpublishFolder -Recurse -Force
    }

    # Step 2: Create necessary folders
    Write-Host "Creating Final Publish Folder: $finalpublishFolder"
    New-Item -ItemType Directory -Path $finalpublishFolder -Force | Out-Null

    Write-Host "Creating Data Folder: $dataFolder"
    New-Item -ItemType Directory -Path $dataFolder -Force | Out-Null

    Write-Host "Creating Destination Marketing Folder: $destmarketingFolder"
    New-Item -ItemType Directory -Path $destmarketingFolder -Force | Out-Null

    Write-Host "Phase 1: Cleaning and Folder Setup Completed Successfully."
}

# Function to copy necessary XML files
function Copy-XMLFiles {
    Write-Host "Starting Phase 2: Copying Required XML Files..."

    # Copy Instruction File
    Write-Host "Copying Instruction File: $sourceinstructionFile to $dataFolder"
    Copy-Item -Path $sourceinstructionFile -Destination $dataFolder -Force

    # Copy Marketing File
    Write-Host "Copying Marketing File: $sourcemarketingFile to $destmarketingFolder"
    Copy-Item -Path $sourcemarketingFile -Destination $destmarketingFolder -Force

    Write-Host "Phase 2: XML File Copy Completed Successfully."
}

# Function to copy binaries
function Copy-Binaries {
    Write-Host "Starting Phase 3: Copying Binaries..."

    # Ensure the compiled destination folder exists
    if (-not (Test-Path -Path $compileddestinationFolder)) {
        Write-Host "Creating Binaries Destination Folder: $compileddestinationFolder"
        New-Item -ItemType Directory -Path $compileddestinationFolder -Force | Out-Null
    }

    # Copy all files from outputFolder to compiled destination
    Write-Host "Copying binaries from $outputFolder to $compileddestinationFolder..."
    Copy-Item -Path "$outputFolder\*" -Destination $compileddestinationFolder -Recurse -Force

    Write-Host "Phase 3: Binaries Copy Completed Successfully."
}

# Function to copy test program if it exists
function Copy-TestProgram {
    if (Test-Path -Path $sourcetestprogramFolder) {
        Write-Host "Starting Phase 4: Copying Test Program..."
        
        # Ensure the destination TestProgram folder exists
        if (-not (Test-Path -Path $destinationprogramFolder)) {
            Write-Host "Creating Test Program Destination Folder: $destinationprogramFolder"
            New-Item -ItemType Directory -Path $destinationprogramFolder -Force | Out-Null
        }

        # Copy all test program files
        Write-Host "Copying test programs from $sourcetestprogramFolder to $destinationprogramFolder..."
        Copy-Item -Path "$sourcetestprogramFolder\*" -Destination $destinationprogramFolder -Recurse -Force

        Write-Host "Phase 4: Test Program Copy Completed Successfully."
    } else {
        Write-Host "No Test Program folder found. Skipping Phase 4."
    }
}

# Function to zip the Data Folder and remove it
function Zip-And-Cleanup {
    Write-Host "Starting Phase 5: Zipping Data Folder and Cleanup..."

    if (Test-Path -Path $dataFolder) {
        Write-Host "Compressing $dataFolder into $zipdataFile..."
        Compress-Archive -Path $dataFolder -DestinationPath $zipdataFile -Force

        Write-Host "Removing Data Folder: $dataFolder"
        Remove-Item -Path $dataFolder -Recurse -Force
        Write-Host "Phase 5: Data Folder compressed and removed successfully."
    } else {
        Write-Host "Data Folder does not exist, skipping compression and cleanup."
    }
}

# Get the solution directory (parent directory of the project)
$solutionDir = Split-Path -Path $solutionPath -Parent
$rootpublishFolder = Join-Path -Path $solutionDir -ChildPath "Publish"
$finalpublishFolder = Join-Path -Path $rootpublishFolder -ChildPath $projectid

$destmarketingFolder = Join-Path -Path $finalpublishFolder -ChildPath "Marketing"
$sourcemarketingFile = Join-Path -Path $outputFolder -ChildPath "Config\$projectid.xml"

$dataFolder = Join-Path -Path $finalpublishFolder -ChildPath $projectid
$zipdataFile = Join-Path -Path $finalpublishFolder -ChildPath "$projectid.zip"

$sourcetestprogramFolder = Join-Path -Path $solutionDir -ChildPath "TestProgram\"
$destinationprogramFolder = Join-Path -Path $dataFolder -ChildPath "TestProgram"

$sourceinstructionFile = Join-Path -Path $outputFolder -ChildPath "Config\instructions.xml"

$compileddestinationFolder = Join-Path -Path $dataFolder -ChildPath $projectName

# Call the folder setup function
Setup-Folders

# Call the file copy functions
Copy-XMLFiles
Copy-Binaries

# Display Debug Information
Write-Host "-----------------------------------------------------------"
Write-Host "    Build Configuration:        [$buildConfig]"
Write-Host "    Compilation Folder:         [$outputFolder]"
Write-Host "    Project Name:               [$projectName]"
Write-Host "    Project ID :                [$projectid]"
Write-Host "    SolutionPath:               [$solutionPath]"
Write-Host " "
Write-Host "    Root SolutionDir:           [$solutionDir]"
Write-Host " "
Write-Host "    Publish Folder:             [$rootpublishFolder]"
Write-Host "    Final Publish Folder:       [$finalpublishFolder]"
Write-Host " "
Write-Host "    Source Marketing File:      [$sourcemarketingFile]"
Write-Host "    Destination Marketing:      [$destmarketingFolder]" 
Write-Host "    "
Write-Host "    Data Folder:                [$dataFolder]"
Write-Host "    Final ZipFile:              [$zipdataFile]"
Write-Host "    "
Write-Host "    Source Test Program:        [$sourcetestprogramFolder]"
Write-Host "    Destination Test Program:   [$destinationprogramFolder]"
Write-Host "    "
Write-Host "    Instructions File:          [$sourceinstructionFile]"
Write-Host "    Destination Instruction:    [$dataFolder]"
Write-Host "    "
Write-Host "    Source Binaries:            [$outputFolder]"   
Write-Host "    Destination Binaries:       [$compileddestinationFolder]"
Write-Host "------------------------------------------------------------"

# Call the validation function
Validate-Parameters

# Call the folder setup function
Setup-Folders

# Call the file copy functions
Copy-XMLFiles
Copy-Binaries
Copy-TestProgram
Zip-And-Cleanup