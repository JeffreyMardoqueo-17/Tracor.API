# API de Usuarios y Autenticación

Endpoints implementados:

- POST /api/users
  - Crea un usuario.
  - Body: `{ "nombre": "...", "email": "...", "password": "...", "rol": "Admin" }`
  - Respuesta: `201 Created` con `UserResponseDto`.

- POST /api/auth/login
  - Inicia sesión con `{ "email": "...", "password": "..." }`.
  - El JWT se guarda en una cookie HttpOnly llamada `access_token`.
  - Respuesta: `200 OK` con `{ expiresAt, user }`.
  - El token es JWT con duración de 24 horas.

- POST /api/auth/logout
  - Elimina la cookie HttpOnly `access_token`.

- GET /api/users
  - Lista usuarios activos. La autenticación se toma desde la cookie `access_token`.

- GET /api/users/{id}
  - Obtiene usuario por id. Requiere autorización.

- PUT /api/users/{id}
  - Actualiza campos permitidos. Requiere autorización.

- DELETE /api/users/{id}
  - Soft delete (marca `Activo=false`). Requiere autorización.

- GET /api/configuracion/sistema
  - Obtiene la configuración global activa del sistema.
  - Requiere rol `Admin`.

- PUT /api/configuracion/sistema
  - Actualiza configuración global (`dias`, `comision`, reglas de cálculo).
  - Requiere rol `Admin`.

- GET /api/configuracion/cortes
  - Lista cortes de pago configurados con sus fechas.
  - Requiere rol `Admin`.

- POST /api/configuracion/cortes
  - Crea una configuración de corte con sus fechas.
  - Requiere rol `Admin`.

- PUT /api/configuracion/cortes/{id}
  - Edita nombre/estado/fechas de un corte.
  - Requiere rol `Admin`.

- DELETE /api/configuracion/cortes/{id}
  - Elimina una configuración de corte.
  - Requiere rol `Admin`.

## Flujo de autenticación JWT

1. Primero se crea el usuario con `POST /api/users`.
2. La contraseña nunca se guarda en texto plano. Se guarda un hash usando BCrypt.
3. Luego se inicia sesión con `POST /api/auth/login` usando `email` y `password`.
4. Si las credenciales son válidas, el API devuelve un JWT firmado.
5. En vez de exponer el token al frontend, el backend lo guarda en una cookie HttpOnly llamada `access_token`.
6. El navegador enviará esa cookie automáticamente en las peticiones al mismo dominio, y el backend la leerá para autenticar al usuario.

Si alguna integración externa necesita usar el API sin navegador, también puede enviar el JWT por el header:

```http
Authorization: Bearer <token>
```

7. El token expira a las 24 horas por diseño. En esta solución el valor se controla con `Jwt:ExpireHours` y por defecto es `24`.

### Qué contiene el JWT

El token se emite con estas referencias principales:

- `sub`: id del usuario.
- `email`: correo del usuario.
- `name`: nombre del usuario.
- `role`: rol del usuario.

El token también se firma con `Jwt:Key`, y se valida contra `Jwt:Issuer` y `Jwt:Audience`.

### Cookie HttpOnly

La cookie `access_token` se emite con estas propiedades:

- `HttpOnly`: evita acceso desde JavaScript.
- `SameSite=Lax`: reduce riesgo de CSRF en navegación normal.
- `Secure`: se activa automáticamente cuando la solicitud llega por HTTPS.
- `Expires`: coincide con la expiración del JWT.

Esto hace que el frontend no pueda leer ni manipular el token, pero sí lo enviará automáticamente al mismo backend.

### Cómo debe funcionar

- El login solo debe aceptar usuarios activos.
- Si la contraseña no coincide, el API responde `401 Unauthorized`.
- Si el token está vencido o fue firmado con otra clave, el endpoint protegido debe rechazarlo.
- El backend no debe aceptar contraseñas sin hash ni emitir tokens sin expiración.

### Configuración requerida

En `appsettings.json` o variables de entorno debes definir:

- `Jwt:Key`: secreto fuerte y privado.
- `Jwt:Issuer`: emisor lógico del token.
- `Jwt:Audience`: audiencia esperada.
- `Jwt:ExpireHours`: duración del token en horas.

Seguridad y buenas prácticas aplicadas:

- Hash de contraseñas usando BCrypt.
- JWT firmado simétricamente con clave configurable en `appsettings.json` o variable de entorno.
- JWT almacenado en cookie HttpOnly para evitar exposición al frontend.
- Tokens expiran exactamente a las 24 horas (configurable mediante `Jwt:ExpireHours`).
- Repositorio y servicios desacoplados (patrón Repository + Service) para testabilidad y escalabilidad.
- Soft delete mediante propiedad `Activo` en la entidad `Usuario`.

Notas de despliegue:

- Cambiar `Jwt:Key` por un secreto fuerte en producción y preferiblemente inyectarlo por variables de entorno.
- Si usa Docker/Kubernetes, inyecte las variables y asegure que el reloj UTC esté en sincronía.
- Si actualizaste el código y en Docker no aparece el endpoint nuevo, normalmente el contenedor sigue usando una imagen vieja o cacheada. Prueba reconstruir explícitamente:

```bash
docker compose up -d --build
```

o bien:

```bash
docker compose build api
docker compose up -d api
```

Si Swagger sigue mostrando lo mismo, revisa también que el contenedor realmente haya sido recreado y no solo reiniciado.

### Nota importante de seguridad

Guardar el JWT en cookie HttpOnly mejora mucho la protección contra XSS, pero si el frontend está en otro dominio o subdominio distinto, puede requerir una estrategia adicional de `SameSite` y CORS. En este proyecto se dejó `SameSite=Lax` para priorizar seguridad y compatibilidad simple dentro del mismo sitio.

## Manejo global de errores

La API implementa middleware global para captura de excepciones y respuesta uniforme, evitando llenar controladores con `try/catch`.

- Middleware: `Tracor.API/Middleware/GlobalExceptionMiddleware.cs`
- Registro en pipeline: `app.UseGlobalExceptionHandling();`
- Documento de referencia: `docs/configuration/error-handling.md`

Excepciones mapeadas actualmente:

- `InvalidOperationException` -> 400
- `KeyNotFoundException` -> 404
- `UnauthorizedAccessException` -> 401
- Otras -> 500
