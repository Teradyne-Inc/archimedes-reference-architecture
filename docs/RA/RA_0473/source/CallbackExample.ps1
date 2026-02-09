<#
.DISCLAIMER
    This script is provided "as is" without warranty of any kind, express or implied,
    including but not limited to the warranties of merchantability, fitness for a particular purpose,
    and noninfringement. The script is intended for demonstration and development purposes only.

    It is not certified for production use and may require adaptation depending on your environment.
    Use at your own risk. Teradyne shall not be held liable for any damages resulting from the use
    or misuse of this code.

    This code may not be distributed outside of Intel without prior consent from Teradyne.
#>


function MyCallback {
    param ($msg)

    Write-Host "==== Message Received ===="
    Write-Host "URL         : $($msg.Url)"
    Write-Host "MessageName : $($msg.MessageName)"
    Write-Host "Body        : $($msg.Body | ConvertTo-Json -Depth 5)"
    Write-Host "=========================="
}
