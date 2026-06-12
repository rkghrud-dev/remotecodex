using System.Collections.Concurrent;
using RemoteCodex.Shared;

namespace RemoteCodex.Host;

public static class GuestRegistry
{
    private static readonly ConcurrentDictionary<string, GuestStatus> Guests = new();

    public static IReadOnlyCollection<GuestStatus> All => Guests.Values.OrderBy(x => x.MachineName).ToArray();

    public static string? FirstConnectionId => Guests.Keys.FirstOrDefault();

    public static void Upsert(GuestStatus guest)
    {
        Guests[guest.ConnectionId] = guest;
    }

    public static void Remove(string connectionId)
    {
        Guests.TryRemove(connectionId, out _);
    }
}
