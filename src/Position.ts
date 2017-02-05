module Digger
{
    export class Position
    {
        public x: number;
        public y: number;

        constructor(x: number, y: number)
        {
            this.x = x;
            this.y = y;
        }

        public equals(position: Position): boolean
        {
            return (this.x == position.x) && (this.y == position.y);
        }

        public clone(): Position
        {
            return new Position(this.x, this.y);
        }
    }
}