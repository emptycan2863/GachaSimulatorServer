using Npgsql;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Postgres")
    ?? throw new InvalidOperationException("Postgres connection string is missing.");

builder.Services.AddNpgsqlDataSource(connectionString);

var app = builder.Build();

app.MapGet("/health", () => {
    return Results.Ok("active");
});

app.MapLoginEndpoints();
app.MapOnlineEndpoints();

app.Run();
