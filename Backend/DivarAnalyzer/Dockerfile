FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

COPY ["DivarAnalyzer.csproj", "./"]
RUN dotnet restore "DivarAnalyzer.csproj"

COPY . .
RUN dotnet publish "DivarAnalyzer.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 5000 5001
ENTRYPOINT ["dotnet", "DivarAnalyzer.dll"]
