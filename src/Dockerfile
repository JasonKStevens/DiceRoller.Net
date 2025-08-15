#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["DiceRoller.Bot/DiceRoller.Bot.csproj", "DiceRoller.Bot/"]
COPY ["DiceRoller.Core/DiceRoller.Core.csproj", "DiceRoller.Core/"]
COPY ["DiceRoller.Discord/DiceRoller.Discord.csproj", "DiceRoller.Discord/"]
COPY ["PartyDSL/PartyDSL.csproj", "PartyDSL/"]
RUN dotnet restore "DiceRoller.Bot/DiceRoller.Bot.csproj"
COPY . .
WORKDIR "/src/DiceRoller.Bot"
RUN dotnet build "DiceRoller.Bot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DiceRoller.Bot.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DiceRoller.Bot.dll"]