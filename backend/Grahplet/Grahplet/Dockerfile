ARG DOTNET_VERSION=10.0
ARG BUILD_CONFIGURATION=Release

FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS build
WORKDIR /src
COPY . .
RUN dotnet restore "./Grahplet.csproj"
RUN dotnet publish Grahplet.csproj -c $BUILD_CONFIGURATION out
RUN ls bin/out