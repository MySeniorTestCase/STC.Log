FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Presentation/STC.Logger.WebApi/STC.Logger.WebApi.csproj", "src/Presentation/STC.Logger.WebApi/"]
COPY ["src/Infrastructure/STC.Logger.Infrastructure/STC.Logger.Infrastructure.csproj", "src/Infrastructure/STC.Logger.Infrastructure/"]
COPY ["Core/STC.Logger.Application/STC.Logger.Application.csproj", "Core/STC.Logger.Application/"]
RUN dotnet restore "src/Presentation/STC.Logger.WebApi/STC.Logger.WebApi.csproj"
COPY . .
WORKDIR "/src/src/Presentation/STC.Logger.WebApi"
RUN dotnet build "STC.Logger.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "STC.Logger.WebApi.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "STC.Logger.WebApi.dll"]
