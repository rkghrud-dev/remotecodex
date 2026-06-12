# RemoteCodex

RemoteCodex is a Host/Guest remote support prototype for helping another Windows PC through screenshots, click/keyboard input, PowerShell, file transfer, and later Codex/Agent AI workflows.

## Projects

- `RemoteCodex.Host`: WPF Host app with an embedded ASP.NET Core SignalR server.
- `RemoteCodex.Guest`: Windows Guest agent that connects to Host and handles screenshot, click, keyboard, and PowerShell commands.
- `RemoteCodex.Shared`: Shared message contracts.

## Development

Build:

```powershell
dotnet build RemoteCodex.sln
```

Run Host:

```powershell
dotnet run --project .\RemoteCodex.Host\RemoteCodex.Host.csproj
```

Run Guest locally:

```powershell
dotnet run --project .\RemoteCodex.Guest\RemoteCodex.Guest.csproj -- http://localhost:7777/remotehub
```

For different networks, run both PCs on the same Tailscale network and pass the Host Tailscale IP:

```powershell
RemoteCodex.Guest.exe http://100.x.x.x:7777/remotehub
```

## Publish EXE

```powershell
.\scripts\publish-host.ps1
.\scripts\publish-guest.ps1
```

Outputs:

- `artifacts\host`
- `artifacts\guest`

These are basic single-file EXE outputs, not MSI installers yet.
