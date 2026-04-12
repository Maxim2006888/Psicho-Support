using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Psicho_Support.Services
{
    public static class PageTransitionService
    {

        public static void Navigate(Frame frame, Page newPage, bool slideLeft = true)
        {
            if (frame.Content is Page oldPage)
            {
                AnimateTransition(frame, oldPage, newPage, slideLeft);
            }
            else
            {
                frame.Content = newPage;
                FadeIn(newPage);
            }
        }

        private static void AnimateTransition(Frame frame, Page oldPage, Page newPage, bool slideLeft)
        {
            double width = frame.ActualWidth;
            if (width == 0)
                width = 800;

            var oldTransform = new TranslateTransform();
            var newTransform = new TranslateTransform();

            oldPage.RenderTransform = oldTransform;
            newPage.RenderTransform = newTransform;

            newTransform.X = slideLeft ? width : -width;

            frame.Content = newPage;

            var duration = TimeSpan.FromMilliseconds(350);

            var oldAnim = new DoubleAnimation
            {
                To = slideLeft ? -width : width,
                Duration = duration,
                EasingFunction = new CubicEase
                {
                    EasingMode = EasingMode.EaseInOut
                }
            };

            var newAnim = new DoubleAnimation
            {
                To = 0,
                Duration = duration,
                EasingFunction = new CubicEase
                {
                    EasingMode = EasingMode.EaseInOut
                }
            };

            oldTransform.BeginAnimation(TranslateTransform.XProperty, oldAnim);
            newTransform.BeginAnimation(TranslateTransform.XProperty, newAnim);
        }

        private static void FadeIn(Page page)
        {
            page.Opacity = 0;

            var anim = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromMilliseconds(250)
            };

            page.BeginAnimation(UIElement.OpacityProperty, anim);
        }
    }
}