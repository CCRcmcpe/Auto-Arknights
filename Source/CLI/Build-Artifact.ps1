if (Test-Path .\artifact\) { Remove-Item -Recurse .\artifact\ }

dotnet clean -c Release -r win-x64
dotnet publish -c Release -p:PublishProfile="Windows x64"