@echo off
cd %systemroot%\system32\inetsrv
appcmd stop apppool /apppool.name:"training_cosmatics"
cd C:\Users\Administrator\cosmatics_api
dotnet publish Cosmatics.csproj -c Release -o C:\inetpub\wwwroot\training_cosmatics
cd %systemroot%\system32\inetsrv
appcmd start apppool /apppool.name:"training_cosmatics"

