FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine as builder
COPY . /src
RUN dotnet publish -c release -o /app /src/Backuper.Api

FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-alpine
RUN apk add --no-cache \
    curl \
    postgresql-client
WORKDIR /app
COPY --from=builder /app /app
VOLUME [ "/app/configuration", "/app/data" ]
CMD ["/app/Backuper.Api"]