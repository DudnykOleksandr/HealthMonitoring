FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["SimpleWebApi/SimpleWebApi.csproj", "SimpleWebApi/"]
RUN dotnet restore "SimpleWebApi/SimpleWebApi.csproj"
COPY . .
WORKDIR "/src/SimpleWebApi"
RUN dotnet build "SimpleWebApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SimpleWebApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SimpleWebApi.dll"]