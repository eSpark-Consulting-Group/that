set ZDIR="..\..\SfxPackage\"
mkdir "%CD%\sfx"
"%ZDIR%\7za.exe" a -mx9  "%CD%\archive.7z" "%CD%\*.txt" "%CD%\archive.7z" "%CD%\*.dll" "%CD%\ForensicAnalysisTool.exe" "%CD%\ForensicAnalysisTool.exe.config" "%CD%\FAT.Bootstrapper.exe" "%CD%\cmdapt86.exe" "%CD%\cmdapt64.exe" "%CD%\Fonts" || goto :error
copy /b "%ZDIR%\7zSD.sfx" + "%ZDIR%\config.txt" + "%CD%\archive.7z" "%CD%\sfx\FAT.exe" || goto :error
del  "%CD%\archive.7z"

copy ResourceCopy.exe sfx\ResourceCopy.exe
copy ForensicAnalysisTool.exe sfx\ForensicAnalysisTool.exe
cd sfx

START /WAIT ResourceCopy.exe ForensicAnalysisTool.exe FAT.exe
ResourceHacker -compile version.rc, version.res
ren FAT.exe FAT_old.exe
ResourceHacker -modify FAT_old.exe, FAT.exe, version.res,,,

del version.rc
del version.res
del FAT_old.exe
del ForensicAnalysisTool.exe
del ResourceCopy.exe