using System.Windows;

namespace VRT.FreelanceJobs.Wpf;

public partial class MainWindow : Window
{
    public MainWindow(MainWindowViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        DataContext = viewModel;
    }
    public MainWindowViewModel ViewModel { get; }

    private void OnWindowLoaded(object sender, RoutedEventArgs e)
    {
        ViewModel.LoadCachedJobsCommand.Execute(null);
    }
}