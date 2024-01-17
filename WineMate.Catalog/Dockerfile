FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["WineMate.Catalog/WineMate.Catalog.csproj", "WineMate.Catalog/"]
RUN dotnet restore "WineMate.Catalog/WineMate.Catalog.csproj"
COPY . .
WORKDIR "/src/WineMate.Catalog"
RUN dotnet build "WineMate.Catalog.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WineMate.Catalog.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WineMate.Catalog.dll"]
