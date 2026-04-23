using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace UareU.Views
{
    public partial class LoaderView : UserControl
    {
        Storyboard spinStoryboard;
        Storyboard showStoryboard;
        Storyboard hideStoryboard;

        RotateTransform SpinnerRotate;

        public LoaderView()
        {
            InitializeComponent();

            SpinnerRotate = new RotateTransform();
            SpinnerEllipse.RenderTransform = SpinnerRotate;

            spinStoryboard = (Storyboard)Resources["LocalSpin"];
            showStoryboard = (Storyboard)Application.Current.Resources["Loader.Show"];
            hideStoryboard = (Storyboard)Application.Current.Resources["Loader.Hide"];


        }

        public void Show()
        {
            Root.Visibility = Visibility.Visible;

            spinStoryboard.Begin();   // no targeting needed anymore

            var show = showStoryboard.Clone();
            Storyboard.SetTarget(show.Children[0], LoaderCard);
            Storyboard.SetTarget(show.Children[1], CardScale);
            Storyboard.SetTarget(show.Children[2], CardScale);

            show.Begin(this, true);
        }

        public void Hide()
        {
            var hide = hideStoryboard.Clone();
            Storyboard.SetTarget(hide.Children[0], LoaderCard);

            hide.Completed += (s, e) =>
            {
                Root.Visibility = Visibility.Collapsed;
            };

            // ⭐ IMPORTANT
            hide.Begin(this, true);
        }
    }
}