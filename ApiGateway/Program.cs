using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

// Ocelot ayar dosyası tanıtıldı
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

// Ocelot servisi eklendi
builder.Services.AddOcelot(builder.Configuration);

var app = builder.Build();

// Ocelot'u devreye al
await app.UseOcelot();

app.Run();