module Digger
{
    export class Base64Reader
    {
        private _alphabet: string = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789+/=';
        private _position: number = 0;
        private _bits: number = 0;
        private _bitsLength: number = 0;
        private _data: string;

        constructor(data: string)
        {
            this._data = data;
        }

        public readByte(): number
        {
            if (this._bitsLength === 0)
            {
                var tailBits: number = 0;
                while (this._position < this._data.length && this._bitsLength < 24)
                {
                    var ch = this._data.charAt(this._position++);
                    var index = this._alphabet.indexOf(ch);
                    if (index < 64)
                    {
                        this._bits = (this._bits << 6) | index;
                    }
                    else
                    {
                        this._bits <<= 6;
                        tailBits += 6;
                    }
                    this._bitsLength += 6;
                }
                if ((this._position >= this._data.length) && (this._bitsLength === 0))
                {
                    return -1;
                }
                tailBits = (tailBits === 6) ? 8 : (tailBits === 12) ? 16 : tailBits;
                this._bits = this._bits >> tailBits;
                this._bitsLength -= tailBits;
            }
            this._bitsLength -= 8;
            return (this._bits >> this._bitsLength) & 0xff;
        }
    }
}