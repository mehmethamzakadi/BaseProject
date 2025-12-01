# Makefile Kullanım Rehberi

BaseProject için oluşturulmuş Makefile ve Windows Batch script'i ile projeyi kolayca yönetebilirsiniz.

## 📋 Kurulum

### Linux/macOS (Makefile)

Makefile doğrudan kullanılabilir:

```bash
# Yardım menüsünü göster
make help

# veya sadece
make
```

### Windows (make.bat)

Windows'ta `make.bat` dosyasını kullanın:

```cmd
# Yardım menüsünü göster
make.bat help

# veya sadece
make.bat
```

## 🚀 Hızlı Başlangıç

### Development Ortamı

```bash
# Tüm servisleri başlat (build ile)
make dev
# veya Windows'ta
make.bat dev

# Sadece başlat (build olmadan)
make dev-up
make.bat dev-up

# Sadece build
make dev-build
make.bat dev-build
```

### Production Ortamı

```bash
# Production ortamını başlat
make prod
make.bat prod

# ÖNEMLİ: Production için .env dosyası gerekli!
```

## 🛑 Servis Yönetimi

### Servisleri Durdurma

```bash
# Volume'ları koruyarak durdur (ÖNERİLEN)
make stop
make.bat stop

# Volume'ları da silerek durdur (DİKKAT!)
make down
make.bat down
```

**Not:** `make down` komutu tüm volume'ları siler. Bu işlem geri alınamaz!

### Servis Durumunu Kontrol Etme

```bash
# Çalışan servisleri listele
make ps
make.bat ps

# Detaylı durum bilgisi
make status
make.bat status

# Health check sonuçları
make health
```

## 📊 Log İşlemleri

```bash
# Tüm servislerin loglarını izle
make logs
make.bat logs

# Sadece API logları
make logs-api
make.bat logs-api

# Sadece Client logları
make logs-client
make.bat logs-client

# Sadece Database logları
make logs-db
make.bat logs-db
```

## 🔄 Migration İşlemleri

### Yeni Migration Oluşturma

```bash
# Migration adı belirtilerek
make migrate NAME=AddUserTable
make.bat migrate NAME=AddUserTable

# Windows'ta environment variable olarak
set NAME=AddUserTable
make.bat migrate
```

### Migration'ları Uygulama

```bash
# Tüm pending migration'ları uygula
make migrate-up
make.bat migrate-up
```

**Not:** API container başlatıldığında migration'lar otomatik olarak uygulanır.

### Migration Geri Alma

```bash
# Son migration'ı geri al (DİKKAT!)
make migrate-down
make.bat migrate-down
```

### Migration Listesi

```bash
# Uygulanmış migration'ları listele
make migrate-list
make.bat migrate-list
```

## 🐚 Container Shell İşlemleri

```bash
# API container'ına bağlan
make shell-api
make.bat shell-api

# Database container'ına bağlan (psql)
make shell-db
make.bat shell-db

# Client container'ına bağlan
make shell-client
make.bat shell-client
```

## 🤖 Ollama AI İşlemleri

```bash
# Model yükle (varsayılan: qwen2.5:7b)
make pull-ollama
make.bat pull-ollama

# Özel model yükle
make pull-ollama MODEL=qwen2.5:3b
make.bat pull-ollama MODEL=qwen2.5:3b

# Yüklü modelleri listele
make list-ollama
make.bat list-ollama
```

## 🧹 Temizleme İşlemleri

```bash
# Build cache'leri temizle
make clean
make.bat clean

# Tüm Docker kaynaklarını temizle (DİKKAT!)
make clean-all
make.bat clean-all
```

## 🔧 Özel İşlemler

```bash
# Servisleri rebuild et ve başlat
make rebuild
make.bat rebuild

# Testleri çalıştır
make test
make.bat test

# Database seed işlemi (genelde otomatik)
make seed
make.bat seed
```

## 📝 Örnek Kullanım Senaryoları

### Senaryo 1: İlk Kurulum

```bash
# 1. Development ortamını başlat
make dev

# 2. Ollama modelini yükle (opsiyonel)
make pull-ollama

# 3. Servis durumunu kontrol et
make status
```

### Senaryo 2: Yeni Migration Ekleme

```bash
# 1. Yeni migration oluştur
make migrate NAME=AddNewFeature

# 2. Migration'ı uygula (otomatik olarak API başlatıldığında uygulanır)
# veya manuel olarak:
make migrate-up

# 3. Migration listesini kontrol et
make migrate-list
```

### Senaryo 3: Servisleri Yeniden Başlatma

```bash
# 1. Servisleri durdur (volume'lar korunur)
make stop

# 2. Servisleri yeniden başlat
make dev-up

# veya tek komutla:
make restart
```

### Senaryo 4: Log İnceleme

```bash
# API'de bir sorun varsa
make logs-api

# Tüm servislerin loglarını izle
make logs
```

### Senaryo 5: Tam Temizlik (DİKKAT!)

```bash
# Tüm servisleri ve volume'ları sil
make down

# Tüm Docker kaynaklarını temizle
make clean-all

# Yeniden başlat
make dev
```

## ⚠️ Önemli Notlar

1. **Volume Yönetimi:**
   - `make stop`: Volume'lar korunur (veriler kaybolmaz)
   - `make down`: Volume'lar silinir (veriler kaybolur!)

2. **Migration Dosyaları:**
   - Migration'lar container içinde oluşturulur
   - Host'a kopyalamak için volume mount gerekir
   - Veya migration dosyalarını container'dan kopyalayın

3. **Production Ortamı:**
   - Production için `.env` dosyası zorunludur
   - `.env.example` dosyasını kopyalayarak oluşturun

4. **Windows Kullanımı:**
   - `make.bat` dosyasını kullanın
   - Environment variable'lar için `set` komutu kullanın
   - Örnek: `set NAME=MigrationName && make.bat migrate`

## 🆘 Sorun Giderme

### Container Çalışmıyor

```bash
# Container durumunu kontrol et
make ps

# Logları incele
make logs-api
```

### Migration Hatası

```bash
# Migration listesini kontrol et
make migrate-list

# Database container'ına bağlan
make shell-db

# Migration'ları manuel kontrol et
```

### Build Hatası

```bash
# Cache'leri temizle
make clean

# Yeniden build et
make dev-build
```

## 📚 Ek Kaynaklar

- [Docker Compose Dokümantasyonu](https://docs.docker.com/compose/)
- [EF Core Migrations](https://docs.microsoft.com/en-us/ef/core/managing-schemas/migrations/)
- [Makefile Dokümantasyonu](https://www.gnu.org/software/make/manual/)

---

**Not:** Bu Makefile ve batch script'i BaseProject projesine özeldir. Farklı projeler için uyarlamanız gerekebilir.

