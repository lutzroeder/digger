
var Input = function(canvas, game)
{
	this.canvas = canvas;
	this.game = game;
	this.touchPosition = null;
	this.mouseDownHandler = this.mouseDown.bind(this);
	this.touchStartHandler = this.touchStart.bind(this);
	this.touchEndHandler = this.touchEnd.bind(this);
	this.touchMoveHandler = this.touchMove.bind(this);
	this.keyDownHandler = this.keyDown.bind(this);
	this.keyPressHandler = this.keyPress.bind(this);
	this.keyUpHandler = this.keyUp.bind(this);
	this.canvas.addEventListener("touchstart", this.touchStartHandler, false);
	this.canvas.addEventListener("touchmove", this.touchMoveHandler, false);
	this.canvas.addEventListener("touchend", this.touchEndHandler, false);
	this.canvas.addEventListener("mousedown", this.mouseDownHandler, false);
	document.addEventListener("keydown", this.keyDownHandler, false);
	document.addEventListener("keypress", this.keyPressHandler, false);
	document.addEventListener("keyup", this.keyUpHandler, false);
	this.isWebKit = typeof navigator.userAgent.split("WebKit/")[1] !== "undefined";
	this.isMozilla = navigator.appVersion.indexOf('Gecko/') >= 0 || ((navigator.userAgent.indexOf("Gecko") >= 0) && !this.isWebKit && (typeof navigator.appVersion !== "undefined"));
};

Input.prototype.keyDown = function(e)
{
	if (!this.isMozilla && !e.ctrlKey && !e.altKey && !e.altKey && !e.metaKey)
	{
		this.processKey(e, e.keyCode);
	}
};

Input.prototype.keyPress = function(e)
{
	if (this.isMozilla && !e.ctrlKey && !e.altKey && !e.altKey && !e.metaKey)
	{
		this.processKey(e, (e.keyCode != 0) ? e.keyCode : (e.charCode === 32) ? 32 : 0);
	}
};

Input.prototype.keyUp = function(e)
{
	     if (e.keyCode == 37) { this.game.removeKey(Key.left);  }
	else if (e.keyCode == 39) { this.game.removeKey(Key.right); }
	else if (e.keyCode == 38) { this.game.removeKey(Key.up);    }
	else if (e.keyCode == 40) { this.game.removeKey(Key.down);  }
};

Input.prototype.processKey = function(e, keyCode)
{
	     if (keyCode == 37) { this.stopEvent(e); this.game.addKey(Key.left);  } // left
	else if (keyCode == 39) { this.stopEvent(e); this.game.addKey(Key.right); } // right
	else if (keyCode == 38) { this.stopEvent(e); this.game.addKey(Key.up);    } // up
	else if (keyCode == 40) { this.stopEvent(e); this.game.addKey(Key.down);  } // down
	else if (keyCode == 27) { this.stopEvent(e); this.game.addKey(Key.reset); } // escape
	else if ((keyCode == 8) || (keyCode == 36)) { this.stopEvent(e); this.game.nextLevel(); } // backspace or delete
	else if (!this.game.isAlive()) { this.stopEvent(e); this.game.addKey(Key.reset); }
};

Input.prototype.mouseDown = function(e) 
{
	e.preventDefault(); 
	this.canvas.focus();
};

Input.prototype.touchStart = function(e)
{
	e.preventDefault();
	if (e.touches.length > 3) // 4 finger touch = jump to next level
	{
		this.game.nextLevel();
	}
	else if ((e.touches.length > 2) || (!this.game.isAlive())) // 3 finger touch = restart current level
	{
		this.game.addKey(Key.reset);
	}
	else
	{
		for (var i = 0; i < e.touches.length; i++)
		{
			this.touchPosition = new Position(e.touches[i].pageX, e.touches[i].pageY);
		}
	}
};

Input.prototype.touchMove = function(e)
{
	e.preventDefault();
	for (var i = 0; i < e.touches.length; i++)
	{
		if (this.touchPosition !== null)
		{
			var x = e.touches[i].pageX;
			var y = e.touches[i].pageY;
			var direction = null;
			if ((this.touchPosition.x - x) > 20)
			{
				direction = Key.left;
			}
			else if ((this.touchPosition.x - x) < -20)
			{
				direction = Key.right;
			}
			else if ((this.touchPosition.y - y) > 20)
			{
				direction = Key.up;
			}
			else if ((this.touchPosition.y - y) < -20)
			{
				direction = Key.down;
			}
			if (direction !== null)
			{
				this.touchPosition = new Position(x, y);			
				for (var i = Key.left; i <= Key.down; i++)
				{
					if (direction == i)
					{
						this.game.addKey(i);
					}
					else
					{ 
						this.game.removeKey(i);
					}
				}
			}
		}
	}
};

Input.prototype.touchEnd = function(e)
{
	e.preventDefault();
	this.touchPosition = null;
	this.game.removeKey(Key.left);
	this.game.removeKey(Key.right);
	this.game.removeKey(Key.up);
	this.game.removeKey(Key.down);
};

Input.prototype.stopEvent = function(e)
{
	e.preventDefault();
	e.stopPropagation();
};