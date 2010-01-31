
var Ghost = function(position, type)
{
	this.position = position;
	this.type = type;
	this.alive = true;
	this.direction = Direction.none;
	this.lastTurn = Direction.none;
};

Ghost.prototype.getImageIndex = function()
{
	return [ 4, 4, 5, 6, 3 ][this.direction];
};
