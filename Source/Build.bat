@echo off 

rd /S /Q bin
rd /S /Q obj

set FrameworkPath=%ProgramFiles%\Microsoft Visual Studio 8\SmartDevices\SDK\CompactFramework\2.0\v1.0\WindowsCE\

set ToolsPath=%ProgramFiles%\Microsoft Visual Studio 8\SmartDevices\SDK\SDKTools\

%SystemRoot%\Microsoft.net\Framework\v1.1.4322\csc.exe /noconfig /nostdlib /target:winexe /out:..\Build\Digger.exe ApplicationWindow.cs Engine.cs /win32icon:Application.ico /reference:"%FrameworkPath%\mscorlib.dll" /reference:"%FrameworkPath%\System.dll" /reference:"%FrameworkPath%\System.Drawing.dll" /reference:"%FrameworkPath%\System.Windows.Forms.dll" /resource:Application.ico,Digger.Application.ico /resource:Font.bin,Digger.Font.bin /resource:Level.bin,Digger.Level.bin /resource:Sprites.bin,Digger.Sprites.bin

pushd ..\Build
copy ..\Source\Digger.inf
"%ToolsPath%\cabwiz.exe" Digger.inf
del Digger.inf
ren Digger.CAB Digger.cab
popd
