FROM maven:3.9-eclipse-temurin-21-alpine AS backend-build
WORKDIR /backend
COPY acme-admin/pom.xml acme-admin/mvnw ./
COPY acme-admin/.mvn .mvn
COPY acme-admin/src src
RUN mvn clean package -DskipTests -B

FROM node:20-alpine AS frontend-build
WORKDIR /frontend
COPY admin-dashboard/package*.json ./
RUN npm ci --ignore-scripts
COPY admin-dashboard/ ./
RUN npm run build

FROM eclipse-temurin:21-jre-alpine
RUN apk add --no-cache nodejs npm supervisor
WORKDIR /app

RUN addgroup -S acme && adduser -S acme -G acme

COPY --from=backend-build /backend/target/*.jar /app/backend.jar
COPY --from=frontend-build /frontend/dist/admin-dashboard /app/frontend
COPY --from=frontend-build /frontend/package.json /app/
RUN cd /app && npm ci --omit=dev --ignore-scripts && npm cache clean --force

COPY <<EOF /etc/supervisor/conf.d/supervisord.conf
[supervisord]
nodaemon=true
user=root
logfile=/dev/stdout
logfile_maxbytes=0
loglevel=info

[program:backend]
command=java -XX:+UseContainerSupport -XX:MaxRAMPercentage=50.0 -Djava.security.egd=file:/dev/./urandom -jar /app/backend.jar
autostart=true
autorestart=true
stdout_logfile=/dev/stdout
stdout_logfile_maxbytes=0
stderr_logfile=/dev/stderr
stderr_logfile_maxbytes=0
user=acme
priority=10

[program:frontend]
command=node /app/frontend/server/server.mjs
directory=/app
autostart=true
autorestart=true
stdout_logfile=/dev/stdout
stdout_logfile_maxbytes=0
stderr_logfile=/dev/stderr
stderr_logfile_maxbytes=0
user=acme
priority=20
EOF

RUN chown -R acme:acme /app

EXPOSE 8080 4000

HEALTHCHECK --interval=30s --timeout=5s --retries=3 \
  CMD wget -qO- http://localhost:8080/actuator/health || wget -qO- http://localhost:4000/ || exit 1

CMD ["/usr/bin/supervisord", "-c", "/etc/supervisor/conf.d/supervisord.conf"]
