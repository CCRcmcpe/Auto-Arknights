if (Test-Path .\artifact\) { Remove-Item -Recurse .\artifact\ }
dotnet publish -c Publish -o artifact
if ($?) { Remove-Item artifact\*.pdb }