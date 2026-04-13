FROM oven/bun:latest AS builder
WORKDIR /app

# Add build argument for Vite environment variable
ARG VITE_API_URL
ENV VITE_API_URL=$VITE_API_URL

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
