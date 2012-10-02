module Digger
{
    export class Level
    {
        private _map: Sprite[][];
        private _diamonds: number;
        private _collected: number;
        private _time: number;
        private _score: number;
        private _ghosts: Ghost[];
        private _player: Player;
        private _soundTable: boolean[];

        constructor(data: string)
        {
            this._collected = 0;
            this._time = 5000;
            this._score = 0;    

            this._map = [];
            for (var x = 0; x < 20; x++)
            {
                this._map[x] = [];
            }

            var reader: Base64Reader = new Base64Reader(data);

            for (var y: number = 0; y < 14; y++)
            {
                for (var x: number = 0; x < 10; x++)
                {
                    var b: number = reader.readByte();
                    this._map[x * 2 + 1][y] = b & 0x0f;
                    this._map[x * 2][y] = b >> 4;
                }
            }

            for (var i: number = 0; i < 5; i++)
            {
                reader.readByte();
            }

            var position = new Position(reader.readByte(), reader.readByte() - 2);
            this._player = new Player(position);
            this._map[this._player.position.x][this._player.position.y] = Sprite.player;
            this._diamonds = reader.readByte();
            this._diamonds = (this._diamonds >> 4) * 10 + (this._diamonds & 0x0f);

            var ghostData: number[] = [];
            for (var i: number = 0; i < 8; i++)
            {
                ghostData.push(reader.readByte());
            }
            var index = 0;
            this._ghosts = [];
            for (var y: number = 0; y < 14; y++)
            {
                for (var x: number = 0; x < 20; x++)
                {
                    if ((this._map[x][y] === Sprite.ghost90L) || (this._map[x][y] === Sprite.ghost90R) || (this._map[x][y] === Sprite.ghost90LR) || (this._map[x][y] === Sprite.ghost180))
                    {
                        var info: number = ((index & 1) !== 0) ? (ghostData[index >> 1] & 0x0f) : (ghostData[index >> 1] >> 4);
                        var direction: Direction = (info < 4) ? [Direction.down, Direction.up, Direction.right, Direction.left][info] : Direction.none;
                        var lastTurn: Direction = ((index & 1) !== 0) ? Direction.right : Direction.left;
                        this._ghosts.push(new Ghost(new Position(x, y), this._map[x][y], direction, lastTurn));
                        index++;
                    }
                }
            }
        }

        public get time(): number
        {
            return this._time;
        }

        public get diamonds(): number
        {
            return this._diamonds;
        }

        public get collected(): number
        {
            return this._collected;
        }

        public get isPlayerAlive(): boolean
        {
            return this._player.alive;
        }

        public updateScore(): number
        {
            var score: number = this._score;
            this._score = 0;
            return score;
        }

        public update()
        {
            // turn buffers into nothing
            for (var y: number = 13; y >= 0; y--)
            {
                for (var x: number = 19; x >= 0; x--)
                {
                    if (this._map[x][y] === Sprite.buffer)
                    {
                        this._map[x][y] = Sprite.nothing;
                    }
                }
            }

            // reset sound state
            this._soundTable = [ false, false, false ];
        }

        public playSound(sound: Sound): boolean
        {
            return this._soundTable[sound];
        }

        public move()
        {   
            // gravity for stones and diamonds
            for (var y: number = 13; y >= 0; y--)
            {
                for (var x: number = 19; x >= 0; x--)
                {
                    if ((this._map[x][y] === Sprite.stone) || (this._map[x][y] === Sprite.diamond) || (this._map[x][y] === Sprite.uvstone))
                    {
                        var dx: number = x;
                        var dy: number = y;
                        if (this._map[x][y + 1] === Sprite.nothing)
                        {
                            dy = y + 1;
                        }
                        else
                        {
                            if ((this._map[x][y + 1] === Sprite.stone) || (this._map[x][y + 1] === Sprite.diamond))
                            {
                                if ((this._map[x - 1][y + 1] === Sprite.nothing) && (this._map[x - 1][y] === Sprite.nothing))
                                {
                                    dx = x - 1;
                                    dy = y + 1;
                                }
                                else if ((this._map[x + 1][y + 1] === Sprite.nothing) && (this._map[x + 1][y] === Sprite.nothing))
                                {
                                    dx = x + 1;
                                    dy = y + 1;
                                }
                            }
                            if ((this._map[x][y + 1] === Sprite.changer) && ((this._map[x][y] === Sprite.stone) || (this._map[x][y] === Sprite.uvstone)) && (this._map[x][y + 2] === Sprite.nothing))
                            {
                                dy = y + 2;
                            }
                        }
                        if ((dx != x) || (dy != y))
                        {
                            this._map[dx][dy] = Sprite.marker;
                        }
                    }
                }
            }

            for (var y: number = 13; y >= 0; y--)
            {
                for (var x: number = 19; x >= 0; x--)
                {
                    if ((this._map[x][y] === Sprite.stone) || (this._map[x][y] === Sprite.diamond) || (this._map[x][y] === Sprite.uvstone))
                    {
                        var dx: number = x;
                        var dy: number = y;
                        if (this._map[x][y + 1] === Sprite.marker)
                        {
                            dy = y + 1;
                        }
                        else
                        {
                            if ((this._map[x][y + 1] === Sprite.stone) || (this._map[x][y + 1] === Sprite.diamond) || (this._map[x][y + 1] === Sprite.nothing))
                            {
                                if ((this._map[x - 1][y + 1] === Sprite.marker) && ((this._map[x - 1][y] === Sprite.nothing) || (this._map[x - 1][y] === Sprite.marker)))
                                {
                                    dx = x - 1;
                                    dy = y + 1;
                                }
                                else if ((this._map[x + 1][y + 1] === Sprite.marker) && ((this._map[x + 1][y] === Sprite.nothing) || (this._map[x + 1][y] === Sprite.marker)))
                                {
                                    dx = x + 1;
                                    dy = y + 1;
                                }
                            }
                            if ((this._map[x][y + 1] === Sprite.changer) && ((this._map[x][y] === Sprite.stone) || (this._map[x][y] === Sprite.uvstone)) && (this._map[x][y + 2] === Sprite.marker))
                            {
                                dy = y + 2;
                            }
                        }
                        if ((dx != x) || (dy != y))
                        {
                            if ((dy - y) === 2)
                            {
                                this._map[dx][dy] = Sprite.diamond;
                            }
                            else
                            {
                                this._map[dx][dy] = this._map[x][y];
                                if (this._map[dx][dy] === Sprite.uvstone)
                                {
                                    this._map[dx][dy] = Sprite.stone;
                                }
                            }
                            this._map[x][y] = Sprite.nothing;

                            if ((this._map[dx][dy+1] === Sprite.stone) || (this._map[dx][dy+1] === Sprite.diamond) || (this._map[dx][dy+1] === Sprite.wall) || (this.isGhost(dx,dy+1)))
                            {
                                this._soundTable[Sound.stone] = true;
                            }

                            if (this.isPlayer(dx, dy+1)) 
                            {
                                this._player.kill();
                            }
                            if (this.isGhost(dx, dy+1)) 
                            {
                                this.killGhost(dx, dy+1);
                            }
                        }                   
                    }
                }
            }

            for (var i: number = 0; i < this._ghosts.length; i++)
            {
                this.moveGhost(this._ghosts[i]);
            }

            if (this._time > 0) 
            {
                this._time--;
            }
            if (this._time === 0)
            {
                this._player.kill();
            }
        }

        public movePlayer(keys: boolean[]): boolean
        {
            if (this._player.alive)
            {
                this._player.direction = Direction.none;
                var p: Position = this._player.position.clone();
                var d: Position = p.clone();
                var z: Position = d.clone();
                if (keys[Key.left])
                {
                    z.x--;
                    this._player.direction = Direction.left;
                }
                else
                {
                    this._player.stone[0] = false;
                    if (keys[Key.right])
                    {
                        z.x++;
                        this._player.direction = Direction.right;
                    }
                    else
                    {
                        this._player.stone[1] = false;
                        if (keys[Key.up])
                        {
                            z.y--;
                            this._player.direction = Direction.up;
                        }
                        else if (keys[Key.down])
                        {
                            z.y++;
                            this._player.direction = Direction.down;
                        }
                    }
                }
                if (!d.equals(z))
                {
                    if (this._map[z.x][z.y] === Sprite.nothing)
                    {
                        this.placePlayer(d.x, d.y);
                    }
                    if (this._map[z.x][z.y] === Sprite.diamond)
                    {
                        this._collected += 1;
                        this._score += 3;
                        this._soundTable[Sound.diamond] = true;
                    }
                    if (this._map[z.x][z.y] === Sprite.stone)
                    {
                        if ((z.x > d.x) && (this._map[z.x+1][z.y] === Sprite.nothing))
                        {
                            if (this._player.stone[1])
                            {
                                this._map[d.x+2][d.y] = this._map[d.x+1][d.y];
                                this._map[d.x+1][d.y] = Sprite.nothing;
                            }
                            this._player.stone[1] = !this._player.stone[1];
                        }

                        if ((z.x < d.x) && (this._map[z.x-1][z.y] === Sprite.nothing))
                        {
                            if (this._player.stone[0])
                            {
                                this._map[d.x-2][d.y] = this._map[d.x-1][d.y];
                                this._map[d.x-1][d.y] = Sprite.nothing;
                            }
                            this._player.stone[0] = !this._player.stone[0];
                        }
                    }

                    if ((this._map[z.x][z.y] === Sprite.nothing) || (this._map[z.x][z.y] === Sprite.ground) || (this._map[z.x][z.y] === Sprite.diamond))
                    {
                        this.placePlayer(z.x, z.y);
                        this._map[d.x][d.y] = Sprite.buffer;
                        this._soundTable[Sound.step] = true;
                    }

                    if ((this._map[z.x][z.y] === Sprite.exit) || (this._map[z.x][z.y] === Sprite.uvexit))
                    {
                        if (this._collected >= this._diamonds)
                        {
                            return true; // next level
                        }
                    }

                    if (this.isGhost(z.x, z.y))
                    {
                        this._player.kill();
                    }
                }

                // animate player
                this._player.animate();
            }
            return false;
        }

        private isPlayer(x: number, y: number): boolean
        {
            return (this._map[x][y] === Sprite.player);
        }

        private placePlayer(x: number, y: number)
        {
            this._map[x][y] = Sprite.player;
            this._player.position.x = x;
            this._player.position.y = y;
        }

        private isGhost(x: number, y: number): boolean
        {
            return (this._map[x][y] == Sprite.ghost90L) || (this._map[x][y] == Sprite.ghost90R) || (this._map[x][y] == Sprite.ghost90LR) || (this._map[x][y] == Sprite.ghost180);
        }

        private moveGhost(ghost: Ghost)
        {
            if (ghost.alive)
            {
                var p: Position = ghost.position.clone();
                var w: Position[] = [ p.clone(), p.clone(), p.clone(), p.clone() ];
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
                    for (var i = 0; i < 4; i++)
                    {
                        if (!p.equals(w[i]))
                        {
                            var d = w[i].clone();
                            if (this.isPlayer(d.x, d.y))
                            {
                                this._player.kill();
                            }
                            if (this._map[d.x][d.y] === Sprite.nothing)
                            {
                                if (d.x < p.x) { ghost.direction = Direction.left; }
                                if (d.x > p.x) { ghost.direction = Direction.right; }
                                if (d.y < p.y) { ghost.direction = Direction.up; }
                                if (d.y > p.y) { ghost.direction = Direction.down; }
                                this.placeGhost(d.x, d.y, ghost);
                                this._map[p.x][p.y] = Sprite.nothing;
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
                    for (var i = 0; i < 4; i++)
                    {
                        if (!p.equals(w[i]))
                        {
                            var d = w[i].clone();
                            if (this.isPlayer(d.x, d.y)) 
                            {
                                this._player.kill();
                            }
                            if (this._map[d.x][d.y] === Sprite.nothing)
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
                                this._map[p.x][p.y] = Sprite.nothing;
                                return;
                            }
                        }
                    }
                }
            }
        }

        private placeGhost(x: number, y: number, ghost: Ghost)
        {
            this._map[x][y] = ghost.type;
            ghost.position.x = x;
            ghost.position.y = y;
        }

        private killGhost(x: number, y: number)
        {
            var ghost: Ghost = this.ghost(x, y);
            if (ghost.alive)
            {
                for (var dy: number = y - 1; dy <= y + 1; dy++)
                {
                    for (var dx: number = x - 1; dx <= x + 1; dx++)
                    {
                        if ((dx > 0) && (dx < 19) && (dy > 0) && (dy < 13))
                        {
                            if (this.isPlayer(dx, dy))
                            {
                                this._player.kill();
                            }
                            else
                            {
                                if (this.isGhost(dx, dy))
                                {
                                    this.ghost(dx, dy).kill();
                                    this._score += 99;
                                }
                                this._map[dx][dy] = Sprite.nothing;
                            }
                        }
                    }
                }

                ghost.kill();
            }
        }

        private ghost(x: number, y: number): Ghost
        {
            for (var i: number = 0; i < this._ghosts.length; i++)
            {
                var ghost: Ghost = this._ghosts[i];
                if ((x == ghost.position.x) && (y == ghost.position.y))
                {
                    return ghost;
                }
            }
            return null;            
        }

        public getSpriteIndex(x: number, y: number, blink: number): number
        {
            switch (this._map[x][y])
            {
                case Sprite.nothing:
                case Sprite.uvexit:
                case Sprite.buffer:    
                case Sprite.marker:    
                case Sprite.uvstone:
                    return 0;
                case Sprite.stone:
                    return 1;
                case Sprite.ground:
                    return 2;
                case Sprite.diamond:
                    return 13 - ((blink + 4) % 6);
                case Sprite.wall:
                    return 14;
                case Sprite.exit:
                    return 32;
                case Sprite.changer:
                    return 33;
                case Sprite.ghost90L:
                case Sprite.ghost90R:
                case Sprite.ghost90LR:
                case Sprite.ghost180:
                    return this.ghost(x, y).imageIndex;
                case Sprite.player:
                    if ((x == this._player.position.x) && (y == this._player.position.y))
                    {
                        return this._player.imageIndex;
                    }
                    return 15;
            }
        }
    }
}