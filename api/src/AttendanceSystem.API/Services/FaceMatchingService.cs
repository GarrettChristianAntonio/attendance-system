using System.Text.Json;
using AttendanceSystem.API.Data;
using AttendanceSystem.API.Models;
using Microsoft.EntityFrameworkCore;

namespace AttendanceSystem.API.Services;

public class FaceMatchResult
{
    public bool IsMatch { get; set; }
    public Employee? Employee { get; set; }
    public double Distance { get; set; }
}

public class FaceMatchingService
{
    private readonly AppDbContext _db;
    private const double MatchThreshold = 0.6;

    public FaceMatchingService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<FaceMatchResult> FindMatchAsync(float[] descriptor)
    {
        var employees = await _db.Employees
            .Where(e => e.IsActive && e.FaceDescriptor != "")
            .ToListAsync();

        Employee? bestMatch = null;
        var bestDistance = double.MaxValue;

        foreach (var employee in employees)
        {
            var storedDescriptor = JsonSerializer.Deserialize<float[]>(employee.FaceDescriptor);
            if (storedDescriptor is null || storedDescriptor.Length != 128)
                continue;

            var distance = EuclideanDistance(descriptor, storedDescriptor);
            if (distance < bestDistance)
            {
                bestDistance = distance;
                bestMatch = employee;
            }
        }

        return new FaceMatchResult
        {
            IsMatch = bestDistance < MatchThreshold,
            Employee = bestDistance < MatchThreshold ? bestMatch : null,
            Distance = bestDistance
        };
    }

    private static double EuclideanDistance(float[] a, float[] b)
    {
        double sum = 0;
        for (int i = 0; i < a.Length && i < b.Length; i++)
        {
            var diff = a[i] - b[i];
            sum += diff * diff;
        }
        return Math.Sqrt(sum);
    }
}
