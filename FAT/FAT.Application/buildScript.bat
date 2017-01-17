set ZDIR="..\..\SfxPackage\"
mkdir "%CD%\sfx"
"%ZDIR%\7za.exe" a -mx9  "%CD%\archive.7z" "%CD%\*.txt" "%CD%\archive.7z" "%CD%\*.dll" "%CD%\ForensicAnalysisTool.exe" "%CD%\ForensicAnalysisTool.exe.config" "%CD%\FAT.Bootstrapper.exe" "%CD%\cmdapt86.exe" "%CD%\cmdapt64.exe" "%CD%\Fonts" || goto :error
copy /b "%ZDIR%\7zSD.sfx" + "%ZDIR%\config.txt" + "%CD%\archive.7z" "%CD%\sfx\FAT.exe" || goto :error
del  "%CD%\archive.7z"