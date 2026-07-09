using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

public static class LoginManager {
    public static void MapLoginManager(this WebApplication app) {
        var login = app.MapGroup("/login");
        login.MapPost("/serverActive", ()=> { 
        });
    }
}