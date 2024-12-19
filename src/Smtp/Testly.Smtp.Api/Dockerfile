# 请参阅 https://aka.ms/customizecontainer 以了解如何自定义调试容器，以及 Visual Studio 如何使用此 Dockerfile 生成映像以更快地进行调试。

# 此阶段用于在快速模式(默认为调试配置)下从 VS 运行时
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080


# 此阶段用于生成服务项目
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Smtp/Testly.Smtp.Api/Testly.Smtp.Api.csproj", "src/Smtp/Testly.Smtp.Api/"]
COPY ["src/Smtp/Testly.Smtp.Application/Testly.Smtp.Application.csproj", "src/Smtp/Testly.Smtp.Application/"]
COPY ["src/BuildingBlocks/Testly.Domain.Analyzers/Testly.Domain.Analyzers.csproj", "src/BuildingBlocks/Testly.Domain.Analyzers/"]
COPY ["src/BuildingBlocks/Testly.Domain.Attributes/Testly.Domain.Attributes.csproj", "src/BuildingBlocks/Testly.Domain.Attributes/"]
COPY ["src/Smtp/Testly.Smtp.Domain/Testly.Smtp.Domain.csproj", "src/Smtp/Testly.Smtp.Domain/"]
COPY ["src/BuildingBlocks/Testly.Domain/Testly.Domain.csproj", "src/BuildingBlocks/Testly.Domain/"]
COPY ["src/BuildingBlocks/Testly.AOP/Testly.AOP.csproj", "src/BuildingBlocks/Testly.AOP/"]
RUN dotnet restore "./src/Smtp/Testly.Smtp.Api/Testly.Smtp.Api.csproj"
COPY . .
WORKDIR "/src/src/Smtp/Testly.Smtp.Api"
RUN dotnet build "./Testly.Smtp.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# 此阶段用于发布要复制到最终阶段的服务项目
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Testly.Smtp.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# 此阶段在生产中使用，或在常规模式下从 VS 运行时使用(在不使用调试配置时为默认值)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Testly.Smtp.Api.dll"]