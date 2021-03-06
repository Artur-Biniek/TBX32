﻿using System;
using System.Collections.Generic;
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
        KeyCodes _gamePad;


        Computer _comp;
        private DateTime LastTime;

        public MainWindow()
        {
            InitializeComponent();

            initializeScreen();

            initilizeKeyboard();

            reset();

            _dispatcherTimer.Interval = TimeSpan.FromMilliseconds(30); // TODO: this get's rounded to 30fps... need to fix it.

            _dispatcherTimer.Tick += _dispatcherTimer_Tick;
        }

        private void initilizeKeyboard()
        {
            this.KeyDown += (o, e) =>
            {
                switch (e.Key)
                {
                    case System.Windows.Input.Key.Left:
                        _gamePad |= KeyCodes.KEY_LEFT;
                        break;
                    case System.Windows.Input.Key.Right:
                        _gamePad |= KeyCodes.KEY_RIGHT;
                        break;
                    case System.Windows.Input.Key.Up:
                        _gamePad |= KeyCodes.KEY_UP;
                        break;
                    case System.Windows.Input.Key.Down:
                        _gamePad |= KeyCodes.KEY_DOWN;
                        break;
                    case System.Windows.Input.Key.Escape:
                        _gamePad |= KeyCodes.KEY_ESC;
                        break;
                }
            };

            this.KeyUp += (o, e) =>
            {
                switch (e.Key)
                {
                    case System.Windows.Input.Key.Left:
                        _gamePad &= ~KeyCodes.KEY_LEFT;
                        break;
                    case System.Windows.Input.Key.Right:
                        _gamePad &= ~KeyCodes.KEY_RIGHT;
                        break;
                    case System.Windows.Input.Key.Up:
                        _gamePad &= ~KeyCodes.KEY_UP;
                        break;
                    case System.Windows.Input.Key.Down:
                        _gamePad &= ~KeyCodes.KEY_DOWN;
                        break;
                    case System.Windows.Input.Key.Escape:
                        _gamePad &= ~KeyCodes.KEY_ESC;
                        break;
                }
            };
        }

        private void reset()
        {
            var prg = ProgramsRepository.Tetris;
            var text = ProgramsRepository.ToString(prg);

            _dispatcherTimer.IsEnabled = false;

            _comp = new Computer(() => _gamePad);
            _comp.LoadProgram(prg);

            screenRefresh();
        }

        #region screen management
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
        }

        private void screenRefresh()
        {
            Title = _comp.PC.ToString();
            txtAvg.Content = _comp[Computer.SEG_DISP].ToString();

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
        #endregion

        private void simStep()
        {
            _comp.Step();

            screenRefresh();
        }

        private void _dispatcherTimer_Tick(object sender, EventArgs e)
        {
            var dif = DateTime.Now - LastTime;

            LastTime = DateTime.Now;

            for (int i = 0; i < 3 *  dif.TotalMilliseconds; i++)
                _comp.Step();

            screenRefresh();
        }

        private void btnStep_Click(object sender, RoutedEventArgs e)
        {
            simStep();
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            reset();
        }

        private void btnRun_Click(object sender, RoutedEventArgs e)
        {
            LastTime = DateTime.Now;
            _dispatcherTimer.IsEnabled = true;
        }
    }
}