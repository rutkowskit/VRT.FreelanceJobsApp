using System.Windows.Controls;
using System.Windows.Threading;

namespace VRT.FreelanceJobs.Wpf.Controls;

[DependencyPropertyGenerator.DependencyProperty<int>("DelayTimeMilliseconds", DefaultValue = DefaultDelayTimeMilliseconds)]
[DependencyPropertyGenerator.DependencyProperty<string>("DelayedText", DefaultBindingMode = DependencyPropertyGenerator.DefaultBindingMode.TwoWay)]
public sealed partial class TextBoxWithDebounce : TextBox
{
    private const int DefaultDelayTimeMilliseconds = 600;
    private readonly DispatcherTimer _timer;

    public TextBoxWithDebounce()
    {
        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(DefaultDelayTimeMilliseconds) };
        _timer.Tick += OnTimerTick;
    }
    protected override void OnTextChanged(TextChangedEventArgs e)
    {
        _timer.Stop();
        base.OnTextChanged(e);
        _timer.Interval = TimeSpan.FromMilliseconds(DelayTimeMilliseconds);
        _timer.Start();
    }

    private void OnTimerTick(object? sender, EventArgs e)
    {
        _timer.Stop();
        DelayedText = Text;
    }
}
