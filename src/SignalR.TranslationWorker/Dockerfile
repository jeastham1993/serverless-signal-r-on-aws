FROM mcr.microsoft.com/dotnet/aspnet:8.0-preview as base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0-preview AS build
RUN mkdir /src
RUN mkdir /src/SignalR.TranslationWorker
RUN mkdir /src/SignalR.Shared
COPY ./SignalR.TranslationWorker/* /src/SignalR.TranslationWorker
COPY ./SignalR.Shared/* /src/SignalR.Shared
WORKDIR /src
RUN ls
RUN dotnet build "SignalR.TranslationWorker/SignalR.TranslationWorker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SignalR.TranslationWorker/SignalR.TranslationWorker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SignalR.TranslationWorker.dll"]