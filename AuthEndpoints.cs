using System.Data;
using Dapper;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Auth;

internal sealed record Member(string Id, string Username, string PasswordHash);

internal sealed record RegisterRequest(string Username, string Password);

internal sealed record RegisterResponse(Member Member, string AccessToken, string RefreshToken);

internal sealed record LoginRequest(string Username, string Password);

internal sealed record LoginResponse(string AccessToken, string RefreshToken);

internal static class AuthEndpoints
{
    private const string ROUTE = "api/v1/auth";

    internal static void MapAuthEndpoints(this WebApplication app)
    {
        RouteGroupBuilder routeGroup = app.MapGroup(ROUTE);

        routeGroup.MapPost("/register", RegisterAsync);
        routeGroup.MapPost("/login", LoginAsync);
    }

    private static async ValueTask<Ok<RegisterResponse>> RegisterAsync(
        RegisterRequest request,
        TokenProvider tokenProvider,
        PasswordHasher passwordHasher,
        IDbConnection dbConnection
    )
    {
        Member member = new(
            Ulid.NewUlid().ToString(),
            request.Username,
            passwordHasher.Hash(request.Password)
        );

        await dbConnection
            .ExecuteAsync(
                """
                INSERT INTO member (id, username, password_hash)
                VALUES (@Id, @Username, @PasswordHash)
                """,
                member
            )
            .ConfigureAwait(false);

        return TypedResults.Ok(
            new RegisterResponse(
                member,
                tokenProvider.Generate(member.Id, member.Username),
                "refreshToken"
            )
        );
    }

    private static async ValueTask<Results<NotFound, Ok<LoginResponse>>> LoginAsync(
        LoginRequest request,
        TokenProvider tokenProvider,
        PasswordHasher passwordHasher,
        IDbConnection dbConnection
    )
    {
        Member? foundMember = await dbConnection
            .QuerySingleOrDefaultAsync<Member>(
                """
                SELECT
                    id as Id,
                    username as Username,
                    password_hash as PasswordHash
                FROM member
                WHERE username = @Username
                """,
                request
            )
            .ConfigureAwait(false);

        return
            foundMember == null
            || !passwordHasher.Verify(request.Password, foundMember.PasswordHash)
            ? TypedResults.NotFound()
            : TypedResults.Ok(
                new LoginResponse(
                    tokenProvider.Generate(foundMember.Id, foundMember.Username),
                    "refreshToken"
                )
            );
    }
}
