public record Device(int Id, string Name, string Status);
public record CreateDevice(string Name);

public interface IDeviceStore
{
    IReadOnlyList<Device> GetAll();
    Device? GetById(int id);
    Device Add(string name);
}

public class InMemoryDeviceStore : IDeviceStore
{
    private readonly object _lock = new();
    private readonly List<Device> _devices = new()
    {
        new(1, "Pump-01", "Online"),
        new(2, "Valve-02", "Offline")
    };

    public IReadOnlyList<Device> GetAll()
    {
        lock (_lock) return _devices.ToList();
    }

    public Device? GetById(int id)
    {
        lock (_lock) return _devices.FirstOrDefault(d => d.Id == id);
    }

    public Device Add(string name)
    {
        lock (_lock)
        {
            var device = new Device(_devices.Max(d => d.Id) + 1, name, "Offline");
            _devices.Add(device);
            return device;
        }
    }
}