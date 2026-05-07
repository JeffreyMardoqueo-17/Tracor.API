# Docker para Tradecorp.Api

Esta solución queda preparada para levantar el backend y PostgreSQL con `docker compose`.

## Servicios

- `api`: construye la imagen desde `Tracor.API/Dockerfile` (entrypoint: `Tradecorp.API.dll`) y expone la API en `http://localhost:8080`.
- `db`: usa `postgres:16-alpine`, expone PostgreSQL en `localhost:5432` y arranca con un volumen persistente vacío para que EF Core aplique las migraciones.
- `db`: se ejecuta en zona horaria UTC (`TZ=UTC`, `PGTZ=UTC`) para mantener consistencia en cálculos diarios y cortes.

## Cómo levantar todo

Desde la raíz del repositorio:

```bash
docker compose up -d --build
```

## Qué hace la base de datos

- Crea la base `Tradecorp`.
- Mantiene los datos en un volumen persistente para que no se pierdan al reiniciar los contenedores.
- El esquema lo controla EF Core mediante migraciones.

## Variables importantes

La API recibe la cadena de conexión por configuración de entorno:

- `ConnectionStrings__DefaultConnection=Host=db;Port=5432;Database=Tradecorp;Username=postgres;Password=postgres`

Eso permite que la misma aplicación funcione también fuera de Docker usando `appsettings.json`.

## Flujo de arranque

- PostgreSQL inicia primero.
- El healthcheck espera a que la base responda.
- La API arranca después, usa el host `db` dentro de la red de Compose y aplica las migraciones antes de atender tráfico.

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
