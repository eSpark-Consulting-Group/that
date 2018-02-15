rmdir bin /s /Q
del UnknownFileHunter.7z
rmdir sfx /s/q
set ZDIR="..\..\SfxPackage\"
mkdir "%CD%\sfx"
mkdir "%CD%\bin"
mkdir "%CD%\data"

copy *.dll bin\*.dll
move UnknownFileHunter.exe bin\UnknownFileHunter.exe
copy UnknownFileHunter.exe.config bin\UnknownFileHunter.exe.config
copy FAT.Extractor.exe bin\FAT.Extractor.exe
copy cmdapt64.exe bin\cmdapt64.exe
copy cmdapt86.exe bin\cmdapt86.exe
mkdir "%CD%\bin\amd64"
mkdir "%CD%\bin\x86"
xcopy /s amd64 bin\amd64
xcopy /s x86 bin\x86
copy *.txt bin\*.txt
ren FAT.Bootstrapper.exe UnknownFileHunter.exe

"%ZDIR%\7za.exe" a  "%CD%\UnknownFileHunter.7z" "%CD%\UnknownFileHunter.exe" "%CD%\bin" "%CD%\data" || goto :error

copy /b "%ZDIR%\7zSD.sfx" + "%ZDIR%\config.txt" + "%CD%\UnknownFileHunter.7z" "%CD%\sfx\UnknownFileHunter.exe" || goto :error

ren UnknownFileHunter.exe FAT.Bootstrapper.exe
move bin\UnknownFileHunter.exe UnknownFileHunter.exe
del UnknownFileHunter.7z /Q
rmdir bin /Q /s
rmdir data /Q /s

copy ResourceCopy.exe sfx\ResourceCopy.exe
copy  UnknownFileHunter.exe sfx\fat.exe
cd sfx
START /WAIT ResourceCopy.exe fat.exe UnknownFileHunter.exe 
ResourceHacker -compile version.rc, version.res
ren UnknownFileHunter.exe UnknownFileHunter_old.exe
ResourceHacker -modify UnknownFileHunter_old.exe, UnknownFileHunter.exe, version.res,,,

del version.rc
del version.res
del UnknownFileHunter_old.exe
del ResourceCopy.exe
del fat.exe