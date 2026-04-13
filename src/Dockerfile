# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0-preview AS build
WORKDIR /src

COPY BBScoreboard.Domain/BBScoreboard.Domain.csproj BBScoreboard.Domain/
COPY BBScoreboard.Application/BBScoreboard.Application.csproj BBScoreboard.Application/
COPY BBScoreboard.Infrastructure/BBScoreboard.Infrastructure.csproj BBScoreboard.Infrastructure/
COPY BBScoreboard.Web/BBScoreboard.Web.csproj BBScoreboard.Web/
COPY BBScoreboard.sln .
RUN dotnet restore

COPY . .
RUN dotnet publish BBScoreboard.Web/BBScoreboard.Web.csproj -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0-preview AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENTRYPOINT ["dotnet", "BBScoreboard.Web.dll"]
