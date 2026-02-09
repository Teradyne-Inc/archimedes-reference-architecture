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

function Start-WebServiceListener {
    param (
        [string]$Url,
        [System.Collections.Concurrent.ConcurrentQueue[object]]$Queue,
        [System.Threading.CancellationToken]$Token
    )

    $listener = New-Object System.Net.HttpListener
    $listener.Prefixes.Add($Url)
    $listener.Start()

    try {
        while (-not $Token.IsCancellationRequested) {
            $context = $listener.GetContext()
            $request = $context.Request
            $response = $context.Response

            $messageName = ($request.Url.AbsolutePath -split '/')[-1]
            $body = (New-Object IO.StreamReader($request.InputStream)).ReadToEnd()
            $jsonBody = $null

            try {
                $jsonBody = $body | ConvertFrom-Json -ErrorAction Stop
            } catch {
                Write-Host "Malformed JSON: $body"
            }

            $msg = [DASMessage]::new($request.Url.AbsolutePath, $messageName, $jsonBody)
            $Queue.Enqueue($msg)

            $response.StatusCode = 200
            $response.Close()
        }
    } finally {
        $listener.Stop()
        $listener.Close()
    }
}
