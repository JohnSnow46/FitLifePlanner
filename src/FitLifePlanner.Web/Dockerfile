# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore
RUN dotnet publish src/FitLifePlanner.Web -c Release -o /app/publish

# Runtime stage — static files served by nginx (Blazor WASM needs no server runtime)
FROM nginx:alpine AS runtime

COPY --from=build /app/publish/wwwroot /usr/share/nginx/html
COPY src/FitLifePlanner.Web/nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80
