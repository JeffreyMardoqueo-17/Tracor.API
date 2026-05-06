# Manejo Global de Errores

Esta API usa un middleware centralizado para manejo de excepciones, evitando saturar controladores con bloques try/catch.

## Objetivo

- Mantener controladores delgados (solo orquestacion HTTP).
- Concentrar traduccion de excepciones a respuestas HTTP en un solo punto.
- Homogeneizar formato de errores para frontend y auditoria.
- Registrar errores con `traceId` para rastreo rapido.

## Middleware

Archivo: `Tracor.API/Middleware/GlobalExceptionMiddleware.cs`

Responsabilidades:

- Capturar excepciones no manejadas del pipeline.
- Mapear excepciones de negocio/infraestructura a codigos HTTP.
- Escribir respuesta `application/problem+json` uniforme.
- Registrar logs estructurados con metodo, ruta y traceId.

Mapa actual:

- `InvalidOperationException` -> `400 Bad Request`
- `KeyNotFoundException` -> `404 Not Found`
- `UnauthorizedAccessException` -> `401 Unauthorized`
- `BadHttpRequestException` -> `400 Bad Request`
- Cualquier otra -> `500 Internal Server Error`

## Formato de respuesta

Respuesta tipo Problem Details simplificada:

```json
{
  "type": "https://httpstatuses.com/400",
  "title": "Solicitud invalida",
  "status": 400,
  "detail": "Mensaje de error de negocio",
  "traceId": "00-..."
}
```

## Uso en Program.cs

Se registra temprano en el pipeline:

```csharp
app.UseGlobalExceptionHandling();
```

Debe ir antes de autenticacion/autorizacion y antes de endpoints para capturar todo.

## Guia de buenas practicas

- En controladores: no usar try/catch para errores de negocio comunes.
- En servicios/repositorios: lanzar excepciones semanticas (`InvalidOperationException`, `KeyNotFoundException`, etc.).
- En middleware: mantener la conversion a HTTP y logging.
- Para casos de dominio complejos: considerar excepciones custom por modulo si necesitas granularidad adicional.
