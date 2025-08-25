# Etapa base
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5001

# Etapa build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copia los archivos de proyecto
COPY SodimacOrders.Application/SodimacOrders.Application.csproj ./SodimacOrders.Application/
COPY SodimacOrders.Infrastructure/SodimacOrders.Infrastructure.csproj ./SodimacOrders.Infrastructure/
COPY SodimacOrders.Domain/SodimacOrders.Domain.csproj ./SodimacOrders.Domain/
COPY SodimacOrders.WebApi/SodimacOrders.WebApi.csproj ./SodimacOrders.WebApi/

# Restaura las dependencias
RUN dotnet restore SodimacOrders.WebApi/SodimacOrders.WebApi.csproj

# Copia el resto de los archivos
COPY SodimacOrders.Application/. ./SodimacOrders.Application/
COPY SodimacOrders.Infrastructure/. ./SodimacOrders.Infrastructure/
COPY SodimacOrders.Domain/. ./SodimacOrders.Domain/
COPY SodimacOrders.WebApi/. ./SodimacOrders.WebApi/

# Compila la aplicación
WORKDIR /src/SodimacOrders.WebApi
RUN dotnet build SodimacOrders.WebApi.csproj -c Release -o /app/build

# Etapa publish
FROM build AS publish
WORKDIR /src/SodimacOrders.WebApi
RUN dotnet publish SodimacOrders.WebApi.csproj -c Release -o /app/publish

# Imagen final
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SodimacOrders.WebApi.dll"]