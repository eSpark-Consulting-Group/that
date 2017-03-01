set ZDIR="..\..\SfxPackage\"
mkdir "%CD%\sfx"
"%ZDIR%\7za.exe" a -mx9  "%CD%\archive.7z" "%CD%\*.txt" "%CD%\archive.7z" "%CD%\*.dll" "%CD%\ForensicAnalysisTool.exe" "%CD%\ForensicAnalysisTool.exe.config" "%CD%\FAT.Bootstrapper.exe" "%CD%\cmdapt86.exe" "%CD%\cmdapt64.exe" "%CD%\amd64" "%CD%\x86" || goto :error
copy /b "%ZDIR%\7zSD.sfx" + "%ZDIR%\config.txt" + "%CD%\archive.7z" "%CD%\sfx\FAT.exe" || goto :error
del  "%CD%\archive.7z"

copy ResourceCopy.exe sfx\ResourceCopy.exe
copy ForensicAnalysisTool.exe sfx\ForensicAnalysisTool.exe
cd sfx

START /WAIT ResourceCopy.exe ForensicAnalysisTool.exe ForensicAnalysisTool.exe
del ForensicAnalysisTool.exe
START /WAIT ResourceHacker -compile version.rc, version.res
START /WAIT ResourceHacker -modify FAT.exe, ForensicAnalysisTool.exe, version.res,,,

del version.rc
del version.res
del FAT.exe
del ResourceCopy.exe