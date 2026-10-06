FROM mcr.microsoft.com/dotnet/sdk:8.0

RUN apt-get update \
  && apt-get install -y curl ca-certificates gnupg \
  && curl -fsSL https://deb.nodesource.com/setup_20.x | bash - \
  && apt-get install -y nodejs \
  && rm -rf /var/lib/apt/lists/*

RUN dotnet tool install DefaultDocumentation.Console -g

ENV PATH="$PATH:/root/.dotnet/tools"