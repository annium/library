FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine as builder
COPY . /code
RUN dotnet publish -c release -o /app /code/Backuper.Api

FROM mcr.microsoft.com/dotnet/aspnet:7.0-alpine
RUN apk add --no-cache \
    curl \
    postgresql-client
WORKDIR /app
COPY --from=builder /app /app
VOLUME [ "/app/configuration", "/app/data" ]
CMD ["/app/Backuper.Api"]