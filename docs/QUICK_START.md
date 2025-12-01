# 🚀 Hızlı Başlangıç - Windows

Windows PowerShell veya Command Prompt'ta Makefile komutlarını kullanmak için:

## ✅ Doğru Kullanım

### PowerShell'de:

```powershell
# PowerShell'de current directory'deki dosyaları çalıştırmak için .\ kullanın
.\make.bat help
.\make.bat dev
.\make.bat stop
```

### Command Prompt (CMD)'de:

```cmd
# CMD'de direkt çalışır
make.bat help
make.bat dev
make.bat stop
```

## 🔧 PowerShell Execution Policy Sorunu

Eğer PowerShell'de "execution policy" hatası alırsanız:

```powershell
# Mevcut policy'yi kontrol et
Get-ExecutionPolicy

# Geçici olarak değiştir (sadece bu session için)
Set-ExecutionPolicy -ExecutionPolicy Bypass -Scope Process

# Veya sadece bu script için
powershell -ExecutionPolicy Bypass -File make.bat help
```

## 📝 Alternatif: Alias Oluşturma

PowerShell'de `make` komutunu kullanmak için alias oluşturabilirsiniz:

```powershell
# PowerShell profil dosyasına ekleyin (kalıcı)
notepad $PROFILE

# Şu satırı ekleyin:
function make { & ".\make.bat" $args }

# Veya sadece bu session için:
Set-Alias -Name make -Value ".\make.bat"
```

## 🎯 Hızlı Komutlar

```powershell
# Development ortamını başlat
.\make.bat dev

# Servisleri durdur (volume'lar korunur)
.\make.bat stop

# Logları izle
.\make.bat logs-api

# Migration oluştur
.\make.bat migrate NAME=AddUserTable
```

## ⚠️ Güvenlik Kısıtlamaları

Ofis bilgisayarınızda güvenlik politikaları varsa:

1. **Execution Policy:** PowerShell execution policy kısıtlaması olabilir
2. **Path Kısıtlaması:** Script'lerin çalıştırılması engellenmiş olabilir
3. **Antivirus:** Batch dosyaları engellenmiş olabilir

### Çözümler:

1. **Yönetici olarak çalıştır:** PowerShell'i "Run as Administrator" ile açın
2. **Execution Policy:** `Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser`
3. **Alternatif:** Docker Compose komutlarını direkt kullanın:

```powershell
# Docker Compose ile direkt kullanım
docker-compose -f docker-compose.yml -f docker-compose.local.yml up -d
docker-compose -f docker-compose.yml -f docker-compose.local.yml down
```

## 📚 Daha Fazla Bilgi

Detaylı kullanım için: [README_MAKEFILE.md](README_MAKEFILE.md)

