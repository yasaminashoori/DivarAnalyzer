FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Backend/DivarAnalyzer/DivarAnalyzer.csproj", "Backend/DivarAnalyzer/"]
RUN dotnet restore "Backend/DivarAnalyzer/DivarAnalyzer.csproj"

COPY . .
WORKDIR "/src/Backend/DivarAnalyzer"
RUN dotnet build "DivarAnalyzer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DivarAnalyzer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
COPY Frontend/ wwwroot/

ENTRYPOINT ["dotnet", "DivarAnalyzer.dll"]