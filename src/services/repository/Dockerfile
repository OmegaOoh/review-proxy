FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/services/repository/repository.csproj", "src/services/repository/"]
COPY ["src/shared/ReviewProxy.Contracts/ReviewProxy.Contracts.csproj", "src/shared/ReviewProxy.Contracts/"]

RUN dotnet restore "src/services/repository/repository.csproj"

COPY ["src/shared/ReviewProxy.Contracts/", "src/shared/ReviewProxy.Contracts/"]
COPY ["src/services/repository/", "src/services/repository/"]

WORKDIR "/src/src/services/repository"
RUN dotnet build "repository.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "repository.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "repository.dll"]
