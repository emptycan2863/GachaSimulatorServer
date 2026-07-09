using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Npgsql;
using NpgsqlTypes;
using System.Security.Cryptography;
using System.Text.Json;

public static class LoginEndpoints {
    public static void MapLoginEndpoints(this WebApplication app) {
        var login = app.MapGroup("/login");
        login.MapPost("/idLogin", IdLogin);
        login.MapPost("/signIn", SignIn);
    }

    private static async Task<IResult> IdLogin(IdLoginRequest request, NpgsqlDataSource db) {
        if (string.IsNullOrWhiteSpace(request.id)) {
            return Results.BadRequest(new LoginResponse {
                success = false,
                id = "",
                user = null,
            });
        }
        string id = request.id;

        await using var command = db.CreateCommand("""
            SELECT id, user_info
            FROM users
            WHERE id = @id
            LIMIT 1
            """);

        command.Parameters.AddWithValue("id", id);

        await using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync()) {
            return Results.Ok(new LoginResponse {
                success = false,
                id = id,
                user = null,
            });
        }

        id = reader.GetString(0);
        string userJson = reader.GetString(1);
        UserInfo? user = JsonSerializer.Deserialize<UserInfo>(userJson);

        if (user == null) {
            return Results.Ok(new LoginResponse {
                success = false,
                id = id,
                user = null,
            });
        }

        return Results.Ok(new LoginResponse {
            success = true,
            id = id,
            user = user,
        });
    }

    private const string randomChars = "abcdefghijklmnopqrstuvwxyz0123456789";
    private static string CreateRandomString(int length) {
        char[] result = new char[length];

        for (int i = 0; i < length; ++i) {
            int index = RandomNumberGenerator.GetInt32(randomChars.Length);
            result[i] = randomChars[index];
        }

        return new string(result);
    }
    private static async Task<bool> IsUserIDExists(NpgsqlDataSource db, string id) {
        await using var command = db.CreateCommand("""
        SELECT EXISTS (
            SELECT 1
            FROM users
            WHERE id = @id
        )
        """);

        command.Parameters.AddWithValue("id", id);

        object? result = await command.ExecuteScalarAsync();

        return result is bool exists && exists;
    }
    private static async Task<string> CreateGuestID(NpgsqlDataSource db) {
        for (int length = 8; length <= 20; ++length) {
            string guestID = "Guest_" + CreateRandomString(length);

            bool alreadyExists = await IsUserIDExists(db, guestID);

            if (!alreadyExists) {
                return guestID;
            }
        }

        throw new Exception("사용 가능한 Guest ID를 생성하지 못했습니다.");
    }

    private static UserInfo NewUserInfo(string id) {
        UserInfo user = new UserInfo {
            id = id,
        };
        return user;
    }

    private static async Task<IResult> SignIn(NpgsqlDataSource db) {
        string id = await CreateGuestID(db);

        UserInfo user = NewUserInfo(id);

        string userJson = JsonSerializer.Serialize(user);

        await using var command = db.CreateCommand("""
            INSERT INTO users (id, user_info)
            VALUES (@id, @user_info)
            RETURNING id
            """);

        command.Parameters.AddWithValue("id", id);

        var userInfoParam = command.Parameters.AddWithValue("user_info", userJson);
        userInfoParam.NpgsqlDbType = NpgsqlDbType.Jsonb;

        object? result = await command.ExecuteScalarAsync();

        if (result == null) {
            return Results.Ok(new LoginResponse {
                success = false,
                id = id,
                user = null,
            });
        }

        return Results.Ok(new LoginResponse {
            success = true,
            id = id,
            user = user,
        });
    }
}