using Microsoft.EntityFrameworkCore;
using ProductService.Data;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<ProductDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// OrderService'deki gibi burada da veritabanı hazır olana kadar bekleyecek bir mekanizma ekledim hata vermesin diye.
CreateDbIfNotExists(app);

app.UseSwagger();
app.UseSwaggerUI();
app.MapGet("/", () => Results.Redirect("/swagger"));
app.MapControllers();
app.Run();

// Retry mekanizması
void CreateDbIfNotExists(IHost host)
{
    using (var scope = host.Services.CreateScope())
    {
        var services = scope.ServiceProvider;
        var logger = services.GetRequiredService<ILogger<Program>>();
        var dbContext = services.GetRequiredService<ProductDbContext>();

        for (int i = 0; i < 5; i++)
        {
            try
            {
                dbContext.Database.EnsureCreated();
                logger.LogInformation("✅ Product Veritabanı BAŞARIYLA oluşturuldu!");
                return;
            }
            catch (Exception ex)
            {
                logger.LogWarning($"⚠️ Veritabanı bekleniyor ({i+1}/5)... Hata: {ex.Message}");
                Thread.Sleep(5000);
            }
        }
    }
}