set ZDIR="..\..\SfxPackage\"
mkdir "%CD%\sfx"
"%ZDIR%\7za.exe" a -mx9  "%CD%\archive.7z" "%CD%\*.txt" "%CD%\archive.7z" "%CD%\*.dll" "%CD%\UnknownFileHunter.exe" "%CD%\UnknownFileHunter.exe.config" "%CD%\APTAT.Bootstrapper.exe" "%CD%\cmdapt86.exe" "%CD%\cmdapt64.exe" "%CD%\amd64" "%CD%\x86" "%CD%\Fonts" || goto :error
copy /b "%ZDIR%\7zSD.sfx" + "%ZDIR%\config.txt" + "%CD%\archive.7z" "%CD%\sfx\UnknownFileHunter.exe" || goto :error
del  "%CD%\archive.7z"