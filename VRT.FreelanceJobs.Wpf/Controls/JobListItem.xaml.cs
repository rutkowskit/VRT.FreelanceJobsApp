using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Navigation;
using VRT.FreelanceJobs.Wpf.Persistence.Jobs;

namespace VRT.FreelanceJobs.Wpf.Controls
{
    /// <summary>
    /// Interaction logic for JobListItem.xaml
    /// </summary>
    public partial class JobListItem : UserControl
    {
        public JobListItem()
        {
            InitializeComponent();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            if (Uri.TryCreate(e.Uri.AbsoluteUri, UriKind.Absolute, out var validUri))
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = validUri!.AbsoluteUri,
                    UseShellExecute = true
                });
            }
            e.Handled = true;
        }

        private void OnHiddenToggle(object sender, System.Windows.RoutedEventArgs e)
        {
            if (DataContext is Job job && sender is ToggleButton toggle)
            {
                job.IsDirty = true;
                job.Hidden = toggle.IsChecked.GetValueOrDefault();
            }
        }
    }
}
