// Corre as migrations de todos os modulos e termina.
// Executado como init container / job de deploy, ANTES da Api e do Worker.
// Nenhum host aplica migrations no arranque - evita corridas entre replicas.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Zelo.MigrationRunner;
using Zelo.Modules.Auto;
using Zelo.Modules.Core;
using Zelo.Modules.Identity;

Console.WriteLine("Zelo :: migration runner");

var configuration = new ConfigurationBuilder()
    .AddEnvironmentVariables()
    .Build();

var services = new ServiceCollection();
services.AddIdentityModule(configuration);
services.AddCoreModule(configuration);
services.AddAutoModule(configuration);
// TODO: services.AddInventoryModule quando tiver DbContext registado
// (ainda e um stub).

await using var provider = services.BuildServiceProvider();

await IdentityModule.MigrateAsync(provider);
await CoreModule.MigrateAsync(provider);
await AutoModule.MigrateAsync(provider);
// TODO: InventoryModule.MigrateAsync

Console.WriteLine("Migrations concluidas.");

var unleashUrl = configuration["FeatureFlags:Url"];
var unleashToken = configuration["FeatureFlags:ApiToken"];
if (!string.IsNullOrWhiteSpace(unleashUrl) && !string.IsNullOrWhiteSpace(unleashToken))
{
    Console.WriteLine("Zelo :: a garantir feature flags no Unleash");
    await UnleashBootstrap.RunAsync(unleashUrl, unleashToken);
    Console.WriteLine("Feature flags prontas.");
}

var garageAdminUrl = configuration["Storage:AdminUrl"];
var garageAdminToken = configuration["Storage:AdminToken"];
var garageBucket = configuration["Storage:Bucket"];
var garageAccessKey = configuration["Storage:AccessKey"];
var garageSecretKey = configuration["Storage:SecretKey"];
if (!string.IsNullOrWhiteSpace(garageAdminUrl) && !string.IsNullOrWhiteSpace(garageAdminToken)
    && !string.IsNullOrWhiteSpace(garageBucket) && !string.IsNullOrWhiteSpace(garageAccessKey)
    && !string.IsNullOrWhiteSpace(garageSecretKey))
{
    Console.WriteLine("Zelo :: a garantir layout/bucket/chave no Garage");
    await GarageBootstrap.RunAsync(garageAdminUrl, garageAdminToken, garageBucket, garageAccessKey, garageSecretKey, "zelo-api-key");
    Console.WriteLine("Garage pronto.");
}

return 0;
