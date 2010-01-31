
var Level = function(data)
{
	var i, x, y;
	var reader = new Base64Reader(data);

	this.map = [];
	for (x = 0; x < 20; x++)
	{
		this.map[x] = [];
	}
	for (y = 0; y < 14; y++)
	{
		for (x = 0; x < 10; x++)
		{
			var b = reader.readByte();
			this.map[x * 2 + 1][y] = b & 0x0f;
			this.map[x * 2][y] = b >> 4;
		}
	}
	for (i = 0; i < 5; i++)
	{
		reader.readByte();
	}
	this.player = new Player(new Position(reader.readByte(), reader.readByte() - 2));
	this.map[this.player.position.x][this.player.position.y] = this.player;
	this.diamonds = reader.readByte();
	this.diamonds = (this.diamonds >> 4) * 10 + (this.diamonds & 0x0f);
	var ghostData = [];
	for (i = 0; i < 8; i++)
	{
		ghostData.push(reader.readByte());
	}
	this.ghosts = [];
	var index = 0;
	for (y = 0; y < 14; y++)
	{
		for (x = 0; x < 20; x++)
		{
			if ((this.map[x][y] === Sprite.ghost90L) || (this.map[x][y] === Sprite.ghost90R) || (this.map[x][y] === Sprite.ghost90LR) || (this.map[x][y] === Sprite.ghost180))
			{
				var ghost = new Ghost(new Position(x, y), this.map[x][y]);
				var info = ghostData[index >> 1];
				if ((index & 1) !== 0)
				{
					info = info & 0x0f;
					ghost.lastTurn = Direction.right;
				}
				else
				{
					info = info >> 4;
					ghost.lastTurn = Direction.left;
				}
				ghost.direction = (info < 4) ? [Direction.down, Direction.up, Direction.right, Direction.left][info] : Direction.none;
				this.ghosts.push(ghost);
				this.map[x][y] = ghost;
				index++;
			}
		}
	}
	this.collected = 0;
	this.time = 5000;
	this.score = 0;	
};

Level.prototype.update = function()
{
	// turn buffers into nothing
	for (var y = 13; y >= 0; y--)
	{
		for (var x = 19; x >= 0; x--)
		{
			if (this.map[x][y] === Sprite.buffer)
			{
				this.map[x][y] = Sprite.nothing;
			}
		}
	}
};

