del bin /Q
del ForensicAnalysisTool.7z
rmdir sfx /s/q
set ZDIR="..\..\SfxPackage\"
mkdir "%CD%\sfx"
mkdir "%CD%\bin"
mkdir "%CD%\data"

copy *.dll bin\*.dll
move ForensicAnalysisTool.exe bin\ForensicAnalysisTool.exe
copy ForensicAnalysisTool.exe.config bin\ForensicAnalysisTool.exe.config
copy cmdapt64.exe bin\cmdapt64.exe
copy cmdapt86.exe bin\cmdapt86.exe
mkdir "%CD%\bin\amd64"
mkdir "%CD%\bin\x86"
copy amd64\* bin\amd64\*
copy x86\* bin\x86\*
copy *.txt bin\*.txt
ren FAT.Bootstrapper.exe ForensicAnalysisTool.exe
Y
"%ZDIR%\7za.exe" a  "%CD%\ForensicAnalysisTool.7z" "%CD%\ForensicAnalysisTool.exe" "%CD%\bin" "%CD%\data" || goto :error

copy /b "%ZDIR%\7zSD.sfx" + "%ZDIR%\config.txt" + "%CD%\ForensicAnalysisTool.7z" "%CD%\sfx\FAT.exe" || goto :error

ren ForensicAnalysisTool.exe FAT.Bootstrapper.exe
move bin\ForensicAnalysisTool.exe ForensicAnalysisTool.exe
del ForensicAnalysisTool.7z /Q
del bin /Q
del data /Q
