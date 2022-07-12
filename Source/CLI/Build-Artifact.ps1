if (Test-Path .\artifact\) { Remove-Item -Recurse .\artifact\ }

git version
$publishCommand = 'dotnet publish -o .\artifact -c Release -p:PublishProfile=win-x64 -p:DebugType=none -p:DebugSymbols=false'
if ($?) 
{
    $tag = git describe --tags --abbrev=0
    $version = $tag.Substring(1)
    $commit = git -c log.showSignature=false log --format=format:%h -n 1
    $infoVersion = "$version+$commit"
    $publishCommand += " -p:Version=$infoVersion"
}
Invoke-Expression $publishCommand