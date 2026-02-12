using OrderService;
using OrderService.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// 1. Veritabanı Bağlantısı
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddControllers();

// RabbitMQ Dinleyicisi
builder.Services.AddHostedService<OrderConsumer>();

var app = builder.Build();

// Veritabanı oluşması biraz zaman aldığı için uygulama açılırken veritabanı hazır olana kadar bekleyecek.
CreateDbIfNotExists(app);

app.MapGet("/", () => "Order Service (DB Connected) Çalışıyor! 🐘");
app.Run();

// Retry kısmı
void CreateDbIfNotExists(IHost host)
{
    using (var scope = host.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();
        var dbContext = services.GetRequiredService<OrderDbContext>();

        // 5 kere dene, her denemede 5 saniye bekle
        for (int i = 0; i < 5; i++)
        {
            try
            {
                dbContext.Database.EnsureCreated();
                logger.LogInformation("✅ Order Veritabanı BAŞARIYLA oluşturuldu/bağlandı!");
                return; // Başarılıysa döngüden çık
            }
            catch (Exception ex)
            {
                logger.LogWarning($"⚠️ Veritabanına bağlanılamadı ({i+1}/5). Tekrar deneniyor... Hata: {ex.Message}");
                Thread.Sleep(5000); // 5 saniye bekle
            }
        }
        
        logger.LogError("❌ Veritabanı oluşturulamadı. Pes ediyorum.");
    }
}