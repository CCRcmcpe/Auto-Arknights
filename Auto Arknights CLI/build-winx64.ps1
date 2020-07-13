dotnet publish -c Release -v n
Remove-Item artifact\*.pdb
$depsFile = 'artifact\Auto Arknights CLI.deps.json'
$deps = [System.IO.File]::ReadAllText($depsFile).Replace('
"runtimes/win-x64/native/OpenCvSharpExtern.pdb": {
  "fileVersion": "0.0.0.0"
},', [System.String]::Empty)
[System.IO.File]::WriteAllText($depsFile, $deps)
Pause