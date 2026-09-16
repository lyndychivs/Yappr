FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props .
COPY Directory.Build.targets .
COPY Directory.Packages.props .
COPY global.json .

COPY src/Yappr.Models/ src/Yappr.Models/
COPY src/Yappr/ src/Yappr/
COPY src/Yappr.ServiceDefaults/ src/Yappr.ServiceDefaults/
COPY src/Yappr.Discord/ src/Yappr.Discord/

RUN dotnet publish src/Yappr.Discord/Yappr.Discord.csproj \
    --configuration Release \
    --output /app/publish \
    -p:EnforceCodeStyleInBuild=false \
    -p:RunAnalyzers=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Yappr.Discord.dll"]
