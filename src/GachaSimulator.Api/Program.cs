var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/serverActive", () => {
    return Results.Ok("active");
});

app.MapLoginManager();

app.Run();
