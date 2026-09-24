FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 1. Restore: сначала только файлы проектов — слой кэшируется, пока не меняются зависимости
COPY Directory.Build.props SalesDashboard.sln ./
COPY src/SalesDashboard.Api/SalesDashboard.Api.csproj src/SalesDashboard.Api/
COPY tests/SalesDashboard.Tests/SalesDashboard.Tests.csproj tests/SalesDashboard.Tests/
RUN dotnet restore SalesDashboard.sln

# 2. Исходники приложения и тестов
COPY src/ src/
COPY tests/ tests/

# 3. Тесты — обязательный шаг сборки: если хоть один упал, образ не соберётся
#    и `docker compose up --build` остановится с ошибкой.
RUN dotnet test tests/SalesDashboard.Tests/SalesDashboard.Tests.csproj \
    -c Release --no-restore --logger "console;verbosity=normal"

# 4. Публикация (сборка уже есть после тестов — publish её переиспользует)
RUN dotnet publish src/SalesDashboard.Api/SalesDashboard.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "SalesDashboard.Api.dll"]
