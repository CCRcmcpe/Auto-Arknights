if (Test-Path .\artifact\) { Remove-Item -Recurse .\artifact\ }

dotnet publish -c Release -p:PublishProfile="Windows x64"