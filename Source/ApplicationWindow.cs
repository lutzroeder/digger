namespace Digger
{
    using System;
    using System.IO;
    using System.Collections;
    using System.Drawing;
    using System.Drawing.Imaging;
    using System.Windows.Forms;

    public class ApplicationWindow : Form
    { 
        private int magnification = 1;
        private Bitmap[] sprites = new Bitmap[34];
        private FontLoader fontLoader;
        private Timer timer;
        private Engine engine;
        private int tick = 0;
        private int lives = 20;
        private bool[] keysRelease = new bool[4];
        private bool[] keysDelay = new bool[4];  

        public static void Main(string[] args)
        {
            Application.Run(new ApplicationWindow());
        }

        public ApplicationWindow()
        {
            this.Icon = new Icon(GetType().Assembly.GetManifestResourceStream("Application.ico"));
            this.Text = ".NET Digger";
            this.FormBorderStyle = FormBorderStyle.FixedToolWindow;
            this.MaximizeBox = false;    

            this.ClientSize = MagnifyRectangle(0, 0, 320, 256).Size;
            
            this.fontLoader = new FontLoader(MagnifyBitmap(new Bitmap(GetType().Assembly.GetManifestResourceStream("Digger.Font.png"))), MagnifyRectangle(0, 0, 8, 8).Size);

            Bitmap sprites = MagnifyBitmap(new Bitmap(GetType().Assembly.GetManifestResourceStream("Digger.Sprite.png")));
            Rectangle cursor = MagnifyRectangle(0, 0, 16, 16);
            for (int i = 0; i < 34; i++)
            {
                this.sprites[i] = sprites.Clone(cursor, PixelFormat.Format32bppArgb);
                cursor.X += cursor.Width;
            }

            using (Stream stream = GetType().Assembly.GetManifestResourceStream("Digger.Level.bin"))
            {
                byte[] data = new Byte[stream.Length];
                stream.Read(data, 0, (int)stream.Length);
                this.engine = new Engine(data);
            }

            this.engine.Load();
            this.timer = new Timer();
            this.timer.Interval = 40;
            this.timer.Tick += new EventHandler(this.Timer_Tick);
            this.timer.Start();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left) { this.engine.Keys[0] = true; keysDelay[0] = false; }
            if (e.KeyCode == Keys.Right) { this.engine.Keys[1] = true; keysDelay[1] = false; }
            if (e.KeyCode == Keys.Up) { this.engine.Keys[2] = true; keysDelay[2] = false; }
            if (e.KeyCode == Keys.Down) { this.engine.Keys[3] = true; keysDelay[3] = false; }
            if ((e.KeyCode == Keys.Escape) && (lives > 0)) { lives--; this.engine.Load(); }
            if ((e.KeyCode == Keys.Home) && (this.engine.Level < 29)) { this.engine.Level++; this.engine.Load(); }
        }

        protected override void OnKeyUp(KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Left) 
                if (keysDelay[0]) this.engine.Keys[0] = false; else keysRelease[0] = true;
            if (e.KeyCode == Keys.Right)
                if (keysDelay[1]) this.engine.Keys[1] = false; else keysRelease[1] = true;
            if (e.KeyCode == Keys.Up)
                if (keysDelay[2]) this.engine.Keys[2] = false; else keysRelease[2] = true;
            if (e.KeyCode == Keys.Down)
                if (keysDelay[3]) this.engine.Keys[3] = false; else keysRelease[3] = true;
        }

        private void Timer_Tick(object sender, EventArgs pe)
        {
            this.tick++;
            this.engine.Tick1();

            if ((this.tick % 2) == 0)
            {
                if (this.keysRelease[0]) { this.engine.Keys[0] = this.keysRelease[0] = false; }
                if (this.keysRelease[1]) { this.engine.Keys[1] = this.keysRelease[1] = false; }
                if (this.keysRelease[2]) { this.engine.Keys[2] = this.keysRelease[2] = false; }
                if (this.keysRelease[3]) { this.engine.Keys[3] = this.keysRelease[3] = false; }
                this.engine.Tick4();
            }

            this.Invalidate(this.MagnifyRectangle(0, 8, 320, 16));

            for (int y = 0; y < 14; y++)
            {
                for (int x = 0; x < 20; x++)
                {
                    Rectangle rectangle = MagnifyRectangle(x * 16, y * 16 + 32, 16, 16);
                    this.Invalidate(rectangle);
                }
            }

            this.Update();
        }

        protected override void OnPaintBackground(PaintEventArgs e) 
        {
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            if ((e.ClipRectangle.Width != 0) && (e.ClipRectangle.Height != 0))
            {
                Graphics graphics = e.Graphics;

                SolidBrush blueBrush = new SolidBrush(Color.FromArgb(0x04, 0x02, 0x8F));
                graphics.FillRectangle(blueBrush, MagnifyRectangle(0, 0, 320, 8));
                graphics.FillRectangle(blueBrush, MagnifyRectangle(0, 24, 320, 8));
                SolidBrush cyanBrush = new SolidBrush(Color.FromArgb(0x2D, 0xE7, 0xC0));
                graphics.FillRectangle(cyanBrush, MagnifyRectangle(0, 2, 320, 4));
                graphics.FillRectangle(cyanBrush, MagnifyRectangle(0, 26, 320, 4));
    
                this.fontLoader.Reset();
                this.fontLoader.Newline();
                this.fontLoader.Write(graphics, "  CAVE:  " + (this.engine.Level + 1).ToString("D2") + " TIME:  " + this.engine.Time.ToString("D5") + " DIAMONDS:  " + this.engine.Diamonds.ToString("D2") + "  ");
                this.fontLoader.Newline();
                this.fontLoader.Write(graphics, "  LIVES: " + lives.ToString("D2") + " SCORE: " + this.engine.Score.ToString("D5") + " COLLECTED: " + this.engine.Collected.ToString("D2") + "  ");
                this.fontLoader.Newline();
    
                for (int y = 0; y < 14; y++)
                {
                    for (int x = 0; x < 20; x++)
                    {
                        Rectangle rectangle = this.MagnifyRectangle(x * 16, y * 16 + 32, 16, 16);
                        if (e.ClipRectangle.IntersectsWith(rectangle)) 
                            graphics.DrawImageUnscaled(sprites[this.engine.GetSpriteImageIndex(x, y)], rectangle.X, rectangle.Y);
                    }
                }
            }
        }

        private Rectangle MagnifyRectangle(int x, int y, int dx, int dy)
        {
            return new Rectangle(x * magnification, y * magnification, dx * magnification, dy * magnification);
        }

        private Bitmap MagnifyBitmap(Bitmap source)
        {
            Bitmap target = new Bitmap(source.Width * magnification, source.Height * magnification);
            for (int x = 0; x < source.Width; x++)
                for (int y = 0; y < source.Height; y++)
                    for (int ix = 0; ix < magnification; ix++)
                        for (int iy = 0; iy < magnification; iy++)
                            target.SetPixel(x * magnification + ix, y * magnification + iy, source.GetPixel(x, y));
            return target;
        }

        private class FontLoader
        {
            private Point position = new Point(0, 0);
            private Size size;
            private Bitmap[] characters = new Bitmap[59];

            public FontLoader(Bitmap bitmap, Size size)
            {
                this.size = size;
                Rectangle cursor = new Rectangle(0, 0, size.Width, size.Height);
                for (int i = 0; i < 59; i++)
                {
                    this.characters[i] = bitmap.Clone(cursor, PixelFormat.Format24bppRgb);
                    this.characters[i].MakeTransparent(Color.FromArgb(0, 0, 0));
                    cursor.Y += cursor.Height;
                }
            }

            public void Reset()
            {
                this.position = new Point(0, 0);
            }

            public void Write(Graphics graphics, string text)
            {
                SolidBrush redBrush = new SolidBrush(Color.FromArgb(0x92, 0x02, 0x05));
                for (int i = 0; i < text.Length; i++)
                {
                    graphics.FillRectangle(redBrush, new Rectangle(this.position.X, this.position.Y, this.size.Width, this.size.Height));
                    graphics.DrawImageUnscaled(this.characters[(byte)(text[i] - 32)], this.position.X, this.position.Y);
                    this.position.X += size.Width;
                }
            }

            public void Newline()
            {
                this.position.X = 0;
                this.position.Y += size.Height;
            }
        }
    }
}
