FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS backend-build
WORKDIR /backend
COPY acme-admin-dotnet/*.csproj ./
RUN dotnet restore ./Acme.Admin.Api.csproj
COPY acme-admin-dotnet/. ./
RUN dotnet publish ./Acme.Admin.Api.csproj -c Release -o /out /p:UseAppHost=false

FROM node:20-alpine AS frontend-build
WORKDIR /frontend
COPY admin-dashboard/package*.json ./
RUN npm install --legacy-peer-deps
COPY admin-dashboard/. ./
RUN npm run build

FROM mcr.microsoft.com/dotnet/aspnet:8.0-alpine
RUN apk add --no-cache nodejs npm supervisor
WORKDIR /app

RUN addgroup -S acme && adduser -S acme -G acme

COPY --from=backend-build /out /app/backend
COPY --from=frontend-build /frontend/dist/admin-dashboard /app/frontend
COPY --from=frontend-build /frontend/package.json /app/
COPY --from=frontend-build /frontend/package-lock.json /app/
RUN cd /app && npm install --omit=dev --legacy-peer-deps && npm cache clean --force

COPY supervisord.conf /etc/supervisor/conf.d/supervisord.conf

ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080 4000

HEALTHCHECK --interval=30s --timeout=5s --retries=3 \
  CMD wget -qO- http://localhost:8080/actuator/health || wget -qO- http://localhost:4000/ || exit 1

CMD ["/usr/bin/supervisord", "-c", "/etc/supervisor/conf.d/supervisord.conf"]
