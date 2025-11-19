<#
.DISCLAIMER
    This script is provided "as is" without warranty of any kind, express or implied,
    including but not limited to the warranties of merchantability, fitness for a particular purpose,
    and noninfringement. The script is intended for demonstration and development purposes only.

    It is not certified for production use and may require adaptation depending on your environment.
    Use at your own risk. Teradyne shall not be held liable for any damages resulting from the use
    or misuse of this code.

    This code may not be distributed outside without prior consent from Teradyne.
#>


# Load required files
. "$PSScriptRoot\\source\\DASMessage.ps1"
. "$PSScriptRoot\\source\\DASListener.ps1"
. "$PSScriptRoot\\source\\CallbackExample.ps1"

# Init queue and cancellation token
$queue = [System.Collections.Concurrent.ConcurrentQueue[object]]::new()
$cts = [System.Threading.CancellationTokenSource]::new()

# Define the callback that will log and display messages
function LiveCallback {
    param ($msg)

    $timestamp = (Get-Date).ToString("HH:mm:ss")
    Write-Host "$timestamp | [$($msg.MessageName)]"
    if ($msg.Body) {
        Write-Host ($msg.Body | ConvertTo-Json -Depth 5) -ForegroundColor Cyan
    }
    else {
        Write-Host "No body received." -ForegroundColor Yellow
    }
}

# Start consumer job
$consumerJob = Start-Job -ScriptBlock {
    param ($queue, $callback, $token)

    while (-not $token.IsCancellationRequested) {
        if (-not $queue.IsEmpty) {
            $ok, [ref]$msg = $null
            $ok = $queue.TryDequeue([ref]$msg)
            if ($ok) {
                & $callback.Invoke($msg.Value)
            }
        } else {
            Start-Sleep -Milliseconds 100
        }
    }

    Write-Host "Consumer stopped."
} -ArgumentList $queue, ${function:LiveCallback}, $cts.Token

# Start the listener (optionally add -LogFilePath)
Start-Job -ScriptBlock {
    param ($queue, $token)
    Start-WebServiceListener -Url "http://127.0.0.1:1000/TestDAS/" -Queue $queue -Token $token -LogFilePath "$env:TEMP\\das.log"
} -ArgumentList $queue, $cts.Token | Out-Null

Write-Host "`nDAS Listener is running at http://127.0.0.1:1000/TestDAS/"
Write-Host "Press 'q' then Enter to stop..."

# Wait for user input
do {
    $input = Read-Host
} while ($input -ne 'q')

# Stop
$cts.Cancel()
Stop-Job $consumerJob -Force
Remove-Job $consumerJob
Write-Host "DAS Listener stopped."
