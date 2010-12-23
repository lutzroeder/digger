namespace Digger
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Windows.Input;
    using System.Windows.Shapes;
    using System.Windows.Threading;

    public sealed class Application : System.Windows.Application
    {
        public Application()
        {
            this.Startup += this.Application_Startup;
        }

        private void Application_Startup(object sender, StartupEventArgs args)
        {
            this.RootVisual = new ApplicationWindow();
        }

        private sealed class ApplicationWindow : Canvas
        {
            private DispatcherTimer timer;
            private Engine engine;
            private int[][] lastIndex = null;
            private Image[][] lastImage = null;
            private int lives = 20;
            private bool[] keysRelease = new bool[4];
            private int tick = 0;
            private ImageLoader spriteLoader;
            private ImageLoader fontLoader;
            private TextLine[] statusLines = new TextLine[2];

            public ApplicationWindow()
            {
                this.Loaded += this.Page_Loaded;
                this.KeyDown += this.Page_KeyDown;
                this.KeyUp += this.Page_KeyUp;

                using (Stream stream = GetType().Assembly.GetManifestResourceStream("Digger.Level.bin"))
                {
                    byte[] data = new byte[stream.Length];
                    stream.Read(data, 0, (int)stream.Length);
                    this.engine = new Engine(data);
                }

                this.Initialize();
            }

            private void Page_Loaded(object sender, EventArgs e)
            {
                this.lives = 20;
                this.engine.Score = 0;
                this.engine.Level = 4;
                this.engine.Load();

                this.lastIndex = new int[20][];
                this.lastImage = new Image[20][];
                for (int i = 0; i < 20; i++)
                {
                    this.lastIndex[i] = new int[14];
                    this.lastImage[i] = new Image[14];
                    for (int j = 0; j < 14; j++)
                    {
                        this.lastIndex[i][j] = 0xff;
                        this.lastImage[i][j] = null;
                    }
                }

                this.spriteLoader = new ImageLoader("Digger.Sprites.bin", 34);
                this.fontLoader = new ImageLoader("Digger.Font.bin", 59);

                this.InitializeStatusBar();
                this.Paint();
            }

            private void Page_KeyDown(object sender, KeyEventArgs e)
            {
                switch (e.Key)
                {
                    case Key.Left: // Left
                    case Key.A: // 'A'
                        this.engine.SetKey(0, true);
                        break;

                    case Key.Right: // Right
                    case Key.D: // 'D'
                        this.engine.SetKey(1, true);
                        break;

                    case Key.Up: // Up
                    case Key.W: // 'W'
                        this.engine.SetKey(2, true);
                        break;

                    case Key.Down: // Down
                    case Key.S: // 'S'
                        this.engine.SetKey(3, true);
                        break;

                    case Key.Escape: // Escape
                    case Key.R: // 'R'
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

                    case Key.Space: // Space
                    case Key.N: // 'N'
                        if (this.engine.Level < 29)
                        {
                            this.engine.Level++;
                            this.engine.Load();
                        }
                        break;
                }
            }

            private void Page_KeyUp(object sender, KeyEventArgs e)
            {
                switch (e.Key)
                {
                    case Key.Left: // Left
                    case Key.A: // 'A'
                        this.keysRelease[0] = true;
                        break;

                    case Key.Right: // Right
                    case Key.D: // 'D'
                        this.keysRelease[1] = true;
                        break;

                    case Key.Up: // Up
                    case Key.W: // 'W'
                        this.keysRelease[2] = true;
                        break;

                    case Key.Down: // Down
                    case Key.S: // 'S'
                        this.keysRelease[3] = true;
                        break;
                }
            }

            private void Timer_Callback(object sender, EventArgs e)
            {
                this.tick++;
                this.engine.Tick1();

                if ((this.tick % 2) == 0)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        if (this.keysRelease[i])
                        {
                            this.engine.SetKey(i, false);
                            this.keysRelease[i] = false;
                        }
                    }

                    this.engine.Tick4();
                }

                this.Paint();
            }

            private void InitializeStatusBar()
            {
                this.Children.Add(this.CreateBorder(2));
                this.Children.Add(this.CreateBorder(26));

                this.statusLines[0] = new TextLine(this.fontLoader);
                Canvas.SetTop(this.statusLines[0], 8f);
                this.Children.Add(this.statusLines[0]);

                this.statusLines[1] = new TextLine(this.fontLoader);
                Canvas.SetTop(this.statusLines[1], 16f);
                this.Children.Add(this.statusLines[1]);
            }

            private void Initialize()
            {
                if (this.timer == null)
                {
                    this.tick = 0;

                    this.timer = new DispatcherTimer();
                    this.timer.Tick += Timer_Callback;
                    this.timer.Interval = new TimeSpan(0, 0, 0, 0, 40);
                    this.timer.Start();
                }
            }

            private void Paint()
            {
                // Update display tree
                for (int x = 0; x < 20; x++)
                {
                    for (int y = 0; y < 14; y++)
                    {
                        int index = this.engine.GetSpriteImageIndex(x, y);

                        if (this.lastIndex[x][y] != index)
                        {
                            Image image = this.spriteLoader.Allocate(index);
                            Canvas.SetLeft(image, (double)(x * 16));
                            Canvas.SetTop(image, (double)((y * 16) + 32));
                            this.Children.Add(image);

                            if (this.lastIndex[x][y] != 0xff)
                            {
                                Image lastImage = this.lastImage[x][y];
                                this.Children.Remove(lastImage);
                                this.spriteLoader.Free(lastImage, this.lastIndex[x][y]);
                            }

                            this.lastImage[x][y] = image;
                            this.lastIndex[x][y] = index;
                        }
                    }
                }

                this.statusLines[0].Write("  ROOM:  " + (this.engine.Level + 1).ToString("D2") + " TIME:  " + this.engine.Time.ToString("D5") + " DIAMONDS:  " + this.engine.Diamonds.ToString("D2") + "  ");
                this.statusLines[1].Write("  LIVES: " + this.lives.ToString("D2") + " SCORE: " + this.engine.Score.ToString("D5") + " COLLECTED: " + this.engine.Collected.ToString("D2") + "  ");
            }

            private Rectangle CreateBorder(double top)
            {
                Rectangle rectangle = new Rectangle();
                rectangle.Fill = new SolidColorBrush(Colors.Cyan);
                rectangle.Width = 320;
                rectangle.Height = 4;
                Canvas.SetTop(rectangle, top);
                return rectangle;
            }

            private class TextLine : Canvas
            {
                private ImageLoader fontLoader;
                private int[] lastIndex;
                private Image[] lastImage;

                public TextLine(ImageLoader fontLoader)
                {
                    this.fontLoader = fontLoader;

                    this.Background = new SolidColorBrush(Color.FromArgb(255, 146, 5, 2));
                    this.Width = 320;
                    this.Height = 8;

                    this.lastIndex = new int[320 / 8];
                    this.lastImage = new Image[320 / 8];

                    for (int i = 0; i < 320 / 8; i++)
                    {
                        this.lastIndex[i] = 0xff;
                        this.lastImage[i] = null;
                    }
                }

                public void Write(string text)
                {
                    for (int i = 0; i < text.Length; i++)
                    {
                        int index = text[i] - 32;

                        if (this.lastIndex[i] != index)
                        {
                            Image image = this.fontLoader.Allocate(index);
                            Canvas.SetLeft(image, (double)(i * 8));
                            this.Children.Add(image);

                            if (this.lastIndex[i] != 0xff)
                            {
                                Image lastImage = this.lastImage[i];
                                this.Children.Remove(lastImage);
                                this.fontLoader.Free(lastImage, this.lastIndex[i]);
                            }

                            this.lastImage[i] = image;
                            this.lastIndex[i] = index;
                        }
                    }
                }
            }

            private class ImageLoader
            {
                private List<byte[]> list = new List<byte[]>();
                private Dictionary<int, List<Image>> table = new Dictionary<int, List<Image>>();

                public ImageLoader(string resourceName, int count)
                {
                    using (BinaryReader reader = new BinaryReader(this.GetType().Assembly.GetManifestResourceStream(resourceName)))
                    {
                        while (reader.BaseStream.Position < reader.BaseStream.Length)
                        {
                            this.table[this.list.Count] = new List<Image>();
                            this.list.Add(reader.ReadBytes(reader.ReadInt32()));
                        }
                    }
                }

                public Image Allocate(int value)
                {
                    Image image = null;

                    List<Image> list = table[value];
                    if (list.Count > 0)
                    {
                        image = list[list.Count - 1];
                        list.RemoveAt(list.Count - 1);
                    }
                    else
                    {
                        using (MemoryStream stream = new MemoryStream(this.list[value]))
                        {
                            BitmapImage bitmapImage = new BitmapImage();
                            bitmapImage.SetSource(stream);

                            image = new Image();
                            image.Source = bitmapImage;
                        }
                    }

                    return image;
                }

                public void Free(Image value, int index)
                {
                    table[index].Add(value);
                }
            }
        }
    }
}
