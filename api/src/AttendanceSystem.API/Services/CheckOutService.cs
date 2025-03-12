using AttendanceSystem.API.Data;
using AttendanceSystem.API.Models;
using Microsoft.EntityFrameworkCore;

namespace AttendanceSystem.API.Services;

public class CheckOutService
{
    private readonly AppDbContext _db;

    public CheckOutService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<bool> ManualCheckOut(Guid recordId, Guid organizationId)
    {
        var record = await _db.AttendanceRecords
            .Include(a => a.Employee)
            .FirstOrDefaultAsync(a => a.Id == recordId && a.Employee.OrganizationId == organizationId);

        if (record is null || record.CheckOutAt.HasValue)
            return false;

        record.CheckOutAt = DateTime.UtcNow;

        if (record.ShiftId.HasValue)
        {
            var shift = await _db.Shifts.FindAsync(record.ShiftId);
            if (shift is not null)
            {
                var checkOutTime = TimeOnly.FromDateTime(record.CheckOutAt.Value);
                if (checkOutTime < shift.EndTime.AddMinutes(-15))
                    record.Status = "EarlyDeparture";
            }
        }

        await _db.SaveChangesAsync();
        return true;
    }
}
