using Npgsql;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Postgres")
    ?? throw new InvalidOperationException("Postgres connection string is missing.");

var app = builder.Build();

builder.Services.AddNpgsqlDataSource(connectionString);

app.MapGet("/serverActive", () => {
    return Results.Ok("active");
});

app.MapLoginManager();

app.Run();
