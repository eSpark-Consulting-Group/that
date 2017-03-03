del bin /Q
del ForensicAnalysisTool.7z
set ZDIR="..\..\SfxPackage\"
mkdir "%CD%\bin"
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
"%ZDIR%\7za.exe" a -mx9 "%CD%\ForensicAnalysisTool.7z" "%CD%\ForensicAnalysisTool.exe" "%CD%\bin" || goto :error

ren ForensicAnalysisTool.exe FAT.Bootstrapper.exe
move bin\ForensicAnalysisTool.exe ForensicAnalysisTool.exe

del bin /Q
