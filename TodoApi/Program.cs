using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
builder.Services
    .AddDbContext<TodoContext>(
        opt => opt.UseInMemoryDatabase("TodoList")
    )
    .AddEndpointsApiExplorer()
    .AddControllers();

// builder.Services.AddDbContext<TodoContext>(options =>
//     options.UseSqlServer(builder.Configuration.GetConnectionString("TodoContext") ?? throw new InvalidOperationException("Connection string 'TodoContext' not found.")));

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Host
    .ConfigureLogging(logging => logging.AddConsole());

var app = builder.Build();

app.UseAuthorization();
app.MapControllers();
app.Run();