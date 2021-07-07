if (Test-Path .\artifact\) { Remove-Item -Recurse .\artifact\ }

git version
if ($?) 
{
    $tag = git describe --abbrev=0
    $version = $tag.Substring(1)
    $commit = git -c log.showSignature=false log --format=format:%h -n 1
    $infoVersion = "$version+$commit"
    dotnet publish -o .\artifact -c Release -p:PublishProfile="Windows x64" -p:Version=$infoVersion
}
else
{
    dotnet publish -o .\artifact -c Release -p:PublishProfile="Windows x64"
}