FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy projects and restore
COPY ["ClinicaAI.Core/ClinicaAI.Core.csproj", "ClinicaAI.Core/"]
COPY ["ClinicaAI.Infrastructure/ClinicaAI.Infrastructure.csproj", "ClinicaAI.Infrastructure/"]
COPY ["ClinicaAI.Api/ClinicaAI.Api.csproj", "ClinicaAI.Api/"]
RUN dotnet restore "ClinicaAI.Api/ClinicaAI.Api.csproj"

# Copy everything and build
COPY . .
WORKDIR "/src/ClinicaAI.Api"
RUN dotnet build "ClinicaAI.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ClinicaAI.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "ClinicaAI.Api.dll"]
