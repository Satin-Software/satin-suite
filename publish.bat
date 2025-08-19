@echo off
setlocal enabledelayedexpansion

echo Build for Win7 (uses .NET 6 runtime) satin-mq-recv
dotnet publish satin-mq-recv\satin-mq-recv.csproj -c Release -f net6.0 -r win-x64 --self-contained true -p:PublishSingleFile=true -o ./publish/win7
echo Build for Win7 (uses .NET 6 runtime) satin-mq-send
dotnet publish satin-mq-send\satin-mq-send.csproj -c Release -f net6.0 -r win-x64 --self-contained true -p:PublishSingleFile=true -o ./publish/win7

echo Build for Win10/11 (uses .NET 8 runtime) satin-mq-recv
dotnet publish satin-mq-recv\satin-mq-recv.csproj -c Release -f net8.0 -r win-x64 --self-contained true -p:PublishSingleFile=true -o ./publish/win10-11
echo Build for Win10/11 (uses .NET 8 runtime) satin-mq-send
dotnet publish satin-mq-send\satin-mq-send.csproj -c Release -f net8.0 -r win-x64 --self-contained true -p:PublishSingleFile=true -o ./publish/win10-11

REM Check if first argument is /z (to create release.zip)
if "%~1"=="/z" (
	:: Remove and clean release folder
	echo Cleaning up old release folder...
	if exist publish\release (
		rmdir /s /q publish\release
	)

	:: Create the release directories if they don’t exist
	echo Creating release folders...
	mkdir publish\release\win7
	mkdir publish\release\win10-11

	:: Copy all .exe files from the current win7 folder to release\win7
	if exist publish\win7 (
		echo Copying executables from win7...
		xcopy /y /i publish\win7\*.exe publish\release\win7\
	)

	:: Copy all .exe files from the current win10-11 folder to release\win10-11
	if exist publish\win10-11 (
		echo Copying executables from win10-11...
		xcopy /y /i "publish\win10-11\*.exe" publish\release\win10-11\
	)

	:: Copy corresponding .ini files for win7 executables
	for %%f in (publish\release\win7\*.exe) do (
		set "exeFile=%%~nxf"
		set "iniFile=C:\satin\%%~nf.ini"
		if exist "!iniFile!" (
			echo Copying ini for %%~nf to win7
			copy /y "!iniFile!" publish\release\win7\
			echo Copying ini for exeFile to win10-11
			copy /y "!iniFile!" publish\release\win10-11\
		)
	)

	:: Create a zip file containing the release folder and its contents (Using Powershell Compress-Archive command)
	echo Creating satin-suite_v.zip ...
	powershell -command "Compress-Archive -Path publish\release\* -DestinationPath publish\satin-suite_v.zip -Force"
)

echo Done!
endlocal
pause