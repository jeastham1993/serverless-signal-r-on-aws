FROM mcr.microsoft.com/dotnet/aspnet:8.0-preview as base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0-preview AS build
RUN mkdir /src
RUN mkdir /src/SignalR
RUN mkdir /src/SignalR.Shared
COPY ./SignalR/* /src/SignalR
COPY ./SignalR.Shared/* /src/SignalR.Shared
WORKDIR /src
RUN ls
RUN dotnet build "SignalR/SignalR.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SignalR/SignalR.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SignalR.dll"]