# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["AquaStore.slnx", "./"]
COPY ["Common/Common.Domain/Common.Domain.csproj", "Common/Common.Domain/"]
COPY ["Common/Common.Application/Common.Application.csproj", "Common/Common.Application/"]
COPY ["Common/Common.Infrastructure/Common.Infrastructure.csproj", "Common/Common.Infrastructure/"]
COPY ["AquaStore.Domain/AquaStore.Domain.csproj", "AquaStore.Domain/"]
COPY ["AquaStore.Application/AquaStore.Application.csproj", "AquaStore.Application/"]
COPY ["AquaStore.Infrastructure/AquaStore.Infrastructure.csproj", "AquaStore.Infrastructure/"]
COPY ["AquaStore.Contracts/AquaStore.Contracts.csproj", "AquaStore.Contracts/"]
COPY ["AquaStore.Api/AquaStore.Api.csproj", "AquaStore.Api/"]

# Restore dependencies
RUN dotnet restore "AquaStore.Api/AquaStore.Api.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/AquaStore.Api"
RUN dotnet build "AquaStore.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "AquaStore.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Copy published app
COPY --from=publish /app/publish .

# Expose port
EXPOSE 8080

# Set environment variables (can be overridden)
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check (using wget which is available in aspnet image)
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD wget --no-verbose --tries=1 --spider http://localhost:8080/health || exit 1

# Run the app
ENTRYPOINT ["dotnet", "AquaStore.Api.dll"]

