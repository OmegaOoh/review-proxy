FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/services/syncing/syncing.csproj", "src/services/syncing/"]
COPY ["src/shared/ReviewProxy.Contracts/ReviewProxy.Contracts.csproj", "src/shared/ReviewProxy.Contracts/"]

RUN dotnet restore "src/services/syncing/syncing.csproj"

COPY ["src/shared/ReviewProxy.Contracts/", "src/shared/ReviewProxy.Contracts/"]
COPY ["src/services/syncing/", "src/services/syncing/"]

WORKDIR "/src/src/services/syncing"
RUN dotnet build "syncing.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "syncing.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_HTTP_PORTS=8000
EXPOSE 8000
ENTRYPOINT ["dotnet", "syncing.dll"]
