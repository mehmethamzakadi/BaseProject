# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
# ✅ Merkezi paket versiyon yönetimi için gerekli dosyalar
COPY ["Directory.Build.props", "./"]
COPY ["Directory.Packages.props", "./"]
# Proje dosyalarını kopyala
COPY ["src/BaseProject.API/BaseProject.API.csproj", "src/BaseProject.API/"]
COPY ["src/BaseProject.Infrastructure/BaseProject.Infrastructure.csproj", "src/BaseProject.Infrastructure/"]
COPY ["src/BaseProject.Application/BaseProject.Application.csproj", "src/BaseProject.Application/"]
COPY ["src/BaseProject.Domain/BaseProject.Domain.csproj", "src/BaseProject.Domain/"]
COPY ["src/BaseProject.Persistence/BaseProject.Persistence.csproj", "src/BaseProject.Persistence/"]
RUN dotnet restore "./src/BaseProject.API/BaseProject.API.csproj"
COPY . .
WORKDIR "/src/src/BaseProject.API"
RUN dotnet build "./BaseProject.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./BaseProject.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .



# --- EKLEMEN GEREKEN KISIM BAŞLANGIÇ ---
# Root kullanıcısına geçici olarak geçip klasörü hazırlarız
USER root
# Klasörü oluştur ve sahipliğini .NET'in kullandığı kullanıcıya (app) ver
RUN mkdir -p /app/wwwroot/uploads && \
    chown -R app:app /app/wwwroot/uploads

# Tekrar güvenli kullanıcıya dön
USER app
# --- EKLEMEN GEREKEN KISIM BİTİŞ ---


ENTRYPOINT ["dotnet", "BaseProject.API.dll"]