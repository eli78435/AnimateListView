using System;
using System.Collections;
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
    public partial class MainWindow : Window
    {
        public static string MoveNextRealListViewStartOffsetKeyFrameKey = "MoveNext_RealListViewStartOffsetKeyFrame";
        public static string MoveNextDummyImageViewEndOffsetKeyFrameKey = "MoveNext_DummyImageViewEndOffsetKeyFrame";

        public static string GoNext3To3StoryboardKey = "GoNext3To3Storyboard"; 
        public static string GoNext3To2StoryboardKey = "GoNext3To2Storyboard"; 
        public static string GoNext2To2StoryboardKey = "GoNext3To2Storyboard";
        public static string GoNext2To3StoryboardKey = "GoNext2To3Storyboard";

        public static string Layout1ListViewsStoryBoardKey = "Layout1ListViewsStoryBoard";
        public static string Layout2ListViewsStoryBoardKey = "Layout2ListViewsStoryBoard";
        public static string Layout3ListViewsStoryBoardKey = "Layout3ListViewsStoryBoard";

        public const int ImageOnTopZIndex = 120;
        public const int ImageRegularZIndex = 1;

        public MainWindow()
        {
            InitializeComponent();
            
            SourceWatchControl.ItemsSource = Enumerable.Range(1, 10000);
            NavigateToListViewItemIndex(700);

            ProjectionWatchControl.ItemsSource = Enumerable.Range(1, 10000);
            ProjectionWatchControl.SelectedIndex = 2;
            ProjectionWatchControl.ScrollIntoView(ProjectionWatchControl.SelectedItem);

            DestinationeWatchControl.ItemsSource = Enumerable.Range(1, 10000);
            DestinationeWatchControl.SelectedIndex = 999;
            DestinationeWatchControl.ScrollIntoView(DestinationeWatchControl.SelectedItem);
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
            SourceWatchControl.SelectedIndex = listViewItemIndex - 1;
            //MyListView.ScrollIntoView(MyListView.Items[0]);
            SourceWatchControl.ScrollIntoView(SourceWatchControl.SelectedItem);
            //item.Focus();
        }

        private void GoToSelectedItem_OnClick(object sender, RoutedEventArgs e)
        {
            //MyListView.ScrollIntoView(MyListView.Items[0]);
            SourceWatchControl.ScrollIntoView(SourceWatchControl.SelectedItem);
        }

        private void GoLRightButton_OnClick(object sender, RoutedEventArgs e)
        {
            _image = CreateImageFromControl(GridWithListViews);

            ImitateLeftViewOnRightView(SourceWatchControl, DestinationeWatchControl);
            AnimateNavigationToRight();
        }

        private void ScrollRight3To3_OnClick(object sender, RoutedEventArgs e)
        {
            ShowScreanshot();

            SetGridLayout(Layout3ListViewsStoryBoardKey);
            RunMoveNextNavigation(SourceWatchControl, DestinationeWatchControl, GoNext3To3StoryboardKey);
        }

        private void ScrollRight3To2_OnClick(object sender, RoutedEventArgs e)
        {
            ShowScreanshot();

            SetGridLayout(Layout2ListViewsStoryBoardKey);
            RunMoveNextNavigation(SourceWatchControl, DestinationeWatchControl, GoNext3To2StoryboardKey);
        }

        private void ScrollRight2To2_OnClick(object sender, RoutedEventArgs e)
        {
            ShowScreanshot();

            SetGridLayout(Layout2ListViewsStoryBoardKey);
            RunMoveNextNavigation(SourceWatchControl, DestinationeWatchControl, GoNext2To2StoryboardKey);
        }

        private void ScrollRight2To3_OnClick(object sender, RoutedEventArgs e)
        {
            ShowScreanshot();

            SetGridLayout(Layout3ListViewsStoryBoardKey);
            RunMoveNextNavigation(SourceWatchControl, DestinationeWatchControl, GoNext2To3StoryboardKey);
        }

        private void ShowScreanshot()
        {
            FillPlaceHolderImage();

            PlaceHolderImage.Visibility = Visibility.Visible;
            Panel.SetZIndex(PlaceHolderImage, ImageOnTopZIndex);
        }

        private void SetGridLayout(string layoutStoryBoardName)
        {
            var storyBoard = ((Storyboard)Resources[layoutStoryBoardName]);
            storyBoard.Begin();
        }

        private void RunMoveNextNavigation(ListView leftView, ListView rightView, string storyBoardName)
        {
            var offset = GetListViewOffset();

            ((EasingDoubleKeyFrame)Resources[MoveNextRealListViewStartOffsetKeyFrameKey]).Value = offset;
            ((EasingDoubleKeyFrame)Resources[MoveNextDummyImageViewEndOffsetKeyFrameKey]).Value = -offset;

            ImitateRightViewOnLeftView(leftView, rightView);

            var storyBoard = ((Storyboard)Resources[storyBoardName]);
            storyBoard.Begin();
        }

        private void FillPlaceHolderImage()
        {
            using (var stream = new MemoryStream())
            {
                RenderTargetBitmap rtb = new RenderTargetBitmap((int)GridWithListViews.ActualWidth, (int)GridWithListViews.ActualHeight, 96, 96, PixelFormats.Pbgra32);
                rtb.Render(GridWithListViews);

                PngBitmapEncoder png = new PngBitmapEncoder();
                png.Frames.Add(BitmapFrame.Create(rtb));
                png.Save(stream);

                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.StreamSource = stream;
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();

                PlaceHolderImage.Source = LoadImage(stream.GetBuffer());
            }
        }

        private double GetListViewOffset()
        {
            Point firstViewPosition = SourceWatchControl.TransformToAncestor(GridWithListViews)
                .Transform(new Point(0, 0));

            Point thierdViewPosition = DestinationeWatchControl.TransformToAncestor(GridWithListViews)
                .Transform(new Point(0, 0));

            return thierdViewPosition.X - firstViewPosition.X;
        }

        #region ImitateLeftViewOnRightView

        private static void ImitateLeftViewOnRightView(ListView leftView, ListView rightView)
        {
            rightView.SelectedIndex = leftView.SelectedIndex;

            var leftScrollViewer = FindVisualChildren<ScrollViewer>(leftView)
                .First();

            var rightScrollViewer = FindVisualChildren<ScrollViewer>(rightView)
                .First();

            rightScrollViewer.ScrollToVerticalOffset(leftScrollViewer.VerticalOffset);
        }

        private static void ImitateRightViewOnLeftView(ListView leftView, ListView rightView)
        {
            leftView.SelectedIndex = rightView.SelectedIndex;

            var leftScrollViewer = FindVisualChildren<ScrollViewer>(leftView)
                .First();

            var rightScrollViewer = FindVisualChildren<ScrollViewer>(rightView)
                .First();

            leftScrollViewer.ScrollToVerticalOffset(rightScrollViewer.VerticalOffset);
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

            //Add Image
            Grid.SetRow(_image, 0);
            Grid.SetColumn(_image, 0);
            GridWithListViews.Children.Add(_image);

            TranslateTransform translateTransform = new TranslateTransform();
            _image.RenderTransform = translateTransform;

            Point rightPoint = ProjectionWatchControl.TransformToAncestor(GridWithListViews)
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

            Storyboard.SetTarget(objectAnimation, ProjectionWatchControl);
            Storyboard.SetTargetProperty(objectAnimation, new PropertyPath("UIElement.Visibility"));

            storyBoard.Completed += StoryBoard_Completed;
            storyBoard.Begin();
        }

        private void StoryBoard_Completed(object sender, EventArgs e)
        {
            GridWithListViews.Children.Remove(_image);

            ProjectionWatchControl.Visibility = Visibility.Visible;
        }

        private static Image CreateImageFromControl(FrameworkElement frameworkElement)
        {
            using (var stream = new MemoryStream())
            {
                RenderTargetBitmap rtb = new RenderTargetBitmap((int)frameworkElement.ActualWidth, (int)frameworkElement.ActualHeight, 96, 96, PixelFormats.Pbgra32);
                rtb.Render(frameworkElement);

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

        private static BitmapSource LoadImage(Byte[] imageData)
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
}
