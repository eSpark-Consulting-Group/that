rmdir bin /s /Q
del UnknownFileHunter.7z
rmdir sfx /s/q
set ZDIR="..\..\SfxPackage\"
set ZDIRFROMBIN="..\..\..\SfxPackage\"
mkdir "%CD%\sfx"
mkdir "%CD%\bin"

copy *.dll bin\*.dll
copy UnknownFileHunter.exe bin\UnknownFileHunter.exe
copy UnknownFileHunter.exe.config bin\UnknownFileHunter.exe.config
copy APTAT.Extractor.exe bin\APTAT.Extractor.exe
copy cmdapt64.exe bin\cmdapt64.exe
copy cmdapt86.exe bin\cmdapt86.exe
mkdir "%CD%\bin\amd64"
mkdir "%CD%\bin\x86"
xcopy /s amd64 bin\amd64
xcopy /s x86 bin\x86
copy *.txt bin\*.txt

cd bin
"%ZDIRFROMBIN%\7za.exe" a  "%CD%\UnknownFileHunter.7z" "*" || goto :error
cd ..
copy /b "%ZDIR%\7zSD.sfx" + "%ZDIR%\config.txt" + "%CD%\bin\UnknownFileHunter.7z" "%CD%\sfx\UnknownFileHunter.exe" || goto :error

rmdir bin /Q /s

copy ResourceHacker.exe sfx\ResourceHacker.exe
copy ResourceCopy.exe sfx\ResourceCopy.exe
copy  UnknownFileHunter.exe sfx\fat.exe
cd sfx
START /WAIT ResourceCopy.exe fat.exe UnknownFileHunter.exe 

ResourceHacker.exe -compile version.rc, version.res
ren UnknownFileHunter.exe UnknownFileHunter_old.exe
ResourceHacker.exe -modify UnknownFileHunter_old.exe, UnknownFileHunter.exe, version.res,,,

del version.rc
del version.res
del UnknownFileHunter_old.exe
del ResourceCopy.exe
del ResourceHacker.*
del fat.exe

cd ..