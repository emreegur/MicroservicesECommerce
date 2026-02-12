# 🛒 Event-Driven Microservices E-Commerce

Bu proje, modern **Mikroservis Mimarisi** prensipleri kullanılarak
geliştirilmiş, **Olay Güdümlü (Event-Driven)** bir E-Ticaret altyapısı
simülasyonudur.

Farklı teknolojilerin (**Polyglot Architecture**) bir arada nasıl
çalıştığını, **Docker** ile nasıl izole edildiğini ve **RabbitMQ**
üzerinden asenkron iletişim kurduğunu gösterir.

------------------------------------------------------------------------

## 🏗️ Mimari ve Akış (Architecture)

``` mermaid
graph LR
    User[Müşteri / Client] -- HTTP (Port 4000) --> Gateway[API Gateway (Ocelot)]
    
    subgraph Docker Network
        Gateway -- /products --> Product[Product Service (.NET)]
        Gateway -- /basket --> Basket[Basket Service (Node.js)]
        
        Basket -- "Sipariş Oluşturuldu" (Event) --> RabbitMQ((RabbitMQ))
        RabbitMQ --> Order[Order Service (.NET Worker)]
        
        Product --> DB[(PostgreSQL)]
        Order --> DB
    end
```

------------------------------------------------------------------------

## 🚀 Kullanılan Teknolojiler (Tech Stack)

-   **API Gateway:** Ocelot (.NET 10)
-   **Product Service:** .NET 10 Web API (Entity Framework Core)
-   **Basket Service:** Node.js (Express)
-   **Order Service:** .NET 10 Worker Service (Background Consumer)
-   **Message Broker:** RabbitMQ
-   **Database:** PostgreSQL
-   **Infrastructure:** Docker & Docker Compose

------------------------------------------------------------------------

## ✨ Özellikler

-   **Tek Kapı Politikası:** Tüm servisler dış dünyaya kapalıdır,
    iletişim sadece **API Gateway (Port 4000)** üzerinden sağlanır.
-   **Asenkron İletişim:** Sepet onaylandığında sipariş servisi anında
    meşgul edilmez, RabbitMQ üzerinden mesajlaşma sağlanır.
-   **Resilience (Dayanıklılık):** Servisler veritabanı veya RabbitMQ
    çökse bile tekrar bağlanmayı dener.
-   **Veri Kalıcılığı:** Docker kapatılsa bile veriler (Volume)
    sayesinde PostgreSQL'de saklanır.

------------------------------------------------------------------------

## 🛠️ Kurulum ve Çalıştırma

Bilgisayarınızda **Docker Desktop** yüklü olması yeterlidir.

### 1️⃣ Projeyi Klonlayın

git clone https://github.com/KULLANICI_ADIN/MicroservicesECommerce.git\
cd MicroservicesECommerce

> ⚠️ `KULLANICI_ADIN` kısmını kendi GitHub kullanıcı adınız ile
> değiştirin.

### 2️⃣ Sistemi Ayağa Kaldırın

docker-compose up --build

İlk çalıştırmada imajların indirilmesi 1--2 dakika sürebilir.

------------------------------------------------------------------------

## 🧪 Test Senaryoları (Endpoints)

### Ürün Ekleme

curl -X POST http://localhost:4000/products -H "Content-Type:
application/json" -d '{"name":"Gaming Laptop", "price":85000}'

### Sepete Ekleme

curl -X POST http://localhost:4000/basket -H "Content-Type:
application/json" -d '{"id":1, "name":"Gaming Laptop", "price":85000}'

### Checkout

curl -X POST http://localhost:4000/checkout

------------------------------------------------------------------------

## 📂 Proje Yapısı

-   /ApiGateway\
-   /ProductService\
-   /BasketService\
-   /OrderService\
-   docker-compose.yml

------------------------------------------------------------------------

## 👨‍💻 Developer

Emre Gür
