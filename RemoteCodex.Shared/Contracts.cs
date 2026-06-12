namespace RemoteCodex.Shared;

public sealed record GuestRegistration(
    string MachineName,
    string UserName,
    string OperatingSystem,
    string AgentVersion);

public sealed record GuestStatus(
    string ConnectionId,
    string MachineName,
    string UserName,
    string OperatingSystem,
    string AgentVersion,
    DateTimeOffset ConnectedAt);

public sealed record PowerShellResult(
    string Command,
    int ExitCode,
    string StandardOutput,
    string StandardError);

public sealed record ScreenshotPayload(
    string ContentType,
    string Base64Image,
    int Width,
    int Height,
    DateTimeOffset CapturedAt);

public sealed record ClickRequest(int X, int Y);

public sealed record TypeTextRequest(string Text);
