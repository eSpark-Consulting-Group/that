rmdir bin /s /Q
del ForensicAnalysisTool.7z
rmdir sfx /s/q
set ZDIR="..\..\SfxPackage\"
mkdir "%CD%\sfx"
mkdir "%CD%\bin"
mkdir "%CD%\data"

copy *.dll bin\*.dll
move ForensicAnalysisTool.exe bin\ForensicAnalysisTool.exe
copy ForensicAnalysisTool.exe.config bin\ForensicAnalysisTool.exe.config
copy FAT.Extractor.exe bin\FAT.Extractor.exe
copy cmdapt64.exe bin\cmdapt64.exe
copy cmdapt86.exe bin\cmdapt86.exe
mkdir "%CD%\bin\amd64"
mkdir "%CD%\bin\x86"
xcopy /s amd64 bin\amd64
xcopy /s x86 bin\x86
copy *.txt bin\*.txt
ren FAT.Bootstrapper.exe ForensicAnalysisTool.exe

"%ZDIR%\7za.exe" a  "%CD%\ForensicAnalysisTool.7z" "%CD%\ForensicAnalysisTool.exe" "%CD%\bin" "%CD%\data" || goto :error

copy /b "%ZDIR%\7zSD.sfx" + "%ZDIR%\config.txt" + "%CD%\ForensicAnalysisTool.7z" "%CD%\sfx\ForensicAnalysisTool.exe" || goto :error

ren ForensicAnalysisTool.exe FAT.Bootstrapper.exe
move bin\ForensicAnalysisTool.exe ForensicAnalysisTool.exe
del ForensicAnalysisTool.7z /Q
rmdir bin /Q /s
rmdir data /Q /s

copy ResourceCopy.exe sfx\ResourceCopy.exe
copy  ForensicAnalysisTool.exe sfx\fat.exe
cd sfx
START /WAIT ResourceCopy.exe fat.exe ForensicAnalysisTool.exe 
ResourceHacker -compile version.rc, version.res
ren ForensicAnalysisTool.exe ForensicAnalysisTool_old.exe
ResourceHacker -modify ForensicAnalysisTool_old.exe, ForensicAnalysisTool.exe, version.res,,,

del version.rc
del version.res
del ForensicAnalysisTool_old.exe
del ResourceCopy.exe
del fat.exe