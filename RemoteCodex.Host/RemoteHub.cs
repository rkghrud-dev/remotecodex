using Microsoft.AspNetCore.SignalR;
using RemoteCodex.Shared;

namespace RemoteCodex.Host;

public sealed class RemoteHub : Hub
{
    public override Task OnDisconnectedAsync(Exception? exception)
    {
        GuestRegistry.Remove(Context.ConnectionId);
        HostEvents.WriteLog($"Guest disconnected: {Context.ConnectionId}");
        HostEvents.GuestsChanged();
        return base.OnDisconnectedAsync(exception);
    }

    public Task RegisterGuest(GuestRegistration registration)
    {
        var guest = new GuestStatus(
            Context.ConnectionId,
            registration.MachineName,
            registration.UserName,
            registration.OperatingSystem,
            registration.AgentVersion,
            DateTimeOffset.Now);

        GuestRegistry.Upsert(guest);
        HostEvents.WriteLog($"Guest connected: {guest.MachineName} ({guest.UserName})");
        HostEvents.GuestsChanged();
        return Task.CompletedTask;
    }

    public Task ReportPowerShellResult(PowerShellResult result)
    {
        HostEvents.WriteLog($"PowerShell result [{result.ExitCode}] {result.Command}");
        if (!string.IsNullOrWhiteSpace(result.StandardOutput))
        {
            HostEvents.WriteLog(result.StandardOutput.Trim());
        }

        if (!string.IsNullOrWhiteSpace(result.StandardError))
        {
            HostEvents.WriteLog(result.StandardError.Trim());
        }

        return Task.CompletedTask;
    }

    public Task ReportScreenshot(ScreenshotPayload screenshot)
    {
        HostEvents.ReceiveScreenshot(screenshot);
        HostEvents.WriteLog($"Screenshot received: {screenshot.Width}x{screenshot.Height}");
        return Task.CompletedTask;
    }
}
