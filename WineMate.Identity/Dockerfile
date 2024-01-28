FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

EXPOSE 8080
ENV ASPNETCORE_ENVIRONMENT Production

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["WineMate.Identity/WineMate.Identity.csproj", "WineMate.Identity/"]
RUN dotnet restore "WineMate.Identity/WineMate.Identity.csproj"
COPY . .
WORKDIR "/src/WineMate.Identity"
RUN dotnet build "WineMate.Identity.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WineMate.Identity.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WineMate.Identity.dll"]
