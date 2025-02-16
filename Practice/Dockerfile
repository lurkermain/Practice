# Базовый образ (используется при запуске из VS в быстром режиме)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Этап сборки проекта
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

# Финальный образ (рабочая среда)
FROM base AS final
WORKDIR /app

# Копируем опубликованное приложение
COPY --from=publish /app/publish .

# Копируем Blender в контейнер
COPY "X:/BlenderFoundation/Blender4.3/blender.exe" "/app/blender"

# Даем права на выполнение (если контейнер Linux)
RUN chmod +x /app/blender/blender.exe || echo "Skipping chmod"

# Запуск .NET-приложения
ENTRYPOINT ["dotnet", "Practice.dll"]
