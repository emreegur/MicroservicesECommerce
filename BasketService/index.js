const express = require("express");
const amqp = require("amqplib"); // RabbitMQ kütüphanesi
const app = express();
const PORT = 5001;

app.use(express.json());

let basket = [];

// RabbitMQ Bağlantı Ayarları
async function sendToQueue(queue, message) {
    try {
        // 1. RabbitMQ'ya bağlan (Docker'daki sunucuya)
        const rabbitUrl = process.env.RABBITMQ_URL || "amqp://localhost";
        const connection = await amqp.connect(rabbitUrl);
        const channel = await connection.createChannel();

        // 2. Kuyruğu oluştur (yoksa yaratır)
        await channel.assertQueue(queue, { durable: false });

        // 3. Mesajı gönder
        channel.sendToQueue(queue, Buffer.from(JSON.stringify(message)));
        console.log(`[x] Gönderildi: ${JSON.stringify(message)}`);

        // 4. Kapat
        await channel.close();
        await connection.close();
    } catch (error) {
        console.error("RabbitMQ Hatası:", error);
    }
}

// Sepeti Getir
app.get("/basket", (req, res) => {
    res.json({ basket, total: basket.length });
});

// Sepete Ekle
app.post("/basket", (req, res) => {
    const item = req.body;
    basket.push(item);
    res.json({ message: "Eklendi", item });
});

// Satın al (checkout)
app.post("/checkout", async (req, res) => {
    if (basket.length === 0) {
        return res.status(400).json({ message: "Sepet boş, neyi alıyorsun?" });
    }

    // Mesaj içeriği (Sipariş Detayı)
    const orderData = {
        products: basket,
        totalPrice: basket.reduce((acc, item) => acc + (item.price || 0), 0),
        date: new Date()
    };

    // RabbitMQ'ya gönder 
    await sendToQueue("order_queue", orderData);

    // Sepeti boşalt
    basket = [];

    res.json({ message: "Sipariş alındı ve işleme konuldu! 🚀", orderData });
});

app.listen(PORT, () => {
    console.log(`Sepet Servisi http://localhost:${PORT} adresinde hazır!`);
});