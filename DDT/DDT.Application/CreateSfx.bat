set ZDIR="..\..\SfxPackage\"
"%ZDIR%\7za.exe" a   "%CD%\archive.7z" "%CD%\archive.7z" "%CD%\*.dll" "%CD%\DDT.Application.exe" "%CD%\DDT.Application.exe.config" "%CD%\DDT.Bootstrapper.exe" "%CD%\DescentHelper.exe" || goto :error
copy /b "%ZDIR%\7zSD.sfx" + "%ZDIR%\config.txt" + "%CD%\archive.7z" "%CD%\ADDT.exe" || goto :error
del  "%CD%\archive.7z"