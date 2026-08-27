FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY RoomFlow.Domain/RoomFlow.Domain.csproj RoomFlow.Domain/
COPY RoomFlow.Application/RoomFlow.Application.csproj RoomFlow.Application/
COPY RoomFlow.Infrastructure/RoomFlow.Infrastructure.csproj RoomFlow.Infrastructure/
COPY RoomFlow.Api/RoomFlow.Api.csproj RoomFlow.Api/
RUN dotnet restore RoomFlow.Api/RoomFlow.Api.csproj

COPY . .
RUN dotnet publish RoomFlow.Api/RoomFlow.Api.csproj -c Release -o /app/publish --no-restore \
    && cp RoomFlow.Api/appsettings.example.json /app/publish/appsettings.json

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

USER root
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .
USER $APP_UID

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

HEALTHCHECK --interval=10s --timeout=5s --start-period=40s --retries=10 \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "RoomFlow.Api.dll"]
