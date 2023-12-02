using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace VRT.FreelanceJobs.Wpf.Controls;

public sealed class TextBoxWithDebounce : TextBox
{
    private const int DefaultDelayTimeMilliseconds = 600;
    private readonly DispatcherTimer _timer;

    public TextBoxWithDebounce()
    {
        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(DefaultDelayTimeMilliseconds) };
        _timer.Tick += OnTimerTick;
    }

    public int DelayTimeMilliseconds
    {
        get { return (int)GetValue(DelayTimeMillisecondsProperty); }
        set { SetValue(DelayTimeMillisecondsProperty, value); }
    }

    public static readonly DependencyProperty DelayTimeMillisecondsProperty =
        DependencyProperty.Register(nameof(DelayTimeMilliseconds), typeof(int), typeof(TextBoxWithDebounce), new PropertyMetadata(DefaultDelayTimeMilliseconds));


    public string DelayedText
    {
        get { return (string)GetValue(DelayedTextProperty); }
        set { SetValue(DelayedTextProperty, value); }
    }

    public static readonly DependencyProperty DelayedTextProperty =
        DependencyProperty.Register(nameof(DelayedText), typeof(string), typeof(TextBoxWithDebounce),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

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
