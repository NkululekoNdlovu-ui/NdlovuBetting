# ===== Stage 1: build =====
# Use the .NET 10 SDK to compile and publish the app.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy the project file first and restore (caches dependencies for faster rebuilds).
COPY ["NdlovuBetting.csproj", "./"]
RUN dotnet restore "NdlovuBetting.csproj"

# Copy the rest of the source and publish a release build.
COPY . .
RUN dotnet publish "NdlovuBetting.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ===== Stage 2: runtime =====
# Use the smaller ASP.NET runtime image (no SDK) to actually run the app.
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# OpenShift runs containers as a random non-root user, so listen on port 8080.
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

# Copy just the published output from the build stage.
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "NdlovuBetting.dll"]