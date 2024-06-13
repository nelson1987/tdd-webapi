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
# EXPOSE 80 5195 7066
# ENV ASPNETCORE_URLS=http://+:5195;https://+:7066
ENTRYPOINT ["dotnet", "Chronos.Api.dll"]