Level.prototype.move = function()
{	
	var x, y, dx, dy;

	// gravity for stones and diamonds
	for (y = 13; y >= 0; y--)
	{
		for (x = 19; x >= 0; x--)
		{
			if ((this.map[x][y] === Sprite.stone) || (this.map[x][y] === Sprite.diamond) || (this.map[x][y] === Sprite.uvstone))
			{
				dx = x;
				dy = y;
				if (this.map[x][y + 1] === Sprite.nothing)
				{
					dy = y + 1;
				}
				else
				{
					if ((this.map[x][y + 1] === Sprite.stone) || (this.map[x][y + 1] === Sprite.diamond))
					{
						if ((this.map[x - 1][y + 1] === Sprite.nothing) && (this.map[x - 1][y] === Sprite.nothing))
						{
							dx = x - 1;
							dy = y + 1;
						}
						else if ((this.map[x + 1][y + 1] === Sprite.nothing) && (this.map[x + 1][y] === Sprite.nothing))
						{
							dx = x + 1;
							dy = y + 1;
						}
					}
					if ((this.map[x][y + 1] === Sprite.changer) && ((this.map[x][y] === Sprite.stone) || (this.map[x][y] === Sprite.uvstone)) && (this.map[x][y + 2] === Sprite.nothing))
					{
						dy = y + 2;
					}
				}
				if ((dx != x) || (dy != y))
				{
					this.map[dx][dy] = Sprite.marker;
				}
			}
		}
	}

	for (y = 13; y >= 0; y--)
	{
		for (x = 19; x >= 0; x--)
		{
			if ((this.map[x][y] === Sprite.stone) || (this.map[x][y] === Sprite.diamond) || (this.map[x][y] === Sprite.uvstone))
			{
				dx = x;
				dy = y;
				if (this.map[x][y + 1] === Sprite.marker)
				{
					dy = y + 1;
				}
				else
				{
					if ((this.map[x][y + 1] === Sprite.stone) || (this.map[x][y + 1] === Sprite.diamond) || (this.map[x][y + 1] === Sprite.nothing))
					{
						if ((this.map[x - 1][y + 1] === Sprite.marker) && ((this.map[x - 1][y] === Sprite.nothing) || (this.map[x - 1][y] === Sprite.marker)))
						{
							dx = x - 1;
							dy = y + 1;
						}
						else if ((this.map[x + 1][y + 1] === Sprite.marker) && ((this.map[x + 1][y] === Sprite.nothing) || (this.map[x + 1][y] === Sprite.marker)))
						{
							dx = x + 1;
							dy = y + 1;
						}
					}
					if ((this.map[x][y + 1] === Sprite.changer) && ((this.map[x][y] === Sprite.stone) || (this.map[x][y] === Sprite.uvstone)) && (this.map[x][y + 2] === Sprite.marker))
					{
						dy = y + 2;
					}
				}
				if ((dx != x) || (dy != y))
				{
					if ((dy - y) === 2)
					{
						this.map[dx][dy] = Sprite.diamond;
					}
					else
					{
						this.map[dx][dy] = this.map[x][y];
						if (this.map[dx][dy] === Sprite.uvstone)
						{
							this.map[dx][dy] = Sprite.stone;
						}
					}
					this.map[x][y] = Sprite.nothing;

					if ((this.map[dx][dy+1] === Sprite.stone) || (this.map[dx][dy+1] === Sprite.diamond) || (this.map[dx][dy+1] === Sprite.wall) || (this.isGhost(dx,dy+1)))
					{
						this.soundTable[Sound.stone] = true;
					}

					if (this.isPlayer(dx, dy+1)) 
					{
						this.player.alive = false;
					}
					if (this.isGhost(dx, dy+1)) 
					{
						this.killGhost(this.map[dx][dy + 1]);
					}
				}					
			}
		}
	}

	for (var i = 0; i < this.ghosts.length; i++)
	{
		this.moveGhost(this.ghosts[i]);
	}

	if (this.time > 0) 
	{
		this.time--;
	}
	if (this.time === 0)
	{
		this.player.alive = false;
	}
};

Level.prototype.isPlayer = function(x, y)
{
	return ((this.map[x][y] instanceof Player) || (this.map[x][y] === Sprite.player));
};

