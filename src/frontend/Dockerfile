# Multi-stage Dockerfile optimized for small final image and configurable build.
# - Builder stage uses the official Bun image to install dependencies and build the app.
# - Final stage uses a minimal nginx:alpine image to serve the static `dist` output.
#
# Build-time args:
#   BUN_IMAGE     - builder base image (default: oven/bun:latest)
#   APP_PORT      - port the final image exposes (default: 80)
#
# Usage (example):
#   docker build --build-arg BUN_IMAGE=oven/bun:latest --build-arg APP_PORT=8080 -t review-proxy-frontend .
#   docker run -p 8080:8080 review-proxy-frontend

ARG BUN_IMAGE=oven/bun:latest
ARG APP_PORT=80

FROM ${BUN_IMAGE} AS builder
WORKDIR /app

# Copy only package metadata first to leverage layer caching for dependency install
COPY package*.json bun.lock ./
RUN bun install

# Copy the rest of the sources and run the production build
COPY . .
# Use the project build script (keeps compatibility with package.json)
RUN bun run build

# Final runtime image
FROM nginx:alpine AS runner

# Configure nginx to serve the built assets. We keep the default nginx config,
# but copy the dist output into the standard html directory.
COPY --from=builder /app/dist /usr/share/nginx/html

# Expose configurable port (default 80)
EXPOSE ${APP_PORT}

# Ensure nginx runs in foreground (default) and allow overriding CMD if needed
CMD ["nginx", "-g", "daemon off;"]
