FROM node:22-alpine AS build
WORKDIR /app

COPY package.json package-lock.json* ./
# TODO: закоммитить package-lock.json и оставить только `npm ci`
RUN if [ -f package-lock.json ]; then npm ci; else npm install; fi

COPY . .
RUN npm run build

FROM nginx:1.27-alpine
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 80
