FROM microsoft/dotnet:2.2-sdk-alpine as builder
COPY . /src
RUN dotnet publish -c release -o /app /src/Backuper.Api

FROM microsoft/dotnet:2.2-runtime-alpine
RUN apk add curl
WORKDIR /app
COPY --from=builder /app /app
VOLUME [ "/app/configuration", "/app/data" ]
CMD ["dotnet", "/app/Backuper.Api.dll"]