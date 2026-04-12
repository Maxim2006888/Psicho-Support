// Services/NavigationService.cs (модифицируем)
using Microsoft.Extensions.DependencyInjection;
using Psicho_Support.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Psicho_Support.Services
{
    public class NavigationService : INavigationService
    {
        private ContentControl _contentControl;
        private readonly Stack<Type> _navigationStack = new Stack<Type>();
        private bool _isMockMode;

        // Конструктор без параметров для DI
        public NavigationService()
        {
            _isMockMode = true;
            System.Diagnostics.Debug.WriteLine("NavigationService created in mock mode");
        }

        // Инициализация после создания окна
        public void Initialize(ContentControl contentControl)
        {
            if (contentControl == null)
                throw new ArgumentNullException(nameof(contentControl));

            _contentControl = contentControl;
            _isMockMode = false;
            System.Diagnostics.Debug.WriteLine("NavigationService initialized with ContentControl");
        }

        public void Navigate(UserControl newPage)
        {
            if (_isMockMode || _contentControl == null)
            {
                System.Diagnostics.Debug.WriteLine("Navigation skipped: mock mode or null ContentControl");
                return;
            }

            if (newPage == null)
                return;

            if (!_contentControl.Dispatcher.CheckAccess())
            {
                _contentControl.Dispatcher.Invoke(() => Navigate(newPage));
                return;
            }

            if (_contentControl.Content != null)
            {
                _navigationStack.Push(_contentControl.Content.GetType());
            }

            var oldPage = _contentControl.Content as UserControl;

            if (oldPage == null)
            {
                _contentControl.Content = newPage;
                AnimateFadeIn(newPage);
                return;
            }

            AnimateFadeOut(oldPage, () =>
            {
                _contentControl.Content = newPage;
                AnimateFadeIn(newPage);
            });
        }

        public void NavigateTo<T>() where T : UserControl, new()
        {
            if (_isMockMode || _contentControl == null)
            {
                System.Diagnostics.Debug.WriteLine($"NavigateTo<{typeof(T).Name}> skipped: mock mode");
                return;
            }

            var page = new T();
            Navigate(page);
        }

        public void GoBack()
        {
            if (_isMockMode || _contentControl == null)
            {
                System.Diagnostics.Debug.WriteLine("GoBack skipped: mock mode");
                return;
            }

            if (_navigationStack.Count > 0)
            {
                var previousPageType = _navigationStack.Pop();
                var page = (UserControl)Activator.CreateInstance(previousPageType);
                Navigate(page);
            }
        }

        private void AnimateFadeOut(UserControl page, Action completed)
        {
            if (page == null)
            {
                completed?.Invoke();
                return;
            }

            var fadeOut = new DoubleAnimation
            {
                From = 1,
                To = 0,
                Duration = TimeSpan.FromMilliseconds(200)
            };

            fadeOut.Completed += (s, e) => completed?.Invoke();
            page.BeginAnimation(UserControl.OpacityProperty, fadeOut);
        }

        private void AnimateFadeIn(UserControl page)
        {
            if (page == null) return;

            var transform = new TranslateTransform { X = 30 };
            page.RenderTransform = transform;
            page.Opacity = 0;

            var fade = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(250));
            var slide = new DoubleAnimation(30, 0, TimeSpan.FromMilliseconds(250));

            page.BeginAnimation(UserControl.OpacityProperty, fade);
            transform.BeginAnimation(TranslateTransform.XProperty, slide);
        }

        public void SwitchToWindow<TWindow>() where TWindow : Window
        {
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                // ✅ Создаём новое окно через DI
                var window = App.Services.GetRequiredService<TWindow>();
                window.Show();

                // Закрываем текущее окно
                foreach (Window w in System.Windows.Application.Current.Windows)
                {
                    if (w != window && w.IsVisible && w != System.Windows.Application.Current.MainWindow)
                    {
                        w.Close();
                        break;
                    }
                }
            });
        }
    }
}