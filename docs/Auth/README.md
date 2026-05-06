# Auth

## Objetivo
Mantener una sesión segura con JWT emitido por el backend y almacenado en la cookie HttpOnly `access_token`.

## Flujo
1. El usuario envía `POST /api/auth/login` con correo y contraseña.
2. El backend valida credenciales y genera un JWT con duración de 24 horas.
3. El JWT se guarda en la cookie HttpOnly `access_token` con `Path=/`, `SameSite=Lax` y expiración alineada al token.
4. El frontend no lee el token; solo envía credenciales y deja que el navegador administre la cookie.
5. El frontend consulta `GET /api/auth/me` para rehidratar la sesión cuando se abre la app.
6. `GET /api/auth/logout` elimina la cookie y cierra la sesión.

## Endpoints
- `POST /api/auth/login`: autentica al usuario y establece la cookie.
- `GET /api/auth/me`: devuelve el usuario autenticado según la cookie actual.
- `POST /api/auth/logout`: elimina la cookie `access_token`.

## Seguridad
- El token no se expone al cliente.
- La autenticación del backend depende de la cookie HttpOnly.
- La duración del token se controla con `Jwt:ExpireHours`.

## Notas
- La validación del JWT se configura en [Tracor.API/Program.cs](../../Tracor.API/Program.cs).
- La creación del token vive en [Tracor.Infrastructure/Services/JwtService.cs](../../Tracor.Infrastructure/Services/JwtService.cs).
- La lógica de login y cookie vive en [Tracor.API/Controllers/AuthController.cs](../../Tracor.API/Controllers/AuthController.cs).