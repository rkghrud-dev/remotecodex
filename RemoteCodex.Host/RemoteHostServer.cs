using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RemoteCodex.Shared;

namespace RemoteCodex.Host;

public sealed class RemoteHostServer
{
    private WebApplication? _app;

    public bool IsRunning => _app is not null;

    public string Url { get; }

    public RemoteHostServer(string url = "http://0.0.0.0:7777")
    {
        Url = url;
    }

    public async Task StartAsync()
    {
        if (_app is not null)
        {
            return;
        }

        var builder = WebApplication.CreateBuilder();
        builder.WebHost.UseUrls(Url);
        builder.Services.AddSignalR();

        _app = builder.Build();
        _app.MapGet("/", () => "RemoteCodex Host is running.");
        _app.MapHub<RemoteHub>("/remotehub");

        await _app.StartAsync();
        HostEvents.WriteLog($"Host server started: {Url}");
    }

    public async Task StopAsync()
    {
        if (_app is null)
        {
            return;
        }

        await _app.StopAsync();
        await _app.DisposeAsync();
        _app = null;
        HostEvents.WriteLog("Host server stopped.");
    }

    public Task RequestScreenshotAsync()
    {
        return SendToFirstGuestAsync("RequestScreenshot");
    }

    public Task RunPowerShellAsync(string command)
    {
        return SendToFirstGuestAsync("RunPowerShell", command);
    }

    public Task ClickAsync(int x, int y)
    {
        return SendToFirstGuestAsync("Click", new ClickRequest(x, y));
    }

    public Task TypeTextAsync(string text)
    {
        return SendToFirstGuestAsync("TypeText", new TypeTextRequest(text));
    }

    private async Task SendToFirstGuestAsync(string method, object? arg = null)
    {
        if (_app is null)
        {
            HostEvents.WriteLog("Host server is not running.");
            return;
        }

        var connectionId = GuestRegistry.FirstConnectionId;
        if (connectionId is null)
        {
            HostEvents.WriteLog("No guest is connected.");
            return;
        }

        var hub = _app.Services.GetRequiredService<IHubContext<RemoteHub>>();
        if (arg is null)
        {
            await hub.Clients.Client(connectionId).SendAsync(method);
        }
        else
        {
            await hub.Clients.Client(connectionId).SendAsync(method, arg);
        }
    }
}
