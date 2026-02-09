#****************************** RegressionDAS - Sample code for SSL Certificate Binding ********************************/
#FileName: CreateAndBindCert.ps1
#Project:  RegressionDAS
#Copyright (c) Teradyne Inc. All rights reserved.
#
#This file contains simple utility methods that can be used when building a custom  Data Analytics Solution.
#This Teradyne-supplied code is provided as part of the TEMS SDK as a starting example to help users quickly create their 
#own customized data analytics solution (DAS).
#
#THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, EITHER EXPRESSED OR IMPLIED, 
#INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES OF FUNCTIONALITY.
#***********************************************************************************************************************/

Param (
    [int]$port,# = $arg1 
    [string]$dnsName # = $arg2
)
$guid = [guid]::NewGuid()  # Generate a new AppId

# 1. Check Existing Bindings
# Run the netsh command to show SSL certificates bound to ports
$sslCerts = netsh http show sslcert

# Filter the result to check if the specified port is bound already
$ipPort = "0.0.0.0:$port"
$bindingExists = $sslCerts | Select-String -Pattern $ipPort

if ($bindingExists) {
     
    Write-Host "Port $port is already bound to a certificate."
    
    # Run netsh to get the certificate bound to port 443
    $sslCertInfo = netsh http show sslcert ipport=$ipPort
    
    # Use regex to extract the Certificate Hash
    $certHash = ($sslCertInfo | Select-String -Pattern "Certificate Hash\s+:\s+([A-Fa-f0-9]+)").Matches.Groups[1].Value
    
    $certificate = Get-ChildItem -Path Cert:\LocalMachine\My | Where-Object { $_.Thumbprint -eq $certHash }

  #  Write-Output "Certificate Hash: $certHash"
    Write-Host "Certificate Subject: $($certificate.Subject)"
    Write-Host "Certificate Thumbprint: $($certificate.Thumbprint)"
    exit 0
}

# 2. Create a Self-Signed Certificate
Write-Host "Creating a self-signed certificate for DNS Name: $dnsName... under personal location"

$cert = New-SelfSignedCertificate -DnsName "$dnsName" -CertStoreLocation "cert:\LocalMachine\My" -FriendlyName "$dnsName SSL Certificate" -NotAfter (Get-Date).AddYears(1)

if ($cert -eq $null) {
    Write-Host "Error creating the certificate!" -ForegroundColor Red
    exit 1
}

Write-Host "Certificate created successfully."

# 3. Export the Certificate to Trusted Root CA.
Write-Host "Exporting and installing certificate to Trusted Root CA..."
$rootStoreLocation = "Cert:\LocalMachine\Root"
$exportCertPath = "C:\$dnsName-cert.cer"
Export-Certificate -Cert $cert -FilePath $exportCertPath
Import-Certificate -FilePath $exportCertPath -CertStoreLocation $rootStoreLocation

Write-Host "Certificate added to Trusted Root Certification Authorities."

# 4. Get the Certificate Thumbprint
$thumbprint = $cert.Thumbprint

Write-Host "Certificate Thumbprint: $thumbprint"

# 5. Bind the Certificate to the Specified Port Using netsh
Write-Host "Binding certificate to port $port with GUID $guid..."
$ipport = "0.0.0.0:$port"
$netshCommand = "http add sslcert ipport=$ipport certhash=$thumbprint appid={$guid}"

# Execute the netsh command
$process = Start-Process -FilePath netsh -ArgumentList $netshCommand -Wait -NoNewWindow -PassThru

if ($process.ExitCode -eq 0) {
    Write-Host "Successfully bound the certificate to port $port."
} else {
    Write-Host "Failed to bind the certificate. Exit code: $($process.ExitCode)" -ForegroundColor Red
    exit 1
}

# 6. Clean up: Remove the exported .cer file (optional)
Remove-Item $exportCertPath -Force
Write-Host "Clean-up complete."
