using System.Threading.Tasks;
using Windows.Devices.Gpio;
using Windows.Foundation;
using Windows.System.Threading;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;

namespace CamControl
{
    public sealed partial class MainPage : Page
    {
        private readonly GpioPin _myPin;
        private IAsyncAction _action;

        public MainPage()
        {
            InitializeComponent();
            var gpio = GpioController.GetDefault();
            if (gpio == null) return;
            _myPin = gpio.OpenPin(20);
        }

        private void Pin(bool on)
        {
            if (_myPin == null) return;
            _myPin.SetDriveMode(GpioPinDriveMode.Output);
            _myPin.Write(@on ? GpioPinValue.High : GpioPinValue.Low);
        }

        private WorkItemHandler Pwm(double freq)
        {
            var delay = 1/freq*1000;
            return operation =>
            {
                while (operation.Status != AsyncStatus.Canceled)
                {
                    Pin(true);
                    Task.Delay((int) delay).Wait();
                    Pin(false);
                    Task.Delay((int) delay).Wait();
                }
            };
        }

        private void StartBlink()
        {
            if (_action != null && _action.Status == AsyncStatus.Started) return;
            _action = ThreadPool.RunAsync(Pwm(slider.Value));
        }

        private void button_Click(object sender, RoutedEventArgs e)
        {
            StartBlink();
        }

        private void button1_Click(object sender, RoutedEventArgs e)
        {
            StopBlink();
        }

        private void StopBlink()
        {
            if (_action != null && _action.Status == AsyncStatus.Started)
            {
                _action.Cancel();
            }
        }

        private void SetGauge(double value, double max)
        {
            var angle = value/max*180;
            var n = new RotateTransform {Angle = angle};
            rect1.RenderTransform = n;
        }

        private void slider_ValueChanged(object sender, RangeBaseValueChangedEventArgs e)
        {
            StopBlink();
            SetGauge(e.NewValue, slider.Maximum);
            StartBlink();
        }
    }
}