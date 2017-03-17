$rootNamespace = "MyLib"
$url = "https://raw.githubusercontent.com/damianh/LibLog/maybe-we-just-download-the-class/releases/latest/LibLog.cs"
$response = Invoke-WebRequest $url
$response.Content.Replace("`$rootnamespace`$", $rootNamespace) | Out-File -Encoding "UTF8" "LibLog.cs"