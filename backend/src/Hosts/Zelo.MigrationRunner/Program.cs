// Corre as migrations de todos os modulos e termina.
// Executado como init container / job de deploy, ANTES da Api e do Worker.
// Nenhum host aplica migrations no arranque - evita corridas entre replicas.

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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
return 0;
