#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=DEBUG
WORKDIR /src
COPY ["Configuration/Configuration.csproj", "Configuration/"]
COPY ["Domain.User/Domain.User.csproj", "Domain.User/"]
COPY ["Domain.Common/Domain.Common.csproj", "Domain.Common/"]
COPY ["Presentation.Api/Presentation.Api.csproj", "Presentation.Api/"]
COPY ["Application.Commands/Application.Commands.csproj", "Application.Commands/"]
COPY ["Application.Common/Application.Common.csproj", "Application.Common/"]
COPY ["Application.Queries/Application.Queries.csproj", "Application.Queries/"]
COPY ["Presentation.Jobs/Presentation.Jobs.csproj", "Presentation.Jobs/"]
RUN dotnet restore "./Configuration/Configuration.csproj"
COPY . .
WORKDIR "/src/Configuration"
RUN dotnet build "./Configuration.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=DEBUG
RUN dotnet publish "./Configuration.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Configuration.dll"]