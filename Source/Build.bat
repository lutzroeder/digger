@echo off

set FrameworkVersion=1.0.3705
if exist %SystemRoot%\Microsoft.NET\Framework\v%FrameworkVersion%\csc.exe goto :Start
set FrameworkVersion=1.1.4322
if exist %SystemRoot%\Microsoft.NET\Framework\v%FrameworkVersion%\csc.exe goto :Start
set FrameworkVersion=2.0.50727
if exist %SystemRoot%\Microsoft.NET\Framework\v%FrameworkVersion%\csc.exe goto :Start
:Start

if exist bin rd /S /Q bin
if exist obj rd /S /Q obj
if exist ..\Build rd /S /Q ..\Build
md ..\Build

%SystemRoot%\Microsoft.net\Framework\v%FrameworkVersion%\csc.exe /target:winexe /out:..\Build\Digger.exe ApplicationWindow.cs Engine.cs /win32icon:Application.ico /resource:Application.ico /resource:Font.png,Digger.Font.png /resource:Level.bin,Digger.Level.bin /resource:Sprite.png,Digger.Sprite.png %1
