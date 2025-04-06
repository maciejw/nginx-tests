# https://hub.docker.com/_/microsoft-dotnet
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /source


COPY *.csproj .
RUN dotnet restore --force

# copy everything else and build app
COPY . .

RUN dotnet publish -c release -o /app --no-restore

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app ./
COPY entrypoint.sh /
ENTRYPOINT ["/entrypoint.sh"]
CMD [ "Api1.dll" ]