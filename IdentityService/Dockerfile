FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
EXPOSE 80

# copy all .csproj files and restore as distinct layers. Use of the same COPY command
# for every dockerfile in the project to take advantage of docker caching
COPY Carsties.sln Carsties.sln
COPY AuctionService/AuctionService.csproj AuctionService/AuctionService.csproj
COPY SearchService/SearchService.csproj SearchService/SearchService.csproj
COPY Contracts/Contracts.csproj Contracts/Contracts.csproj
COPY GatewayService/GatewayService.csproj GatewayService/GatewayService.csproj
COPY IdentityService/IdentityService.csproj IdentityService/IdentityService.csproj

# Restore package deps
RUN dotnet restore Carsties.sln

# Copy the src folders over
COPY IdentityService IdentityService

WORKDIR /src/IdentityService
RUN dotnet publish -c Release -o /src/out

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /src
COPY --from=build /src/out .
ENTRYPOINT ["dotnet", "IdentityService.dll"]