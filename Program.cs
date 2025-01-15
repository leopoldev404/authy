using System.Data;
using Auth;
using Npgsql;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder
    .Services.AddOptions<JwtSettings>()
    .BindConfiguration(JwtSettings.CONFIGURATION_SECTION_NAME)
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddScoped<IDbConnection>(_ =>
{
    NpgsqlConnection connection = new(builder.Configuration["DbConnectionString"]);
    connection.Open();
    return connection;
});

builder.Services.AddTransient<TokenProvider>();
builder.Services.AddTransient<PasswordHasher>();

builder.Services.AddHealthChecks();

WebApplication app = builder.Build();

app.MapAuthEndpoints();

app.MapHealthChecks("/healthy");

app.InitDb();
await app.RunAsync().ConfigureAwait(false);
