﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Threading;
using System.Windows.Media.Animation;
using System.Windows.Media;
using System.Windows.Shapes;
using System.IO.IsolatedStorage;
using System.Windows.Media.Imaging;
using Bottle.Repository;
using System.IO;
using System.Text;
using MSPToolkit.Encodings;
using Coding4Fun.Toolkit.Controls;
using System.Globalization;
using Bottle.Resources;

namespace Bottle
{
    public partial class Game : PhoneApplicationPage
    {
        private Random random { get; set; }
        private int CountGamers { get; set; }
        private int CurrentGamer { get; set; }
        private Color colorLine { get; set; }
        IsolatedStorageSettings AppSettings;
        private bool IsAudioStopped { get; set; }
        private bool IsRotateStarted { get; set; }
        private List<string> Desires { get; set; }
        public string Desire { get; set; }
        private Image BottleImage { get; set; }
        private int numberBottle { get; set; }
        BackgroundRepository backgroundRepository;
        BottleRepository bottleRepository;

        public Game()
        {
            InitializeComponent();
            CurrentGamer = 1;
            AppSettings = IsolatedStorageSettings.ApplicationSettings;
            random = new Random();
            backgroundRepository = new BackgroundRepository();
            bottleRepository = new BottleRepository();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            string[] parameters = NavigationContext.QueryString["ParameterString"].ToString().Split(',');
            CountGamers = Convert.ToInt32(parameters[0]);
            numberBottle = Convert.ToInt32(parameters[1]);
            int numberBackground = Convert.ToInt32(parameters[2]);
            ImageBrush img = new ImageBrush();
            img.ImageSource = new BitmapImage(new Uri(backgroundRepository.GetBackground(numberBackground).Path, UriKind.RelativeOrAbsolute));
            LayoutRoot.Background = img;
            colorLine = backgroundRepository.GetBackground(numberBackground).ColorLine;
            CurrentGamerLabel.Foreground = new SolidColorBrush(colorLine);
            CurrentGamerLabel.Text = String.Format(AppResources.CurrentGamer, CurrentGamer);
            Desires = GetListDesires();
        }

        private void rotate(double degrees)
        {
            Storyboard MyStory = new Storyboard();
            MyStory.Duration = TimeSpan.FromMilliseconds(degrees);
            DoubleAnimation My_Double = new DoubleAnimation();
            My_Double.To = degrees;
            My_Double.Duration = TimeSpan.FromMilliseconds(degrees);
            MyStory.Children.Add(My_Double);
            RotateTransform MyTransform = new RotateTransform();
            Storyboard.SetTarget(My_Double, MyTransform);
            Storyboard.SetTargetProperty(My_Double, new PropertyPath("Angle"));
            BottleImage.RenderTransform = MyTransform;
            BottleImage.RenderTransformOrigin = new Point(0.5, 0.5);
            IsAudioStopped = false;
            mediaElement.Play();
            MyStory.Begin();
            GridLabelCurrentGamer.Visibility = Visibility.Collapsed;
            MyStory.Completed += animationRotateBottle_Completed;
        }

        private void animationRotateBottle_Completed(object sender, EventArgs e)
        {
            IsAudioStopped = true;
            mediaElement.Stop();
            SetDesire();

            StackPanel sp = new StackPanel();
            Viewbox vb = new Viewbox();
            vb.HorizontalAlignment = HorizontalAlignment.Stretch;
            TextBlock tbTitle = new TextBlock();
            tbTitle.FontFamily = new FontFamily("Comic Sans MS");
            tbTitle.Text = String.Format(AppResources.GamerDesireText, CurrentGamer);
            vb.Child = tbTitle;
            TextBlock tbBoby = new TextBlock();
            tbBoby.HorizontalAlignment = HorizontalAlignment.Center;
            tbBoby.TextWrapping = TextWrapping.Wrap;
            tbBoby.FontFamily = new FontFamily("Comic Sans MS");
            tbBoby.Text = Desire;
            tbBoby.Foreground = new SolidColorBrush(colorLine);
            tbBoby.FontSize = GridNumberGamer.ActualHeight * 60 / 100;
            tbBoby.TextAlignment = TextAlignment.Center;
            sp.Children.Add(vb);
            sp.Children.Add(tbBoby);

            var messagePrompt = new MessagePrompt
            {
                Body = sp,
                IsAppBarVisible = true,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Center,
                Opacity = 0.7
            };
            messagePrompt.Completed += MessagePrompt_Completed;
            messagePrompt.Show();
            CurrentGamerLabel.Text = String.Format(AppResources.CurrentGamer, CurrentGamer);
        }

