using System.Threading.Channels;

namespace AttendanceSystem.API.Services;

public record LiveCheckInEvent(
    Guid OrganizationId,
    string EmployeeName,
    string? PhotoUrl,
    string Status,
    DateTime CheckInAt
);

public class AttendanceLiveService
{
    private readonly Dictionary<Guid, List<ChannelWriter<LiveCheckInEvent>>> _subscribers = new();
    private readonly Lock _lock = new();

    public ChannelReader<LiveCheckInEvent> Subscribe(Guid organizationId)
    {
        var channel = Channel.CreateBounded<LiveCheckInEvent>(50);
        lock (_lock)
        {
            if (!_subscribers.ContainsKey(organizationId))
                _subscribers[organizationId] = new List<ChannelWriter<LiveCheckInEvent>>();
            _subscribers[organizationId].Add(channel.Writer);
        }
        return channel.Reader;
    }

    public void Unsubscribe(Guid organizationId, ChannelReader<LiveCheckInEvent> reader)
    {
        // Cleanup handled by completion detection
    }

    public async Task PublishAsync(LiveCheckInEvent evt)
    {
        List<ChannelWriter<LiveCheckInEvent>>? writers;
        lock (_lock)
        {
            if (!_subscribers.TryGetValue(evt.OrganizationId, out writers))
                return;
            writers = new List<ChannelWriter<LiveCheckInEvent>>(writers);
        }

        var deadWriters = new List<ChannelWriter<LiveCheckInEvent>>();
        foreach (var writer in writers)
        {
            if (!writer.TryWrite(evt))
                deadWriters.Add(writer);
        }

        if (deadWriters.Count > 0)
        {
            lock (_lock)
            {
                if (_subscribers.TryGetValue(evt.OrganizationId, out var list))
                    list.RemoveAll(w => deadWriters.Contains(w));
            }
        }
    }
}
