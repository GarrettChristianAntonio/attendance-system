using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;

namespace AttendanceSystem.Tests.Integration;

public class EmployeesControllerTests : IntegrationTestBase
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public EmployeesControllerTests(TestWebApplicationFactory factory) : base(factory) { }

    private MultipartFormDataContent CreateEmployeeForm(string name, string? email = null)
    {
        var descriptor = new float[128];
        for (int i = 0; i < 128; i++) descriptor[i] = 0.1f + i * 0.001f;

        var form = new MultipartFormDataContent();
        form.Add(new StringContent(name), "name");
        if (email != null)
            form.Add(new StringContent(email), "email");
        form.Add(new StringContent(JsonSerializer.Serialize(descriptor)), "faceDescriptor");

        var photoBytes = new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 }; // minimal JPEG header
        var photoContent = new ByteArrayContent(photoBytes);
        photoContent.Headers.ContentType = new MediaTypeHeaderValue("image/jpeg");
        form.Add(photoContent, "photo", "test.jpg");

        return form;
    }

    [Fact]
    public async Task CreateEmployee_ValidData_ReturnsCreated()
    {
        var form = CreateEmployeeForm("John Doe", "john@test.com");

        var response = await Client.PostAsync("/api/employees", form);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("name").GetString().Should().Be("John Doe");
        body.GetProperty("email").GetString().Should().Be("john@test.com");
    }

    [Fact]
    public async Task GetAll_ReturnsEmployees()
    {
        var form = CreateEmployeeForm("List Employee");
        await Client.PostAsync("/api/employees", form);

        var response = await Client.GetAsync("/api/employees");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var employees = await response.Content.ReadFromJsonAsync<JsonElement[]>(JsonOptions);
        employees.Should().NotBeNull();
        employees!.Should().Contain(e => e.GetProperty("name").GetString() == "List Employee");
    }

    [Fact]
    public async Task GetById_ExistingEmployee_ReturnsOk()
    {
        var form = CreateEmployeeForm("Get By Id Employee");
        var createResponse = await Client.PostAsync("/api/employees", form);
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var id = created.GetProperty("id").GetString();

        var response = await Client.GetAsync($"/api/employees/{id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>();
        body.GetProperty("name").GetString().Should().Be("Get By Id Employee");
    }

    [Fact]
    public async Task DeleteEmployee_ReturnsNoContent()
    {
        var form = CreateEmployeeForm("Delete Me");
        var createResponse = await Client.PostAsync("/api/employees", form);
        var created = await createResponse.Content.ReadFromJsonAsync<JsonElement>();
        var id = created.GetProperty("id").GetString();

        var deleteResponse = await Client.DeleteAsync($"/api/employees/{id}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await Client.GetAsync($"/api/employees/{id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAll_MissingApiKey_Returns401()
    {
        var client = Factory.CreateClient();
        // No API key header set

        var response = await client.GetAsync("/api/employees");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
