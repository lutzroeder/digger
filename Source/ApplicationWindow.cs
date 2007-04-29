namespace Digger
{
    using System;
    using System.Collections;
    using System.Drawing;
    using System.IO;
    using System.Windows.Forms;

    internal class ApplicationWindow : Form
    { 
        private byte[][] lastScreen = null;
        private Bitmap[] sprites = new Bitmap[34];
        private Bitmap[] characters = new Bitmap[63];
        private Point origin = new Point(0, 0);
        private Timer timer;
        private int tick = 0;
        private Engine engine;
        private int lives = 20;
        private bool[] keysRelease = new bool[4];
        private bool[] keysDelay = new bool[4];
        private Size spriteSize;
        private Size gridSize;
        private Size clientSize;
        private Image image;
        
        private static void Main(string[] args)
        {
            Application.Run(new ApplicationWindow());
        }

        public ApplicationWindow()
        {
            this.Text = "Digger";
            this.Icon = new Icon(this.GetType().Assembly.GetManifestResourceStream("Digger.Application.ico"));

            int width = (this.ClientSize.Width > 320) ? 320 : this.ClientSize.Width;
            int height = (this.ClientSize.Height > 232) ? 232 : this.ClientSize.Height;
            this.clientSize = new Size(width, height);
            this.image = new Bitmap(this.ClientSize.Width, this.ClientSize.Height);

            using (Graphics graphics = Graphics.FromImage(this.image))
            {
                graphics.FillRectangle(new SolidBrush(Color.Black), 0, 0, this.image.Width, this.image.Height);
            }

            using (Stream stream = this.GetType().Assembly.GetManifestResourceStream("Digger.Sprites.bin"))
            {
                BinaryReader reader = new BinaryReader(stream);
                for (int i = 0; i < 34; i++)
                {
                    int length = reader.ReadInt32();
                    byte[] bytes = reader.ReadBytes(length);
                    MemoryStream memoryStream = new MemoryStream(bytes);
                    this.sprites[i] = new Bitmap(memoryStream);
                    memoryStream.Close();
                }
            }

            using (Stream stream = this.GetType().Assembly.GetManifestResourceStream("Digger.Font.bin"))
            {
                BinaryReader reader = new BinaryReader(stream);
                for (int i = 0; i < 63; i++)
                {
                    int length = reader.ReadInt32();
                    byte[] bytes = reader.ReadBytes(length);
                    MemoryStream memoryStream = new MemoryStream(bytes);
                    this.characters[i] = new Bitmap(memoryStream);
                    memoryStream.Close();
                }
            }

            using (Stream stream = GetType().Assembly.GetManifestResourceStream("Digger.Level.bin"))
            {
                byte[] data = new Byte[stream.Length];
                stream.Read(data, 0, (int)stream.Length);
                this.engine = new Engine(data);
            }

            this.spriteSize = new Size(this.sprites[0].Width, this.sprites[0].Height);
            this.gridSize = new Size();

            this.gridSize.Width = this.clientSize.Width / this.spriteSize.Width;
            if ((this.clientSize.Width % this.spriteSize.Width) != 0)
            {
                this.gridSize.Width++;
            }

            this.gridSize.Height = (this.clientSize.Height - 8) / this.spriteSize.Height;
            if (((this.clientSize.Height - 8) % this.spriteSize.Height) != 0)
            {
                this.gridSize.Height++;
            }

            this.lives = 20;
            this.engine.Level = 0;
            this.engine.Load();

            this.timer = new Timer();
            this.timer.Interval = 40;
            this.timer.Tick += new EventHandler(this.Timer_Tick);
            this.timer.Enabled = true;

            this.lastScreen = new byte[this.gridSize.Width][];
            for (int i = 0; i < this.gridSize.Width; i++)
            {
                this.lastScreen[i] = new byte[this.gridSize.Height];
                for (int j = 0; j < this.gridSize.Height; j++)
                {
                    this.lastScreen[i][j] = 0xff;
                }
            }
        }

        protected override void OnPaintBackground(PaintEventArgs e) 
        {
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.DrawImage(this.image, e.ClipRectangle, e.ClipRectangle, GraphicsUnit.Pixel);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Left:
                    this.engine.Keys[0] = true; 
                    keysDelay[0] = false; 
                    break;

                case Keys.Right:
                    this.engine.Keys[1] = true; 
                    keysDelay[1] = false; 
                    break;

                case Keys.Up:
                    this.engine.Keys[2] = true; 
                    keysDelay[2] = false; 
                    break;

                case Keys.Down:
                    this.engine.Keys[3] = true; 
                    keysDelay[3] = false; 
                    break;

                case Keys.F8:
                    if (this.lives > 0)
                    {
                        this.lives--;
                        this.engine.Load();
                    }
                    else
                    {
                        // Restart game
                        this.lives = 20;
                        this.engine.Level = 0;
                        this.engine.Load();
                    }
                    break;

                case Keys.F9:
                    ApplicationWindow applicationWindow = (ApplicationWindow) this.TopLevelControl;
                    applicationWindow.Close();
                    break;

                case Keys.D0:
                    if (this.engine.Level < 29)
                    {
                        this.engine.Level++;
                        this.engine.Load();
                    }
                    break;
            }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left) 
            {
                if (keysDelay[0]) 
                {
                    this.engine.Keys[0] = false; 
                }
                else 
                {
                    keysRelease[0] = true;
                }
            }

            if (e.KeyCode == Keys.Right) 
            {
                if (keysDelay[1]) 
                {
                    this.engine.Keys[1] = false; 
                }
                else 
                {
                    keysRelease[1] = true;
                }
            }

            if (e.KeyCode == Keys.Up) 
            {
                if (keysDelay[2]) 
                {
                    this.engine.Keys[2] = false; 
                }
                else 
                {
                    keysRelease[2] = true;
                }
            }

            if (e.KeyCode == Keys.Down) 
            {
                if (keysDelay[3]) 
                {
                    this.engine.Keys[3] = false; 
                }
                else 
                {
                    keysRelease[3] = true;
                }
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            this.tick++;
            this.engine.Tick1();

            if ((this.tick % 2) == 0)
            {
                if (this.keysRelease[0]) 
                {
                    this.engine.Keys[0] = this.keysRelease[0] = false; 
                }
                
                if (this.keysRelease[1]) 
                {
                    this.engine.Keys[1] = this.keysRelease[1] = false; 
                }
                
                if (this.keysRelease[2]) 
                {
                    this.engine.Keys[2] = this.keysRelease[2] = false; 
                }
                
                if (this.keysRelease[3]) 
                {
                    this.engine.Keys[3] = this.keysRelease[3] = false; 
                }

                this.engine.Tick4();
            }

            int[] position = this.engine.GetCurrentPosition();

            if (this.origin.X < (20 - this.gridSize.Width))
            {
                if (((this.origin.X + this.gridSize.Width) - position[0]) < 6)
                {
                    this.origin.X++;
                }
            }

            if (this.origin.Y < (14 - this.gridSize.Height))
            {
                if (((this.origin.Y + this.gridSize.Height) - position[1]) < 5)
                {
                    this.origin.Y++;
                }
            }

            if ((this.origin.X > 0) && ((position[0] - this.origin.X) < 6))
            {
                this.origin.X--;
            }

            if ((this.origin.Y > 0) && ((position[1] - this.origin.Y) < 5))
            {
                this.origin.Y--;
            }

            using (Graphics graphics = Graphics.FromImage(this.image))
            {
                string text = " [" + (this.engine.Level + 1).ToString("D2") + " \\" + this.lives.ToString("D2") + " ]" + this.engine.Collected.ToString("D2") + "/" + this.engine.Diamonds.ToString("D2") + " ^" + this.engine.Time.ToString("D4") + " ";
                Point p = new Point(0, 0);
                for (int i = 0; i < text.Length; i++)
                {
                    graphics.DrawImage(this.characters[(byte)(text[i] - 32)], p.X, p.Y);
                    p.X += 8;
                }

                this.Invalidate(new Rectangle(0, 0, 180, 8));

                for (int x = 0; x < this.gridSize.Width; x++)
                {
                    for (int y = 0; y < this.gridSize.Height; y++)
                    {
                        Rectangle rectangle = new Rectangle(x * 16, (y * 16) + 8, 16, 16);

                        if (rectangle.Bottom > this.clientSize.Height)
                        {
                            rectangle.Height = this.clientSize.Height - rectangle.Top;
                        }

                        if (rectangle.Right > this.clientSize.Width)
                        {
                            rectangle.Width = this.clientSize.Width - rectangle.Left;
                        }

                        byte imageIndex = (byte)this.engine.GetSpriteImageIndex(this.origin.X + x, this.origin.Y + y);
                        if (imageIndex != this.lastScreen[x][y])
                        {
                            Rectangle srcRect = new Rectangle(0, 0, rectangle.Width, rectangle.Height);
                            graphics.DrawImage(this.sprites[imageIndex], rectangle, srcRect, GraphicsUnit.Pixel);
                            this.lastScreen[x][y] = imageIndex;

                            this.Invalidate(rectangle);
                        }
                    }
                }
            }

            this.Update();
        }
    }
}
