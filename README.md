# GravityFlip Backend — Versión Dockerizada

Backend completo del videojuego **GravityFlip** con todo dentro de contenedores Docker. **Solo necesitas Docker instalado.**

> Esta es la versión **dockerizada** del proyecto. Si prefieres correrlo nativamente con .NET y Node.js, mira el repositorio:
> [gravityflip-backend](https://github.com/Ghost-GAM/gravityflip-backend)

## Arquitectura

```
        Cliente (Postman / Unity / Web)
                    │
        ┌───────────┴────────────┐
        ▼                        ▼
   API .NET                 API NestJS
   puerto 5044              puerto 3000
   (escrituras + JWT)       (solo lectura)
        │                        │
        ▼                        ▼
   SQL Server               PostgreSQL
   puerto 1433              puerto 5432
   (PRIMARIO)               (RÉPLICA)
        │                        ▲
        └───── CRON cada 6s ─────┘
```

Los **4 servicios** corren en contenedores Docker conectados por una red interna.

## Tecnologías

| Servicio | Tecnología | Rol |
|---|---|---|
| API principal | .NET 8 (C#) hexagonal | Escrituras + autenticación JWT |
| API secundaria | NestJS (Node.js + TS) | Solo lectura |
| Base de datos primaria | SQL Server 2022 | Recibe escrituras |
| Base de datos réplica | PostgreSQL 16 | Solo lectura |

## Requisitos

Solo necesitas:

- [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- [Git](https://git-scm.com/)

**No necesitas instalar:** .NET, Node.js, SQL Server ni PostgreSQL.

## Cómo correr

1. Abre **Docker Desktop**
2. Clona el repositorio:
```bash
git clone https://github.com/Ghost-GAM/gravityflip-backend-docker.git
cd gravityflip-backend-docker
```

3. Levanta todos los servicios:
```bash
docker-compose up -d
```

> La primera vez tarda 2-3 minutos porque descarga imágenes y compila el código.
> Las siguientes veces tarda 5-15 segundos.

## Endpoints

### API .NET (puerto 5044)

#### `POST /api/auth/login` — Login / Registro

```json
{
  "nombreJugador": "Kevin",
  "password": "1234"
}
```

Devuelve un token JWT.

#### `POST /api/resultado/guardar` — Guardar puntaje (requiere JWT)

Headers:
```
Authorization: Bearer <token>
```

Body:
```json
{
  "nombreJugador": "Kevin",
  "password": "1234",
  "tiempoSegundos": 150,
  "muertes": 2
}
```

#### `GET /api/resultado/tabla` — Top 10 (público)

### API NestJS (puerto 3000)

#### `GET /api/resultado/tabla` — Top 10

Lee de PostgreSQL (réplica).

#### `GET /api/resultado/estadisticas` — Estadísticas globales

```json
{
  "totalJugadores": 5,
  "mejorPuntaje": 50000,
  "promedioPuntaje": 44800,
  "totalMuertes": 2,
  "fuente": "PostgreSQL (replica de solo lectura)"
}
```

## Seguridad

| Vulnerabilidad | Protección |
|---|---|
| **XSS / SQL Injection** | Validación estricta con regex y filtro de patrones peligrosos |
| **Suplantación de credenciales** | Contraseñas hasheadas con SHA256 |
| **Consultas sin autorización** | JWT obligatorio para escrituras |

## Replicación

- **SQL Server** recibe todas las escrituras (INSERT, UPDATE)
- **PostgreSQL** es solo lectura
- Un servicio CRON sincroniza ambas bases cada **6 segundos**

## Detener todo

```bash
docker-compose down
```

Para borrar también los datos:
```bash
docker-compose down -v
```

## Ver logs

```bash
# API .NET
docker logs gravityflip_api_net

# API NestJS
docker logs gravityflip_api_nestjs

# Bases de datos
docker logs gravityflip_db
docker logs gravityflip_db_replica
```