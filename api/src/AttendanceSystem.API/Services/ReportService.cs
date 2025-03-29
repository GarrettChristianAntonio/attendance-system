using System.Globalization;
using System.Text;
using AttendanceSystem.API.Data;
using AttendanceSystem.API.Models;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AttendanceSystem.API.Services;

public class ReportService
{
    private readonly AppDbContext _db;

    public ReportService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<byte[]> GenerateCsv(Guid organizationId, DateOnly from, DateOnly to)
    {
        var fromDt = from.ToDateTime(TimeOnly.MinValue);
        var toDt = to.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var records = await _db.AttendanceRecords
            .Include(a => a.Employee)
            .Include(a => a.Shift)
            .Where(a => a.Employee.OrganizationId == organizationId
                && a.CheckInAt >= fromDt && a.CheckInAt < toDt)
            .OrderBy(a => a.CheckInAt)
            .ToListAsync();

        var sb = new StringBuilder();
        sb.AppendLine("Employee,Date,CheckIn,CheckOut,Status,Shift,Confidence");

        foreach (var r in records)
        {
            var checkOut = r.CheckOutAt?.ToString("HH:mm:ss") ?? "";
            var shift = r.Shift?.Name ?? "";
            var confidence = Math.Max(0, (1 - r.Confidence) * 100).ToString("F0");
            sb.AppendLine($"\"{r.Employee.Name}\",{r.CheckInAt:yyyy-MM-dd},{r.CheckInAt:HH:mm:ss},{checkOut},{r.Status},{shift},{confidence}%");
        }

        return Encoding.UTF8.GetBytes(sb.ToString());
    }

    public async Task<byte[]> GeneratePdf(Guid organizationId, DateOnly from, DateOnly to, string orgName)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var fromDt = from.ToDateTime(TimeOnly.MinValue);
        var toDt = to.AddDays(1).ToDateTime(TimeOnly.MinValue);

        var records = await _db.AttendanceRecords
            .Include(a => a.Employee)
            .Include(a => a.Shift)
            .Where(a => a.Employee.OrganizationId == organizationId
                && a.CheckInAt >= fromDt && a.CheckInAt < toDt)
            .OrderBy(a => a.CheckInAt)
            .ToListAsync();

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(40);

                page.Header().Column(col =>
                {
                    col.Item().Text($"Attendance Report — {orgName}")
                        .FontSize(18).Bold().FontColor(Colors.Blue.Darken3);
                    col.Item().Text($"{from:yyyy-MM-dd} to {to:yyyy-MM-dd}")
                        .FontSize(10).FontColor(Colors.Grey.Darken1);
                    col.Item().PaddingBottom(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                });

                page.Content().Table(table =>
                {
                    table.ColumnsDefinition(cols =>
                    {
                        cols.RelativeColumn(3); // Employee
                        cols.RelativeColumn(2); // Date
                        cols.RelativeColumn(1.5f); // Check-in
                        cols.RelativeColumn(1.5f); // Check-out
                        cols.RelativeColumn(1.5f); // Status
                        cols.RelativeColumn(2); // Shift
                    });

                    table.Header(header =>
                    {
                        var headerStyle = TextStyle.Default.FontSize(9).Bold().FontColor(Colors.White);
                        header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Employee").Style(headerStyle);
                        header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Date").Style(headerStyle);
                        header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Check-in").Style(headerStyle);
                        header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Check-out").Style(headerStyle);
                        header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Status").Style(headerStyle);
                        header.Cell().Background(Colors.Blue.Darken2).Padding(5).Text("Shift").Style(headerStyle);
                    });

                    var rowStyle = TextStyle.Default.FontSize(8);
                    foreach (var (r, i) in records.Select((r, i) => (r, i)))
                    {
                        var bg = i % 2 == 0 ? Colors.White : Colors.Grey.Lighten4;
                        table.Cell().Background(bg).Padding(4).Text(r.Employee.Name).Style(rowStyle);
                        table.Cell().Background(bg).Padding(4).Text(r.CheckInAt.ToString("yyyy-MM-dd")).Style(rowStyle);
                        table.Cell().Background(bg).Padding(4).Text(r.CheckInAt.ToString("HH:mm")).Style(rowStyle);
                        table.Cell().Background(bg).Padding(4).Text(r.CheckOutAt?.ToString("HH:mm") ?? "-").Style(rowStyle);
                        table.Cell().Background(bg).Padding(4).Text(r.Status).Style(rowStyle);
                        table.Cell().Background(bg).Padding(4).Text(r.Shift?.Name ?? "-").Style(rowStyle);
                    }
                });

                page.Footer().AlignCenter().Text(t =>
                {
                    t.Span("Page ").FontSize(8);
                    t.CurrentPageNumber().FontSize(8);
                    t.Span(" of ").FontSize(8);
                    t.TotalPages().FontSize(8);
                });
            });
        });

        return document.GeneratePdf();
    }
}
