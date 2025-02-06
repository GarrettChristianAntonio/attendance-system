using AttendanceSystem.API.Data;
using AttendanceSystem.API.DTOs;
using AttendanceSystem.API.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AttendanceSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EmployeesController : ControllerBase
{
    private readonly AppDbContext _db;

    public EmployeesController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<ActionResult<List<EmployeeDto>>> GetAll()
    {
        var employees = await _db.Employees
            .Where(e => e.IsActive)
            .OrderBy(e => e.Name)
            .Select(e => new EmployeeDto
            {
                Id = e.Id,
                Name = e.Name,
                Email = e.Email,
                PhotoUrl = e.PhotoPath,
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
        if (employee is null || !employee.IsActive)
            return NotFound();

        return Ok(new EmployeeDto
        {
            Id = employee.Id,
            Name = employee.Name,
            Email = employee.Email,
            PhotoUrl = employee.PhotoPath,
            IsActive = employee.IsActive,
            CreatedAt = employee.CreatedAt
        });
    }

    [HttpPost]
    public async Task<ActionResult<EmployeeDto>> Create([FromBody] CreateEmployeeDto dto)
    {
        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            Name = dto.Name,
            Email = dto.Email,
            FaceDescriptor = dto.FaceDescriptor,
            PhotoPath = string.Empty,
            CreatedAt = DateTime.UtcNow
        };

        _db.Employees.Add(employee);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = employee.Id }, new EmployeeDto
        {
            Id = employee.Id,
            Name = employee.Name,
            Email = employee.Email,
            PhotoUrl = employee.PhotoPath,
            IsActive = employee.IsActive,
            CreatedAt = employee.CreatedAt
        });
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<EmployeeDto>> Update(Guid id, [FromBody] UpdateEmployeeDto dto)
    {
        var employee = await _db.Employees.FindAsync(id);
        if (employee is null || !employee.IsActive)
            return NotFound();

        employee.Name = dto.Name;
        employee.Email = dto.Email;

        if (!string.IsNullOrEmpty(dto.FaceDescriptor))
            employee.FaceDescriptor = dto.FaceDescriptor;

        await _db.SaveChangesAsync();

        return Ok(new EmployeeDto
        {
            Id = employee.Id,
            Name = employee.Name,
            Email = employee.Email,
            PhotoUrl = employee.PhotoPath,
            IsActive = employee.IsActive,
            CreatedAt = employee.CreatedAt
        });
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var employee = await _db.Employees.FindAsync(id);
        if (employee is null)
            return NotFound();

        employee.IsActive = false;
        await _db.SaveChangesAsync();

        return NoContent();
    }
}
