using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Npgsql;
using System.Collections.Concurrent;

public static class OnlineEndpoints {
    private class OnlineUserState {
        public string id { get; set; } = "";
        public DateTimeOffset joinTime { get; set; }
        public DateTimeOffset lastSeenTime { get; set; }

        public bool IsOnline() {
            return lastSeenTime + TimeSpan.FromSeconds(20) > DateTimeOffset.UtcNow;
        }
    }

    private static readonly ConcurrentDictionary<string, OnlineUserState> onlineUsers = new();

    public static void MapOnlineEndpoints(this WebApplication app) {
        var online = app.MapGroup("/online");

        online.MapPost("/join", Join);
        online.MapPost("/ping", Ping);
    }

    private static async Task<IResult> Join(OnlineRequest request, NpgsqlDataSource db) {
        if (string.IsNullOrWhiteSpace(request.id)) {
            return Results.BadRequest(new OnlineResponse {
                success = false,
            });
        }

        string id = request.id;

        bool userExists = await IsUserExists(db, id);

        if (!userExists) {
            return Results.Ok(new OnlineResponse {
                success = false,
            });
        }

        DateTimeOffset now = DateTimeOffset.UtcNow;

        onlineUsers.AddOrUpdate(
            id,
            new OnlineUserState {
                id = id,
                joinTime = now,
                lastSeenTime = now,
            },
            (key, oldValue) => {
                oldValue.joinTime = now;
                oldValue.lastSeenTime = now;
                return oldValue;
            }
        );

        return Results.Ok(new OnlineResponse {
            success = true,
        });
    }

    private static async Task<bool> IsUserExists(NpgsqlDataSource db, string id) {
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

    private static IResult Ping(OnlineRequest request) {
        if (string.IsNullOrWhiteSpace(request.id)) {
            return Results.BadRequest(new OnlineResponse {
                success = false,
            });
        }

        string id = request.id;

        if (!onlineUsers.TryGetValue(id, out OnlineUserState? user)) {
            return Results.Ok(new OnlineResponse {
                success = false,
            });
        }

        user.lastSeenTime = DateTimeOffset.UtcNow;

        return Results.Ok(new OnlineResponse {
            success = true,
        });
    }
}