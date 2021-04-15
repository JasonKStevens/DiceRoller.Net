FROM mcr.microsoft.com/dotnet/sdk:3.1-alpine AS build
WORKDIR /src
COPY ["DiceRollerCmd/DiceRollerCmd.csproj", "DiceRollerCmd/"]
RUN dotnet restore "DiceRollerCmd/DiceRollerCmd.csproj" -r linux-musl-x64
COPY . .
WORKDIR "/src/DiceRollerCmd"
RUN dotnet build "DiceRollerCmd.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DiceRollerCmd.csproj" -c Release -r linux-musl-x64 --no-restore -o /app/publish /p:PublishReadyToRun=true

FROM mcr.microsoft.com/dotnet/runtime-deps:3.1-alpine AS base
WORKDIR /app

# Creates a non-root user with an explicit UID and adds permission to access the /app folder
# For more info, please refer to https://aka.ms/vscode-docker-dotnet-configure-containers
RUN adduser -u 5678 --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

LABEL version="0.1"

ENTRYPOINT ["./DiceRollerCmd"]
