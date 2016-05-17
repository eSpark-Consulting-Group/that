set ZDIR="..\..\SfxPackage\"
"%ZDIR%\7za.exe" a   "%CD%\archive.7z" "%CD%\*.txt" "%CD%\*.rtf"  "%CD%\archive.7z" "%CD%\*.dll" "%CD%\UnknownFileHunter.exe" "%CD%\UnknownFileHunter.exe.config" "%CD%\APTAT.Bootstrapper.exe" "%CD%\cmdapt86.exe" "%CD%\cmdapt64.exe" "%CD%\Fonts" || goto :error
copy /b "%ZDIR%\7zSD.sfx" + "%ZDIR%\config.txt" + "%CD%\archive.7z" "%CD%\ValkyrieHunter.exe" || goto :error
del  "%CD%\archive.7z"