        private void MessagePrompt_Completed(object sender, PopUpEventArgs<string, PopUpResult> e)
        {
            IsRotateStarted = false;
            GridLabelCurrentGamer.Visibility = Visibility.Visible;
        }

        private void SetDesire()
        {
            string desire;
            do
            {
                desire = Desires[random.Next(0, Desires.Count())];
            } while (Desire == desire);
            Desire = desire;
        }

        private void mediaElement_MediaEnded(object sender, RoutedEventArgs e)
        {
            if (!IsAudioStopped)
                mediaElement.Play();
        }

        private void bottleImage_Start(object sender, System.Windows.Input.GestureEventArgs e)
        {
            if (!IsRotateStarted)
            {
                IsRotateStarted = true;
                double degrees = 0;
                do
                {
                    degrees = random.Next(360, 1080);
                } while (GetNumberGamer(degrees) == CurrentGamer);
                CurrentGamer = GetNumberGamer(degrees);
                rotate(degrees);
            }
        }

        private int GetNumberGamer(double angle)
        {
            var n = Math.Floor(angle / 360);
            angle = angle - n * 360;
            int numberGamer = Convert.ToInt32(Math.Floor(angle / (360.0 / CountGamers)) + 1);
            return numberGamer;
        }

        private void DivideGrid(int countGamers)
        {
            try
            {
                double angle = 360.0 / countGamers;
                double transformAngleLine = -90;
                double transformAngleNumber = 0;
                for (int i = 0; i < countGamers; i++)
                {
                    Line myLine = new Line();
                    transformAngleLine += angle;
                    transformAngleNumber = transformAngleLine - angle / 2;
                    string nameLine = "myLine" + i.ToString();
                    myLine.Stroke = new SolidColorBrush(colorLine);
                    myLine.X1 = LayoutRoot.ActualWidth / 2;
                    myLine.X2 = LayoutRoot.ActualWidth;
                    myLine.Y1 = LayoutRoot.ActualHeight / 2;
                    myLine.Y2 = LayoutRoot.ActualHeight / 2;
                    myLine.StrokeThickness = 3;
                    myLine.SetValue(Grid.RowProperty, 0);
                    myLine.SetValue(Grid.RowSpanProperty, 2);
                    myLine.Name = nameLine;
                    RotateTransform MyTransform = new RotateTransform();
                    myLine.RenderTransform = MyTransform;
                    myLine.RenderTransformOrigin = new Point(0.5, 0.5);
                    MyTransform.Angle = transformAngleLine;
                    LayoutRoot.Children.Add(myLine);

                    TextBlock numberGamer = new TextBlock();
                    numberGamer.Name = "number" + i.ToString();
                    numberGamer.Text = (i + 1).ToString();
                    numberGamer.HorizontalAlignment = HorizontalAlignment.Center;
                    numberGamer.VerticalAlignment = VerticalAlignment.Center;
                    numberGamer.FontSize = GridNumberGamer.ActualHeight * 80 / 100;
                    numberGamer.TextWrapping = TextWrapping.NoWrap;
                    numberGamer.FontFamily = new FontFamily("Comic Sans MS");


                    Border b = new Border();
                    Border b2 = new Border();
                    b.BorderThickness = b2.BorderThickness = new Thickness(3);
                    b.CornerRadius = b2.CornerRadius = new CornerRadius(20);
                    b.BorderBrush = b2.BorderBrush = new SolidColorBrush(colorLine);
                    b.Child = numberGamer;
                    b.Height = b.Width = b2.Height = b2.Width = GridNumberGamer.ActualHeight;
                    if (GridNumberGamer.ActualHeight > GridNumberGamer.ColumnDefinitions[1].ActualWidth)
                    {
                        b.Margin = b2.Margin = new Thickness(0, 0, -GridNumberGamer.ActualHeight + GridNumberGamer.ColumnDefinitions[1].ActualWidth, 0);
                    }
                    b2.Background = new SolidColorBrush(Colors.Black);
                    b2.Opacity = 0.4;
                    var radius = LayoutRoot.ActualHeight / 2 * 84 / 100;

                    var y0 = LayoutRoot.ActualHeight / 2 * 84 / 100;
                    var х1 = (Math.Cos(Math.PI * transformAngleNumber / 180) * radius);
                    var у1 = y0 + (Math.Sin(Math.PI * transformAngleNumber / 180) * radius);
                    MatrixTransform matrixTransform = new MatrixTransform();
                    b.RenderTransform = b2.RenderTransform = matrixTransform;
                    matrixTransform.Matrix = new Matrix(1, 0, 0, 1, х1, у1);

                    b2.SetValue(Grid.ColumnProperty, 1);
                    b.SetValue(Grid.ColumnProperty, 1);
                    GridNumberGamer.Children.Add(b2);
                    GridNumberGamer.Children.Add(b);
                }

                BottleImage = new Image();
                BottleImage.Name = "bottleImage";
                BottleImage.Source = new BitmapImage(new Uri(bottleRepository.GetBottle(numberBottle).Path, UriKind.RelativeOrAbsolute));
                BottleImage.RenderTransformOrigin = new Point(0.5, 0.5);
                RotateTransform rotateBottle = new RotateTransform();
                rotateBottle.Angle = 10;
                BottleImage.RenderTransform = rotateBottle;
                BottleImage.SetValue(Grid.RowProperty, 0);
                BottleImage.SetValue(Grid.RowSpanProperty, 2);
                BottleImage.Tap += bottleImage_Start;
                BottleImage.MouseMove += bottleImage_MouseMove;
                BottleImage.Height = GridBottle.ActualHeight;
                BottleImage.HorizontalAlignment = HorizontalAlignment.Center;
                BottleImage.VerticalAlignment = VerticalAlignment.Center;
                LayoutRoot.Children.Add(BottleImage);
            }
            catch (Exception ex)
            {
                var a = ex.Message;
            }
        }

        private void q_Click(object sender, RoutedEventArgs e)
        {
            var zIndex = Canvas.GetZIndex(FindName("myLine2") as UIElement);
            var zIndex2 = Canvas.GetZIndex(FindName("bottleImage") as UIElement);
            MessageBox.Show(zIndex.ToString() + "," + zIndex2.ToString());
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            DivideGrid(Convert.ToInt32(AppSettings["countGamers"]));
        }

        private void bottleImage_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!IsRotateStarted)
            {
                IsRotateStarted = true;
                double degrees = 0;
                do
                {
                    degrees = random.Next(360, 1080);
                } while (GetNumberGamer(degrees) == CurrentGamer);
                CurrentGamer = GetNumberGamer(degrees);
                rotate(degrees);
            }
        }

        private List<string> GetListDesires()
        {
            var currentCulture = CultureInfo.CurrentCulture.ToString().Substring(0, 2);
            List<string> listText = new List<string>();
            using (var objReader = new StreamReader("Desires\\" + currentCulture.ToString() + "desire.txt", new Windows1251Encoding()))
            {
                while (!objReader.EndOfStream)
                {
                    listText.Add(objReader.ReadLine());
                }
            }
            return listText;
        }
    }
}