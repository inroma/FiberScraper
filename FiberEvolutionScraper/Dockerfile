# DEV
# FROM node:lts-alpine as build-stage
# 
# WORKDIR /app
# COPY ./package*.json .
# RUN npm install
# 
# COPY ./ .
# RUN npm run build
# 
# # production stage
# FROM nginx:stable-alpine as production-stage
# 
# COPY --from=build-stage /app/dist /usr/share/nginx/html
# EXPOSE 8080
# CMD ["nginx", "-g", "daemon off;"]

# PROD
FROM arm64v8/node:lts-alpine as build-stage
WORKDIR /app
COPY ./package*.json .
RUN npm install
COPY ./ .
RUN npm run build

# production stage
FROM arm64v8/nginx:stable-alpine as production-stage
RUN mkdir /app
COPY --from=build-stage /app/dist /app
COPY nginx.conf /etc/nginx/nginx.conf