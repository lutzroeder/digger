namespace Digger
{
    using System;
    using System.IO;
    using System.Collections;
    using System.Drawing;
    using System.Drawing.Drawing2D;
    using System.Drawing.Imaging;
    using System.WinForms;

    public enum Directions
    {
        Left, Right, Up, Down, None
    }

    public class Position
    {
        public int x;
        public int y;

        public Position() { x = 0; y = 0; }
        public Position(int x, int y) { this.x = x; this.y = y; }
        public Position(Position p) { this.x = p.x; this.y = p.y; }
        public bool Equal(Position p) { return ((this.x == p.x) && (this.y == p.y)); }
    }

    public class Sprite
    {
        public bool Invalidate = true;
        public virtual int GetBitmap() { return 0; }
    }

    public class Nothing : Sprite
    {
        public override int GetBitmap() { return 0; }
    }

    public class Ground : Sprite
    {
        public override int GetBitmap() { return 2; }
    }

    public class Ghost : Sprite
    {
        public Directions Direction = Directions.None;
        public bool Death = false;

        public override int GetBitmap()
        {
            if (Direction == Directions.Left) return 4;
            if (Direction == Directions.Right) return 5;
            if (Direction == Directions.Up) return 6;
            if (Direction == Directions.Down) return 3;

            return 3;
        }

        public virtual void Move(Level l)
        {
        }

        public void Die()
        {
            Death = true;
        }

        public void Blast(Level l)
        {
            if (Death) return;

            Position p = l.GetSpritePosition(this);

            for (int y = p.y - 1; y <= p.y + 1; y++)
                for (int x = p.x - 1; x <= p.x + 1; x++)
                {
                    if ((x > 0) && (x < 19) && (y > 0) && (y < 13))
                    {
                        if (l.GetSprite(x, y).GetType() == typeof(Digger))
                        {
                            Digger d = (Digger)l.GetSprite(x, y);
                            l.Digger.Die();
                        }
                        else
                        {
                            if (l.GetSprite(x, y).GetType().BaseType == typeof(Ghost))
                            {
                                Ghost g = (Ghost)l.GetSprite(x, y);
                                g.Die();
                                l.AddScore(99);
                            }

                            l.SetSprite(x, y, new Nothing());
                        }
                    }
                }

            Die();
        }
    }

    public class Ghost180 : Ghost
    {
        public override void Move(Level l)
        {
            if (Death) return;

            Position p = l.GetSpritePosition(this);
            Position[] w = { new Position(p), new Position(p), new Position(p), new Position(p) };

            if (Direction == Directions.Left) { w[0].x--; w[1].x++; }
            if (Direction == Directions.Right) { w[0].x++; w[1].x--; }
            if (Direction == Directions.Up) { w[0].y--; w[1].y++; }
            if (Direction == Directions.Down) { w[0].y++; w[1].y--; }

            for (int i = 0; i < 4; i++)
            {
                if (!p.Equal(w[i]))
                {
                    Position d = new Position(w[i]);

                    // Digger
                    if (l.GetSprite(d.x, d.y).GetType() == typeof(Digger))
                        l.Digger.Die();

                    // Nothing
                    if (l.GetSprite(d.x, d.y).GetType() == typeof(Nothing))
                    {
                        if (d.x < p.x) Direction = Directions.Left;
                        if (d.x > p.x) Direction = Directions.Right;
                        if (d.y < p.y) Direction = Directions.Up;
                        if (d.y > p.y) Direction = Directions.Down;

                        l.SetSprite(d.x, d.y, this);
                        l.SetSprite(p.x, p.y, new Nothing());

                        return;
                    }
                }
            }
        }
    }

    public class Ghost90L : Ghost
    {
        public override void Move(Level l)
        {
            if (Death) return;

            Position p = l.GetSpritePosition(this);
            Position[] w = { new Position(p), new Position(p), new Position(p), new Position(p) };

            if (Direction == Directions.Left) { w[0].x--; w[1].y++; w[2].y--; w[3].x++; }
            if (Direction == Directions.Right) { w[0].x++; w[1].y--; w[2].y++; w[3].x--; }
            if (Direction == Directions.Up) { w[0].y--; w[1].x--; w[2].x++; w[3].y++; }
            if (Direction == Directions.Down) { w[0].y++; w[1].x++; w[2].x--; w[3].y--; }

            for (int i = 0; i < 4; i++)
            {
                if (!p.Equal(w[i]))
                {
                    Position d = new Position(w[i]);

                    // Digger
                    if (l.GetSprite(d.x, d.y).GetType() == typeof(Digger))
                        l.Digger.Die();

                    // Nothing
                    if (l.GetSprite(d.x, d.y).GetType() == typeof(Nothing))
                    {
                        if (d.x < p.x) Direction = Directions.Left;
                        if (d.x > p.x) Direction = Directions.Right;
                        if (d.y < p.y) Direction = Directions.Up;
                        if (d.y > p.y) Direction = Directions.Down;

                        l.SetSprite(d.x, d.y, this);
                        l.SetSprite(p.x, p.y, new Nothing());

                        return;
                    }
                }
            }
        }
    }

    public class Ghost90LR : Ghost
    {
        public Directions Lastturn = Directions.None;

        public override void Move(Level l)
        {

            if (Death) return;

            Position p = l.GetSpritePosition(this);
            Position[] w = { new Position(p), new Position(p), new Position(p), new Position(p) };

            if (Direction == Directions.Left)
            {
                w[0].x--; w[3].x++;
                if (Lastturn == Directions.Left) { w[1].y--; w[2].y++; } else { w[1].y++; w[2].y--; };
            }

            if (Direction == Directions.Right)
            {
                w[0].x++; w[3].x--;
                if (Lastturn == Directions.Left) { w[1].y++; w[2].y--; } else { w[1].y--; w[2].y++; };
            }

            if (Direction == Directions.Up)
            {
                w[0].y--; w[3].y++;
                if (Lastturn == Directions.Left) { w[1].x++; w[2].x--; } else { w[1].x--; w[2].x++; };
            }

            if (Direction == Directions.Down)
            {
                w[0].y++; w[3].y--;
                if (Lastturn == Directions.Left) { w[1].x--; w[2].x++; } else { w[1].x++; w[2].x--; };
            }

            for (int i = 0; i < 4; i++)
            {
                if (!p.Equal(w[i]))
                {
                    Position d = new Position(w[i]);

                    // Digger
                    if (l.GetSprite(d.x, d.y).GetType() == typeof(Digger))
                        l.Digger.Die();

                    // Nothing
                    if (l.GetSprite(d.x, d.y).GetType() == typeof(Nothing))
                    {
                        Directions LastDirection = Direction;

                        if (d.x < p.x) Direction = Directions.Left;
                        if (d.x > p.x) Direction = Directions.Right;
                        if (d.y < p.y) Direction = Directions.Up;
                        if (d.y > p.y) Direction = Directions.Down;

                        switch (LastDirection)
                        {
                            case Directions.Left:
                                if (Direction == Directions.Down) Lastturn = Directions.Left;
                                if (Direction == Directions.Up) Lastturn = Directions.Right;
                                break;
                            case Directions.Right:
                                if (Direction == Directions.Down) Lastturn = Directions.Right;
                                if (Direction == Directions.Up) Lastturn = Directions.Left;
                                break;
                            case Directions.Up:
                                if (Direction == Directions.Left) Lastturn = Directions.Left;
                                if (Direction == Directions.Right) Lastturn = Directions.Right;
                                break;
                            case Directions.Down:
                                if (Direction == Directions.Left) Lastturn = Directions.Right;
                                if (Direction == Directions.Right) Lastturn = Directions.Left;
                                break;
                        }

                        l.SetSprite(d.x, d.y, this);
                        l.SetSprite(p.x, p.y, new Nothing());

                        return;
                    }
                }
            }
        }
    }

    public class Ghost90R : Ghost
    {
        public override void Move(Level l)
        {
            if (Death) return;

            Position p = l.GetSpritePosition(this);
            Position[] w = { new Position(p), new Position(p), new Position(p), new Position(p) };

            if (Direction == Directions.Left) { w[0].x--; w[1].y--; w[2].y++; w[3].x++; }
            if (Direction == Directions.Right) { w[0].x++; w[1].y++; w[2].y--; w[3].x--; }
            if (Direction == Directions.Up) { w[0].y--; w[1].x++; w[2].x--; w[3].y++; }
            if (Direction == Directions.Down) { w[0].y++; w[1].x--; w[2].x++; w[3].y--; }

            for (int i = 0; i < 4; i++)
            {
                if (!p.Equal(w[i]))
                {
                    Position d = new Position(w[i]);

                    // Digger
                    if (l.GetSprite(d.x, d.y).GetType() == typeof(Digger))
                        l.Digger.Die();

                    // Nothing
                    if (l.GetSprite(d.x, d.y).GetType() == typeof(Nothing))
                    {
                        if (d.x < p.x) Direction = Directions.Left;
                        if (d.x > p.x) Direction = Directions.Right;
                        if (d.y < p.y) Direction = Directions.Up;
                        if (d.y > p.y) Direction = Directions.Down;

                        l.SetSprite(d.x, d.y, this);
                        l.SetSprite(p.x, p.y, new Nothing());

                        return;
                    }
                }
            }
        }
    }

    public class Wall : Sprite
    {
        public override int GetBitmap() { return 14; }
    }

    public class Buffer : Sprite
    {
        public override int GetBitmap() { return 0; }
    }

    public class Digger : Sprite
    {
        Directions Direction = Directions.None;
        public ArrayList Keys = new ArrayList();
        private int Step = 0;
        private bool Dead = false;
        private bool StoneLeft = false;
        private bool StoneRight = false;

        public override int GetBitmap()
        {
            if (Dead) return 31;

            if (Direction == Directions.Left)
            {
                if (Step == 0) return 16;
                if (Step == 1) return 17;
                if (Step == 2) return 18;
                if (Step == 3) return 19;
                if (Step == 4) return 18;
                if (Step == 5) return 17;
            }

            if (Direction == Directions.Right)
            {
                if (Step == 0) return 20;
                if (Step == 1) return 21;
                if (Step == 2) return 22;
                if (Step == 3) return 23;
                if (Step == 4) return 22;
                if (Step == 5) return 21;
            }

            if (Direction == Directions.Up)
            {
                if (Step == 0) return 24;
                if (Step == 1) return 25;
            }

            if (Direction == Directions.Down)
            {
                if (Step == 0) return 26;
                if (Step == 1) return 27;
            }

            if (Direction == Directions.None)
            {
                if ((Step == 8) || (Step == 9)) return 28;
                if ((Step == 12) || (Step == 13)) return 28;
                if ((Step == 20) || (Step == 21)) return 29;
                if ((Step == 22) || (Step == 23)) return 30;
                if ((Step == 24) || (Step == 25)) return 29;
            }

            return 15;
        }

        public void Animate(Level l, int x, int y)
        {
            if (Dead) return;

            switch (Direction)
            {
                case Directions.Left:
                    Step++;
                    if (Step >= 6) Step = 0;
                    l.Digger.Invalidate = true;
                    break;

                case Directions.Right:
                    Step++;
                    if (Step >= 6) Step = 0;
                    l.Digger.Invalidate = true;
                    break;

                case Directions.Up:
                    Step++;
                    if (Step >= 2) Step = 0;
                    l.Digger.Invalidate = true;
                    break;

                case Directions.Down:
                    Step++;
                    if (Step >= 2) Step = 0;
                    l.Digger.Invalidate = true;
                    break;

                case Directions.None:
                    Step++;
                    if (Step >= 30) Step = 0;
                    l.Digger.Invalidate = true;
                    break;
            }
        }

        public void Move(Level l)
        {
            if (Dead) return;

            Position p = l.GetSpritePosition(this);
            Position d = new Position(p);
            Position z = new Position(d);

            if (Keys.Count > 0)
            {
                Direction = (Directions)Keys[Keys.Count - 1];
                if (Direction == Directions.Left) z.x--;
                if (Direction == Directions.Right) z.x++;
                if (Direction == Directions.Up) z.y--;
                if (Direction == Directions.Down) z.y++;
            }
            else
            {
                Direction = Directions.None;
            }

            if (!Keys.Contains(Directions.Left)) StoneLeft = false;
            if (!Keys.Contains(Directions.Right)) StoneRight = false;

            if (!d.Equal(z))
            {
                // Nothing
                if (l.GetSprite(z.x, z.y).GetType() == typeof(Nothing))
                {
                    l.SetSprite(d.x, d.y, this);
                }

                // Diamond
                if (l.GetSprite(z.x, z.y).GetType() == typeof(Diamond))
                {
                    l.Collected += 1;
                    l.AddScore(3);
                    // TODO: DIAMOND SOUND
                }

                // Stone
                if (l.GetSprite(z.x, z.y).GetType() == typeof(Stone))
                {
                    if ((z.x > d.x) && (l.GetSprite(z.x + 1, z.y).GetType() == typeof(Nothing)))
                    {
                        if (StoneRight)
                        {
                            StoneRight = false;
                            l.SetSprite(d.x + 2, d.y, l.GetSprite(d.x + 1, d.y));
                            l.SetSprite(d.x + 1, d.y, new Nothing());
                        }
                        else
                        {
                            StoneRight = true;
                        }
                    }

                    if ((z.x < d.x) && (l.GetSprite(z.x - 1, z.y).GetType() == typeof(Nothing)))
                    {
                        if (StoneLeft)
                        {
                            StoneLeft = false;
                            l.SetSprite(d.x - 2, d.y, l.GetSprite(d.x - 1, d.y));
                            l.SetSprite(d.x - 1, d.y, new Nothing());
                        }
                        else
                        {
                            StoneLeft = true;
                        }
                    }
                }

                // Ground
                if (l.GetSprite(z.x, z.y).GetType() == typeof(Nothing) ||
                        l.GetSprite(z.x, z.y).GetType() == typeof(Ground) ||
                        l.GetSprite(z.x, z.y).GetType() == typeof(Diamond))
                {
                    l.SetSprite(z.x, z.y, this);
                    l.SetSprite(d.x, d.y, new Buffer());
                }

                // Exit
                if (l.GetSprite(z.x, z.y).GetType() == typeof(Exit))
                {
                    l.Exit();
                }

                // Ghost
                if (l.GetSprite(z.x, z.y).GetType().BaseType == typeof(Ghost))
                {
                    l.Digger.Die();
                }
            }

            Animate(l, z.x, z.y);
        }

        public void Die()
        {
            Dead = true;
            Invalidate = true;
        }
    };

    public class Exit : Sprite
    {
        public override int GetBitmap() { return 32; }
    };

    public class Changer : Sprite
    {
        public override int GetBitmap() { return 33; }
    };

    public class Gravitation : Sprite
    {
        public void Simulate(Level l, int x, int y)
        {
            int dx = x;
            int dy = y;

            // Nothing
            if (l.GetSprite(x, y + 1).GetType() == typeof(Nothing))
            {
                dy = y + 1;
            }
            else
            {
                // Stone or diamond
                if ((l.GetSprite(x, y + 1).GetType() == typeof(Stone)) || (l.GetSprite(x, y + 1).GetType() == typeof(Diamond)))
                {
                    if (l.GetSprite(x - 1, y + 1).GetType() == typeof(Nothing) && l.GetSprite(x - 1, y).GetType() == typeof(Nothing))
                    {
                        dx = x - 1;
                        dy = y + 1;
                    }
                    else
                    {
                        if (l.GetSprite(x + 1, y + 1).GetType() == typeof(Nothing) && l.GetSprite(x + 1, y).GetType() == typeof(Nothing))
                        {
                            dx = x + 1;
                            dy = y + 1;
                        }
                    }
                }

                // Changer
                if ((l.GetSprite(x, y + 1).GetType() == typeof(Changer)) && (l.GetSprite(x, y).GetType() == typeof(Stone)) && (l.GetSprite(x, y + 2).GetType() == typeof(Nothing)))
                    dy = y + 2;
            }

            // Mark
            if ((dx != x) || (dy != y)) l.SetSprite(dx, dy, new Marker());
        }

        public void Move(Level l, int x, int y)
        {
            int dx = x;
            int dy = y;

            // Follow gravitation
            if (l.GetSprite(x, y + 1).GetType() == typeof(Marker))
            {
                dy = y + 1;
            }
            else
            {
                // Falls to the left or to the right
                if (l.GetSprite(x, y + 1).GetType() == typeof(Stone) || l.GetSprite(x, y + 1).GetType() == typeof(Diamond) || l.GetSprite(x, y + 1).GetType() == typeof(Nothing))
                {
                    if (l.GetSprite(x - 1, y + 1).GetType() == typeof(Marker) && (l.GetSprite(x - 1, y).GetType() == typeof(Nothing) || l.GetSprite(x - 1, y).GetType() == typeof(Marker)))
                    {
                        dx = x - 1;
                        dy = y + 1;
                    }
                    else
                    {
                        if (l.GetSprite(x + 1, y + 1).GetType() == typeof(Marker) && (l.GetSprite(x + 1, y).GetType() == typeof(Nothing) || l.GetSprite(x + 1, y).GetType() == typeof(Marker)))
                        {
                            dx = x + 1;
                            dy = y + 1;
                        }
                    }
                }

                // Changer
                if (l.GetSprite(x, y + 1).GetType() == typeof(Changer) && l.GetSprite(x, y).GetType() == typeof(Stone) && l.GetSprite(x, y + 2).GetType() == typeof(Marker))
                    dy = y + 2;
            }

            // Move
            if ((dx != x) || (dy != y))
            {
                if ((dy - y) == 2)
                {
                    // changer
                    l.SetSprite(dx, dy, new Diamond());
                }
                else
                {
                    l.SetSprite(dx, dy, l.GetSprite(x, y));
                    if (l.GetSprite(dx, dy).GetType() == typeof(UvStone))
                    {
                        l.SetSprite(dx, dy, new Stone());
                    }
                }

                l.SetSprite(x, y, new Nothing());

                // Kill digger if necessary
                if (l.GetSprite(dx, dy + 1).GetType() == typeof(Digger))
                {
                    l.Digger.Die();

                    // TODO: STONE SOUND
                }

                if (l.GetSprite(dx, dy + 1).GetType().BaseType == typeof(Ghost))
                {
                    Ghost g = (Ghost)l.GetSprite(dx, dy + 1);
                    g.Blast(l);

                    // TODO: STONE SOUND
                }
            }
        }
    }

    public class Marker : Gravitation
    {
        public override int GetBitmap() { return 31; }
    }

    public class Stone : Gravitation
    {
        public override int GetBitmap() { return 1; }
    };

    public class Diamond : Gravitation
    {
        private int Counter = 0;

        public void Blink()
        {
            Counter++;
            if (Counter == 6) Counter = 0;
            Invalidate = true;
        }

        public override int GetBitmap()
        {
            return (13 - ((Counter + 4 * 1) % 6));
        }
    };

    public class UvStone : Gravitation
    {
        public override int GetBitmap() { return 0; }
    }

    public class Level
    {
        private string File;
        public int Number;
        private Sprite[,] Map = new Sprite[20, 14];
        public Digger Digger;
        public Ghost[] Ghosts = new Ghost[16];
        public int Diamonds;
        public int Collected;
        public int Grounds;
        public int Time;
        public int Score = 0;

        public Level(string f, int n)
        {
            File = f;
            Number = n;
            Score = 0;
        }

        public void Load()
        {
            FileStream fs = new FileStream(File, FileMode.Open);
            fs.Seek(Number * 156, SeekOrigin.Begin);
            BinaryReader reader = new BinaryReader(fs);
            byte[] data = new byte[156];
            data = reader.ReadBytes(156);
            fs.Close();

            // Clean ghost variables
            for (int g = 0; g < 16; g++)
                Ghosts[g] = null;

            // Read level map
            for (int y = 0; y < 14; y++)
                for (int x = 0; x < 10; x++)
                {
                    byte id1 = (byte)(data[y * 10 + x] >> 4);
                    CreateSprite(x * 2, y, id1);
                    byte id2 = (byte)(data[y * 10 + x] & 0x0F);
                    CreateSprite(x * 2 + 1, y, id2);
                }

            // Read digger position
            Sprite DiggerPosition = Map[data[145], data[146] - 2];
            if (DiggerPosition.GetType() == typeof(Digger))
                Digger = (Digger)DiggerPosition;
            else
                MessageBox.Show("Invalid Digger position.\r");

            // Read ghost directions
            int Ghost = 0;
            for (int y = 0; y < 14; y++)
                for (int x = 0; x < 20; x++)
                {
                    if (GetSprite(x, y).GetType().BaseType == typeof(Ghost))
                    {
                        Ghost g = (Ghost)GetSprite(x, y);
                        Ghosts[Ghost] = g;

                        int GhostInfo = data[0x94 + (Ghost >> 1)];

                        if ((Ghost & 1) != 0)
                        {
                            GhostInfo = GhostInfo & 0x0F;

                            if (g.GetType() == typeof(Ghost90LR))
                            {
                                Ghost90LR g90 = (Ghost90LR)g;
                                g90.Lastturn = Directions.Right;
                            }
                        }
                        else
                        {
                            GhostInfo = GhostInfo >> 4;

                            if (g.GetType() == typeof(Ghost90LR))
                            {
                                Ghost90LR g90 = (Ghost90LR)g;
                                g90.Lastturn = Directions.Left;
                            }
                        }

                        if (GhostInfo == 3) g.Direction = Directions.Left;
                        if (GhostInfo == 2) g.Direction = Directions.Right;
                        if (GhostInfo == 1) g.Direction = Directions.Up;
                        if (GhostInfo == 0) g.Direction = Directions.Down;

                        Ghost++;
                    }
                }

            // Set level data
            Diamonds = (data[147] / 0x10) * 10 + (data[147] % 0x10);
            Collected = 0;
            Grounds = 0;
            Time = 5000;
        }

        public void Exit()
        {
            if (Collected < Diamonds) return;
            if (Number >= 29) return;

            Number++;
            Load();
        }

        public void AddScore(int Score)
        {
            this.Score += Score;
        }

        public void CreateSprite(int x, int y, int id)
        {
            if (id == 0) SetSprite(x, y, new Nothing());
            if (id == 1) SetSprite(x, y, new Stone());
            if (id == 2) SetSprite(x, y, new Ground());
            if (id == 3) SetSprite(x, y, new Ghost180());
            if (id == 4) MessageBox.Show("Unexpected LDIGGER ID.\r");
            if (id == 5) SetSprite(x, y, new Diamond());
            if (id == 6) SetSprite(x, y, new Wall());
            if (id == 7) SetSprite(x, y, new Ghost90L());
            if (id == 8) MessageBox.Show("Unexpected FSTODMD ID.\r");
            if (id == 9) SetSprite(x, y, new UvStone());
            if (id == 10) SetSprite(x, y, new Digger());
            if (id == 11) SetSprite(x, y, new Ghost90LR());
            if (id == 12) SetSprite(x, y, new Exit());
            if (id == 13) MessageBox.Show("Unexpected UNKOWND ID.");
            if (id == 14) SetSprite(x, y, new Changer());
            if (id == 15) SetSprite(x, y, new Ghost90R());
        }

        public void SetSprite(int x, int y, Sprite s)
        {
            Map[x, y] = s;
            s.Invalidate = true;
        }

        public Sprite GetSprite(int x, int y)
        {
            return Map[x, y];
        }

        public Position GetSpritePosition(Sprite s)
        {
            Position p = new Position();

            for (int x = 0; x < 20; x++)
                for (int y = 0; y < 14; y++)
                    if (GetSprite(x, y) == s)
                    {
                        p = new Position(x, y);
                        return p;
                    }

            return p;
        }

        public void Tick1()
        {
            for (int y = 0; y < 14; y++)
                for (int x = 0; x < 20; x++)
                    if (Map[x, y].GetType() == typeof(Diamond))
                    {
                        Diamond d = (Diamond)Map[x, y];
                        d.Blink();
                    }
        }

        public void Tick2()
        {
        }

        public void Tick4()
        {
            // Sound 1

            // Turn buffers into nothing
            for (int y = 13; y >= 0; y--)
                for (int x = 19; x >= 0; x--)
                    if (Map[x, y].GetType() == typeof(Buffer))
                        SetSprite(x, y, new Nothing());

            // Digger
            Digger.Move(this);

            // Stones and Diamonds (UVStones?)
            for (int y = 13; y >= 0; y--)
                for (int x = 19; x >= 0; x--)
                    if (Map[x, y].GetType() == typeof(Stone) || Map[x, y].GetType() == typeof(Diamond) || Map[x, y].GetType() == typeof(UvStone))
                    {
                        Gravitation g = (Gravitation)Map[x, y];
                        g.Simulate(this, x, y);
                    }

            for (int y = 13; y >= 0; y--)
                for (int x = 19; x >= 0; x--)
                    if (Map[x, y].GetType() == typeof(Stone) || Map[x, y].GetType() == typeof(Diamond) || Map[x, y].GetType() == typeof(UvStone))
                    {
                        Gravitation g = (Gravitation)Map[x, y];
                        g.Move(this, x, y);
                    }

            for (int g = 0; g < 16; g++)
            {
                if (Ghosts[g] != null)
                    Ghosts[g].Move(this);
            }

            // Time
            if (Time > 0)
                Time--;
            else
                Digger.Die();

            // Sound 2
        }
    }

    public class BitmapMagnifier
    {
        public Bitmap Bitmap;

        public BitmapMagnifier(string File, int Magnification)
        {
            Bitmap b = new Bitmap(File);
            Bitmap r = new Bitmap(b.Width * Magnification, b.Height * Magnification);

            for (int x = 0; x < b.Width; x++)
                for (int y = 0; y < b.Height; y++)
                    for (int ix = 0; ix < Magnification; ix++)
                        for (int iy = 0; iy < Magnification; iy++)
                            r.SetPixel(x * Magnification + ix, y * Magnification + iy, b.GetPixel(x, y));

            Bitmap = r;
        }
    }

    public class BitmapFont
    {
        private Bitmap[] FontBitmaps = new Bitmap[96];
        public Point Point = new Point(0, 0);
        private Size Size = new Size();

        public BitmapFont(string File, int Magnification, Color Foreground, Color Background)
        {
            // Font bitmap
            BitmapMagnifier BitmapMagnifier = new BitmapMagnifier(File, Magnification);
            Bitmap b = BitmapMagnifier.Bitmap;

            for (int x = 0; x < b.Width; x++)
                for (int y = 0; y < b.Height; y++)
                    if (b.GetPixel(x, y) == Color.FromARGB(0, 0, 0))
                        b.SetPixel(x, y, Foreground);
                    else
                        b.SetPixel(x, y, Background);

            Rectangle r = new Rectangle(0, 0, 8 * Magnification, 8 * Magnification);

            for (int i = 0; i < 94; i++)
            {
                FontBitmaps[i] = b.Clone(r, PixelFormat.Format24bppRGB);
                r.Y += r.Height;
            }

            Size.Width = r.Width;
            Size.Height = r.Height;
        }

        public void WriteCharacter(Graphics g, char c)
        {
            g.DrawImage(FontBitmaps[(byte)(c - 32)], Point.X, Point.Y, Size.Width, Size.Height);
            Point.X += Size.Width;
        }

        public void WriteString(Graphics g, string s)
        {
            for (int i = 0; i < s.Length; i++)
                WriteCharacter(g, s[i]);
        }

        public void WriteInteger(Graphics g, int Number, int Digits)
        {
            if (Digits != 1) WriteInteger(g, Number / 10, Digits - 1);
            WriteCharacter(g, (char)((int)'0' + (Number % 10)));
        }

        public void Newline()
        {
            Point.X = 0;
            Point.Y += Size.Height;
        }
    }

    public class Game : Form
    {
        private int Magnification = 2;
        private Bitmap[] SpriteBitmaps = new Bitmap[34];
        private Timer Timer;
        private int Tick = 0;
        private Level Level;
        private int Lives = 20;
        private BitmapFont WhiteOnRed;
        private BitmapFont CyanOnBlue;

        public static int Main(string[] args)
        {
            Application.Run(new Game());
            return 0;
        }

        public Game()
        {
            this.Text = "Digger.net";
            this.ClientSize = new Size(320 * Magnification, 256 * Magnification);

            // Sprite bitmap
            BitmapMagnifier BitmapMagnifier = new BitmapMagnifier("sprite.png", Magnification);
            Bitmap Sprite = BitmapMagnifier.Bitmap;
            Rectangle r = new Rectangle(0, 0, 16 * Magnification, 16 * Magnification);

            for (int i = 0; i < 34; i++)
            {
                SpriteBitmaps[i] = Sprite.Clone(r, PixelFormat.Format32bppARGB);
                r.X += r.Width;
            }

            // Font bitmap
            CyanOnBlue = new BitmapFont("font.png", Magnification, Color.FromARGB(0x2D, 0xE7, 0xC0), Color.FromARGB(0x04, 0x02, 0x8F));
            WhiteOnRed = new BitmapFont("font.png", Magnification, Color.FromARGB(0xF3, 0xF0, 0xF9), Color.FromARGB(0x92, 0x02, 0x05));

            // Level
            Level = new Level("level.bin", 0);
            Level.Load();

            // Timer
            Timer = new Timer();
            Timer.Interval = 40;
            Timer.AddOnTimer(new EventHandler(OnTimer));
            Timer.Start();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Up:
                    if (!Level.Digger.Keys.Contains(Directions.Up))
                        Level.Digger.Keys.Add(Directions.Up);
                    break;

                case Keys.Down:
                    if (!Level.Digger.Keys.Contains(Directions.Down))
                        Level.Digger.Keys.Add(Directions.Down);
                    break;

                case Keys.Left:
                    if (!Level.Digger.Keys.Contains(Directions.Left))
                        Level.Digger.Keys.Add(Directions.Left);
                    break;

                case Keys.Right:
                    if (!Level.Digger.Keys.Contains(Directions.Right))
                        Level.Digger.Keys.Add(Directions.Right);
                    break;

                case Keys.Escape:
                    if (Lives > 0)
                    {
                        Lives--;
                        Level.Load();
                    }
                    break;

                case Keys.Home:
                    if (Level.Number < 29)
                    {
                        Level.Number++;
                        Level.Load();
                    }
                    break;
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Up:
                    if (Level.Digger.Keys.Contains(Directions.Up))
                        Level.Digger.Keys.Remove(Directions.Up);
                    break;

                case Keys.Down:
                    if (Level.Digger.Keys.Contains(Directions.Down))
                        Level.Digger.Keys.Remove(Directions.Down);
                    break;

                case Keys.Left:
                    if (Level.Digger.Keys.Contains(Directions.Left))
                        Level.Digger.Keys.Remove(Directions.Left);
                    break;

                case Keys.Right:
                    if (Level.Digger.Keys.Contains(Directions.Right))
                        Level.Digger.Keys.Remove(Directions.Right);
                    break;
            }
        }

        protected override void OnInvalidate(InvalidateEventArgs e)
        {
        }

        protected override void OnGotFocus(EventArgs e)
        {
            for (int y = 0; y < 14; y++)
                for (int x = 0; x < 20; x++)
                    Level.GetSprite(x, y).Invalidate = true;

            this.Refresh();
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
        }

        private void OnTimer(object sender, EventArgs pe)
        {
            Tick++;

            Level.Tick1();

            if ((Tick % 1) == 0)
                Level.Tick2();

            if ((Tick % 2) == 0)
            {
                Level.Tick4();

                Tick = 0;
            }

            this.Refresh();
        }

        protected override void OnPaint(PaintEventArgs pe)
        {
            Graphics g = pe.Graphics;

            CyanOnBlue.Point = new Point(0, 0);
            CyanOnBlue.WriteString(g, "]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]");
            CyanOnBlue.Newline();
            CyanOnBlue.Newline();
            CyanOnBlue.Newline();
            CyanOnBlue.WriteString(g, "]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]");
            WhiteOnRed.Point = new Point(0, 0);
            WhiteOnRed.Newline();
            WhiteOnRed.WriteString(g, "  CAVE:  ");
            WhiteOnRed.WriteInteger(g, Level.Number + 1, 2);
            WhiteOnRed.WriteString(g, " TIME:  ");
            WhiteOnRed.WriteInteger(g, Level.Time, 5);
            WhiteOnRed.WriteString(g, " DIAMONDS:  ");
            WhiteOnRed.WriteInteger(g, Level.Diamonds, 2);
            WhiteOnRed.WriteString(g, "  ");
            WhiteOnRed.Newline();
            WhiteOnRed.WriteString(g, "  LIVES: ");
            WhiteOnRed.WriteInteger(g, Lives, 2);
            WhiteOnRed.WriteString(g, " SCORE: ");
            WhiteOnRed.WriteInteger(g, Level.Score, 5);
            WhiteOnRed.WriteString(g, " COLLECTED: ");
            WhiteOnRed.WriteInteger(g, Level.Collected, 2);
            WhiteOnRed.WriteString(g, "  ");
            WhiteOnRed.Newline();

            int Top = 32 * Magnification;
            int Size = 16 * Magnification;

            for (int y = 0; y < 14; y++)
                for (int x = 0; x < 20; x++)
                {
                    Sprite Sprite = Level.GetSprite(x, y);
                    if (Sprite.Invalidate)
                    {
                        g.DrawImage(SpriteBitmaps[Sprite.GetBitmap()], x * Size, y * Size + Top, Size, Size);
                        Sprite.Invalidate = false;
                    }
                }
        }
    }
}