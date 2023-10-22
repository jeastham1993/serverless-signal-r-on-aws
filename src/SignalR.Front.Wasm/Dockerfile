FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["SignalR.Front.Wasm/SignalR.Front.Wasm.csproj", "SignalR.Front.Wasm/"]
RUN dotnet restore "SignalR.Front.Wasm/SignalR.Front.Wasm.csproj"
COPY . .
WORKDIR "/src/SignalR.Front.Wasm"
RUN dotnet build "SignalR.Front.Wasm.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SignalR.Front.Wasm.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SignalR.Front.Wasm.dll"]
