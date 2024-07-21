FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["BookStoreApi/BookStoreApi.csproj", "BookStoreApi/"]
RUN dotnet restore "BookStoreApi/BookStoreApi.csproj"
COPY . .
WORKDIR "/src/BookStoreApi"
RUN dotnet build "BookStoreApi.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BookStoreApi.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BookStoreApi.dll"]
