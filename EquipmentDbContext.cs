using Microsoft.EntityFrameworkCore;

public class EquipmentDbContext(DbContextOptions<EquipmentDbContext> options) : DbContext(options)
{
    public DbSet<DeviceEntity> Devices => Set<DeviceEntity>();
}

public class DeviceEntity
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Status { get; set; } = "Offline";
}

public class SqlDeviceStore(EquipmentDbContext db) : IDeviceStore
{
    public IReadOnlyList<Device> GetAll() =>
        db.Devices.OrderBy(d => d.Id)
            .Select(d => new Device(d.Id, d.Name, d.Status))
            .ToList();

    public Device? GetById(int id) =>
        db.Devices.Where(d => d.Id == id)
            .Select(d => new Device(d.Id, d.Name, d.Status))
            .FirstOrDefault();

    public Device Add(string name)
    {
        var entity = new DeviceEntity { Name = name };
        db.Devices.Add(entity);
        db.SaveChanges();
        return new Device(entity.Id, entity.Name, entity.Status);
    }
}