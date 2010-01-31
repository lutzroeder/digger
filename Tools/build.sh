#!/bin/bash
rm -r ../Build
mkdir ../Build
mkdir ../Build/Debug
mkdir ../Build/Release
cd ../Source
cp Default.html ../Build/Debug/default.html
cp Default.html ../Build/Release/default.html
echo Building \'Debug/digger.js\'
target=../Build/Debug/digger.js
cat	Direction.js \
	Sprite.js \
	Key.js \
	Sound.js \
	Player.js \
	Ghost.js \
	Position.js \
	Base64Reader.js \
	Level.js \
	Display.js \
	Loader.js \
	Input.js \
	Function.js \
	Digger.js \
	LevelData.js \
	> $target
echo 'Digger.prototype.imageData = [ "'$(openssl base64 -in Sprite.png | tr -d '\n')'","'$(openssl base64 -in Font.png | tr -d '\n')'" ];' >> $target  
echo 'Digger.prototype.soundData = [ "'$(openssl base64 -in Diamond.wav | tr -d '\n')'","'$(openssl base64 -in Stone.wav | tr -d '\n')'","'$(openssl base64 -in Step.wav | tr -d '\n')'" ];' >> $target  
echo Building \'Release/digger.js\'
cd ../Tools
echo Done.
