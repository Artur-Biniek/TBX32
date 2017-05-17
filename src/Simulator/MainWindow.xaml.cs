using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

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

        uint[] _screenMemory = new uint[32];


        public MainWindow()
        {
            InitializeComponent();

            initializeScreen();
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

                    rect.Stroke = new SolidColorBrush(Color.FromArgb(0x11,0xff,0xff,0xff));
                    rect.StrokeThickness = 0.5;

                    Grid.SetRow(rect, i);
                    Grid.SetColumn(rect, j);

                    grdLedScreen.Children.Add(rect);

                    _pixels[i, j] = rect;
                }
            }

            _screenMemory[20] = 0b100100010;

            screenRefresh();
        }

        private void screenRefresh()
        {
            for (int row = 0; row < 32; row++)
            {
                var bit = (uint)1 << 31;

                for (var col = 0; col < 32; col++, bit >>= 1)
                {
                    if ((_screenMemory[row] & bit) != 0)
                    {
                        _pixels[row, col].Fill = new SolidColorBrush(Color.FromRgb(0x00, 0x33, 0x66));
                    }
                    else
                    {
                        _pixels[row, col].Fill = new SolidColorBrush(Colors.Black);
                    }
                }
            }
        }
    }
}