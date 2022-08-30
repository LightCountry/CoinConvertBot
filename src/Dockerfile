#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Telegram.CoinConvertBot/Telegram.CoinConvertBot.csproj", "Telegram.CoinConvertBot/"]
RUN dotnet restore "Telegram.CoinConvertBot/Telegram.CoinConvertBot.csproj"
COPY . .
WORKDIR "/src/Telegram.CoinConvertBot"
RUN dotnet build "Telegram.CoinConvertBot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Telegram.CoinConvertBot.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Telegram.CoinConvertBot.dll"]