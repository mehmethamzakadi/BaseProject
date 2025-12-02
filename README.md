# BaseProject

<div align="center">

![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=for-the-badge&logo=dotnet&logoColor=white)
![React](https://img.shields.io/badge/React-18.3-61DAFB?style=for-the-badge&logo=react&logoColor=black)
![PostgreSQL](https://img.shields.io/badge/PostgreSQL-16-4169E1?style=for-the-badge&logo=postgresql&logoColor=white)
![Redis](https://img.shields.io/badge/Redis-Latest-DC382D?style=for-the-badge&logo=redis&logoColor=white)
![RabbitMQ](https://img.shields.io/badge/RabbitMQ-3-FF6600?style=for-the-badge&logo=rabbitmq&logoColor=white)
![Docker](https://img.shields.io/badge/Docker-Ready-2496ED?style=for-the-badge&logo=docker&logoColor=white)

**Modern, ölçeklenebilir ve güvenli proje temeli (Base Project Template)**

[Özellikler](#-özellikler) •
[Mimari](#-mimari) •
[Kurulum](#-kurulum) •
[API Dokümantasyonu](#-api-dokümantasyonu) •
[Geliştirme](#-geliştirme)

</div>

---

## 📋 Genel Bakış

BaseProject, **Clean Architecture** ve **Domain-Driven Design (DDD)** prensiplerine dayalı, kurumsal düzeyde bir proje temelidir. Modern teknolojiler ve en iyi pratikler kullanılarak geliştirilmiştir. Yeni projeleriniz için temel olarak kullanabileceğiniz, tam özellikli bir başlangıç şablonudur.

## ✨ Özellikler

### Backend

- 🏗️ **Clean Architecture** - Katmanlı mimari ile sürdürülebilir kod
- 📦 **DDD (Domain-Driven Design)** - Aggregate Root, Value Objects, Domain Events
- 🔄 **CQRS Pattern** - MediatR ile Command/Query ayrımı
- 🔐 **JWT Authentication** - Access Token & Refresh Token rotation
- 🛡️ **Permission-Based Authorization** - Granüler yetkilendirme sistemi
- 📬 **Outbox Pattern** - Güvenilir mesaj iletimi (RabbitMQ)
- ⚡ **Redis Caching** - Dağıtık önbellek desteği
- 📊 **Activity Logging** - Detaylı aktivite takibi
- 🔒 **Rate Limiting** - DDoS koruması
- 📝 **Serilog** - Yapılandırılmış loglama (Console, File, PostgreSQL, Seq)
  - Docker ve Local ortam desteği
  - Ortam bazlı log seviyesi optimizasyonu (Development: Information, Production: Warning)
- 📊 **OpenTelemetry** - Dağıtık sistem takibi ve observability
  - HTTP Request, EF Core, MassTransit (RabbitMQ) tracing
  - Metrics ve Logs entegrasyonu
  - **Jaeger Integration** - Trace görselleştirme arayüzü
- 🤖 **AI-Powered Features** - Ollama (Qwen 2.5:7b) ile yapay zeka destekli özellikler
  - AI ile kategori açıklaması üretme
- 🔄 **Resilience Patterns** - Polly retry policy ile dayanıklı HTTP istekleri

### Frontend

- ⚛️ **React 18** - Modern UI framework
- 📘 **TypeScript** - Tip güvenli geliştirme
- 🎨 **Tailwind CSS** - Utility-first CSS framework
- 🔄 **TanStack Query** - Server state management
- 🐻 **Zustand** - Client state management
- 📝 **React Hook Form + Zod** - Form validation
- 🚀 **Vite** - Hızlı build tool

### DevOps

- 🐳 **Docker & Docker Compose** - Container orchestration
- 🔄 **CI/CD Ready** - Pipeline hazır yapı
- 📈 **Seq Integration** - Merkezi log yönetimi ve analizi
- 🔍 **Jaeger Integration** - Distributed tracing ve görselleştirme
- 🤖 **Ollama Integration** - Docker container'da AI model desteği

---

## 🏛️ Mimari

```
┌─────────────────────────────────────────────────────────────────┐
│                        Presentation Layer                        │
│  ┌─────────────────┐  ┌─────────────────┐  ┌─────────────────┐  │
│  │ BaseProject.API │  │  React Client   │  │    Swagger UI   │  │
│  └────────┬────────┘  └────────┬────────┘  └─────────────────┘  │
└───────────┼────────────────────┼────────────────────────────────┘
            │                    │
┌───────────▼────────────────────▼────────────────────────────────┐
│                       Application Layer                          │
│  ┌─────────────────────────────────────────────────────────┐    │
│  │           BaseProject.Application                        │    │
│  │  • Commands & Queries (CQRS)                            │    │
│  │  • Validators (FluentValidation)                        │    │
│  │  • Behaviors (Logging, Validation, Caching)             │    │
│  │  • AutoMapper Profiles                                  │    │
│  └─────────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────────┘
            │
┌───────────▼─────────────────────────────────────────────────────┐
│                         Domain Layer                             │
│  ┌─────────────────────────────────────────────────────────┐    │
│  │            BaseProject.Domain                            │    │
│  │  • Entities (User, Category, Role, etc.)                │    │
│  │  • Value Objects (Email, UserName)                      │    │
│  │  • Domain Events                                        │    │
│  │  • Repository Interfaces                                │    │
│  │  • Domain Services                                      │    │
│  └─────────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────────┘
            │
┌───────────▼─────────────────────────────────────────────────────┐
│                     Infrastructure Layer                         │
│  ┌──────────────────────┐  ┌──────────────────────┐             │
│  │BaseProject.Infrastructure│  │BaseProject.Persistence  │             │
│  │ • JWT Token Service   │  │ • EF Core DbContext  │             │
│  │ • Email Service       │  │ • Repositories       │             │
│  │ • Redis Cache         │  │ • Unit of Work       │             │
│  │ • RabbitMQ/MassTransit│  │ • Migrations         │             │
│  │ • Background Services │  │ • Seeders            │             │
│  └──────────────────────┘  └──────────────────────┘             │
└─────────────────────────────────────────────────────────────────┘
```

### Klasör Yapısı

```
BaseProject/
├── src/
│   ├── BaseProject.API/                 # REST API & Controllers
│   │   ├── Controllers/
│   │   ├── Middlewares/
│   │   ├── Filters/
│   │   └── Configuration/
│   ├── BaseProject.Application/         # Business Logic
│   │   ├── Features/
│   │   │   ├── Categories/
│   │   │   ├── Users/
│   │   │   ├── Roles/
│   │   │   ├── Auths/
│   │   │   └── Dashboards/
│   │   ├── Behaviors/
│   │   └── Abstractions/
│   ├── BaseProject.Domain/              # Core Domain
│   │   ├── Entities/
│   │   ├── ValueObjects/
│   │   ├── Events/
│   │   ├── Repositories/
│   │   └── Services/
│   ├── BaseProject.Infrastructure/      # External Services
│   │   ├── Services/
│   │   ├── Models/
│   │   │   └── Ollama/                  # AI API Models
│   │   ├── Consumers/
│   │   └── Authorization/
│   └── BaseProject.Persistence/         # Data Access
│       ├── Contexts/
│       ├── Repositories/
│       ├── Configurations/
│       └── Migrations/
├── clients/
│   └── baseproject-client/              # React Frontend
│       ├── src/
│       │   ├── components/
│       │   ├── features/
│       │   ├── hooks/
│       │   ├── pages/
│       │   └── stores/
│       └── ...
├── tests/
│   ├── Domain.UnitTests/
│   └── Application.UnitTests/
├── docs/                            # Documentation
└── deploy/                          # Docker & Nginx configs
```

---

## 🚀 Kurulum

### Gereksinimler

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Node.js 20+](https://nodejs.org/)
- [Docker & Docker Compose](https://www.docker.com/)
- [PostgreSQL 16](https://www.postgresql.org/) (Docker ile otomatik)
- [Redis](https://redis.io/) (Docker ile otomatik)
- [RabbitMQ](https://www.rabbitmq.com/) (Docker ile otomatik)
- [Ollama](https://ollama.ai/) (Docker ile otomatik - AI model desteği için)

### Docker ile Hızlı Başlangıç

```bash
# Repository'yi klonla
git clone https://github.com/mehmethamzakadi/BaseProject.git
cd BaseProject

# Tüm servisleri başlat (Backend + Frontend + Tüm servisler)
docker compose -f docker-compose.yml -f docker-compose.local.yml up --build -d

# Veya sadece local dosyası ile
docker compose -f docker-compose.local.yml up --build -d

# Ollama modelini yükle (ilk kez - AI özellikleri için opsiyonel)
docker exec -it baseproject_ollama_dev ollama pull qwen2.5:7b

# Logları izle
docker compose -f docker-compose.local.yml logs -f baseproject.api
docker compose -f docker-compose.local.yml logs -f baseproject.client

# Servislerin durumunu kontrol et
docker compose -f docker-compose.local.yml ps
```

**Erişim URL'leri:**

- **Frontend (React Client):** http://localhost:5173
- **Backend API:** http://localhost:6060
- **API Dokümantasyonu (Scalar):** http://localhost:6060/scalar/v1
- **Seq Log Viewer:** http://localhost:5341
- **Jaeger Tracing UI:** http://localhost:16686
- **RabbitMQ Management:** http://localhost:15672
- **Ollama API:** http://localhost:11434

### Manuel Kurulum (Docker Olmadan)

#### 1. Veritabanı ve Servisleri Başlat

```bash
# Sadece bağımlılık servislerini başlat
docker-compose -f docker-compose.local.yml up -d postgresdb redis.cache rabbitmq seq ollama

# Ollama modelini yükle (ilk kez)
docker exec -it baseproject_ollama_dev ollama pull qwen2.5:7b
```

#### 2. Backend'i Çalıştır

```bash
cd src/BaseProject.API

# User secrets ayarla (ilk kez)
dotnet user-secrets set "ConnectionStrings:BaseProjectPostgreConnectionString" "Host=localhost;Port=5435;Database=BaseProjectDb;Username=postgres;Password=postgres"
dotnet user-secrets set "ConnectionStrings:RedisCache" "localhost:6379"
dotnet user-secrets set "TokenOptions:SecurityKey" "your-super-secret-key-here-at-least-32-chars!"

# Uygulamayı çalıştır
dotnet run
```

#### 3. Frontend'i Çalıştır

```bash
cd clients/baseproject-client

# Bağımlılıkları yükle
npm install

# Environment variables otomatik yüklenir (.env.development)
# Gerekirse clients/baseproject-client/.env.development dosyasını güncelleyin
# VITE_API_URL=http://localhost:6060 (Docker API için)
# VITE_API_URL=http://localhost:5285 (Local .NET için)

# Development server başlat
npm run dev
```

**Not:** Docker ile çalıştırırken frontend otomatik olarak build edilir ve Nginx ile serve edilir. Manuel kurulum sadece development için önerilir.

### Environment Variables

Proje kök dizininde ortam bazlı `.env` dosyaları kullanılır:

#### Development Ortamı (`.env.development`)

```bash
# Development için hazır değerlerle gelir
cp .env.example .env.development
# Gerekirse değerleri güncelleyin
```

#### Production Ortamı (`.env.production`)

```bash
# Production için .env.production dosyasını oluşturun
cp .env.example .env.production
# ÖNEMLİ: Tüm değerleri production ortamınıza göre güncelleyin!
```

#### Environment Variables Listesi

| Değişken                | Açıklama           | Development             | Production               |
| ----------------------- | ------------------ | ----------------------- | ------------------------ |
| `POSTGRES_DB`           | Veritabanı adı     | `BaseProjectDb`         | `BaseProjectDb`          |
| `POSTGRES_USER`         | DB kullanıcı adı   | `postgres`              | `baseproject_user`       |
| `POSTGRES_PASSWORD`     | DB şifresi         | `postgres`              | **Güçlü şifre**          |
| `RABBITMQ_DEFAULT_USER` | RabbitMQ kullanıcı | `baseproject`           | `baseproject`            |
| `RABBITMQ_DEFAULT_PASS` | RabbitMQ şifre     | `supersecret`           | **Güçlü şifre**          |
| `REDIS_PASSWORD`        | Redis şifre        | (boş)                   | **Güçlü şifre**          |
| `SEQ_ADMIN_PASSWORD`    | Seq admin şifre    | `Admin123!`             | **Güçlü şifre**          |
| `TOKEN_SECURITY_KEY`    | JWT secret key     | `DevSecretKey...`       | **32+ karakter**         |
| `APP_URL`               | Uygulama URL       | `http://localhost:5173` | `https://yourdomain.com` |

**ÖNEMLİ:** Production ortamında mutlaka güçlü şifreler ve secret key'ler kullanın!

#### .NET Environment Variables

| Değişken                                                | Açıklama                      | Varsayılan                                                         |
| ------------------------------------------------------- | ----------------------------- | ------------------------------------------------------------------ |
| `ASPNETCORE_ENVIRONMENT`                                | Ortam                         | `Development`                                                      |
| `ConnectionStrings__BaseProjectPostgreConnectionString` | PostgreSQL bağlantısı         | -                                                                  |
| `ConnectionStrings__RedisCache`                         | Redis bağlantısı              | -                                                                  |
| `TokenOptions__SecurityKey`                             | JWT secret key                | -                                                                  |
| `RabbitMQOptions__HostName`                             | RabbitMQ host                 | `localhost`                                                        |
| `RabbitMQOptions__UserName`                             | RabbitMQ kullanıcı            | `baseproject`                                                      |
| `RabbitMQOptions__Password`                             | RabbitMQ şifre                | -                                                                  |
| `OllamaOptions__Endpoint`                               | Ollama API endpoint           | `http://localhost:11434`                                           |
| `OllamaOptions__ModelId`                                | Ollama model ID               | `qwen2.5:7b`                                                       |
| `OllamaOptions__TimeoutMinutes`                         | HTTP timeout (dakika)         | `2`                                                                |
| `OllamaOptions__RetryCount`                             | Retry sayısı                  | `3`                                                                |
| `OllamaOptions__RetryDelaySeconds`                      | Retry gecikmesi (saniye)      | `2`                                                                |
| `Serilog__SeqUrl`                                       | Seq log server URL            | `http://seq:80` (Docker) / `http://localhost:5341` (Local)         |
| `Serilog__SeqApiKey`                                    | Seq API key (opsiyonel)       | -                                                                  |
| `OTEL_EXPORTER_OTLP_ENDPOINT`                           | OpenTelemetry OTLP endpoint   | `http://jaeger:4317` (Docker) / `http://localhost:4317` (Local)    |
| `OTEL_EXPORTER_OTLP_PROTOCOL`                           | OTLP protokol (grpc/http)     | `grpc`                                                             |
| `VITE_API_URL`                                          | Frontend API URL (build-time) | `http://localhost:6060` (Docker) / `http://localhost:5285` (Local) |

---

## 📚 API Dokümantasyonu

### Endpoints

API başladığında Scalar UI üzerinden dokümantasyona erişebilirsiniz:

```
http://localhost:5000/scalar/v1
```

### Ana Endpoint'ler

| Endpoint                             | Method | Açıklama                        | Auth |
| ------------------------------------ | ------ | ------------------------------- | ---- |
| `/api/auth/login`                    | POST   | Kullanıcı girişi                | ❌   |
| `/api/auth/register`                 | POST   | Kullanıcı kaydı                 | ❌   |
| `/api/auth/refresh-token`            | POST   | Token yenileme                  | ❌   |
| `/api/category`                      | GET    | Kategori listesi                | ❌   |
| `/api/category/generate-description` | GET    | AI ile kategori açıklaması üret | ✅   |
| `/api/user`                          | GET    | Kullanıcı listesi               | ✅   |
| `/api/role`                          | GET    | Rol listesi                     | ✅   |
| `/api/Dashboards/statistics`         | GET    | Dashboard istatistikleri        | ✅   |
| `/api/ActivityLogs/search`           | POST   | Aktivite logları                | ✅   |

### Örnek İstekler

#### Login

```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email": "admin@baseproject.com", "password": "Admin123!"}'
```

#### Kategori Oluşturma

```bash
curl -X POST http://localhost:5000/api/Category \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer {token}" \
  -d '{
    "name": "Yeni Kategori",
    "description": "Kategori açıklaması"
  }'
```

#### AI ile Kategori Açıklaması Üretme

```bash
curl -X GET "http://localhost:5000/api/category/generate-description?categoryName=Teknoloji" \
  -H "Authorization: Bearer {token}"
```

---

## 🛠️ Geliştirme

### Makefile ile Hızlı Başlangıç (Önerilen)

Projeyi yönetmek için Makefile veya Windows batch script'i kullanabilirsiniz:

```bash
# Linux/macOS
make help          # Tüm komutları göster
make dev           # Development ortamını başlat
make stop          # Servisleri durdur (volume'lar korunur)
make down          # Servisleri durdur ve volume'ları sil

# Windows PowerShell
.\make.bat help    # Tüm komutları göster (PowerShell'de .\ gerekli)
.\make.bat dev     # Development ortamını başlat
.\make.bat stop    # Servisleri durdur (volume'lar korunur)

# Windows Command Prompt (CMD)
make.bat help      # Tüm komutları göster
make.bat dev       # Development ortamını başlat
make.bat stop      # Servisleri durdur (volume'lar korunur)
```

**⚠️ Windows PowerShell'de:** Current directory'deki script'leri çalıştırmak için `.\` prefix'i gereklidir.

**Detaylı kullanım için:** [QUICK_START.md](QUICK_START.md) (Windows için) | [Makefile Kullanım Rehberi](README_MAKEFILE.md)

### Geliştirme Ortamı Kurulumu

```bash
# Repository'yi klonla
git clone https://github.com/mehmethamzakadi/BaseProject.git
cd BaseProject

# Makefile ile (ÖNERİLEN)
make dev

# Veya manuel olarak
docker-compose -f docker-compose.yml -f docker-compose.local.yml up --build -d

# API'yi manuel çalıştırmak için
cd src/BaseProject.API
dotnet watch run
```

### Migration Oluşturma

```bash
# Makefile ile (ÖNERİLEN - Docker container içinde)
make migrate NAME=MigrationName
# veya Windows'ta
make.bat migrate NAME=MigrationName

# Migration'ları uygula
make migrate-up
make.bat migrate-up

# Migration listesini göster
make migrate-list
make.bat migrate-list

# Manuel olarak (local development)
cd src/BaseProject.API
dotnet ef migrations add MigrationName -p ../BaseProject.Persistence -o Migrations/PostgreSql
dotnet ef database update -p ../BaseProject.Persistence
```

### Testleri Çalıştırma

```bash
# Tüm testleri çalıştır
dotnet test

# Coverage raporu ile
dotnet test --collect:"XPlat Code Coverage"
```

### Kod Kalitesi

```bash
# Format kontrolü
dotnet format --verify-no-changes

# Analyzer çalıştır
dotnet build /p:TreatWarningsAsErrors=true
```

---

## 📊 Monitoring ve Servis Erişimleri

### Frontend (React Client)

```
http://localhost:5173
```

Docker ile çalıştırıldığında Nginx üzerinden serve edilir. Production build otomatik olarak yapılır.

### Backend API

```
http://localhost:6060
```

API dokümantasyonu: http://localhost:6060/scalar/v1

### Seq Log Viewer

```
http://localhost:5341
```

Varsayılan şifre: `Admin123!`

**Not:** Docker ortamında API otomatik olarak Seq'e log gönderir. Local development için Seq'u Docker'da çalıştırın:

```bash
docker run -d -p 5341:80 -e ACCEPT_EULA=Y datalust/seq:latest
```

### Jaeger - Distributed Tracing UI

```
http://localhost:16686
```

OpenTelemetry trace'lerini görselleştirmek için Jaeger UI kullanılır. API'den gelen tüm HTTP istekleri, EF Core sorguları ve RabbitMQ mesajları otomatik olarak trace edilir.

**Özellikler:**

- HTTP Request/Response tracing
- EF Core query tracing
- MassTransit (RabbitMQ) message tracing
- Trace ID correlation (loglarla bağlantılı)
- Service map ve dependency görselleştirme

**Docker Ortamında:**

- Otomatik olarak `http://jaeger:4317` üzerinden trace gönderilir

**Local Development:**

- Jaeger'ı Docker'da çalıştırın: `docker run -d -p 16686:16686 -p 4317:4317 -p 4318:4318 -e COLLECTOR_OTLP_ENABLED=true jaegertracing/all-in-one:latest`
- API `http://localhost:4317` üzerinden trace gönderir

### RabbitMQ Management

```
http://localhost:15672
```

Kullanıcı/Şifre: `baseproject/supersecret`

### Ollama AI Service (Opsiyonel)

Ollama servisi Docker container'ında çalışmaktadır ve AI özellikleri için kullanılır. **Önemli:** API Ollama olmadan da çalışabilir, AI özellikleri opsiyoneldir. Model yükleme ve yönetimi için:

```bash
# Model listesi
docker exec baseproject_ollama_dev ollama list

# Yeni model yükle
docker exec baseproject_ollama_dev ollama pull qwen2.5:7b

# Model sil
docker exec baseproject_ollama_dev ollama rm qwen2.5:7b
```

API Endpoint: `http://localhost:11434`

### Redis Commander (Opsiyonel)

```bash
docker run -d -p 8081:8081 --name redis-commander \
  -e REDIS_HOSTS=local:redis.cache:6379 \
  rediscommander/redis-commander
```

---

## 🔐 Güvenlik

- **JWT Token Rotation:** Access ve Refresh token mekanizması
- **Password Hashing:** PBKDF2 ile güvenli şifre saklama
- **Rate Limiting:** IP bazlı istek sınırlama
- **CORS Policy:** Yapılandırılabilir origin kontrolü
- **SQL Injection:** Parametreli sorgular (EF Core)
- **XSS Protection:** Input validation ve sanitization
- **AI Service Security:** Timeout ve retry mekanizmaları ile güvenli API çağrıları
- **Permission-Based AI Features:** AI özellikleri permission kontrolü ile korunuyor

---

## 🤝 Katkıda Bulunma

1. Fork yapın
2. Feature branch oluşturun (`git checkout -b feature/amazing-feature`)
3. Commit yapın (`git commit -m 'feat: Add amazing feature'`)
4. Push yapın (`git push origin feature/amazing-feature`)
5. Pull Request açın

### Commit Mesajları

[Conventional Commits](https://www.conventionalcommits.org/) standardını kullanın:

- `feat:` Yeni özellik
- `fix:` Bug düzeltmesi
- `docs:` Dokümantasyon
- `refactor:` Kod iyileştirmesi
- `test:` Test ekleme
- `chore:` Bakım işleri

---

## 📄 Lisans

Bu proje MIT lisansı altında lisanslanmıştır. Detaylar için [LICENSE](LICENSE) dosyasına bakın.

---

## 📞 İletişim

- **Proje Sahibi:** Mehmet Hamza Kadi
- **GitHub:** [@mehmethamzakadi](https://github.com/mehmethamzakadi)

---

<div align="center">

**BaseProject** ile ❤️ yapıldı

[⬆ Başa Dön](#baseproject)

</div>
