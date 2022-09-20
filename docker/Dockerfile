FROM mcr.microsoft.com/dotnet/aspnet:6.0
LABEL maintainer="support@cosr.eu.org"

RUN apt-get update && apt-get install -y wget ca-certificates unzip
RUN wget https://github.com/LightCountry/CoinConvertBot/releases/latest/download/linux-x64.zip
RUN unzip -d /work linux-x64.zip && rm -rf linux-x64.zip
WORKDIR /work

RUN chmod +x Telegram.CoinConvertBot

ENTRYPOINT ["./Telegram.CoinConvertBot"]