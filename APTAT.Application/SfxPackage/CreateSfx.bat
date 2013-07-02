@echo on

setlocal

rem set all environment variables for VisualStudio 

set PROJECT_DIR=%~dp0..\
set SOURCEDIR=%~dp0..\..\..\
set OUTDIR=%~dp1
set VERSION=%~2

if "%VERSION%"=="" set VERSION=1.0.0.0

set SFX_FILENAME=APTAT%VERSION%.exe

"%PROJECT_DIR%7zip\7za.exe" a  -t7z -mx1 "%OUTDIR%archive.7z" "%OUTDIR%*" || goto :error
copy /b "%OUTDIR%7zSD.sfx" + "%PROJECT_DIR%7zip\config.txt" + "%OUTDIR%archive.7z" "%OUTDIR%%SFX_FILENAME%" || goto :error
del  "%OUTDIR%archive.7z"
  
@echo SFX created.
exit /B 0

:error
@echo Build failed.
exit /B 1

