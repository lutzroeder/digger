module Digger
{
    export class Ghost
    {
        private _position: Position;
        private _type: Sprite;
        private _direction: Direction;
        private _lastTurn: Direction;
        private _alive: boolean = true;

        constructor(position: Position, type: Sprite, direction: Direction, lastTurn: Direction)
        {
            this._position = position;
            this._type = type;
            this._direction = direction;
            this._lastTurn = lastTurn;
        }

        public kill()
        {
            this._alive = false;
        }

        public get alive(): boolean
        {
            return this._alive;
        }

        public get position(): Position
        {
            return this._position;
        }

        public get type(): Sprite
        {
            return this._type;
        }

        public get direction(): Direction
        {
            return this._direction;
        }

        public set direction(value: Direction)
        {
            this._direction = value;
        }

        public get lastTurn(): Direction
        {
            return this._lastTurn;
        }

        public set lastTurn(value: Direction)
        {
            this._lastTurn = value;
        }

        public get imageIndex(): number
        {
            return [ 4, 4, 5, 6, 3 ][this._direction];
        }
    }
}
