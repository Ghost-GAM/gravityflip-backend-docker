# ─────────────────────────────────────────────────────────────────────────────
# ETAPA 1: Compilar la aplicación NestJS
# ─────────────────────────────────────────────────────────────────────────────
FROM node:20-alpine AS build

WORKDIR /app

# Copiar package.json y package-lock.json primero (mejor cache)
COPY package*.json ./

# Instalar dependencias (incluyendo dev para poder compilar)
RUN npm install

# Copiar el resto del código
COPY . .

# Compilar TypeScript a JavaScript
RUN npm run build


# ─────────────────────────────────────────────────────────────────────────────
# ETAPA 2: Imagen final ligera (solo lo necesario)
# ─────────────────────────────────────────────────────────────────────────────
FROM node:20-alpine

WORKDIR /app

# Copiar solo lo necesario de la etapa anterior
COPY --from=build /app/package*.json ./
COPY --from=build /app/dist ./dist
COPY --from=build /app/node_modules ./node_modules

# Exponer el puerto de la API
EXPOSE 3000

# Comando que se ejecuta al iniciar el contenedor
CMD ["node", "dist/main.js"]
