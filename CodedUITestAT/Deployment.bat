REM Gets build deployment arguments (Deploy step) and sets the necessary local machine variables

set IsStaging=%~1

set BuildLocation=%~2

set EndpointName=%~3

set ServerLogin=tester

set ServerPassword=test

set EndpointLogin=%ServerLogin%

set EndpointPassword=%ServerPassword%

set ServerRoot=C

set EndpointRoot=%ServerRoot%

set TargetDir=%ServerRoot%:\CNAT

set ProductExeName=ValkyrieHunter.exe

set DataSource=AutotestsData\AT

set MalwaresFolder=Malfolder



REM Stores the local machine variables to be used in autotests

setx _IsStaging %IsStaging% /m

setx _ServerLogin %ServerLogin% /m

setx _ServerPassword %ServerPassword% /m

setx _EndpointLogin %EndpointLogin% /m

setx _EndpointPassword %EndpointPassword% /m

setx _EndpointName %EndpointName% /m

setx _TargetDir %TargetDir% /m

setx _ServerRoot %ServerRoot% /m

setx _EndpointRoot %EndpointRoot% /m

setx _ProductExeName %ProductExeName% /m

setx _MalwaresFolder %MalwaresFolder% /m



REM Copies files necessary for testing to the server VM

xcopy "%BuildLocation%\%ProductExeName%" "%TargetDir%\" /y

xcopy "\\esmbuildstorage\%DataSource%\%MalwaresFolder%" "%ServerRoot%:\%MalwaresFolder%\" /e /y



REM Copies files necessary for testing to the endpoint VM

net use x: "\\%EndpointName%\%EndpointRoot%$" /user:%EndpointLogin% %EndpointPassword%

net use s: "\\esmbuildstorage\%DataSource%"

xcopy "s:\%MalwaresFolder%" "x:\%MalwaresFolder%\" /e /y

net use x: /delete

net use s: /delete