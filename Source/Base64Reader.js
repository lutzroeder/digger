
var Base64Reader = function(data)
{
	this.alphabet = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=';
	this.data = data;
	this.position = 0;
	this.bits = 0;
	this.bitsLength = 0;
};

Base64Reader.prototype.readByte = function()
{
	if (this.bitsLength === 0)
	{
		var tailBits = 0;
		while (this.position < this.data.length && this.bitsLength < 24)
		{
			var ch = this.data.charAt(this.position++);
			var index = this.alphabet.indexOf(ch);
			if (index < 64)
			{
				this.bits = (this.bits << 6) | index;
			}
			else
			{
				this.bits <<= 6;
				tailBits += 6;
			}
			this.bitsLength += 6;
		}
		if ((this.position >= this.data.length) && (this.bitsLength === 0))
		{
			return -1;
		}
		tailBits = (tailBits === 6) ? 8 : (tailBits === 12) ? 16 : tailBits;
		this.bits = this.bits >> tailBits;
		this.bitsLength -= tailBits;
	}
	this.bitsLength -= 8;
	return (this.bits >> this.bitsLength) & 0xff;
};