Level.prototype.movePlayer = function(keys)
{
	if (this.player.alive)
	{
		this.player.direction = Direction.none;
		var p = new Position(this.player.position);
		var d = new Position(p);
		var z = new Position(d);
		if (keys[Key.left])
		{
			z.x--;
			this.player.direction = Direction.left;
		}
		else
		{
			this.player.stone[0] = false;
			if (keys[Key.right])
			{
				z.x++;
				this.player.direction = Direction.right;
			}
			else
			{
				this.player.stone[1] = false;
				if (keys[Key.up])
				{
					z.y--;
					this.player.direction = Direction.up;
				}
				else if (keys[Key.down])
				{
					z.y++;
					this.player.direction = Direction.down;
				}
			}
		}
		if (!d.equals(z))
		{
			if (this.map[z.x][z.y] === Sprite.nothing)
			{
				this.placePlayer(d.x, d.y);
			}
			if (this.map[z.x][z.y] === Sprite.diamond)
			{
				this.collected += 1;
				this.score += 3;
				this.soundTable[Sound.diamond] = true;
			}
			if (this.map[z.x][z.y] === Sprite.stone)
			{
				if ((z.x > d.x) && (this.map[z.x+1][z.y] === Sprite.nothing))
				{
					if (this.player.stone[1])
					{
						this.map[d.x+2][d.y] = this.map[d.x+1][d.y];
						this.map[d.x+1][d.y] = Sprite.nothing;
					}
					this.player.stone[1] = !this.player.stone[1];
				}

				if ((z.x < d.x) && (this.map[z.x-1][z.y] === Sprite.nothing))
				{
					if (this.player.stone[0])
					{
						this.map[d.x-2][d.y] = this.map[d.x-1][d.y];
						this.map[d.x-1][d.y] = Sprite.nothing;
					}
					this.player.stone[0] = !this.player.stone[0];
				}
			}

			if ((this.map[z.x][z.y] === Sprite.nothing) || (this.map[z.x][z.y] === Sprite.ground) || (this.map[z.x][z.y] === Sprite.diamond))
			{
				this.placePlayer(z.x, z.y);
				this.map[d.x][d.y] = Sprite.buffer;
				this.soundTable[Sound.step] = true;
			}

			if ((this.map[z.x][z.y] === Sprite.exit) || (this.map[z.x][z.y] === Sprite.uvexit))
			{
				if (this.collected >= this.diamonds)
				{
					return true; // next level
				}
			}

			if (this.isGhost(z.x, z.y))
			{
				this.player.alive = false;
			}
		}

		// animate player
		this.player.step++;
		switch (this.player.direction)
		{
			case Direction.left:
			case Direction.right:
				if (this.player.step >= 6)
				{
					this.player.step = 0;
				}
				break;
			case Direction.up:
			case Direction.down:
				if (this.player.step >= 2)
				{
					this.player.step = 0;
				}
				break;
			default:
				if (this.player.step >= 30)
				{
					this.player.step = 0;
				}
				break;
		}
	}
	return false;
};

Level.prototype.placePlayer = function(x, y)
{
	this.map[x][y] = this.map[this.player.position.x][this.player.position.y];
	this.player.position.x = x;
	this.player.position.y = y;
};

Level.prototype.isGhost = function(x, y)
{
	return (this.map[x][y] instanceof Ghost);
};

