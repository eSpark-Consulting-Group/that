rmdir sfx /s/q
set ZDIR="..\..\SfxPackage\"
mkdir "%CD%\sfx"

"%ZDIR%\7za.exe" a -mx9 "%CD%\archive.7z" "%CD%\archive.7z" "%CD%\*.dll" "%CD%\DDT.Application.exe" "%CD%\DDT.Application.exe.config" "%CD%\DDT.Bootstrapper.exe" "%CD%\DescentHelper.exe" || goto :error
copy /b "%ZDIR%\7zSD.sfx" + "%ZDIR%\config.txt" + "%CD%\archive.7z" "%CD%\ADDT.exe" || goto :error
del  "%CD%\archive.7z"

copy ResourceCopy.exe sfx\ResourceCopy.exe
copy ResourceHacker.exe sfx\ResourceHacker.exe
copy  ADDT.exe sfx\ADDT.exe
copy  DDT.Application.exe sfx\DDT.Application.exe

cd sfx
START /WAIT ResourceCopy.exe DDT.Application.exe ADDT.exe 

ResourceHacker.exe -compile version.rc, version.res
ren ADDT.exe ADDT_old.exe
ResourceHacker.exe -modify ADDT_old.exe, ADDT.exe, version.res,,,

del version.rc
del version.res
del ADDT_old.exe
del ResourceCopy.exe
del ResourceHacker.*
del DDT.Application.exe

cd ..