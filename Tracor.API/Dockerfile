FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_HTTP_PORTS=8080

RUN apt-get update \
	&& apt-get install -y --no-install-recommends curl \
	&& rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Tracor.API/Tracor.API.csproj", "Tracor.API/"]
COPY ["Tracor.Application/Tracor.Application.csproj", "Tracor.Application/"]
COPY ["Tracor.Domain/Tracor.Domain.csproj", "Tracor.Domain/"]
COPY ["Tracor.Infrastructure/Tracor.Infrastructure.csproj", "Tracor.Infrastructure/"]
RUN dotnet restore "Tracor.API/Tracor.API.csproj"
COPY . .
WORKDIR "/src/Tracor.API"

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Tracor.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

HEALTHCHECK --interval=30s --timeout=5s --start-period=30s --retries=3 \
	CMD curl -fsS http://localhost:8080/healthz || exit 1

ENTRYPOINT ["dotnet", "Tradecorp.API.dll"]
