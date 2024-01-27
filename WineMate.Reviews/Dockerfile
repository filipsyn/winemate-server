FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

EXPOSE 8080
ENV ASPNETCORE_ENVIRONMENT Production

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["WineMate.Reviews/WineMate.Reviews.csproj", "WineMate.Reviews/"]
RUN dotnet restore "WineMate.Reviews/WineMate.Reviews.csproj"
COPY . .
WORKDIR "/src/WineMate.Reviews"
RUN dotnet build "WineMate.Reviews.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "WineMate.Reviews.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "WineMate.Reviews.dll"]
