# Docker para Tradecorp.Api

Esta solución queda preparada para levantar el backend y PostgreSQL con `docker compose`.

## Servicios

- `api`: construye la imagen desde `Tracor.API/Dockerfile` (entrypoint: `Tradecorp.API.dll`) y expone la API en `http://localhost:8080`.
- `db`: usa `postgres:16-alpine`, expone PostgreSQL en `localhost:5432` y monta `Tracor.Infrastructure/database/schema.sql` como script de inicialización.

## Cómo levantar todo

Desde la raíz del repositorio:

```bash
docker compose up -d --build
```

## Qué hace la base de datos

- Crea la base `Tradecorp`.
- Ejecuta `schema.sql` solo en el primer arranque del volumen `postgres_data`.
- Mantiene los datos en un volumen persistente para que no se pierdan al reiniciar los contenedores.

## Variables importantes

La API recibe la cadena de conexión por configuración de entorno:

- `ConnectionStrings__DefaultConnection=Host=db;Port=5432;Database=Tradecorp;Username=postgres;Password=postgres`

Eso permite que la misma aplicación funcione también fuera de Docker usando `appsettings.json`.

## Flujo de arranque

- PostgreSQL inicia primero.
- El healthcheck espera a que la base responda.
- La API arranca después y usa el host `db` dentro de la red de Compose.

## Puertos

- API: `8080`
- PostgreSQL: `5432`

## Detener y limpiar

```bash
docker compose down
```

Para borrar también el volumen de datos:

```bash
docker compose down -v
```
