# PROD
FROM --platform=linux/arm64 arm64v8/node:lts-alpine AS build-stage

WORKDIR /app
COPY ./package*.json .
RUN npm install

COPY . .
RUN npm run build

# production stage
FROM --platform=linux/arm64 arm64v8/nginx:stable-alpine AS production-stage

COPY ./nginx.conf /etc/nginx/nginx.conf
COPY --from=build-stage /app/dist /usr/share/nginx/html
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]

# DEV
# FROM arm64v8/node:lts-alpine AS build-stage
# WORKDIR /app
# COPY ./package*.json .
# RUN npm install
# COPY ./ .
# RUN npm run build

# ENTRYPOINT [ "npm", "run", "dev", "--", "--host" ]