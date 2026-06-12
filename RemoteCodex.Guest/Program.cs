using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;
using Microsoft.AspNetCore.SignalR.Client;
using RemoteCodex.Shared;

var hostUrl = args.FirstOrDefault() ?? "http://localhost:7777/remotehub";

Console.WriteLine($"RemoteCodex Guest starting.");
Console.WriteLine($"Host: {hostUrl}");

var connection = new HubConnectionBuilder()
    .WithUrl(hostUrl)
    .WithAutomaticReconnect()
    .Build();

connection.On("RequestScreenshot", async () =>
{
    Console.WriteLine("Screenshot requested.");
    var screenshot = ScreenshotService.CapturePrimaryScreen();
    await connection.InvokeAsync("ReportScreenshot", screenshot);
});

connection.On<string>("RunPowerShell", async command =>
{
    Console.WriteLine($"PowerShell requested: {command}");
    var result = await PowerShellService.RunAsync(command);
    await connection.InvokeAsync("ReportPowerShellResult", result);
});

connection.On<ClickRequest>("Click", request =>
{
    Console.WriteLine($"Click requested: {request.X}, {request.Y}");
    InputService.LeftClick(request.X, request.Y);
});

connection.On<TypeTextRequest>("TypeText", request =>
{
    Console.WriteLine($"Type requested: {request.Text}");
    InputService.TypeText(request.Text);
});

await connection.StartAsync();
await connection.InvokeAsync("RegisterGuest", new GuestRegistration(
    Environment.MachineName,
    Environment.UserName,
    Environment.OSVersion.ToString(),
    "0.1.0"));

Console.WriteLine("Connected. Press Ctrl+C to stop.");
await Task.Delay(Timeout.Infinite);

internal static class ScreenshotService
{
    public static ScreenshotPayload CapturePrimaryScreen()
    {
        var bounds = Screen.PrimaryScreen?.Bounds ?? new Rectangle(0, 0, 1, 1);
        using var bitmap = new Bitmap(bounds.Width, bounds.Height);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.CopyFromScreen(bounds.Left, bounds.Top, 0, 0, bounds.Size);

        using var stream = new MemoryStream();
        bitmap.Save(stream, ImageFormat.Png);
        return new ScreenshotPayload(
            "image/png",
            Convert.ToBase64String(stream.ToArray()),
            bounds.Width,
            bounds.Height,
            DateTimeOffset.Now);
    }
}

internal static class PowerShellService
{
    public static async Task<PowerShellResult> RunAsync(string command)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command {Quote(command)}",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            StandardOutputEncoding = Encoding.UTF8,
            StandardErrorEncoding = Encoding.UTF8
        };

        using var process = Process.Start(startInfo);
        if (process is null)
        {
            return new PowerShellResult(command, -1, string.Empty, "Failed to start powershell.exe");
        }

        var outputTask = process.StandardOutput.ReadToEndAsync();
        var errorTask = process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        return new PowerShellResult(command, process.ExitCode, await outputTask, await errorTask);
    }

    private static string Quote(string value)
    {
        return "\"" + value.Replace("\"", "\\\"") + "\"";
    }
}

internal static class InputService
{
    private const int InputMouse = 0;
    private const int InputKeyboard = 1;
    private const uint MouseEventFLeftDown = 0x0002;
    private const uint MouseEventFLeftUp = 0x0004;
    private const uint KeyEventFUnicode = 0x0004;
    private const uint KeyEventFKeyUp = 0x0002;

    public static void LeftClick(int x, int y)
    {
        Cursor.Position = new Point(x, y);

        var inputs = new[]
        {
            Input.Mouse(MouseEventFLeftDown),
            Input.Mouse(MouseEventFLeftUp)
        };

        SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<Input>());
    }

    public static void TypeText(string text)
    {
        foreach (var character in text)
        {
            var down = Input.Keyboard(character, keyUp: false);
            var up = Input.Keyboard(character, keyUp: true);
            var inputs = new[] { down, up };
            SendInput((uint)inputs.Length, inputs, Marshal.SizeOf<Input>());
        }
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern uint SendInput(uint nInputs, Input[] pInputs, int cbSize);

    [StructLayout(LayoutKind.Sequential)]
    private struct Input
    {
        public int Type;
        public InputUnion Union;

        public static Input Mouse(uint flags)
        {
            return new Input
            {
                Type = InputMouse,
                Union = new InputUnion
                {
                    MouseInput = new MouseInput
                    {
                        DwFlags = flags
                    }
                }
            };
        }

        public static Input Keyboard(char character, bool keyUp)
        {
            return new Input
            {
                Type = InputKeyboard,
                Union = new InputUnion
                {
                    KeyboardInput = new KeyboardInput
                    {
                        WVk = 0,
                        WScan = character,
                        DwFlags = KeyEventFUnicode | (keyUp ? KeyEventFKeyUp : 0)
                    }
                }
            };
        }
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct InputUnion
    {
        [FieldOffset(0)]
        public MouseInput MouseInput;

        [FieldOffset(0)]
        public KeyboardInput KeyboardInput;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MouseInput
    {
        public int Dx;
        public int Dy;
        public uint MouseData;
        public uint DwFlags;
        public uint Time;
        public IntPtr DwExtraInfo;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct KeyboardInput
    {
        public ushort WVk;
        public char WScan;
        public uint DwFlags;
        public uint Time;
        public IntPtr DwExtraInfo;
    }
}
