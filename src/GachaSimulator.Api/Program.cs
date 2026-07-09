using Npgsql;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("Postgres")
    ?? throw new InvalidOperationException("Postgres connection string is missing.");

builder.Services.AddNpgsqlDataSource(connectionString);

var app = builder.Build();

app.MapGet("/serverActive", () => {
    return Results.Ok("active");
});

app.MapLoginEndpoints();

app.Run();
