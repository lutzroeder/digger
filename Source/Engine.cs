namespace Digger
{
    using System;

    internal class Engine
    {
        private static byte blinkCounter = 0;
        private byte[] data;
        private Sprite[][] map = null;
        private Ghost[] ghosts = null;
        private Digger digger;
        public bool[] keys = new bool[4];

        public Engine(byte[] data)
        {
            this.data = data;
        }

        public int Level { get; set; }

        public int Diamonds { get; set; }

        public int Collected { get; set; }

        public int Time { get; set; }

        public int Score { get; set; }

        public void SetKey(int key, bool value)
        {
            this.keys[key] = value;
        }

        public int GetSpriteImageIndex(int x, int y)
        {
            return this.GetSprite(x, y).ImageIndex;
        }

        public void Load()
        {
            this.ghosts = new Ghost[16];

            this.map = new Sprite[20][];
            for (int i = 0; i < 20; i++)
            {
                this.map[i] = new Sprite[14];
            }

            byte[] level = new byte[156];
            for (int i = 0; i < 156; i++)
            {
                level[i] = this.data[this.Level * 156 + i];
            }

            Type[] type = { typeof(Nothing), typeof(Stone), typeof(Ground), typeof(Ghost180), null, typeof(Diamond), typeof(Wall), typeof(Ghost90L), null, typeof(UvStone), typeof(Digger), typeof(Ghost90LR), typeof(Exit), null, typeof(Changer), typeof(Ghost90R) };
            for (int y = 0; y < 14; y++)
            {
                for (int x = 0; x < 10; x++)
                {
                    int hi = (byte) (level[y * 10 + x] >> 4);
                    this.SetSprite(x * 2, y, (Sprite) Activator.CreateInstance(type[hi]));
                    int low = (byte)(level[y * 10 + x] & 0x0f);
                    this.SetSprite(x * 2 + 1, y, (Sprite) Activator.CreateInstance(type[low]));
                }
            }

            this.digger = (Digger)this.map[level[145]][level[146] - 2];

            for (int g = 0; g < 16; g++)
            {
                ghosts[g] = null;
            }

            int ghostIndex = 0;
            for (int y = 0; y < 14; y++)
            {
                for (int x = 0; x < 20; x++)
                {
                    if (GetSprite(x, y).GetType().BaseType == typeof(Ghost))
                    {
                        Ghost ghost = (Ghost)GetSprite(x, y);
                        ghosts[ghostIndex] = ghost;
                        int ghostInfo = level[0x94 + (ghostIndex >> 1)];
                        if ((ghostIndex & 1) != 0)
                        {
                            ghostInfo = ghostInfo & 0x0F;
                            if (ghost.GetType() == typeof(Ghost90LR))
                            {
                                ((Ghost90LR) ghost).LastTurn = Direction.Right;
                            }
                        }
                        else
                        {
                            ghostInfo = ghostInfo >> 4;
                            if (ghost.GetType() == typeof(Ghost90LR))
                            {
                                ((Ghost90LR) ghost).LastTurn = Direction.Left;
                            }
                        }

                        Direction[] directions = { Direction.Down, Direction.Up, Direction.Right, Direction.Left };
                        ghost.Direction = directions[ghostInfo];
                        ghostIndex++;
                    }
                }
            }

            this.Diamonds = (level[147] / 0x10) * 10 + (level[147] % 0x10);
            this.Collected = 0;
            this.Time = 5000;
        }

        private int[] GetCurrentPosition()
        {
            Position position = this.GetSpritePosition(this.digger);
            int[] result = new int[2];
            result[0] = position.X;
            result[1] = position.Y;
            return result;
        }

        private void LoadNext()
        {
            if (this.Collected < this.Diamonds)
            {
                return;
            }

            if (this.Level >= 29)
            {
                return;
            }

            this.Level++;
            this.Load();
        }

        private void Die()
        {
            this.digger.Die();
        }

        private void SetSprite(int x, int y, Sprite sprite)
        {
            this.map[x][y] = sprite;
        }

        private Sprite GetSprite(int x, int y)
        {
            return this.map[x][y];
        }

        private Position GetSpritePosition(Sprite s)
        {
            for (int x = 0; x < 20; x++)
            {
                for (int y = 0; y < 14; y++)
                {
                    if (this.GetSprite(x, y) == s)
                    {
                        return new Position(x, y);
                    }
                }
            }

            return new Position();
        }

        public void Tick1()
        {
            blinkCounter++;

            if (blinkCounter == 6)
            {
                blinkCounter = 0;
            }
        }

        public void Tick4()
        {
            // Turn buffers into nothing
            for (int y = 13; y >= 0; y--)
            {
                for (int x = 19; x >= 0; x--)
                {
                    if (map[x][y].GetType() == typeof(Buffer))
                    {
                        this.SetSprite(x, y, new Nothing());
                    }
                }
            }

            // Digger
            this.digger.Move(this);

            // Stones and Diamonds (UVStones?)
            for (int y = 13; y >= 0; y--)
            {
                for (int x = 19; x >= 0; x--)
                {
                    if (this.map[x][y].GetType() == typeof(Stone) || this.map[x][y].GetType() == typeof(Diamond) || this.map[x][y].GetType() == typeof(UvStone))
                    {
                        Gravitation gravitation = (Gravitation)this.map[x][y];
                        gravitation.Simulate(this, x, y);
                    }
                }
            }

            for (int y = 13; y >= 0; y--)
            {
                for (int x = 19; x >= 0; x--)
                {
                    if (this.map[x][y].GetType() == typeof(Stone) || this.map[x][y].GetType() == typeof(Diamond) || this.map[x][y].GetType() == typeof(UvStone))
                    {
                        Gravitation gravitation = (Gravitation)this.map[x][y];
                        gravitation.Move(this, x, y);
                    }
                }
            }

            for (int i = 0; i < 16; i++)
            {
                if (ghosts[i] != null)
                {
                    ghosts[i].Move(this);
                }
            }

            if (this.Time > 0)
            {
                this.Time--;
            }

            if (this.Time == 0)
            {
                this.digger.Die();
            }
        }

        private struct Position
        {
            public int X;
            public int Y;

            public Position(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }

            public Position(Position position)
            {
                this.X = position.X;
                this.Y = position.Y;
            }

            public override bool Equals(object obj)
            {
                Position position = (Position)obj;
                return ((this.X == position.X) && (this.Y == position.Y));
            }

            public override int GetHashCode()
            {
                return base.GetHashCode();
            }

            public static bool operator ==(Position a, Position b)
            {
                return a.Equals(b);
            }

            public static bool operator !=(Position a, Position b)
            {
                return !a.Equals(b);
            }

            public override string ToString()
            {
                return "(" + this.X + ", " + this.Y + ")";
            }
        }

        private class Sprite
        {
            public virtual int ImageIndex
            {
                get
                {
                    return 0;
                }
            }
        }

        private class Ground : Sprite
        {
            public override int ImageIndex
            {
                get
                {
                    return 2;
                }
            }
        }

        private class Ghost : Sprite
        {
            public Direction Direction = Direction.None;
            public bool IsAlive = true;

            public override int ImageIndex
            {
                get
                {
                    switch (this.Direction)
                    {
                        case Direction.Left:
                            return 4;

                        case Direction.Right:
                            return 5;

                        case Direction.Up:
                            return 6;

                        case Direction.Down:
                            return 3;
                    }

                    return 3;
                }
            }

            public virtual void Move(Engine level)
            {
            }

            public void Die()
            {
                this.IsAlive = false;
            }

            public void Blast(Engine level)
            {
                if (this.IsAlive)
                {
                    Position p = level.GetSpritePosition(this);
                    for (int y = p.Y - 1; y <= p.Y + 1; y++)
                    {
                        for (int x = p.X - 1; x <= p.X + 1; x++)
                        {
                            if ((x > 0) && (x < 19) && (y > 0) && (y < 13))
                            {
                                if (level.GetSprite(x, y).GetType() == typeof(Digger))
                                {
                                    Digger d = (Digger)level.GetSprite(x, y);
                                    level.Die();
                                }
                                else
                                {
                                    if (level.GetSprite(x, y).GetType().BaseType == typeof(Ghost))
                                    {
                                        Ghost g = (Ghost)level.GetSprite(x, y);
                                        g.Die();
                                        level.Score += 99;
                                    }

                                    level.SetSprite(x, y, new Nothing());
                                }
                            }
                        }
                    }

                    this.Die();
                }
            }
        }

        private class Ghost180 : Ghost
        {
            public override void Move(Engine level)
            {
                if (this.IsAlive)
                {
                    Position p = level.GetSpritePosition(this);
                    Position[] w = { new Position(p), new Position(p), new Position(p), new Position(p) };

                    if (this.Direction == Direction.Left)
                    {
                        w[0].X--;
                        w[1].X++;
                    }

                    if (this.Direction == Direction.Right)
                    {
                        w[0].X++;
                        w[1].X--;
                    }

                    if (this.Direction == Direction.Up)
                    {
                        w[0].Y--;
                        w[1].Y++;
                    }

                    if (this.Direction == Direction.Down)
                    {
                        w[0].Y++;
                        w[1].Y--;
                    }

                    for (int i = 0; i < 4; i++)
                    {
                        if (p != w[i])
                        {
                            Position d = new Position(w[i]);

                            // Digger
                            if (level.GetSprite(d.X, d.Y).GetType() == typeof(Digger))
                            {
                                level.Die();
                            }

                            // Nothing
                            if (level.GetSprite(d.X, d.Y).GetType() == typeof(Nothing))
                            {
                                if (d.X < p.X)
                                {
                                    this.Direction = Direction.Left;
                                }

                                if (d.X > p.X)
                                {
                                    this.Direction = Direction.Right;
                                }

                                if (d.Y < p.Y)
                                {
                                    this.Direction = Direction.Up;
                                }

                                if (d.Y > p.Y)
                                {
                                    this.Direction = Direction.Down;
                                }

                                level.SetSprite(d.X, d.Y, this);
                                level.SetSprite(p.X, p.Y, new Nothing());

                                return;
                            }
                        }
                    }
                }
            }
        }

        private class Ghost90L : Ghost
        {
            public override void Move(Engine level)
            {
                if (this.IsAlive)
                {
                    Position p = level.GetSpritePosition(this);
                    Position[] w = { new Position(p), new Position(p), new Position(p), new Position(p) };

                    if (Direction == Direction.Left) { w[0].X--; w[1].Y++; w[2].Y--; w[3].X++; }
                    if (Direction == Direction.Right) { w[0].X++; w[1].Y--; w[2].Y++; w[3].X--; }
                    if (Direction == Direction.Up) { w[0].Y--; w[1].X--; w[2].X++; w[3].Y++; }
                    if (Direction == Direction.Down) { w[0].Y++; w[1].X++; w[2].X--; w[3].Y--; }

                    for (int i = 0; i < 4; i++)
                    {
                        if (p != w[i])
                        {
                            Position d = new Position(w[i]);

                            // Digger
                            if (level.GetSprite(d.X, d.Y).GetType() == typeof(Digger))
                            {
                                level.Die();
                            }

                            // Nothing
                            if (level.GetSprite(d.X, d.Y).GetType() == typeof(Nothing))
                            {
                                if (d.X < p.X) Direction = Direction.Left;
                                if (d.X > p.X) Direction = Direction.Right;
                                if (d.Y < p.Y) Direction = Direction.Up;
                                if (d.Y > p.Y) Direction = Direction.Down;
                                level.SetSprite(d.X, d.Y, this);
                                level.SetSprite(p.X, p.Y, new Nothing());
                                return;
                            }
                        }
                    }
                }
            }
        }

        private class Ghost90LR : Ghost
        {
            public Direction LastTurn = Direction.None;

            public override void Move(Engine level)
            {
                if (this.IsAlive)
                {
                    Position p = level.GetSpritePosition(this);
                    Position[] w = { new Position(p), new Position(p), new Position(p), new Position(p) };

                    if (this.Direction == Direction.Left)
                    {
                        w[0].X--; 
                        w[3].X++;
                        if (this.LastTurn == Direction.Left)
                        {
                            w[1].Y--;
                            w[2].Y++;
                        }
                        else
                        {
                            w[1].Y++;
                            w[2].Y--;
                        };
                    }

                    if (this.Direction == Direction.Right)
                    {
                        w[0].X++;
                        w[3].X--;
                        if (this.LastTurn == Direction.Left)
                        {
                            w[1].Y++;
                            w[2].Y--;
                        }
                        else
                        {
                            w[1].Y--;
                            w[2].Y++;
                        };
                    }

                    if (this.Direction == Direction.Up)
                    {
                        w[0].Y--; w[3].Y++;
                        if (this.LastTurn == Direction.Left) 
                        { 
                            w[1].X++; 
                            w[2].X--; 
                        }
                        else 
                        { 
                            w[1].X--; 
                            w[2].X++; 
                        }
                    }

                    if (this.Direction == Direction.Down)
                    {
                        w[0].Y++; w[3].Y--;
                        if (this.LastTurn == Direction.Left) 
                        { 
                            w[1].X--; 
                            w[2].X++; 
                        }
                        else 
                        { 
                            w[1].X++; 
                            w[2].X--; 
                        };
                    }

                    for (int i = 0; i < 4; i++)
                    {
                        if (p != w[i])
                        {
                            Position d = new Position(w[i]);

                            // Digger
                            if (level.GetSprite(d.X, d.Y).GetType() == typeof(Digger))
                            {
                                level.Die();
                            }

                            // Nothing
                            if (level.GetSprite(d.X, d.Y).GetType() == typeof(Nothing))
                            {
                                Direction lastDirection = this.Direction;

                                if (d.X < p.X) this.Direction = Direction.Left;
                                if (d.X > p.X) this.Direction = Direction.Right;
                                if (d.Y < p.Y) this.Direction = Direction.Up;
                                if (d.Y > p.Y) this.Direction = Direction.Down;

                                switch (lastDirection)
                                {
                                    case Direction.Left:
                                        if (Direction == Direction.Down) this.LastTurn = Direction.Left;
                                        if (Direction == Direction.Up) this.LastTurn = Direction.Right;
                                        break;
                                    case Direction.Right:
                                        if (Direction == Direction.Down) this.LastTurn = Direction.Right;
                                        if (Direction == Direction.Up) this.LastTurn = Direction.Left;
                                        break;
                                    case Direction.Up:
                                        if (Direction == Direction.Left) this.LastTurn = Direction.Left;
                                        if (Direction == Direction.Right) this.LastTurn = Direction.Right;
                                        break;
                                    case Direction.Down:
                                        if (Direction == Direction.Left) this.LastTurn = Direction.Right;
                                        if (Direction == Direction.Right) this.LastTurn = Direction.Left;
                                        break;
                                }

                                level.SetSprite(d.X, d.Y, this);
                                level.SetSprite(p.X, p.Y, new Nothing());

                                return;
                            }
                        }
                    }
                }
            }
        }

        private class Ghost90R : Ghost
        {
            public override void Move(Engine level)
            {
                if (this.IsAlive)
                {
                    Position p = level.GetSpritePosition(this);
                    Position[] w = { new Position(p), new Position(p), new Position(p), new Position(p) };

                    if (Direction == Direction.Left) { w[0].X--; w[1].Y--; w[2].Y++; w[3].X++; }
                    if (Direction == Direction.Right) { w[0].X++; w[1].Y++; w[2].Y--; w[3].X--; }
                    if (Direction == Direction.Up) { w[0].Y--; w[1].X++; w[2].X--; w[3].Y++; }
                    if (Direction == Direction.Down) { w[0].Y++; w[1].X--; w[2].X++; w[3].Y--; }

                    for (int i = 0; i < 4; i++)
                    {
                        if (p != w[i])
                        {
                            Position d = new Position(w[i]);

                            // Digger
                            if (level.GetSprite(d.X, d.Y).GetType() == typeof(Digger))
                            {
                                level.Die();
                            }

                            // Nothing
                            if (level.GetSprite(d.X, d.Y).GetType() == typeof(Nothing))
                            {
                                if (d.X < p.X) Direction = Direction.Left;
                                if (d.X > p.X) Direction = Direction.Right;
                                if (d.Y < p.Y) Direction = Direction.Up;
                                if (d.Y > p.Y) Direction = Direction.Down;

                                level.SetSprite(d.X, d.Y, this);
                                level.SetSprite(p.X, p.Y, new Nothing());

                                return;
                            }
                        }
                    }
                }
            }
        }

        private class Wall : Sprite
        {
            public override int ImageIndex
            {
                get
                {
                    return 14;
                }
            }
        }

        private class Digger : Sprite
        {
            private Direction direction = Direction.None;
            private int step = 0;
            private bool alive = true;
            private bool[] stone = new bool[2];

            public override int ImageIndex
            {
                get
                {
                    if (this.alive)
                    {
                        if ((this.direction == Direction.Left) && (step <= 5))
                        {
                            int[] left = { 16, 17, 18, 19, 18, 17 };
                            return left[step];
                        }

                        if ((this.direction == Direction.Right) && (step <= 5))
                        {
                            int[] right = { 20, 21, 22, 23, 22, 21 };
                            return right[step];
                        }

                        if ((this.direction == Direction.Up) && (step <= 1))
                        {
                            int[] up = { 24, 25 };
                            return up[step];
                        }

                        if ((this.direction == Direction.Down) && (step <= 1))
                        {
                            int[] down = { 26, 27 };
                            return down[step];
                        }

                        byte[] center = { 15, 15, 15, 15, 15, 15, 15, 15, 28, 28, 15, 15, 28, 28, 15, 15, 15, 15, 15, 15, 29, 29, 30, 30, 29, 29, 15, 15, 15, 15 };
                        return center[step];
                    }

                    return 31;
                }
            }

            public void Animate(Engine level, int x, int y)
            {
                if (this.alive)
                {
                    switch (this.direction)
                    {
                        case Direction.Left:
                            this.step++;
                            if (this.step >= 6)
                            {
                                this.step = 0;
                            }
                            break;

                        case Direction.Right:
                            this.step++;
                            if (this.step >= 6)
                            {
                                this.step = 0;
                            }
                            break;

                        case Direction.Up:
                            this.step++;
                            if (this.step >= 2)
                            {
                                this.step = 0;
                            }
                            break;

                        case Direction.Down:
                            this.step++;
                            if (this.step >= 2)
                            {
                                this.step = 0;
                            }
                            break;

                        case Direction.None:
                            this.step++;
                            if (this.step >= 30)
                            {
                                this.step = 0;
                            }
                            break;
                    }
                }
            }

            public void Move(Engine level)
            {
                if (this.alive)
                {
                    Position p = level.GetSpritePosition(this);
                    Position d = new Position(p);
                    Position z = new Position(d);

                    direction = Direction.None;
                    if (level.keys[0])
                    {
                        z.X--; direction = Direction.Left;
                    }
                    else
                    {
                        stone[0] = false;
                        if (level.keys[1])
                        {
                            z.X++; direction = Direction.Right;
                        }
                        else
                        {
                            stone[1] = false;
                            if (level.keys[2])
                            {
                                z.Y--; direction = Direction.Up;
                            }
                            else
                            {
                                if (level.keys[3])
                                {
                                    z.Y++; direction = Direction.Down;
                                }
                            }
                        }
                    }

                    if (d != z)
                    {
                        // Nothing
                        if (level.GetSprite(z.X, z.Y).GetType() == typeof(Nothing))
                        {
                            level.SetSprite(d.X, d.Y, this);
                        }

                        // Diamond
                        if (level.GetSprite(z.X, z.Y).GetType() == typeof(Diamond))
                        {
                            level.Collected += 1;
                            level.Score += 3;
                        }

                        // Stone
                        if (level.GetSprite(z.X, z.Y).GetType() == typeof(Stone))
                        {
                            if ((z.X > d.X) && (level.GetSprite(z.X + 1, z.Y).GetType() == typeof(Nothing)))
                            {
                                if (stone[1])
                                {
                                    level.SetSprite(d.X + 2, d.Y, level.GetSprite(d.X + 1, d.Y));
                                    level.SetSprite(d.X + 1, d.Y, new Nothing());
                                }
                                stone[1] = !stone[1];
                            }

                            if ((z.X < d.X) && (level.GetSprite(z.X - 1, z.Y).GetType() == typeof(Nothing)))
                            {
                                if (stone[0])
                                {
                                    level.SetSprite(d.X - 2, d.Y, level.GetSprite(d.X - 1, d.Y));
                                    level.SetSprite(d.X - 1, d.Y, new Nothing());
                                }
                                stone[0] = !stone[0];
                            }
                        }

                        // Ground
                        if (level.GetSprite(z.X, z.Y).GetType() == typeof(Nothing) || level.GetSprite(z.X, z.Y).GetType() == typeof(Ground) || level.GetSprite(z.X, z.Y).GetType() == typeof(Diamond))
                        {
                            level.SetSprite(z.X, z.Y, this);
                            level.SetSprite(d.X, d.Y, new Buffer());
                        }

                        // Exit
                        if (level.GetSprite(z.X, z.Y).GetType() == typeof(Exit))
                        {
                            level.LoadNext();
                        }

                        // Ghost
                        if (level.GetSprite(z.X, z.Y).GetType().BaseType == typeof(Ghost))
                        {
                            level.Die();
                        }
                    }

                    this.Animate(level, z.X, z.Y);
                }
            }

            public void Die()
            {
                this.alive = false;
            }
        }

        private class Exit : Sprite
        {
            public override int ImageIndex
            {
                get
                {
                    return 32;
                }
            }
        }

        private class Changer : Sprite
        {
            public override int ImageIndex
            {
                get
                {
                    return 33;
                }
            }
        }

        private class Gravitation : Sprite
        {
            public void Simulate(Engine level, int x, int y)
            {
                int dx = x;
                int dy = y;

                // Nothing
                if (level.GetSprite(x, y + 1).GetType() == typeof(Nothing))
                {
                    dy = y + 1;
                }
                else
                {
                    // Stone or diamond
                    if ((level.GetSprite(x, y + 1).GetType() == typeof(Stone)) || (level.GetSprite(x, y + 1).GetType() == typeof(Diamond)))
                    {
                        if (level.GetSprite(x - 1, y + 1).GetType() == typeof(Nothing) && level.GetSprite(x - 1, y).GetType() == typeof(Nothing))
                        {
                            dx = x - 1;
                            dy = y + 1;
                        }
                        else
                        {
                            if (level.GetSprite(x + 1, y + 1).GetType() == typeof(Nothing) && level.GetSprite(x + 1, y).GetType() == typeof(Nothing))
                            {
                                dx = x + 1;
                                dy = y + 1;
                            }
                        }
                    }

                    // Changer
                    if ((level.GetSprite(x, y + 1).GetType() == typeof(Changer)) && (level.GetSprite(x, y).GetType() == typeof(Stone)) && (level.GetSprite(x, y + 2).GetType() == typeof(Nothing)))
                    {
                        dy = y + 2;
                    }
                }

                // Mark
                if ((dx != x) || (dy != y))
                {
                    level.SetSprite(dx, dy, new Marker());
                }
            }

            public void Move(Engine level, int x, int y)
            {
                int dx = x;
                int dy = y;

                // Follow gravitation
                if (level.GetSprite(x, y + 1).GetType() == typeof(Marker))
                {
                    dy = y + 1;
                }
                else
                {
                    // Falls to the left or to the right
                    if (level.GetSprite(x, y + 1).GetType() == typeof(Stone) || level.GetSprite(x, y + 1).GetType() == typeof(Diamond) || level.GetSprite(x, y + 1).GetType() == typeof(Nothing))
                    {
                        if (level.GetSprite(x - 1, y + 1).GetType() == typeof(Marker) && (level.GetSprite(x - 1, y).GetType() == typeof(Nothing) || level.GetSprite(x - 1, y).GetType() == typeof(Marker)))
                        {
                            dx = x - 1;
                            dy = y + 1;
                        }
                        else
                        {
                            if (level.GetSprite(x + 1, y + 1).GetType() == typeof(Marker) && (level.GetSprite(x + 1, y).GetType() == typeof(Nothing) || level.GetSprite(x + 1, y).GetType() == typeof(Marker)))
                            {
                                dx = x + 1;
                                dy = y + 1;
                            }
                        }
                    }

                    // Changer
                    if (level.GetSprite(x, y + 1).GetType() == typeof(Changer) && level.GetSprite(x, y).GetType() == typeof(Stone) && level.GetSprite(x, y + 2).GetType() == typeof(Marker))
                    {
                        dy = y + 2;
                    }
                }

                // Move
                if ((dx != x) || (dy != y))
                {
                    if ((dy - y) == 2)
                    {
                        // Changer
                        level.SetSprite(dx, dy, new Diamond());
                    }
                    else
                    {
                        level.SetSprite(dx, dy, level.GetSprite(x, y));
                        if (level.GetSprite(dx, dy).GetType() == typeof(UvStone))
                        {
                            level.SetSprite(dx, dy, new Stone());
                        }
                    }

                    level.SetSprite(x, y, new Nothing());

                    // Kill digger if necessary
                    if (level.GetSprite(dx, dy + 1).GetType() == typeof(Digger))
                    {
                        level.Die();
                    }

                    if (level.GetSprite(dx, dy + 1).GetType().BaseType == typeof(Ghost))
                    {
                        Ghost g = (Ghost)level.GetSprite(dx, dy + 1);
                        g.Blast(level);
                    }
                }
            }
        }

        private class Marker : Gravitation
        {
            public override int ImageIndex
            {
                get
                {
                    return 31;
                }
            }
        }

        private class Stone : Gravitation
        {
            public override int ImageIndex
            {
                get
                {
                    return 1;
                }
            }
        }

        private class Diamond : Gravitation
        {
            public override int ImageIndex
            {
                get
                {
                    return (int) (13 - ((blinkCounter + 4) % 6));
                }
            }
        }

        private class Nothing : Sprite
        {
        }

        private class Buffer : Sprite
        {
        }

        private class UvStone : Gravitation
        {
        }

        private enum Direction
        {
            Left,
            Right,
            Up,
            Down,
            None
        }
    }
}
