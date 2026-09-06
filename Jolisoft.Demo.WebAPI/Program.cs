using Jolisoft.Demo.WebAPI.Data;
using Jolisoft.Demo.WebAPI.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddCors(options => options.AddPolicy("ShowcaseShell", policy =>
	policy.WithOrigins("http://localhost:6173", "http://127.0.0.1:6173")
		.AllowAnyHeader()
		.AllowAnyMethod()));
builder.Services.AddSingleton<IPlatformGateway, FakePlatformGateway>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
	builder.Services.AddSingleton<IWorkflowStore, InMemoryWorkflowStore>();
}
else
{
	builder.Services.AddDbContext<WorkflowDbContext>(options => options.UseSqlServer(connectionString));
	builder.Services.AddScoped<IWorkflowStore, EfWorkflowStore>();
}

var app = builder.Build();

app.UseMiddleware<RequestTimingMiddleware>();
app.UseCors("ShowcaseShell");
app.MapControllers();

if (!string.IsNullOrWhiteSpace(connectionString))
{
	using var scope = app.Services.CreateScope();
	await scope.ServiceProvider.GetRequiredService<WorkflowDbContext>().Database.EnsureCreatedAsync();
}

app.Run();
