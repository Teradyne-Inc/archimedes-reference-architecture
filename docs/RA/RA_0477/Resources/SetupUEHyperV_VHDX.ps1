#Requires -RunAsAdministrator

# Author: Yannis Chicha Teradyne DIA (c) 2024
# Disclaimer: This script is provided "as is" without warranty.

# Input (Rename the Virtual box by using SetupVirtualBox.ps1 <Name>)
param ([string]$boxName = 'UltraEdge', [string]$netName = 'UltraEdge')

$adapterIPAddress = "10.100.100.254"
$referenceVhdFile = ".\UE_Reference.vhdx"
$vhdFile = ".\UE.vhdx"

# Output error information
function Manage-Error($message)
{
    Write-Host ""
    Write-Host $message -ForegroundColor Red
	Write-Host ""
    exit 2
}

# Output simple information
function Write-Message($message)
{
    Write-Host $message
}

# Output title information
function Write-Title($title)
{
    Write-Message ""
    Write-Message "========== $title =========="
}

# Hyper-V management: check and activation
function CheckAndEnable-HyperV
{
    # Check if Hyper-V feature is installed
    $hyperVInstalled = Get-WindowsOptionalFeature -FeatureName Microsoft-Hyper-V-All -Online | Where-Object { $_.State -eq "Enabled" }

    if ($hyperVInstalled) {
	    Write-Message "Hyper-V detected"
    }
    else
    {
        Write-Message "Installing Hyper-V."

        # Install Hyper-V feature
        Enable-WindowsOptionalFeature -FeatureName Microsoft-Hyper-V-All -All -Online

        # Check if Hyper-V feature is installed
        $hyperVInstalled = Get-WindowsOptionalFeature -FeatureName Microsoft-Hyper-V-All -Online | Where-Object { $_.State -eq "Enabled" }

        if (-not $hyperVInstalled) {
            Manage-Error "Failed to install Hyper-V. Please check if you have administrative privileges and try again."
        }
    }
}

# Verify that the VM we want to create does not already exist
function Check-VMExistence
{
    # Check if the VM exists
    $vmExists = Get-VM -Name $boxName -ErrorAction SilentlyContinue
    if ($vmExists) {
        Manage-Error "Virtual machine '$boxName' already exists. Exiting script."
    }
    else
    {
	    Write-Message "Virtual machine '$boxName' not detected. Installation continues."
    }
}

# Check availability of the reference VHDX and make a copy
function Check-SourceVHDX
{
    # Check if the VHDX file does not exist
    if (Test-Path $referenceVhdFile -PathType Leaf) {
	    Write-Message "Found reference virtual hard disk $referenceVhdFile"
	    if (Test-Path $vhdFile -PathType Leaf) {
		    Write-Message "Deleting $vhdFile..."
		    Remove-Item $vhdFile
	    }
	    Write-Message "Copying $referenceVhdFile to $vhdFile..."
	    Copy-Item $referenceVhdFile -Destination $vhdFile 
    }
    else
    {
	    # The VHDX file does not exist
	    Manage-Error "This script expects $refVhdFile to be present. Installation aborted."
    }
}

# Verify the network setup
function Check-NetworkSetup
{
    # Check if network switch already exists
    $switchCounts = (Get-VMSwitch | Where-Object -Property Name -EQ -Value $netName).count
    if ($switchCounts -eq '1')
    {
	    Manage-Error "Network switch '$netName' already exists. Use the Hyper-V manager to remove it. Installation aborted."
    }


    # Check if IP Address is available
    $existingIP = Get-NetIPAddress |  Where-Object -Property IPAddress -EQ -Value $adapterIPAddress

    if ($existingIP)
    {
        Manage-Error "IP Address $adapterIPAddress is already in use. Installation aborted."
    }
    else
    {
        Write-Message "IP Address $adapterIPAddress available. Installation continues."
    }
}

# Create the new network adapter and assign the IP address
function Do-NetworkSetup($netName)
{
    # Check if the switch exists
    $switchCounts = (Get-VMSwitch | Where-Object -Property Name -EQ -Value $netName).count
    if ($switchCounts -eq '1')
    {
	    Write-Message "Network switch '$netName' found. Removing..."
	    Remove-VMSwitch -Force -Name $netName
    }

    Write-Message "Creating network switch: $netName"
    New-VMSwitch -Name $netName -SwitchType Internal | out-null

    # Find the network adapter by its name
    $adapterFilter = "vEthernet ($netName)"
    $netConnectID = Get-NetAdapter -Name $adapterFilter | Select-Object -ExpandProperty Name

    Write-Message "Found ConnectID = $netConnectID"

    if (-not $netConnectID) {
        Manage-Error "Hyper-V UltraEdge Virtual Ethernet Adapter $adapterFilter not found!"
    }

    # Disable all bindings (we can skip ipv4 and VirtualBox, but we need to make sure they are enabled anyways)
    Write-Message "NetAdapter: Disable binding"
    Disable-NetAdapterBinding -Name $netConnectID -ComponentID *

    # Enable ipv4
    Write-Message "NetAdapter: Reenable binding"
    Enable-NetAdapterBinding -Name $netConnectID -ComponentID ms_tcpip

    Write-Message "Setting network address to $adapterIPAddress"

    # (Re)Set the IPV4 - If the IPV4 is already set, this can't seem to change it. Clear and reset.
    $index = (Get-NetAdapter -Name $netConnectID).ifIndex
    Write-Message "Found adapter interface index: $index"
    Start-Sleep -Seconds 5
    Write-Message "Setting up net IP address to $adapterIPAddress"
    New-NetIPAddress -InterfaceIndex $index -IPAddress $adapterIPAddress -PrefixLength 24 | out-null
}

# Create the virtual machine
function Create-VM($memory)
{
    # Create a new VM
    $memgb = $memory/1GB
    Write-Message "Creating new VM: $boxName with ${memgb}GB of memory"
    New-VM -Name $boxName -MemoryStartupBytes $memory -Generation 2 | out-null
}

# Assign elements (disk, network, ...) to the virtual machine
function Do-VMSetup($vhdx)
{
    # Attach the virtual hard disk to the VM
    Write-Message "Attaching virtual hard disk to virtual machine"
    Add-VMHardDiskDrive -VMName $boxName -Path $vhdx | out-null

    # Assign the switch to the VM
    Write-Message "Remove all default network switches"
    Remove-VMNetworkAdapter -VMName UltraEdge
    Write-Message "Add the new network switch"
    Add-VMNetworkAdapter -VMName $boxName -SwitchName $netName | out-null

    # Enable secure boot for Linux
    Write-Message "Disable secure boot"
    Set-VMFirmware UltraEdge -EnableSecureBoot On -SecureBootTemplate 'MicrosoftUEFICertificateAuthority'  | out-null
}

# Run the VM
function Launch-VM
{
    # Start the VM
    Write-Message "Starting VM..."
    Start-VM -Name $boxName | out-null
}

Write-Title "UltraEdge Hyper-V setup initial checks"

CheckAndEnable-HyperV
Check-VMExistence
Check-SourceVHDX
Check-NetworkSetup

Write-Title "UltraEdge Network adapter setup"

Do-NetworkSetup $netName

Write-Title "UltraEdge Hyper-V VM creation"

Create-VM 4096MB
Do-VMSetup $vhdFile
Launch-VM

# Print a success Write-Message
Write-Title "UltraEdge Hyper-V setup finalization"
Write-Message "Virtual machine $boxName has been created and started."
Write-Message ""
