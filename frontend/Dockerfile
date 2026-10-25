FROM node:18-alpine AS build
WORKDIR /app

# Объявляем аргумент, который прилетит из docker-compose
ARG VITE_BOT_URL
# Делаем его доступным как переменную окружения на этапе сборки Vite
ENV VITE_BOT_URL=$VITE_BOT_URL

COPY package*.json ./
RUN npm install
COPY . .

# Во время npm run build Vite автоматически подставит VITE_BOT_URL в собственный код
RUN npm run build

FROM nginx:alpine
COPY --from=build /app/dist /usr/share/nginx/html
COPY default.conf.template /etc/nginx/templates/default.conf.template

EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]