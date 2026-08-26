using Zelo.Messaging;
using Zelo.Modules.Auto;
using Zelo.Modules.Core;
using Zelo.Modules.Identity;
using Zelo.Modules.Inventory;
using Zelo.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddZeloServiceDefaults();
builder.Services.AddZeloMessaging(builder.Configuration);

// Modulos: apenas o registo base. Sem consumidores - isso e do Worker.
builder.Services.AddIdentityModule(builder.Configuration);
builder.Services.AddCoreModule(builder.Configuration);
builder.Services.AddAutoModule(builder.Configuration);
builder.Services.AddInventoryModule(builder.Configuration);

var app = builder.Build();

app.MapHealthChecks("/health");

// NOTA: nenhum Database.Migrate() aqui. As migrations sao do MigrationRunner.

app.Run();
