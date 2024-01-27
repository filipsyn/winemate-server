FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

EXPOSE 8080
ENV ASPNETCORE_ENVIRONMENT Production

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["WineMate.Gateway/WineMate.Gateway.csproj", "WineMate.Gateway/"]
RUN dotnet restore "WineMate.Gateway/WineMate.Gateway.csproj"
COPY . .
WORKDIR "/src/WineMate.Gateway"
RUN dotnet build "WineMate.Gateway.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WineMate.Gateway.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WineMate.Gateway.dll"]
