using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;
using Tbx32.Core;

namespace ArturBiniek.Tbx32.Simulator
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const int PIXEL_SIZE = 10;

        const int WIDTH = 32;
        const int HEIGHT = 32;

        DispatcherTimer _dispatcherTimer = new DispatcherTimer();

        Rectangle[,] _pixels = new Rectangle[32, 32];

        Computer _comp = new Computer();

        public MainWindow()
        {
            InitializeComponent();

            initializeScreen();

            Reset();

            _dispatcherTimer.Interval = TimeSpan.FromMilliseconds(1); // TODO: this get's rounded to 30fps... need to fix it.
            _dispatcherTimer.Tick += _dispatcherTimer_Tick;
        }

        private void _dispatcherTimer_Tick(object sender, EventArgs e)
        {
            for (int i = 0; i < 33; i++)
                _comp.Step();

            screenRefresh();
        }

        private void Reset()
        {
            _dispatcherTimer.IsEnabled = false;

            _comp = new Computer();

            var builder = new CodeBuilder();

            var video = builder.CreateLabel();
            var putPixel = builder.CreateLabel();
            var whileLoop = builder.CreateLabel();
            var exitLoop = builder.CreateLabel();

            var prg = builder

                        .Ld(R.G0, video)
                        .Movi(R.S0, 0)
                        .Movi(R.S1, 31)

                        .MarkLabel(whileLoop)
                            .Bgt(R.S0, R.S1, exitLoop)
                            .Push(R.Fp)
                            .Push(R.S0)
                            .Push(R.S0)
                            .Jal(R.Ra, putPixel)
                            .Addi(R.S0, R.S0, 1)
                            .Jmp(whileLoop)

                        .MarkLabel(exitLoop)
                            .Hlt()

                        .MarkLabel(putPixel)
                            // prolog
                            .Mov(R.Fp, R.Sp)
                            .Push(R.Ra)

                            .Ldr(R.T0, R.Fp, 1)        // T0 <- x
                            .Ldr(R.T1, R.Fp, 2)        // T1 <- y
                            .Add(R.T2, R.G0, R.T0)     // T2 <- VIDEO + x
                            .Movi(R.T3, 1)
                            .Movi(R.T4, 31)
                            .Sub(R.T4, R.T4, R.T1)
                            .Shl(R.T4, R.T3, R.T4)
                            .Ldr(R.T3, R.T2)
                            .Or(R.T3, R.T3, R.T4)
                            .Str(R.T3, R.T2)

                            // epilog
                            .Pop(R.Ra)
                            .Addi(R.Sp, R.Sp, 2)
                            .Pop(R.Fp)
                            .Jmpr(R.Ra)



                        .MarkLabel(video)
                        .Data((int)Computer.VIDEO_START)

                        .Build();

            _comp.LoadProgram(prg);

            screenRefresh();
        }

        private void initializeScreen()
        {
            for (int i = 0; i < 32; i++)
            {
                var def = new RowDefinition();
                def.Height = GridLength.Auto;

                grdLedScreen.RowDefinitions.Add(def);

                var colDef = new ColumnDefinition();
                colDef.Width = GridLength.Auto;
                grdLedScreen.ColumnDefinitions.Add(colDef);
            }

            for (int i = 0; i < 32; i++)
            {
                grdLedScreen.RowDefinitions.Add(new RowDefinition());

                for (int j = 0; j < 32; j++)
                {
                    var rect = new Rectangle();

                    rect.Width = PIXEL_SIZE;
                    rect.Height = PIXEL_SIZE;
                    rect.Fill = new SolidColorBrush(Colors.Black);

                    rect.Stroke = new SolidColorBrush(Color.FromArgb(0x11, 0xff, 0xff, 0xff));
                    rect.StrokeThickness = 0.5;

                    Grid.SetRow(rect, i);
                    Grid.SetColumn(rect, j);

                    grdLedScreen.Children.Add(rect);

                    _pixels[i, j] = rect;
                }
            }

            screenRefresh();
        }

        private void screenRefresh()
        {
            this.Title = _comp.PC.ToString();

            for (int row = 0; row < 32; row++)
            {
                var bit = (uint)1 << 31;

                for (var col = 0; col < 32; col++, bit >>= 1)
                {
                    if ((_comp[(uint)(Computer.VIDEO_START + row)] & bit) != 0)
                    {
                        _pixels[row, col].Fill = new SolidColorBrush(Color.FromRgb(0x33, 0x66, 0x99));
                    }
                    else
                    {
                        _pixels[row, col].Fill = new SolidColorBrush(Colors.Black);
                    }
                }
            }
        }

        private void SimStep()
        {
            _comp.Step();

            screenRefresh();
        }

        private void btnStep_Click(object sender, RoutedEventArgs e)
        {
            SimStep();
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            Reset();
        }

        private void btnRun_Click(object sender, RoutedEventArgs e)
        {
            _dispatcherTimer.IsEnabled = true;
        }
    }
}