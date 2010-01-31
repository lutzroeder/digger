
var Player = function(position)
{
	this.position = position;
	this.alive = true;
	this.direction = Direction.none;
	this.stone = [false, false];
	this.step = 0;
};

Player.prototype.getImageIndex = function()
{
	if (this.alive)
	{
		if ((this.direction === Direction.left) && (this.step < 6))
		{
			return [16, 17, 18, 19, 18, 17][this.step];
		}
		else if ((this.direction === Direction.right) && (this.step < 6))
		{
			return [20, 21, 22, 23, 22, 21][this.step];
		}
		else if ((this.direction === Direction.up) && (this.step < 2))
		{
			return [24, 25][this.step];
		}
		else if ((this.direction === Direction.down) && (this.step < 2))
		{
			return [26, 27][this.step];
		}
		return [15, 15, 15, 15, 15, 15, 15, 15, 28, 28, 15, 15, 28, 28, 15, 15, 15, 15, 15, 15, 29, 29, 30, 30, 29, 29, 15, 15, 15, 15][this.step];
	}
	return 31;
};