Level.prototype.moveGhost = function(ghost)
{
	var i, d;
	if (ghost.alive)
	{
		var p = new Position(ghost.position.x, ghost.position.y);
		var w = [ new Position(p), new Position(p), new Position(p), new Position(p) ];
		if ((ghost.type === Sprite.ghost180) || (ghost.type === Sprite.ghost90L) || (ghost.type === Sprite.ghost90R))
		{
			if (ghost.type === Sprite.ghost180)
			{
				if (ghost.direction === Direction.left)  { w[0].x--; w[1].x++; }
				if (ghost.direction === Direction.right) { w[0].x++; w[1].x--; }
				if (ghost.direction === Direction.up)    { w[0].y--; w[1].y++; }
				if (ghost.direction === Direction.down)  { w[0].y++; w[1].y--; }
			}
			else if (ghost.type === Sprite.ghost90L)
			{
				if (ghost.direction === Direction.left)  { w[0].x--; w[1].y++; w[2].y--; w[3].x++; }
				if (ghost.direction === Direction.right) { w[0].x++; w[1].y--; w[2].y++; w[3].x--; }
				if (ghost.direction === Direction.up)    { w[0].y--; w[1].x--; w[2].x++; w[3].y++; }
				if (ghost.direction === Direction.down)  { w[0].y++; w[1].x++; w[2].x--; w[3].y--; }
			}
			else if (ghost.type === Sprite.ghost90R)
			{
				if (ghost.direction === Direction.left)  { w[0].x--; w[1].y--; w[2].y++; w[3].x++; }
				if (ghost.direction === Direction.right) { w[0].x++; w[1].y++; w[2].y--; w[3].x--; }
				if (ghost.direction === Direction.up)    { w[0].y--; w[1].x++; w[2].x--; w[3].y++; }
				if (ghost.direction === Direction.down)  { w[0].y++; w[1].x--; w[2].x++; w[3].y--; }
			}
			for (i = 0; i < 4; i++)
			{
				if (!p.equals(w[i]))
				{
					d = new Position(w[i]);
					if (this.isPlayer(d.x, d.y))
					{
						this.player.alive = false;
					}
					if (this.map[d.x][d.y] === Sprite.nothing)
					{
						if (d.x < p.x) { ghost.direction = Direction.left; }
						if (d.x > p.x) { ghost.direction = Direction.right; }
						if (d.y < p.y) { ghost.direction = Direction.up; }
						if (d.y > p.y) { ghost.direction = Direction.down; }
						this.placeGhost(d.x, d.y, ghost);
						this.map[p.x][p.y] = Sprite.nothing;
						return;
					}
				}
			}
		}
		else if (ghost.type === Sprite.ghost90LR)
		{
			if (ghost.direction === Direction.left)
			{
				w[0].x--; w[3].x++;
				if (ghost.lastTurn === Direction.left) { w[1].y--; w[2].y++; } else { w[1].y++; w[2].y--; }
			}
			else if (ghost.direction === Direction.right)
			{
				w[0].x++; w[3].x--;
				if (ghost.lastTurn === Direction.left) { w[1].y++; w[2].y--; } else { w[1].y--; w[2].y++; }
			}
			else if (ghost.direction === Direction.up)
			{
				w[0].y--; w[3].y++;
				if (ghost.lastTurn === Direction.left) { w[1].x++; w[2].x--; } else { w[1].x--; w[2].x++; }
			}
			else if (ghost.direction === Direction.down)
			{
				w[0].y++; w[3].y--;
				if (ghost.lastTurn === Direction.left) { w[1].x--; w[2].x++; } else { w[1].x++; w[2].x--; }
			}
			for (i = 0; i < 4; i++)
			{
				if (!p.equals(w[i]))
				{
					d = new Position(w[i]);
					if (this.isPlayer(d.x, d.y)) 
					{
						this.player.alive = false;
					}
					if (this.map[d.x][d.y] === Sprite.nothing)
					{
						var lastDirection = ghost.direction;
						if (d.x < p.x) { ghost.direction = Direction.left;  }
						if (d.x > p.x) { ghost.direction = Direction.right; }
						if (d.y < p.y) { ghost.direction = Direction.up;    }
						if (d.y > p.y) { ghost.direction = Direction.down;  }
						if (lastDirection === Direction.left)
						{
							if (ghost.direction === Direction.down)  { ghost.lastTurn = Direction.left;  }
							if (ghost.direction === Direction.up)    { ghost.lastTurn = Direction.right; }
						}
						else if (lastDirection === Direction.right)
						{
							if (ghost.direction === Direction.down)  { ghost.lastTurn = Direction.right; }
							if (ghost.direction === Direction.up)    { ghost.lastTurn = Direction.left;  }
						}
						else if (lastDirection === Direction.up)
						{
							if (ghost.direction === Direction.left)  { ghost.lastTurn = Direction.left;  }
							if (ghost.direction === Direction.right) { ghost.lastTurn = Direction.right; }
						}
						else if (lastDirection === Direction.down)
						{
							if (ghost.direction === Direction.left)  { ghost.lastTurn = Direction.right; }
							if (ghost.direction === Direction.right) { ghost.lastTurn = Direction.left;  }
						}
						this.placeGhost(d.x, d.y, ghost);
						this.map[p.x][p.y] = Sprite.nothing;
						return;
					}
				}
			}
		}
	}
};

Level.prototype.placeGhost = function(x, y, ghost)
{
	this.map[x][y] = ghost;
	ghost.position.x = x;
	ghost.position.y = y;
};

Level.prototype.killGhost = function(ghost)
{
	if (ghost.alive)
	{
		var p = new Position(ghost.position.x, ghost.position.y);
		for (var y = p.y - 1; y <= p.y + 1; y++)
		{
			for (var x = p.x - 1; x <= p.x + 1; x++)
			{
				if ((x > 0) && (x < 19) && (y > 0) && (y < 13))
				{
					if (this.isPlayer(x, y))
					{
						this.player.alive = false;
					}
					else
					{
						if (this.isGhost(x, y))
						{
							this.map[x][y].alive = false;
							this.score += 99;
						}
						this.map[x][y] = Sprite.nothing;
					}
				}
			}
		}
		ghost.alive = false;
	}
};
