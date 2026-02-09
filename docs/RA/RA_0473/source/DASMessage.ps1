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

class DASMessage {
    [string]$Url
    [string]$MessageName
    [object]$Body

    DASMessage([string]$url, [string]$messageName, [object]$body) {
        $this.Url = $url
        $this.MessageName = $messageName
        $this.Body = $body
    }
}
