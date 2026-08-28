using System.Text.Json.Serialization;
using Zelo.Messaging;
using Zelo.Modules.Auto;
using Zelo.Modules.Auto.Endpoints;
using Zelo.Modules.Core;
using Zelo.Modules.Core.Endpoints;
using Zelo.Modules.Identity;
using Zelo.Modules.Identity.Endpoints;
using Zelo.Modules.Inventory;
using Zelo.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);

// Enums como string na API HTTP (o frontend envia "Ligeiros", nao 0) - so
// aqui, os eventos AMQP continuam a serializar enums como int por omissao.
builder.Services.ConfigureHttpJsonOptions(o =>
    o.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddOpenApi();

// Em dev os 3 frontends vivem em portas diferentes (origens diferentes);
// atras do gateway (Caddyfile) passam a ser o mesmo dominio e isto deixa
// de ser preciso, mas fica configuravel para nao partir esse caminho.
const string FrontendCorsPolicy = "frontend";
var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? ["http://localhost:3000", "http://localhost:3001", "http://localhost:3002"];
builder.Services.AddCors(o => o.AddPolicy(FrontendCorsPolicy, p => p
    .WithOrigins(corsOrigins)
    .AllowAnyHeader()
    .AllowAnyMethod()));

builder.Services.AddZeloServiceDefaults(builder.Configuration);
builder.Services.AddZeloMessaging(builder.Configuration);

// Modulos: apenas o registo base. Sem consumidores - isso e do Worker.
builder.Services.AddIdentityModule(builder.Configuration);
builder.Services.AddCoreModule(builder.Configuration);
builder.Services.AddAutoModule(builder.Configuration);
builder.Services.AddInventoryModule(builder.Configuration);

var app = builder.Build();

app.MapHealthChecks("/health");
app.MapOpenApi(); // /openapi/v1.json - e o que frontend/tools/scripts/gen-api-client.sh consome
app.UseCors(FrontendCorsPolicy);
app.UseAuthentication();
app.UseAuthorization();
app.MapIdentityEndpoints();
app.MapCoreEndpoints();
app.MapAutoEndpoints();

// NOTA: nenhum Database.Migrate() aqui. As migrations sao do MigrationRunner.

app.Run();
