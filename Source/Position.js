
var Position = function()
{
	if (arguments.length === 1) // copy constructor
	{
		this.x = arguments[0].x;
		this.y = arguments[0].y;
	}
	if (arguments.length === 2) // (x, y)
	{
		this.x = arguments[0];
		this.y = arguments[1];
	}
};

Position.prototype.equals = function(position)
{
	return (this.x == position.x) && (this.y == position.y);
};
