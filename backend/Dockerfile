FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Сначала только csproj — слой restore кэшируется, пока не меняются зависимости
COPY Directory.Build.props ./
COPY src/SalesDashboard.Api/SalesDashboard.Api.csproj src/SalesDashboard.Api/
RUN dotnet restore src/SalesDashboard.Api/SalesDashboard.Api.csproj

COPY src/ src/
RUN dotnet publish src/SalesDashboard.Api/SalesDashboard.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "SalesDashboard.Api.dll"]
