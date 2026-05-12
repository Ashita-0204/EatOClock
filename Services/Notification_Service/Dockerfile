FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Notification-Service.csproj", "./"]
RUN dotnet restore "Notification-Service.csproj"
COPY . .
RUN dotnet build "Notification-Service.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Notification-Service.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Notification-Service.dll"]
