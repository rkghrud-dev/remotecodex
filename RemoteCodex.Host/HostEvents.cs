using RemoteCodex.Shared;

namespace RemoteCodex.Host;

public static class HostEvents
{
    public static event Action<string>? LogWritten;
    public static event Action? GuestListChanged;
    public static event Action<ScreenshotPayload>? ScreenshotReceived;

    public static void WriteLog(string message)
    {
        LogWritten?.Invoke($"[{DateTime.Now:HH:mm:ss}] {message}");
    }

    public static void GuestsChanged()
    {
        GuestListChanged?.Invoke();
    }

    public static void ReceiveScreenshot(ScreenshotPayload screenshot)
    {
        ScreenshotReceived?.Invoke(screenshot);
    }
}
