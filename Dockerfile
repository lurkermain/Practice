# Базовый образ ASP.NET Core
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER root  # Нужно, чтобы были права на установку
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Устанавливаем Blender
RUN apt-get update && \
    apt-get install -y blender && \
    apt-get clean && \
    rm -rf /var/lib/apt/lists/*

# Копируем Blender и скрипт в контейнер
COPY ["Backend/Blender/blender.exe", "/usr/local/bin/blender"]
COPY ["Backend/Blender/script3.py", "/app/script3.py"]
# Меняем пользователя обратно (если нужно)
USER $APP_UID

# Этап сборки
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Practice/Practice.csproj", "Practice/"]
RUN dotnet restore "./Practice/Practice.csproj"
COPY . .
WORKDIR "/src/Practice"
RUN dotnet build "./Practice.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Этап публикации
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Practice.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Финальный контейнер
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Practice.dll"]
