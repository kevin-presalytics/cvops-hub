FROM mcr.microsoft.com/dotnet/sdk:6.0 as build
WORKDIR /build
COPY . .
RUN dotnet nuget update source nuget.org
RUN dotnet restore --force
RUN dotnet publish -c Debug -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:6.0-bullseye-slim as final
RUN apt update && apt install -y curl \
    && curl -sSL https://aka.ms/getvsdbgsh | /bin/sh /dev/stdin -v latest -l /root/vsdbg
WORKDIR /app
COPY --from=build /app/publish .
CMD ["sh", "-c", "dotnet api.dll"]