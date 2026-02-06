ARG DOTNET_VERSION=10.0
ARG BUILD_CONFIGURATION=Release

FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_VERSION} AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY . .
RUN dotnet restore "./Grahplet.csproj"
RUN dotnet publish Grahplet.csproj -c $BUILD_CONFIGURATION -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:${DOTNET_VERSION} AS runtime
WORKDIR /app
COPY --from=build /app/out .
ENV ASPNETCORE_URLS=http://+:5188
EXPOSE 5188
ENTRYPOINT ["dotnet", "Grahplet.dll"]