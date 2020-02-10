FROM mcr.microsoft.com/dotnet/core/sdk:3.1 AS build
WORKDIR /source

COPY ./Azurite/*.csproj Azurite/
COPY ./Azurite.Core/*.csproj Azurite.Core/
COPY ./Azurite.Index/*.csproj Azurite.Index/
COPY ./Azurite.Wiki/*.csproj Azurite.Wiki/
RUN dotnet restore Azurite/Azurite.csproj

COPY Azurite/ Azurite/
COPY Azurite.Core/ Azurite.Core/
COPY Azurite.Index/ Azurite.Index/
COPY Azurite.Wiki/ Azurite.Wiki/
WORKDIR /source/Azurite
RUN dotnet build -c release --no-restore

FROM build AS publish
RUN dotnet publish -c release --no-build -o /app

FROM mcr.microsoft.com/dotnet/core/runtime:3.1
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Azurite.dll"]