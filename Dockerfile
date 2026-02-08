FROM maven:3.9-eclipse-temurin-21-alpine AS backend-build
WORKDIR /backend
COPY acme-admin/pom.xml acme-admin/mvnw ./
COPY acme-admin/.mvn .mvn
COPY acme-admin/src src
RUN mvn clean package -DskipTests -B

FROM node:20-alpine AS frontend-build
WORKDIR /frontend
COPY admin-dashboard/package*.json ./
RUN npm install --legacy-peer-deps
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

COPY supervisord.conf /etc/supervisor/conf.d/supervisord.conf
RUN chown -R acme:acme /app

EXPOSE 8080 4000

HEALTHCHECK --interval=30s --timeout=5s --retries=3 \
  CMD wget -qO- http://localhost:8080/actuator/health || wget -qO- http://localhost:4000/ || exit 1

CMD ["/usr/bin/supervisord", "-c", "/etc/supervisor/conf.d/supervisord.conf"]
