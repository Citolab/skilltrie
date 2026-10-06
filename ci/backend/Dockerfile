FROM mcr.microsoft.com/dotnet/sdk:8.0
WORKDIR /src/backend
COPY ["API/API.csproj", "API/"]
COPY ["AA/AA.csproj", "AA/"]
COPY ["AI/AI.csproj", "AI/"]
RUN dotnet restore "API/API.csproj"
COPY . .
RUN dotnet tool install --global dotnet-ef
ENV PATH="$PATH:/root/.dotnet/tools"
ENV ASPNETCORE_ENVIRONMENT=Production
ENV PostHog__ProjectApiKey=$PostHog__ProjectApiKey
ENV PostHog__HostUrl=$PostHog__HostUrl
WORKDIR /src/backend/API
CMD ["sh", "-c", "dotnet restore && dotnet ef database update && dotnet run --urls http://+:${PORT:-8000} --no-launch-profile"]