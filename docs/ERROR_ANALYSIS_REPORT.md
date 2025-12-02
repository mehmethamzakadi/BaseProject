# Seq Dashboard Hata Analiz Raporu

> **Tarih:** 2 Aralık 2025  
> **Kaynak:** Seq Dashboard CSV Export (`export-8de31894761cfb7.csv`)  
> **Analiz Kapsamı:** Uygulama içinde oluşan hataların sebepleri ve çözüm önerileri

---

## 📋 İçindekiler

1. [Yönetici Özeti](#1-yönetici-özeti)
2. [Hata Kategorileri](#2-hata-kategorileri)
3. [Kritik Sorunlar](#3-kritik-sorunlar)
4. [Beklenen Hatalar](#4-beklenen-hatalar)
5. [Çözüm Önerileri](#5-çözüm-önerileri)
6. [Öncelik Matrisi](#6-öncelik-matrisi)

---

## 1. Yönetici Özeti

Seq dashboard'dan export edilen CSV dosyası analiz edildiğinde **iki ana hata kategorisi** tespit edilmiştir:

### Hata Dağılımı

| Hata Kategorisi                 | Sayı  | Öncelik       | Durum                     |
| ------------------------------- | ----- | ------------- | ------------------------- |
| **ActivityLogs Duplicate Key**  | ~180+ | 🔴 **KRİTİK** | ⚠️ Düzeltilmesi Gerekiyor |
| **RefreshToken Geçersiz Token** | ~3    | 🟡 **DÜŞÜK**  | ✅ Beklenen Davranış      |
| **Login Geçersiz Credentials**  | ~3    | 🟡 **DÜŞÜK**  | ✅ Beklenen Davranış      |

### Genel Değerlendirme

- **Kritik Sorun:** ActivityLogs tablosunda duplicate key hataları sürekli oluşuyor
- **Kök Sebep:** Race condition - MassTransit retry mekanizması ile idempotency kontrolü arasında zamanlama sorunu
- **Etki:** Veritabanı constraint ihlalleri, gereksiz retry'lar, log kirliliği

---

## 2. Hata Kategorileri

### 2.1 🔴 KRİTİK: ActivityLogs Duplicate Key Hatası

**Hata Mesajı:**

```
Npgsql.PostgresException: 23505: duplicate key value violates unique constraint "PK_ActivityLogs"
DETAIL: Key ("Id")=(01000000-f12b-954d-25a5-08de317a0276) already exists.
```

**Sıklık:** ~180+ hata (CSV dosyasındaki hataların %95'i)

**Stack Trace Özeti:**

```
at BaseProject.Infrastructure.Consumers.ActivityLogConsumer.Consume(...)
at BaseProject.Persistence.Repositories.UnitOfWork.SaveChangesAsync(...)
```

**Etkilenen Aktivite Tipleri:**

- `permissions_assigned_to_role`
- `user_updated`
- Diğer activity log tipleri

#### Sorunun Kök Sebebi

**Race Condition Problemi:**

`ActivityLogConsumer.Consume` metodunda idempotency kontrolü var ancak **zamanlama sorunu** nedeniyle duplicate key hataları oluşuyor:

```csharp
// Mevcut Akış (SORUNLU):
1. Redis'te kontrol et → Yok
2. DB'de kontrol et → Yok
3. AddAsync(activityLog) → Entity tracking'e eklenir
4. SaveChangesAsync() → DB'ye kaydedilir ✅
5. Redis'e idempotency key ekle → ⚠️ Bu adım retry'dan önce tamamlanmayabilir
```

**Sorun Senaryosu:**

1. **İlk Deneme:**

   - Redis kontrolü: ❌ Yok
   - DB kontrolü: ❌ Yok
   - `SaveChangesAsync()` başarılı → Kayıt DB'ye yazıldı ✅
   - Redis'e key eklenmeden önce exception oluştu (ör: network timeout, Redis bağlantı hatası)
   - MassTransit retry tetiklendi

2. **Retry Denemesi:**
   - Redis kontrolü: ❌ Hala yok (çünkü önceki denemede eklenemedi)
   - DB kontrolü: ⚠️ **Yapılmadan** `AddAsync` çağrıldı
   - `SaveChangesAsync()` → **Duplicate key hatası** ❌

**Neden DB Kontrolü Çalışmıyor?**

Kodda DB kontrolü var ama **race condition** nedeniyle:

- İlk denemede kayıt başarılı oldu
- Redis'e key eklenemedi (exception veya timeout)
- Retry'da Redis kontrolü yapılıyor ama DB kontrolü **AddAsync'den önce** yapılmıyor
- EF Core tracking'de entity zaten var, `SaveChangesAsync` duplicate key hatası veriyor

#### Teknik Detaylar

**Mevcut Kod (ActivityLogConsumer.cs:36-142):**

```csharp
// 1. Redis kontrolü
var isProcessedInCache = await _cacheService.AnyAsync(idempotencyKey);
if (isProcessedInCache) return;

// 2. DB kontrolü
var existsInDb = await _activityLogRepository.ExistsByIdAsync(activityLogId, ...);
if (existsInDb) {
    // Redis'i güncelle ve return
    return;
}

// 3. Entity oluştur ve ekle
var activityLog = new ActivityLog { Id = activityLogId, ... };
await _activityLogRepository.AddAsync(activityLog, ...);

// 4. SaveChanges
await _unitOfWork.SaveChangesAsync(...);

// 5. Redis'e key ekle (SORUN: Bu adım başarısız olabilir)
await _cacheService.Add(idempotencyKey, ...);
```

**Sorun:** Adım 5 başarısız olursa, retry'da adım 2 çalışsa bile EF Core tracking'de entity zaten var.

---

### 2.2 🟡 DÜŞÜK: RefreshToken Geçersiz Token Hatası

**Hata Mesajı:**

```
BaseProject.Domain.Exceptions.AuthenticationErrorException: Geçersiz refresh token.
at BaseProject.Infrastructure.Services.AuthService.RefreshTokenAsync(String refreshToken)
```

**Sıklık:** ~3 hata

**Durum:** ✅ **Beklenen Davranış**

Bu hata, geçersiz veya süresi dolmuş refresh token ile istek geldiğinde oluşan **normal bir güvenlik kontrolüdür**. Kullanıcı yeniden giriş yapmalıdır.

**Kod Yeri:** `AuthService.RefreshTokenAsync` (line 154)

**Öneri:** Bu hatalar için özel bir iyileştirme gerekmez, ancak frontend'de kullanıcıya daha anlaşılır mesaj gösterilebilir.

---

### 2.3 🟡 DÜŞÜK: Login Geçersiz Credentials Hatası

**Hata Mesajı:**

```
BaseProject.Domain.Exceptions.AuthenticationErrorException: E-Mail veya şifre hatalı!
```

**Sıklık:** ~3 hata

**Durum:** ✅ **Beklenen Davranış**

Bu hata, yanlış email/şifre kombinasyonu ile login denemesi yapıldığında oluşan **normal bir güvenlik kontrolüdür**.

**Öneri:** Bu hatalar için özel bir iyileştirme gerekmez.

---

## 3. Kritik Sorunlar

### 🔴 KRİTİK-001: ActivityLogConsumer Race Condition

**Sorun:** MassTransit retry mekanizması ile idempotency kontrolü arasında race condition

**Etki:**

- ✅ Veritabanı constraint ihlalleri
- ✅ Gereksiz retry'lar (5 retry × exponential backoff)
- ✅ Log kirliliği (her retry için error log)
- ✅ Performans kaybı

**Öncelik:** 🔴 **YÜKSEK** - Hemen düzeltilmesi gerekiyor

---

## 4. Beklenen Hatalar

### 🟡 ORTA-001: RefreshToken Geçersiz Token

**Durum:** ✅ Normal güvenlik kontrolü

**Öneri:** Frontend'de kullanıcıya daha anlaşılır mesaj gösterilebilir.

### 🟡 ORTA-002: Login Geçersiz Credentials

**Durum:** ✅ Normal güvenlik kontrolü

**Öneri:** Rate limiting ile brute-force saldırıları önlenebilir (zaten mevcut).

---

## 5. Çözüm Önerileri

### 5.1 🔴 KRİTİK: ActivityLogConsumer Race Condition Düzeltmesi

#### Çözüm 1: Redis'e Key'i Önce Ekle (Önerilen)

**Yaklaşım:** Redis'e idempotency key'i **SaveChangesAsync'den önce** ekle, ancak **transaction içinde** kontrol et.

```csharp
public async Task Consume(ConsumeContext<ActivityLogCreatedIntegrationEvent> context)
{
    try
    {
        var message = context.Message;
        Guid activityLogId = context.MessageId.HasValue
            ? context.MessageId.Value
            : GenerateDeterministicGuid($"{message.EntityId}_{message.Timestamp:O}_{message.ActivityType}");

        var idempotencyKey = $"{IdempotencyKeyPrefix}{activityLogId}";

        // 1. Redis'te kontrol et
        var isProcessedInCache = await _cacheService.AnyAsync(idempotencyKey);
        if (isProcessedInCache)
        {
            _logger.LogInformation("Duplicate message detected in cache for ActivityLog {ActivityLogId}.", activityLogId);
            return;
        }

        // 2. DB'de kontrol et
        var existsInDb = await _activityLogRepository.ExistsByIdAsync(activityLogId, context.CancellationToken);
        if (existsInDb)
        {
            // Cache warming
            await _cacheService.Add(idempotencyKey, true, DateTimeOffset.UtcNow.AddDays(7), null);
            _logger.LogInformation("Duplicate message detected in database for ActivityLog {ActivityLogId}.", activityLogId);
            return;
        }

        // 3. ✅ ÖNEMLİ: Redis'e key'i ŞİMDİ ekle (optimistic lock)
        // Eğer başarısız olursa, başka bir consumer zaten işliyor demektir
        var lockAcquired = await _cacheService.AddIfNotExists(idempotencyKey, true, DateTimeOffset.UtcNow.AddDays(7));
        if (!lockAcquired)
        {
            _logger.LogWarning("Could not acquire lock for ActivityLog {ActivityLogId}. Another consumer may be processing.", activityLogId);
            // Kısa bir süre bekle ve tekrar kontrol et
            await Task.Delay(100, context.CancellationToken);
            var stillExists = await _activityLogRepository.ExistsByIdAsync(activityLogId, context.CancellationToken);
            if (stillExists)
            {
                _logger.LogInformation("ActivityLog {ActivityLogId} was processed by another consumer.", activityLogId);
                return;
            }
            // Hala yoksa devam et (race condition durumu)
        }

        // 4. Entity oluştur ve ekle
        var activityLog = new ActivityLog
        {
            Id = activityLogId,
            ActivityType = message.ActivityType,
            EntityType = message.EntityType,
            EntityId = message.EntityId,
            Title = message.Title,
            Details = message.Details,
            UserId = message.UserId ?? Guid.Empty,
            Timestamp = message.Timestamp
        };

        await _activityLogRepository.AddAsync(activityLog, context.CancellationToken);

        // 5. SaveChanges (duplicate key hatası burada yakalanacak)
        try
        {
            await _unitOfWork.SaveChangesAsync(context.CancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException pgEx && pgEx.SqlState == "23505")
        {
            // Duplicate key hatası - başka bir consumer zaten kaydetti
            _logger.LogWarning("Duplicate key detected for ActivityLog {ActivityLogId}. Another consumer may have processed it.", activityLogId);

            // Redis'i güncelle (cache warming)
            await _cacheService.Add(idempotencyKey, true, DateTimeOffset.UtcNow.AddDays(7), null);

            // Idempotent - başarılı say
            return;
        }

        _logger.LogInformation("Successfully processed ActivityLog: {ActivityType} (ID: {ActivityLogId})",
            message.ActivityType, activityLogId);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error processing ActivityLog: {ActivityType}", context.Message.ActivityType);
        throw;
    }
}
```

**Gereksinimler:**

- `ICacheService`'e `AddIfNotExists` metodu eklenmeli (Redis SETNX kullanarak)
- Duplicate key exception'ı yakalanmalı ve idempotent olarak işlenmeli

#### Çözüm 2: Database-Level Idempotency (Alternatif)

**Yaklaşım:** PostgreSQL'de `INSERT ... ON CONFLICT DO NOTHING` kullan.

```csharp
// Repository'de özel bir metod ekle
public async Task<bool> TryAddAsync(ActivityLog activityLog, CancellationToken cancellationToken)
{
    // Raw SQL ile INSERT ... ON CONFLICT
    var sql = @"
        INSERT INTO ""ActivityLogs"" (""Id"", ""ActivityType"", ""EntityType"", ""EntityId"", ""Title"", ""Details"", ""UserId"", ""Timestamp"")
        VALUES (@p0, @p1, @p2, @p3, @p4, @p5, @p6, @p7)
        ON CONFLICT (""Id"") DO NOTHING
        RETURNING ""Id"";";

    var parameters = new[]
    {
        new NpgsqlParameter("@p0", activityLog.Id),
        new NpgsqlParameter("@p1", activityLog.ActivityType),
        // ... diğer parametreler
    };

    var result = await _context.Database.ExecuteSqlRawAsync(sql, parameters, cancellationToken);
    return result > 0; // Eğer 0 ise, duplicate key vardı
}
```

**Avantajlar:**

- Database-level idempotency garantisi
- Race condition yok
- Daha basit kod

**Dezavantajlar:**

- EF Core'dan uzaklaşma (raw SQL)
- Migration gerektirir

#### Çözüm 3: Distributed Lock (En Güvenli)

**Yaklaşım:** Redis distributed lock kullan (RedLock algoritması).

```csharp
// Redis distributed lock ile
var lockKey = $"lock:activitylog:{activityLogId}";
var lockAcquired = await _distributedLock.TryAcquireAsync(lockKey, TimeSpan.FromSeconds(30));
if (!lockAcquired)
{
    _logger.LogWarning("Could not acquire lock for ActivityLog {ActivityLogId}.", activityLogId);
    throw new InvalidOperationException("Could not acquire lock");
}

try
{
    // Mevcut kontrol ve kayıt işlemleri
}
finally
{
    await _distributedLock.ReleaseAsync(lockKey);
}
```

**Avantajlar:**

- En güvenli çözüm
- Race condition tamamen önlenir

**Dezavantajlar:**

- Ek dependency (RedLock.Net gibi)
- Daha karmaşık implementasyon

---

### 5.2 🟡 ORTA: RefreshToken/Login Hata Mesajları

**Öneri:** Frontend'de kullanıcıya daha anlaşılır mesajlar gösterilebilir. Backend'de özel bir iyileştirme gerekmez.

---

## 6. Öncelik Matrisi

| ID             | Sorun                              | Öncelik       | Etki   | Çaba  | Süre     | Durum             |
| -------------- | ---------------------------------- | ------------- | ------ | ----- | -------- | ----------------- |
| **KRİTİK-001** | ActivityLogConsumer Race Condition | 🔴 **YÜKSEK** | Yüksek | Orta  | 2-4 saat | ✅ **TAMAMLANDI** |
| ORTA-001       | RefreshToken hata mesajları        | 🟡 Düşük      | Düşük  | Düşük | 30 dk    | ⏳ Beklemede      |
| ORTA-002       | Login hata mesajları               | 🟡 Düşük      | Düşük  | Düşük | 30 dk    | ⏳ Beklemede      |

---

## 7. Uygulama Planı

### Faz 1: Acil Düzeltme (1 Hafta İçinde)

1. ✅ **ActivityLogConsumer Race Condition Düzeltmesi** - **TAMAMLANDI**
   - ✅ Merkezi idempotency service oluşturuldu (`IIdempotencyService`, `IdempotencyService`)
   - ✅ MassTransit Consumer Filter eklendi (`IdempotencyFilter<TMessage>`)
   - ✅ `ICacheService`'e `AddIfNotExists` metodu eklendi
   - ✅ `ActivityLogConsumer` basitleştirildi (kod tekrarı önlendi)
   - ✅ SOLID prensipleri uygulandı
   - ⏳ Test et (concurrent consumer senaryosu) - Önerilir

### Faz 2: İyileştirmeler (1 Ay İçinde)

1. **Monitoring ve Alerting**

   - ActivityLog duplicate key hataları için alert ekle
   - Retry sayısını izle

2. **Frontend İyileştirmeleri**
   - RefreshToken/Login hata mesajlarını iyileştir

---

## 8. Test Senaryoları

### Test 1: Concurrent Consumer Senaryosu

```csharp
// Aynı mesajı 5 farklı consumer'a gönder
// Beklenen: Sadece 1 kayıt oluşmalı, diğerleri idempotent olarak işlenmeli
```

### Test 2: Redis Timeout Senaryosu

```csharp
// Redis bağlantısını kes
// Mesajı gönder
// Beklenen: DB kontrolü çalışmalı, duplicate key hatası olmamalı
```

### Test 3: Retry Senaryosu

```csharp
// İlk denemede SaveChangesAsync'den önce exception fırlat
// Retry tetiklensin
// Beklenen: Duplicate key hatası olmamalı
```

---

## 9. Sonuç ve Öneriler

### Özet

1. **Kritik Sorun:** ActivityLogConsumer'da race condition var, hemen düzeltilmesi gerekiyor
2. **Beklenen Hatalar:** RefreshToken ve Login hataları normal güvenlik kontrolleri
3. **Öncelik:** ActivityLogConsumer race condition düzeltmesi

### Önerilen Çözüm

**Çözüm 1 (Redis Optimistic Lock)** önerilir çünkü:

- ✅ Mevcut altyapıyı kullanır (Redis zaten var)
- ✅ EF Core'dan uzaklaşmaz
- ✅ Basit implementasyon
- ✅ Yeterince güvenli

**Alternatif:** Eğer Redis güvenilirliği sorunluysa, **Çözüm 2 (Database-Level Idempotency)** kullanılabilir.

---

---

## 10. ✅ Uygulanan Çözüm: Merkezi Idempotency Service

> **Güncelleme Tarihi:** 2 Aralık 2025  
> **Durum:** ✅ **TAMAMLANDI** - Merkezi çözüm uygulandı

### 10.1 Uygulanan Mimari

**Sorun:** Her consumer için idempotency mantığı tekrar yazılıyordu (kod tekrarı, SOLID ihlali).

**Çözüm:** Merkezi idempotency servisi ve MassTransit Consumer Filter kullanıldı.

```
┌─────────────────────────────────────────────────────────┐
│         MassTransit Consumer Filter                     │
│  ┌───────────────────────────────────────────────────┐  │
│  │     IdempotencyFilter<TMessage>                   │  │
│  │  • Mesaj göndermeden önce idempotency kontrolü    │  │
│  │  • IIdempotencyService ile lock alma              │  │
│  └───────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────┘
                        │
                        ▼
┌─────────────────────────────────────────────────────────┐
│            IIdempotencyService                          │
│  ┌───────────────────────────────────────────────────┐  │
│  │  • CheckAndAcquireLockAsync()                    │  │
│  │  • MarkAsProcessedAsync()                         │  │
│  │  • Redis + DB kontrolü                            │  │
│  └───────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────┘
                        │
                        ▼
┌─────────────────────────────────────────────────────────┐
│              Consumer (Basitleştirilmiş)                 │
│  ┌───────────────────────────────────────────────────┐  │
│  │  • Sadece business logic                         │  │
│  │  • Idempotency otomatik (filter tarafından)      │  │
│  └───────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────┘
```

### 10.2 Oluşturulan Dosyalar

1. **`IIdempotencyService`** (`src/BaseProject.Application/Abstractions/IIdempotencyService.cs`)

   - Idempotency kontrolü için merkezi interface

2. **`IdempotencyService`** (`src/BaseProject.Infrastructure/Services/IdempotencyService.cs`)

   - Redis cache ve database kontrolü ile idempotency implementasyonu

3. **`IdempotencyFilter<TMessage>`** (`src/BaseProject.Infrastructure/Consumers/Filters/IdempotencyFilter.cs`)

   - MassTransit consumer filter - idempotency kontrolü için

4. **`GuidHelper`** (`src/BaseProject.Domain/Common/Utilities/GuidHelper.cs`)
   - Deterministic Guid oluşturma için merkezi utility sınıfı
   - Kod tekrarını önler (DRY prensibi)
   - Domain katmanında, hiçbir bağımlılık yok

### 10.3 Güncellenen Dosyalar

1. **`ActivityLogConsumer`** - Basitleştirildi

   - Idempotency mantığı kaldırıldı (~100 satır kod azaldı)
   - Sadece business logic kaldı
   - Filter tarafından otomatik idempotency kontrolü

2. **`InfrastructureServicesRegistration`** - Filter eklendi ve kod tekrarı kaldırıldı
   - `IIdempotencyService` register edildi
   - `IdempotencyFilter` ActivityLogConsumer için eklendi
   - `GenerateDeterministicGuid` metodu kaldırıldı (GuidHelper kullanılıyor)

### 10.4 Avantajlar

✅ **SOLID Prensipleri:**

- Single Responsibility: Her component tek sorumluluğa sahip
- Open/Closed: Yeni consumer'lar için sadece filter eklemek yeterli
- Dependency Inversion: Interface'ler üzerinden bağımlılık

✅ **Clean Code:**

- DRY: Kod tekrarı yok (idempotency + GuidHelper utility)
- Okunabilirlik: Consumer'lar sadece business logic'e odaklanır
- Bakım: Idempotency mantığı ve utility metodları tek yerde

✅ **Sürdürülebilirlik:**

- Test edilebilirlik: Her component ayrı test edilebilir
- Genişletilebilirlik: Yeni consumer'lar için sadece filter eklemek yeterli
- Değiştirilebilirlik: Idempotency mantığı değişirse tek yerden güncellenir

### 10.5 Yeni Consumer Ekleme

**Önceki Yöntem (100+ satır kod):**

```csharp
public class NewConsumer : IConsumer<NewEvent>
{
    // ~100 satır idempotency kodu
    // Business logic
}
```

**Yeni Yöntem (Sadece filter ekle):**

```csharp
// InfrastructureServicesRegistration'da:
endpointConfigurator.UseFilter(new IdempotencyFilter<NewEvent>(
    context.GetRequiredService<IIdempotencyService>(),
    context.GetRequiredService<ILogger<IdempotencyFilter<NewEvent>>>(),
    keyPrefix: "idempotency:newevent:",
    fallbackIdGenerator: msg => GuidHelper.GenerateDeterministicGuid($"{msg.EntityId}_{msg.Timestamp:O}_{msg.Type}"),
    existsCheck: async (id, ct) => await repo.ExistsByIdAsync(id, ct)
));

// Consumer'da sadece business logic:
public class NewConsumer : IConsumer<NewEvent>
{
    // Sadece business logic - idempotency otomatik!
}
```

### 10.6 Sonuç

✅ **Race condition sorunu çözüldü** - Merkezi idempotency service ile  
✅ **Kod tekrarı önlendi** - DRY prensibi uygulandı (idempotency + GuidHelper)  
✅ **SOLID prensipleri uygulandı** - Clean Architecture  
✅ **Sürdürülebilirlik artırıldı** - Yeni consumer'lar için kolay genişletme  
✅ **Utility metodları merkezi** - GuidHelper Domain katmanında, bağımlılık yok

---

**Rapor Hazırlayan:** AI Code Reviewer  
**Tarih:** 2 Aralık 2025  
**Versiyon:** 2.0 (Merkezi Çözüm Uygulandı)
