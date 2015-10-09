using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace AnimateListViewDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            
            MyListView.ItemsSource = Enumerable.Range(1, 10000);
            NavigateToListViewItemIndex(700);

            SecondListView.ItemsSource = Enumerable.Range(1, 10000); ;
        }

        private void GoToItem1_OnClick(object sender, RoutedEventArgs e)
        {
            NavigateToListViewItemIndex(1);
        }

        private void GoToItem100_OnClick(object sender, RoutedEventArgs e)
        {
            NavigateToListViewItemIndex(100);
        }

        private void GoToItem700_OnClick(object sender, RoutedEventArgs e)
        {
            NavigateToListViewItemIndex(700);
        }

        private void NavigateToListViewItemIndex(int listViewItemIndex)
        {
            MyListView.SelectedIndex = listViewItemIndex - 1;
            //MyListView.ScrollIntoView(MyListView.Items[0]);
            MyListView.ScrollIntoView(MyListView.SelectedItem);
            //item.Focus();
        }

        private void GoToSelectedItem(object sender, RoutedEventArgs e)
        {
            //MyListView.ScrollIntoView(MyListView.Items[0]);
            MyListView.ScrollIntoView(MyListView.SelectedItem);
        }

        private void GoLeftButton_OnClick(object sender, RoutedEventArgs e)
        {
            //TranslateTransform trans = new TranslateTransform();
            //image.RenderTransform = trans;

            //Point leftePoint = image.TransformToAncestor(GridWithListViews)
            //              .Transform(new Point(0, 0));

            //Point rightPoint = RightContentControl.TransformToAncestor(GridWithListViews)
            //    .Transform(new Point(0, 0));

            //DoubleAnimation animationX= new DoubleAnimation(leftePoint.X, rightPoint.X - leftePoint.X, TimeSpan.FromSeconds(10));
            ////DoubleAnimation animationY = new DoubleAnimation(leftePoint.Y, rightPoint.Y - leftePoint.Y, TimeSpan.FromSeconds(10));
            //trans.BeginAnimation(TranslateTransform.XProperty, animationX);
            ////trans.BeginAnimation(TranslateTransform.YProperty, animationY);
        }
        
        private void GoLRightButton_OnClick(object sender, RoutedEventArgs e)
        {
            _image = CreateImageFromControl(MyListView);

            ImitateLeftViewOnRightView();
            AnimateNavigationToRight();
        }

        #region ImitateLeftViewOnRightView

        private void ImitateLeftViewOnRightView()
        {
            SecondListView.SelectedIndex = MyListView.SelectedIndex;
            //SecondListView.ScrollIntoView(MyListView.SelectedItem);

            var leftScrollViewer = FindVisualChildren<ScrollViewer>(MyListView)
                .First();

            var rightScrollViewer = FindVisualChildren<ScrollViewer>(SecondListView)
                .First();

            rightScrollViewer.ScrollToVerticalOffset(leftScrollViewer.VerticalOffset);

            SecondListView.Visibility = Visibility.Hidden;
        }

        public static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            if (depObj != null)
            {
                for (int i = 0; i < VisualTreeHelper.GetChildrenCount(depObj); i++)
                {
                    DependencyObject child = VisualTreeHelper.GetChild(depObj, i);
                    if (child != null && child is T)
                    {
                        yield return (T)child;
                    }

                    foreach (T childOfChild in FindVisualChildren<T>(child))
                    {
                        yield return childOfChild;
                    }
                }
            }
        }
        #endregion ImitateLeftViewOnRightView

        #region AnimateNavigationToRight

        private Image _image;
        private void AnimateNavigationToRight()
        {
            var timeSpanForAllAnimations = new TimeSpan(0, 0, 0, 0, 700);

            //Image movement
            Grid.SetRow(_image, 0);
            Grid.SetColumn(_image, 0);
            GridWithListViews.Children.Add(_image);

            TranslateTransform translateTransform = new TranslateTransform();
            _image.RenderTransform = translateTransform;

            Point rightPoint = SecondListView.TransformToAncestor(GridWithListViews)
                .Transform(new Point(0, 0));

            var translateTransformKeyFrame =
                new EasingDoubleKeyFrame(rightPoint.X, KeyTime.FromTimeSpan(timeSpanForAllAnimations));
            //KeyTime.FromTimeSpan(new TimeSpan(0, 0, 0, 0, 700)), new CircleEase() {EasingMode = EasingMode.EaseIn});

            var doubleAnimation = new DoubleAnimationUsingKeyFrames();
            doubleAnimation.KeyFrames.Add(translateTransformKeyFrame);
            var storyBoard = new Storyboard();
            Storyboard.SetTarget(doubleAnimation, _image);
            Storyboard.SetTargetProperty(doubleAnimation, new PropertyPath("RenderTransform.(TranslateTransform.X)"));
            storyBoard.Children.Add(doubleAnimation);

            //Visibility
            var visiblityTransformKeyFrameSecond =
                new DiscreteObjectKeyFrame(Visibility.Visible, KeyTime.FromTimeSpan(timeSpanForAllAnimations));

            var objectAnimation = new ObjectAnimationUsingKeyFrames();
            objectAnimation.KeyFrames.Add(visiblityTransformKeyFrameSecond);

            Storyboard.SetTarget(objectAnimation, SecondListView);
            Storyboard.SetTargetProperty(objectAnimation, new PropertyPath("UIElement.Visibility"));

            storyBoard.Completed += StoryBoard_Completed;
            storyBoard.Begin();
        }

        private void StoryBoard_Completed(object sender, EventArgs e)
        {
            GridWithListViews.Children.Remove(_image);

            SecondListView.Visibility = Visibility.Visible;
        }

        private Image CreateImageFromControl(Control control)
        {
            using (var stream = new MemoryStream())
            {
                RenderTargetBitmap rtb = new RenderTargetBitmap((int)MyListView.ActualWidth, (int)MyListView.ActualHeight, 96, 96, PixelFormats.Pbgra32);
                rtb.Render(MyListView);

                PngBitmapEncoder png = new PngBitmapEncoder();
                png.Frames.Add(BitmapFrame.Create(rtb));
                png.Save(stream);

                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = stream;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();

                var image = new Image();
                image.Source = LoadImage(stream.GetBuffer());
                return image;
            }
        }

        BitmapSource LoadImage(Byte[] imageData)
        {
            using (MemoryStream ms = new MemoryStream(imageData))
            {
                var decoder = BitmapDecoder.Create(ms,
                    BitmapCreateOptions.PreservePixelFormat, BitmapCacheOption.OnLoad);
                return decoder.Frames[0];
            }
        }
        #endregion AnimateNavigationToRight
    }

    //public class ScrollPreserver : DependencyObject
    //{
    //    public static readonly DependencyProperty PreserveScrollProperty =
    //        DependencyProperty.RegisterAttached("PreserveScroll",
    //            typeof(bool),
    //            typeof(ScrollPreserver),
    //            new PropertyMetadata(new PropertyChangedCallback(OnScrollGroupChanged)));

    //    public static bool GetPreserveScroll(DependencyObject invoker)
    //    {
    //        return (bool)invoker.GetValue(PreserveScrollProperty);
    //    }

    //    public static void SetPreserveScroll(DependencyObject invoker, bool value)
    //    {
    //        invoker.SetValue(PreserveScrollProperty, value);
    //    }

    //    private static Dictionary<ScrollViewer, bool> scrollViewers_States =
    //    new Dictionary<ScrollViewer, bool>();

    //    private static void OnScrollGroupChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    //    {
    //        ScrollViewer scrollViewer = d as ScrollViewer;
    //        if (scrollViewer != null && (bool)e.NewValue == true)
    //        {
    //            if (!scrollViewers_States.ContainsKey(scrollViewer))
    //            {
    //                scrollViewer.ScrollChanged += new ScrollChangedEventHandler(scrollViewer_ScrollChanged);
    //                scrollViewers_States.Add(scrollViewer, false);
    //            }
    //        }
    //    }

    //    static void scrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
    //    {
    //        if (scrollViewers_States[sender as ScrollViewer])
    //            (sender as ScrollViewer).ScrollToVerticalOffset(e.VerticalOffset + e.ExtentHeightChange);

    //        scrollViewers_States[sender as ScrollViewer] = e.VerticalOffset != 0;
    //    }
    //}
}
