using System.Data;
using Auth;
using Npgsql;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<TokenProvider>();
builder.Services.AddSingleton<PasswordHasher>();

builder.Services.AddSingleton<IDbConnection>(_ =>
{
    NpgsqlConnection connection = new(builder.Configuration["DbConnectionString"]);
    connection.Open();
    return connection;
});

builder.Services.AddHealthChecks();

WebApplication app = builder.Build();

app.MapAuthEndpoints();

app.MapHealthChecks("/healthy");

app.InitDb();
await app.RunAsync().ConfigureAwait(false);
