using AttendanceSystem.API.Data;
using AttendanceSystem.API.DTOs;
using AttendanceSystem.API.Models;
using AttendanceSystem.API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AttendanceSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IWebHostEnvironment _env;
    private readonly IOrganizationContext _orgContext;

    public EmployeesController(AppDbContext db, IWebHostEnvironment env, IOrganizationContext orgContext)
    {
        _db = db;
        _env = env;
        _orgContext = orgContext;
    }

    [HttpGet]
    public async Task<ActionResult<List<EmployeeDto>>> GetAll()
    {
        var employees = await _db.Employees
            .Where(e => e.IsActive && e.OrganizationId == _orgContext.OrganizationId)
            .OrderBy(e => e.Name)
            .Select(e => new EmployeeDto
            {
                Id = e.Id,
                Name = e.Name,
                Email = e.Email,
                PhotoUrl = string.IsNullOrEmpty(e.PhotoPath) ? null : e.PhotoPath,
                IsActive = e.IsActive,
                CreatedAt = e.CreatedAt
            })
            .ToListAsync();

        return Ok(employees);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<EmployeeDto>> GetById(Guid id)
    {
        var employee = await _db.Employees.FindAsync(id);
        if (employee is null || !employee.IsActive || employee.OrganizationId != _orgContext.OrganizationId)
            return NotFound();

        return Ok(new EmployeeDto
        {
            Id = employee.Id,
            Name = employee.Name,
            Email = employee.Email,
            PhotoUrl = string.IsNullOrEmpty(employee.PhotoPath) ? null : employee.PhotoPath,
            IsActive = employee.IsActive,
            CreatedAt = employee.CreatedAt
        });
    }

    [HttpPost]
    public async Task<ActionResult<EmployeeDto>> Create(
        [FromForm] string name,
        [FromForm] string? email,
        [FromForm] string faceDescriptor,
        IFormFile? photo)
    {
        if (string.IsNullOrWhiteSpace(name))
            return BadRequest(new { error = "Name is required" });

        if (string.IsNullOrWhiteSpace(faceDescriptor))
            return BadRequest(new { error = "Face descriptor is required" });

        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            OrganizationId = _orgContext.OrganizationId,
            Name = name,
            Email = email,
            FaceDescriptor = faceDescriptor,
            CreatedAt = DateTime.UtcNow
        };

        if (photo is not null && photo.Length > 0)
        {
            var uploadsDir = Path.Combine(_env.ContentRootPath, "uploads", "photos");
            Directory.CreateDirectory(uploadsDir);

            var extension = Path.GetExtension(photo.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(extension)) extension = ".jpg";
            var fileName = $"{employee.Id}{extension}";
            var filePath = Path.Combine(uploadsDir, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await photo.CopyToAsync(stream);

            employee.PhotoPath = $"/uploads/photos/{fileName}";
        }

        _db.Employees.Add(employee);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = employee.Id }, new EmployeeDto
        {
            Id = employee.Id,
            Name = employee.Name,
            Email = employee.Email,
            PhotoUrl = string.IsNullOrEmpty(employee.PhotoPath) ? null : employee.PhotoPath,
            IsActive = employee.IsActive,
            CreatedAt = employee.CreatedAt
        });
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<EmployeeDto>> Update(
        Guid id,
        [FromForm] string name,
        [FromForm] string? email,
        [FromForm] string? faceDescriptor,
        IFormFile? photo)
    {
        var employee = await _db.Employees.FindAsync(id);
        if (employee is null || !employee.IsActive || employee.OrganizationId != _orgContext.OrganizationId)
            return NotFound();

        employee.Name = name;
        employee.Email = email;

        if (!string.IsNullOrEmpty(faceDescriptor))
            employee.FaceDescriptor = faceDescriptor;

        if (photo is not null && photo.Length > 0)
        {
            var uploadsDir = Path.Combine(_env.ContentRootPath, "uploads", "photos");
            Directory.CreateDirectory(uploadsDir);

            var extension = Path.GetExtension(photo.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(extension)) extension = ".jpg";
            var fileName = $"{employee.Id}{extension}";
            var filePath = Path.Combine(uploadsDir, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await photo.CopyToAsync(stream);

            employee.PhotoPath = $"/uploads/photos/{fileName}";
        }

        await _db.SaveChangesAsync();

        return Ok(new EmployeeDto
        {
            Id = employee.Id,
            Name = employee.Name,
            Email = employee.Email,
            PhotoUrl = string.IsNullOrEmpty(employee.PhotoPath) ? null : employee.PhotoPath,
            IsActive = employee.IsActive,
            CreatedAt = employee.CreatedAt
        });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var employee = await _db.Employees.FindAsync(id);
        if (employee is null || employee.OrganizationId != _orgContext.OrganizationId)
            return NotFound();

        employee.IsActive = false;
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
