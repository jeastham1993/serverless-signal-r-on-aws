﻿FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine AS build-env
WORKDIR /App
COPY SignalR.Shared/*.cs SignalR.Shared/*.csproj ./SignalR.Shared/
COPY src/SignalR.Gateway/*.cs src/SignalR.Gateway/*.csproj ./src/SignalR.Gateway/
RUN dotnet build "src/SignalR.Gateway/SignalR.Gateway.csproj" -c Release -o /app/build --self-contained true
FROM build-env AS publish
RUN dotnet publish "src/SignalR.Gateway/SignalR.Gateway.csproj" -c Release -o /app/publish --self-contained true

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:7.0-alpine
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SignalR.Gateway.dll"]