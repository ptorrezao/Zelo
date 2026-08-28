using Zelo.Messaging;
using Zelo.Modules.Auto;
using Zelo.Modules.Core;
using Zelo.Modules.Identity;
using Zelo.Modules.Inventory;
using Zelo.ServiceDefaults;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddZeloServiceDefaults(builder.Configuration);
builder.Services.AddZeloMessaging(builder.Configuration);

builder.Services.AddIdentityModule(builder.Configuration);
builder.Services.AddCoreModule(builder.Configuration);
builder.Services.AddAutoModule(builder.Configuration);
builder.Services.AddInventoryModule(builder.Configuration);

// A diferenca face a Api: os consumidores e os jobs agendados vivem aqui.
builder.Services.AddIdentityConsumers();
builder.Services.AddCoreConsumers();
builder.Services.AddAutoConsumers();
builder.Services.AddInventoryConsumers();

// Depois de todos os AddXConsumers terem registado os seus handlers.
builder.Services.AddZeloMessagingConsumers();

var host = builder.Build();
await host.RunAsync();
