using System.Data;
using Dapper;

namespace Auth;

internal sealed record User(string Id, string Username, string PasswordHash);

internal sealed record RegisterUserRequest(string Username, string Password);

internal sealed record LoginUserRequest(string Username, string Password);

internal static class AuthEndpoints
{
    private const string ROUTE = "api/v1/auth";

    internal static void MapAuthEndpoints(this WebApplication app)
    {
        RouteGroupBuilder routeGroup = app.MapGroup(ROUTE);

        routeGroup.MapPost("/register", RegisterAsync);
        routeGroup.MapPost("/login", LoginAsync);
    }

    private static async ValueTask<IResult> RegisterAsync(
        RegisterUserRequest request,
        TokenProvider tokenProvider,
        PasswordHasher passwordHasher,
        IDbConnection dbConnection
    )
    {
        User user = new(
            Ulid.NewUlid().ToString(),
            request.Username,
            passwordHasher.Hash(request.Password)
        );

        await dbConnection
            .ExecuteAsync(
                """
                INSERT INTO customer (id, username, password_hash)
                VALUES (@Id, @Username, @PasswordHash)
                """,
                user
            )
            .ConfigureAwait(false);

        return TypedResults.Ok(
            new
            {
                createdUser = user,
                accessToken = tokenProvider.Generate(user.Id, user.Username),
                refreshToken = "",
            }
        );
    }

    private static async ValueTask<IResult> LoginAsync(
        LoginUserRequest request,
        TokenProvider tokenProvider,
        PasswordHasher passwordHasher,
        IDbConnection dbConnection
    )
    {
        User? foundUser = await dbConnection
            .QuerySingleOrDefaultAsync<User>(
                """
                SELECT id as Id, username as Username, password_hash as PasswordHash
                FROM Customer WHERE username = @Username
                """,
                request
            )
            .ConfigureAwait(false);

        return foundUser == null || !passwordHasher.Verify(request.Password, foundUser.PasswordHash)
            ? TypedResults.NotFound()
            : TypedResults.Ok(
                new
                {
                    user = foundUser,
                    accessToken = tokenProvider.Generate(foundUser.Id, foundUser.Username),
                    refreshToken = "",
                }
            );
    }
}
