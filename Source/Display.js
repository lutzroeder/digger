
var Display = function(canvas, imageData)
{
	this.context = canvas.getContext("2d");
	this.imageData = imageData;
	
	this.context.fillStyle = "#00ffff"; 
	this.context.fillRect(0,  2, 320, 4);
	this.context.fillRect(0, 26, 320, 4);
	this.context.fillStyle = "#920205"; 
	this.context.fillRect(0, 8, 320, 16);
	this.drawText(this.context, 0,  8, "  ROOM:     TIME:        DIAMONDS:      ");
	this.drawText(this.context, 0, 16, "  LIVES:    SCORE:       COLLECTED:     ");

	this.screenTable = [];
	for (var x = 0; x < 20; x++)
	{
		this.screenTable[x] = [];
		for (var y = 0; y < 14; y++)
		{
			this.screenTable[x][y] = 0;
		}
	}
};

Display.prototype.paint = function(game, level, blink)
{
	// update statusbar
	this.context.fillStyle = "#920205"; 
	this.drawText(this.context,  9 * 8,  8, this.formatNumber(game.room + 1, 2));
	this.drawText(this.context,  9 * 8, 16, this.formatNumber(game.lives, 2));
	this.drawText(this.context, 19 * 8, 16, this.formatNumber(game.score, 5));
	this.drawText(this.context, 19 * 8,  8, this.formatNumber(level.time, 5));
	this.drawText(this.context, 36 * 8,  8, this.formatNumber(level.diamonds, 2));
	this.drawText(this.context, 36 * 8, 16, this.formatNumber(level.collected, 2));

	// paint sprites
	for (var x = 0; x < 20; x++)
	{
		for (var y = 0; y < 14; y++)
		{
			var spriteIndex = this.getSpriteIndex(level.map[x][y], blink);
			if (this.screenTable[x][y] != spriteIndex)
			{
				this.screenTable[x][y] = spriteIndex;
				this.context.drawImage(this.imageData[0], spriteIndex * 16, 0, 16, 16, x * 16, y * 16 + 32, 16, 16);
			}
		}
	}
};

Display.prototype.drawText = function(context, x, y, text)
{
	for (var i = 0; i < text.length; i++)
	{
		var index = text.charCodeAt(i) - 32;
		this.context.fillRect(x, y, 8, 8);
		this.context.drawImage(this.imageData[1], 0, index * 8, 8, 8, x, y, 8, 8);
		x += 8;
	}	
};

Display.prototype.formatNumber = function(value, digits)
{
	var text = value.toString();
	while (text.length < digits)
	{
		text = "0" + text;
	}
	return text; 
};

Display.prototype.getSpriteIndex = function(value, blink)
{
	switch (value)
	{
		case Sprite.nothing:   return 0;
		case Sprite.stone:     return 1;
		case Sprite.ground:    return 2;
		case Sprite.uvexit:    return 0;
		case Sprite.diamond:   return 13 - ((blink + 4) % 6);
		case Sprite.wall:      return 14;
		case Sprite.exit:      return 32;
		case Sprite.changer:   return 33;
		case Sprite.buffer:    return 0;
		case Sprite.marker:    return 0;
		case Sprite.uvstone:   return 0;
		case Sprite.player:    return 15;
		default:               return value.getImageIndex();
	}
};
