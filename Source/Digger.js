
var Digger = function(element)
{
	this.canvas = element;
	this.canvas.focus();
	this.loader = new Loader();
	this.loader.loadAudioData(this.soundData);
	this.loader.loadImageData(this.imageData);
	this.loader.start(this.loaderCallback.bind(this));
};

Digger.prototype.loaderCallback = function()
{
	this.display = new Display(this.canvas, this.imageData);
	this.input = new Input(this.canvas, this);
	this.blink = 0;
	this.restart();
	this.intervalHandler = this.interval.bind(this);
	window.setInterval(this.intervalHandler, 50);
};

Digger.prototype.addKey = function(key)
{
	if (key < 4)
	{
		this.keys[key] = true;
	}
	else if (key == Key.reset)
	{
		this.lives--;
		if (this.lives >= 0)
		{
			this.loadLevel();
		}
		else
		{
			this.restart();
		}
	}
};

Digger.prototype.removeKey = function(key)
{
	if (key < 4)
	{
		this.keysRelease[key] = true;
	}
};

Digger.prototype.restart = function()
{
	this.lives = 20;
	this.score = 0;
	this.room = 0;
	this.loadLevel();
};

Digger.prototype.loadLevel = function()
{
	this.level = new Level(this.levelData[this.room]);
	this.keys = [ false, false, false, false ];
	this.keysRelease = [ false, false, false, false ];
	this.tick = 0;
	this.paint();
};

Digger.prototype.nextLevel = function()
{
	if (this.room < (this.levelData.length - 1))
	{
		this.room++;
		this.loadLevel();
	}
};

Digger.prototype.isAlive = function()
{
	return (this.level === null) || (this.level.player.alive);
};

Digger.prototype.interval = function()
{
	var i;
	this.tick++;
	this.blink++;
	if (this.blink == 6)
	{
		this.blink = 0;
	}
	if ((this.tick % 2) === 0)
	{
		this.level.soundTable = [];
		for (i = 0; i < this.soundData.length; i++)
		{
			this.level.soundTable[i] = false;
		}

		// keyboard
		for (i = 0; i < 4; i++)
		{
			if (this.keysRelease[i])
			{
				this.keys[i] = false;
				this.keysRelease[i] = false;
			}
		}

		this.level.update();
		if (this.level.movePlayer(this.keys))
		{
			this.nextLevel();
		}
		else
		{
			this.level.move();

			// play sound
			for (i = 0; i < this.level.soundTable.length; i++)
			{
				if (this.level.soundTable[i] && this.soundData[i])
				{
					if (!!this.soundData[i].currentTime)
					{
						this.soundData[i].pause();
						this.soundData[i].currentTime = 0;
					}
					this.soundData[i].play();
					break;
				}
			}
		}
	}

	this.score += this.level.score;
	this.level.score = 0;

	this.paint();
};

Digger.prototype.paint = function()
{
	var blink = ((this.blink + 4) % 6);
	this.display.paint(this, this.level, blink);
};
