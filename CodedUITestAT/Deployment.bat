REM Gets build deployment arguments (Deploy step) and sets the respective local machine variables

set BuildLocation=%~1

set EndpointName=%~2

set ServerLogin=tester

set ServerPassword=test

set EndpointLogin=%ServerLogin%

set EndpointPassword=%ServerPassword%

set ServerRoot=C

set EndpointRoot=%ServerRoot%

set TargetDir=%ServerRoot%:\CNAT

set ProductExeName=APTAT.exe



REM Stores the local machine variables to be used in autotests

setx _ServerLogin %ServerLogin% /m

setx _ServerPassword %ServerPassword% /m

setx _EndpointLogin %EndpointLogin% /m

setx _EndpointPassword %EndpointPassword% /m

setx _EndpointName %EndpointName% /m

setx _TargetDir %TargetDir% /m

setx _ServerRoot %ServerRoot% /m

setx _EndpointRoot %EndpointRoot% /m

setx _ProductExeName %ProductExeName% /m



REM Copies files necessary for testing to the server VM

xcopy "%BuildLocation%\%ProductExeName%" "%TargetDir%\" /y