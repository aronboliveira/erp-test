FROM node:20-alpine AS build
WORKDIR /app
COPY package.json package-lock.json* ./
RUN npm ci --ignore-scripts
COPY . .
RUN npm run build

FROM node:20-alpine
WORKDIR /app
RUN addgroup -S dashboard && adduser -S dashboard -G dashboard
COPY --from=build /app/dist/admin-dashboard ./dist/admin-dashboard
COPY --from=build /app/package.json ./
RUN npm ci --omit=dev --ignore-scripts && npm cache clean --force
RUN chown -R dashboard:dashboard /app
USER dashboard
EXPOSE 4000
HEALTHCHECK --interval=30s --timeout=5s --retries=3 \
  CMD wget -qO- http://localhost:4000/ || exit 1
CMD ["node", "dist/admin-dashboard/server/server.mjs"]
