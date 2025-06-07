using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Todo.ApplicationCore.Interfaces;
using Todo.ApplicationCore.Services;
using Todo.Infrastructure.Repositories;
using TodoApi.Models;
using TodoMcpServer.Interfaces;

var builder = WebApplication.CreateBuilder(args);
builder
    .Services.AddDbContext<TodoContext>(opt =>
        opt.UseSqlServer(builder.Configuration.GetConnectionString("TodoContext"))
    );
    
builder.Services.AddScoped<DbContext>(provider => provider.GetRequiredService<TodoContext>());
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddControllers();

builder.Services.AddScoped<IItemsService, ItemService>();
builder.Services.AddScoped<ITodoListsService, TodoListsService>();
builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped(typeof(ITodoListRepository), typeof(TodoListRepository));

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

var app = builder.Build();

app.UseAuthorization();
app.MapControllers();
app.Run();
