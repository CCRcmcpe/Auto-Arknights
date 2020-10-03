dotnet publish -c Release -v n
Remove-Item artifact/*.pdb
Remove-Item -R artifact/dll
$depsFile = [System.IO.Path]::Combine($pwd, 'artifact/Auto Arknights CLI.deps.json')
$deps = [System.IO.File]::ReadAllText($depsFile).Replace('
          "runtimes/win-x64/native/OpenCvSharpExtern.pdb": {
            "fileVersion": "0.0.0.0"
          },', [System.String]::Empty)
[System.IO.File]::WriteAllText($depsFile, $deps)
try { Pause } catch { }