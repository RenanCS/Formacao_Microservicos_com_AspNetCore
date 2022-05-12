#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["AwesomeShop.Services.Orders.Api/AwesomeShop.Services.Orders.Api.csproj", "AwesomeShop.Services.Orders.Api/"]
COPY ["AwesomeShop.Services.Orders.Application/AwesomeShop.Services.Orders.Application.csproj", "AwesomeShop.Services.Orders.Application/"]
COPY ["AwesomeShop.Services.Orders.Core/AwesomeShop.Services.Orders.Core.csproj", "AwesomeShop.Services.Orders.Core/"]
RUN dotnet restore "AwesomeShop.Services.Orders.Api/AwesomeShop.Services.Orders.Api.csproj"
COPY . .
WORKDIR "/src/AwesomeShop.Services.Orders.Api"
RUN dotnet build "AwesomeShop.Services.Orders.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AwesomeShop.Services.Orders.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AwesomeShop.Services.Orders.Api.dll"]
