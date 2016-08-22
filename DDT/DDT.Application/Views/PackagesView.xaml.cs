
using System.Windows;
using System.Windows.Controls;
using DDT.Application.Extensions;
using DDT.Application.Helpers;
using DDT.Application.Transitions;
using System.Windows.Media.Animation;
using System;

namespace DDT.Application.Views
{
    public partial class PackagesView
    {
        public PackagesView()
        {
            InitializeComponent();

            SizeChanged += PackagesView_SizeChanged;
        }

        private double _buttonsWidth;

        private void PackagesView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            _buttonsWidth = ((Border)App.Current.MainWindow.Content).ActualWidth - 36;

            if (b1.Width > 0)
                Expand(b1, true);

            if (b2.Width > 0)
                Expand(b2, true);
        }

        private void OpenFileDialog(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is Button)
            {
                var button = e.OriginalSource as Button;
                if (button.Name == "close")
                {
                    (sender as Button).Tag = null;
                    Collapse(b1);
                    return;
                }
            }

            OpenFileDialog(sender as Button, b1);
        }

        private void OpenFileDialog2(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is Button)
            {
                var button = e.OriginalSource as Button;
                if (button.Name == "close")
                {
                    (sender as Button).Tag = null;
                    Collapse(b2);
                    return;
                }
            }

            OpenFileDialog(sender as Button, b2);
        }

        private void OpenFileDialog3(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is Button)
            {
                var button = e.OriginalSource as Button;
                if (button.Name == "close")
                {
                    (sender as Button).Tag = null;
                    return;
                }
            }

            string size;
            var file = DialogHelper.Instance.OpenFileDialog("Package Files (*.mst) |*.mst", out size);
            if (!string.IsNullOrEmpty(file))
            {
                (sender as Button).Tag = $"[{size}] {file}";
            }
        }

        private void OpenFileDialog(Button button, Button b)
        {
            string size;
            var file = DialogHelper.Instance.OpenFileDialog("Package Files (*.exe, *.msi) |*.exe;*.msi", out size);
            if (!string.IsNullOrEmpty(file))
            {
                button.Tag = $"[{size}] {file}";

                if (file.ToLower().EndsWith("msi"))
                    Expand(b);
                else
                    Collapse(b);
            }
        }

        private void Expand(Button b, bool ignoreExpanded = false)
        {
            if (b.Width > 0 && !ignoreExpanded)
                return;

            b.Visibility = Visibility.Visible;
            b.Tag = null;

            EasingDoubleKeyFrame frame = new EasingDoubleKeyFrame(_buttonsWidth / 2 - 2, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(ignoreExpanded || TransitionManager.CurrentStrategy is NoneTransitionStrategy ? 0 : 0.5)), new CubicEase());
            EasingDoubleKeyFrame frame2 = new EasingDoubleKeyFrame(1d, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(ignoreExpanded || TransitionManager.CurrentStrategy is NoneTransitionStrategy ? 0 : 0.5)), new CubicEase());
            DoubleAnimationUsingKeyFrames widthAnimation = new DoubleAnimationUsingKeyFrames();
            widthAnimation.KeyFrames.Add(frame);
            Storyboard.SetTarget(widthAnimation, b);
            Storyboard.SetTargetProperty(widthAnimation, new PropertyPath("Width"));
            DoubleAnimationUsingKeyFrames opacityAnimation = new DoubleAnimationUsingKeyFrames();
            opacityAnimation.KeyFrames.Add(frame2);
            Storyboard.SetTarget(opacityAnimation, b);
            Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath("Opacity"));

            Storyboard s = new Storyboard();
            s.Children.Add(widthAnimation);
            s.Children.Add(opacityAnimation);

            s.Completed += (o, args) => b.Width = _buttonsWidth / 2 - 2;
            s.Begin();
        }

        private void Collapse(Button b)
        {
            if (b.Width == 0)
                return;

            b.Tag = null;

            EasingDoubleKeyFrame frame = new EasingDoubleKeyFrame(0d, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(TransitionManager.CurrentStrategy is NoneTransitionStrategy ? 0 : 0.5)), new CubicEase());
            DoubleAnimationUsingKeyFrames widthAnimation = new DoubleAnimationUsingKeyFrames();
            widthAnimation.KeyFrames.Add(frame);
            Storyboard.SetTarget(widthAnimation, b);
            Storyboard.SetTargetProperty(widthAnimation, new PropertyPath("Width"));
            DoubleAnimationUsingKeyFrames opacityAnimation = new DoubleAnimationUsingKeyFrames();
            opacityAnimation.KeyFrames.Add(frame);
            Storyboard.SetTarget(opacityAnimation, b);
            Storyboard.SetTargetProperty(opacityAnimation, new PropertyPath("Opacity"));

            Storyboard s = new Storyboard();
            s.Children.Add(widthAnimation);
            s.Children.Add(opacityAnimation);

            s.Completed += (o, args) => b.Width = 0;
            s.Begin();
        }

        private void ParametersCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            Parameters.DelayedFocus();
        }

        private void Parameters64Checkbox_Checked(object sender, RoutedEventArgs e)
        {
            Parameters64.DelayedFocus();
        }

        private void SeparateCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            if (TransitionManager.CurrentStrategy is NoneTransitionStrategy)
                VisualStateManager.GoToElementState(grid1, "Down2", false);
            else
                VisualStateManager.GoToElementState(grid1, "Down", false);
        }

        private void SeparateCheckbox_Unchecked(object sender, RoutedEventArgs e)
        {
            if (TransitionManager.CurrentStrategy is NoneTransitionStrategy)
                VisualStateManager.GoToElementState(grid1, "Up2", false);
            else
                VisualStateManager.GoToElementState(grid1, "Up", false);
        }
    }
}