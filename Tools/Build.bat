@echo off

pushd ..\Source
msbuild.exe Digger.csproj /target:Build /property:Configuration=Release
if exist obj rd /q /s obj
popd

if exist ..\Build\Digger.pdb del ..\Build\Digger.pdb
if exist ..\Build\Digger.dll del ..\Build\Digger.dll
if exist ..\Build\AppManifest.xaml del ..\Build\AppManifest.xaml
