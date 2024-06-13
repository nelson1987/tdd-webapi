FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine as base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
COPY . /src
WORKDIR /src
RUN ls
RUN dotnet build "Chronos.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Chronos.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Chronos.Api.dll"]