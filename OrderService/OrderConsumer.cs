using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using OrderService.Data;
using OrderService.Models;

namespace OrderService;

public class OrderConsumer : BackgroundService
{
    // Veritabanı işlemleri için servis sağlayıcı
    private readonly IServiceProvider _serviceProvider;

    public OrderConsumer(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var hostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost";
        var factory = new ConnectionFactory { HostName = hostName };

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                using var connection = await factory.CreateConnectionAsync(stoppingToken);
                using var channel = await connection.CreateChannelAsync(cancellationToken: stoppingToken);

                await channel.QueueDeclareAsync(queue: "order_queue", durable: false, exclusive: false, autoDelete: false, arguments: null);

                var consumer = new AsyncEventingBasicConsumer(channel);
                
                // Mesaj geldiğinde çalışacak
                consumer.ReceivedAsync += async (model, ea) =>
                {
                    var body = ea.Body.ToArray();
                    var message = Encoding.UTF8.GetString(body);

                    Console.WriteLine($"[Order Service] 🔔 Mesaj Geldi: {message}");

                    // --- Veritabanı kayıt
                    try 
                    {
                        // Scope (Kapsam) oluşturuyoruz
                        using (var scope = _serviceProvider.CreateScope())
                        {
                            var dbContext = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
                            
                            // Gelen mesajı basit bir sipariş olarak kaydedelim
                            var order = new Order
                            {
                                OrderDate = DateTime.UtcNow,
                                TotalPrice = 0, // Detayları sonra JSON'dan parse edebiliriz
                                CustomerName = "Musteri_" + new Random().Next(100, 999)
                            };

                            dbContext.Orders.Add(order);
                            await dbContext.SaveChangesAsync();
                            
                            Console.WriteLine($"[Order Service] ✅ Sipariş (ID: {order.Id}) veritabanına kaydedildi! 🐘");
                        }
                    }
                    catch (Exception dbEx)
                    {
                         Console.WriteLine($"[Order Service] ❌ Kayıt Hatası: {dbEx.Message}");
                    }
                    // -----------------------------------

                    await Task.CompletedTask;
                };

                await channel.BasicConsumeAsync(queue: "order_queue", autoAck: true, consumer: consumer);
                Console.WriteLine($"[Order Service] 🐰 RabbitMQ'ya bağlandı, sipariş bekleniyor...");

                await Task.Delay(Timeout.Infinite, stoppingToken);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Order Service] ⚠️ Bağlantı bekleniyor... ({ex.Message})");
                await Task.Delay(5000, stoppingToken);
            }
        }
    }
}