// Controls/StateIndicatorControl.xaml.cs
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Psicho_Support.Controls
{
    public partial class StateIndicatorControl : UserControl
    {
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(int), typeof(StateIndicatorControl),
                new PropertyMetadata(75, new PropertyChangedCallback(OnValueChanged)));

        public static readonly DependencyProperty StateLevelProperty =
            DependencyProperty.Register("StateLevel", typeof(string), typeof(StateIndicatorControl),
                new PropertyMetadata("Стабильное"));

        public static readonly DependencyProperty StateColorProperty =
            DependencyProperty.Register("StateColor", typeof(Brush), typeof(StateIndicatorControl),
                new PropertyMetadata(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#4CAF50"))));

        public static readonly DependencyProperty IsPulsingProperty =
            DependencyProperty.Register("IsPulsing", typeof(bool), typeof(StateIndicatorControl),
                new PropertyMetadata(true, new PropertyChangedCallback(OnIsPulsingChanged)));

        public int Value
        {
            get { return (int)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        public string StateLevel
        {
            get { return (string)GetValue(StateLevelProperty); }
            set { SetValue(StateLevelProperty, value); }
        }

        public Brush StateColor
        {
            get { return (Brush)GetValue(StateColorProperty); }
            set { SetValue(StateColorProperty, value); }
        }

        public bool IsPulsing
        {
            get { return (bool)GetValue(IsPulsingProperty); }
            set { SetValue(IsPulsingProperty, value); }
        }

        public StateIndicatorControl()
        {
            InitializeComponent();
            UpdateDisplay();
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            StateIndicatorControl control = (StateIndicatorControl)d;
            control.UpdateDisplay();
        }

        private static void OnIsPulsingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            StateIndicatorControl control = (StateIndicatorControl)d;
            control.UpdatePulsing();
        }

        private void UpdateDisplay()
        {
            if (ValueText == null) return;

            ValueText.Text = Value + "%";

            // Обновляем эмодзи на основе значения
            if (Value >= 80)
                StateEmoji.Text = "😊";
            else if (Value >= 60)
                StateEmoji.Text = "🙂";
            else if (Value >= 40)
                StateEmoji.Text = "😐";
            else if (Value >= 20)
                StateEmoji.Text = "😟";
            else
                StateEmoji.Text = "😢";

            // Обновляем цвета
            if (Indicator != null)
                Indicator.Background = StateColor;
            if (PulseBorder != null)
                PulseBorder.BorderBrush = StateColor;
            if (GlowEffect != null)
                GlowEffect.Background = StateColor;

            UpdatePulsing();
        }

        private void UpdatePulsing()
        {
            if (PulseBorder == null) return;

            Storyboard storyboard = (Storyboard)FindResource("PulseStoryboard");

            if (IsPulsing)
            {
                storyboard.Begin(this, true);
            }
            else
            {
                storyboard.Stop(this);
                // Сбрасываем трансформации
                PulseBorder.RenderTransform = new ScaleTransform(1, 1);
                if (GlowEffect != null)
                {
                    GlowEffect.Opacity = 0.5;
                    if (GlowEffect.Effect is System.Windows.Media.Effects.BlurEffect)
                    {
                        var blur = (System.Windows.Media.Effects.BlurEffect)GlowEffect.Effect;
                        blur.Radius = 12;
                    }
                }
            }
        }
    }
}