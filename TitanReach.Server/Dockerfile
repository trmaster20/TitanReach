FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
COPY TitanReach_Server/bin/Debug/net5.0/ App/
WORKDIR /App
ENTRYPOINT ["sh", "./run.sh"]
