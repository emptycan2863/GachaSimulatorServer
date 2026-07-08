var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/health", () => {
    return Results.Ok(new {
        status = "ok",
        serverTime = DateTimeOffset.UtcNow
    });
});

app.Run();
