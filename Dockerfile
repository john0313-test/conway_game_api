FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["ConwayGameOfLife.Api/ConwayGameOfLife.Api.csproj", "ConwayGameOfLife.Api/"]
COPY ["ConwayGameOfLife.Core/ConwayGameOfLife.Core.csproj", "ConwayGameOfLife.Core/"]
COPY ["ConwayGameOfLife.Infrastructure/ConwayGameOfLife.Infrastructure.csproj", "ConwayGameOfLife.Infrastructure/"]
RUN dotnet restore "ConwayGameOfLife.Api/ConwayGameOfLife.Api.csproj"
COPY . .
WORKDIR "/src/ConwayGameOfLife.Api"
RUN dotnet build "ConwayGameOfLife.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ConwayGameOfLife.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ConwayGameOfLife.Api.dll"]