FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine as builder
COPY . /code
RUN dotnet publish -c Release -o /dist /code

FROM nginx:alpine
COPY --from=build /dist/wwwroot/ /usr/share/nginx/html/
COPY ./nginx.conf /etc/nginx/nginx.conf