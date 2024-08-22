FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /source

# copy csproj and restore as distinct layers
COPY . .
RUN dotnet restore -a amd64

RUN dotnet publish -a amd64 --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine
EXPOSE 5001

ENV \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    LC_ALL=en_US.UTF-8 \
    LANG=en_US.UTF-8 \
    ASPNETCORE_URLS=http://*:5001
RUN apk add --no-cache \
    icu-data-full \
    icu-libs

WORKDIR /app/CompleteProject/net8.0
COPY --from=build /source/CompleteProject/net8.0/ .
USER $APP_UID
ENTRYPOINT ["dotnet","DAVID.dll"]