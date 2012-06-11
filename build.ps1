param( [int]$buildNumber = 0 )
Import-Module .\tools\psake\psake.psm1
Invoke-Psake .\default.ps1 default -framework "4.0" -properties @{ buildNumber=$buildNumber }
Remove-Module psake