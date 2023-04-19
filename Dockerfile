ARG PROJECT_NAME

FROM mcr.microsoft.com/dotnet/sdk:6.0 as build
ARG PROJECT_NAME
ENV PROJECT_FILE=$PROJECT_NAME.csproj
WORKDIR /build
COPY $PROJECT_NAME/$PROJECT_FILE $PROJECT_NAME/
COPY "./lib/lib.csproj" lib/
RUN dotnet nuget update source nuget.org
RUN dotnet restore $PROJECT_NAME/$PROJECT_FILE --force
COPY . .
RUN dotnet publish $PROJECT_NAME/$PROJECT_FILE -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:6.0-alpine as final
ARG PROJECT_NAME
ENV EXECUTABLE_FILE=$PROJECT_NAME.dll
WORKDIR /app
COPY --from=build /app/publish .
CMD ["sh", "-c", "dotnet $EXECUTABLE_FILE"]