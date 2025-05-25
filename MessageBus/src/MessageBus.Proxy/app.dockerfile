FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine as builder
COPY . /code
RUN dotnet publish -c release -o /app /code

FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine
WORKDIR /app
COPY --from=builder /app /app
VOLUME [ "/app/configuration.yml" ]
CMD ["/app/MessageBus.Proxy"]