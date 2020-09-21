# FileWatcherAsService
Simple FileWatcher which may run action if something has been change in file system



Install service with command:

cd C:\Windows\Microsoft.NET\Framework\v4.0.30319 
installUtil C:\...\bin\Debug\PentahoRunOnFileChangeService.exe
pause



Uninstall service with command:

cd C:\Windows\Microsoft.NET\Framework\v4.0.30319 
installUtil -u C:\...\bin\Debug\PentahoRunOnFileChangeService.exe
pause




Command.json example:

[{
	"folderName":"C:\\SomeFolder",
	"commandPath":"C:\\SomeFolde\\SomeActionFolder\\some.bat"
}]