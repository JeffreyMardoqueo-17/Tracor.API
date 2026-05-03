# API de Usuarios y Autenticación

Endpoints implementados:

- POST /api/users
  - Crea un usuario.
  - Body: `{ "nombre": "...", "email": "...", "password": "...", "rol": "Admin" }`
  - Respuesta: `201 Created` con `UserResponseDto`.

- POST /api/auth/login
  - Inicia sesión con `{ "email": "...", "password": "..." }`.
  - Respuesta: `200 OK` con `{ token, expiresAt, user }`.
  - El token es JWT con duración de 24 horas.

- GET /api/users
  - Lista usuarios activos. Requiere `Authorization: Bearer <token>`.

- GET /api/users/{id}
  - Obtiene usuario por id. Requiere autorización.

- PUT /api/users/{id}
  - Actualiza campos permitidos. Requiere autorización.

- DELETE /api/users/{id}
  - Soft delete (marca `Activo=false`). Requiere autorización.

## Flujo de autenticación JWT

1. Primero se crea el usuario con `POST /api/users`.
2. La contraseña nunca se guarda en texto plano. Se guarda un hash usando BCrypt.
3. Luego se inicia sesión con `POST /api/auth/login` usando `email` y `password`.
4. Si las credenciales son válidas, el API devuelve un JWT firmado.
5. Ese token debe enviarse en los endpoints protegidos con el header:

```http
Authorization: Bearer <token>
```

6. El token expira a las 24 horas por diseño. En esta solución el valor se controla con `Jwt:ExpireHours` y por defecto es `24`.

### Qué contiene el JWT

El token se emite con estas referencias principales:

- `sub`: id del usuario.
- `email`: correo del usuario.
- `name`: nombre del usuario.
- `role`: rol del usuario.

El token también se firma con `Jwt:Key`, y se valida contra `Jwt:Issuer` y `Jwt:Audience`.

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
