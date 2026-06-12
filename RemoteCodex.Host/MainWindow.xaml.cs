using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using RemoteCodex.Shared;

namespace RemoteCodex.Host;

public partial class MainWindow : Window
{
    private readonly RemoteHostServer _server = new();
    private ScreenshotPayload? _currentScreenshot;

    public MainWindow()
    {
        InitializeComponent();
        HostEvents.LogWritten += OnLogWritten;
        HostEvents.GuestListChanged += OnGuestListChanged;
        HostEvents.ScreenshotReceived += OnScreenshotReceived;
    }

    protected override async void OnClosed(EventArgs e)
    {
        HostEvents.LogWritten -= OnLogWritten;
        HostEvents.GuestListChanged -= OnGuestListChanged;
        HostEvents.ScreenshotReceived -= OnScreenshotReceived;
        await _server.StopAsync();
        base.OnClosed(e);
    }

    private async void StartButton_Click(object sender, RoutedEventArgs e)
    {
        await _server.StartAsync();
        ServerStatusText.Text = $"Server running: {_server.Url}";
        StartButton.IsEnabled = false;
        StopButton.IsEnabled = true;
    }

    private async void StopButton_Click(object sender, RoutedEventArgs e)
    {
        await _server.StopAsync();
        ServerStatusText.Text = "Server stopped";
        StartButton.IsEnabled = true;
        StopButton.IsEnabled = false;
    }

    private async void ScreenshotButton_Click(object sender, RoutedEventArgs e)
    {
        await _server.RequestScreenshotAsync();
    }

    private async void TypeButton_Click(object sender, RoutedEventArgs e)
    {
        await _server.TypeTextAsync(TypeTextBox.Text);
    }

    private async void PowerShellButton_Click(object sender, RoutedEventArgs e)
    {
        await _server.RunPowerShellAsync(PowerShellBox.Text);
    }

    private async void ScreenshotImage_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (_currentScreenshot is null)
        {
            return;
        }

        var point = e.GetPosition(ScreenshotImage);
        await _server.ClickAsync((int)point.X, (int)point.Y);
    }

    private void OnLogWritten(string message)
    {
        Dispatcher.Invoke(() =>
        {
            LogBox.AppendText(message + Environment.NewLine);
            LogBox.ScrollToEnd();
        });
    }

    private void OnGuestListChanged()
    {
        Dispatcher.Invoke(() =>
        {
            GuestsList.ItemsSource = GuestRegistry.All;
        });
    }

    private void OnScreenshotReceived(ScreenshotPayload screenshot)
    {
        Dispatcher.Invoke(() =>
        {
            _currentScreenshot = screenshot;
            ScreenshotImage.Source = DecodeImage(screenshot.Base64Image);
        });
    }

    private static BitmapImage DecodeImage(string base64Image)
    {
        var bytes = Convert.FromBase64String(base64Image);
        using var stream = new MemoryStream(bytes);
        var image = new BitmapImage();
        image.BeginInit();
        image.CacheOption = BitmapCacheOption.OnLoad;
        image.StreamSource = stream;
        image.EndInit();
        image.Freeze();
        return image;
    }
}
