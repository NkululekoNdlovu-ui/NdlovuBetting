# ===== Stage 1: build =====
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy the project file and restore dependencies
COPY ["NdlovuBetting.csproj", "./"]
RUN dotnet restore "NdlovuBetting.csproj"

# Copy source and publish
COPY . .
RUN dotnet publish "NdlovuBetting.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ===== Stage 2: runtime =====
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8081
EXPOSE 8081

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "NdlovuBetting.dll"]
