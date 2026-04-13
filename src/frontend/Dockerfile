FROM oven/bun:latest AS builder
WORKDIR /app

COPY package*.json bun.lock ./
RUN bun install

COPY . .
RUN bun run build

# Final runtime image
FROM nginx:alpine AS runner

COPY --from=builder /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 3000
CMD ["nginx", "-g", "daemon off;"]
