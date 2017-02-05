module Digger
{
    export class Player
    {
        private _position: Position;
        private _direction: Direction = Direction.none;
        private _stone: boolean[] = [false, false];
        private _step: number = 0;
        private _alive: boolean = true;

        constructor(position: Position)
        {
            this._position = position;
        }

        public kill()
        {
            this._alive = false;
        }

        public get position(): Position
        {
            return this._position;
        }

        public get alive(): boolean
        {
            return this._alive;
        }

        public get direction(): Direction
        {
            return this._direction;
        }

        public set direction(value: Direction)
        {
            this._direction = value;
        }

        public get stone(): boolean[]
        {
            return this._stone;
        }

        public animate()
        {
            this._step++;

            switch (this._direction)
            {
                case Direction.left:
                case Direction.right:
                    if (this._step >= 6)
                    {
                        this._step = 0;
                    }
                    break;
                case Direction.up:
                case Direction.down:
                    if (this._step >= 2)
                    {
                        this._step = 0;
                    }
                    break;
                default:
                    if (this._step >= 30)
                    {
                        this._step = 0;
                    }
                    break;
            }
        }

        public get imageIndex(): number
        {
            if (this._alive)
            {
                if ((this._direction === Direction.left) && (this._step < 6))
                {
                    return [16, 17, 18, 19, 18, 17][this._step];
                }
                else if ((this._direction === Direction.right) && (this._step < 6))
                {
                    return [20, 21, 22, 23, 22, 21][this._step];
                }
                else if ((this._direction === Direction.up) && (this._step < 2))
                {
                    return [24, 25][this._step];
                }
                else if ((this._direction === Direction.down) && (this._step < 2))
                {
                    return [26, 27][this._step];
                }
                return [15, 15, 15, 15, 15, 15, 15, 15, 28, 28, 15, 15, 28, 28, 15, 15, 15, 15, 15, 15, 29, 29, 30, 30, 29, 29, 15, 15, 15, 15][this._step];
            }
            return 31;
        }
    }
}