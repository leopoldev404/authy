using DbUp;
using DbUp.Engine;

namespace Auth;

internal static class Extensions
{
    internal static void InitDb(this WebApplication app)
    {
        EnsureDatabase.For.PostgresqlDatabase(app.Configuration["DbConnectionString"]);

        UpgradeEngine engine = DeployChanges
            .To.PostgresqlDatabase(app.Configuration["DbConnectionString"])
            .WithScriptsEmbeddedInAssembly(typeof(Extensions).Assembly)
            .Build();

        engine.PerformUpgrade();
    }
}
