# Documentación de Endpoints de Configuración

Base URL: `http://localhost:8080/api/Configuracion`

Todos los endpoints requieren autenticación JWT con rol **Admin**.

---

## 1. GET /sistema

Obtiene la configuración activa del sistema.

### Respuesta exitosa (200)
```json
{
  "id": 1,
  "diasCuatrimestreBase": 120,
  "diasMesBase": 30,
  "comisionEmpresaPorcentaje": 5.0,
  "usarDiasExactosPrimerCuatrimestre": true,
  "aplicarReglaAnioBisiesto": true,
  "activo": true,
  "fechaActualizacion": "2026-05-04T00:00:00Z",
  "cortesPago": [
    {
      "id": 1,
      "nombre": "Corte Mensual",
      "activo": true,
      "fechas": [
        {
          "id": 1,
          "orden": 1,
          "dia": 15,
          "mes": 1
        }
      ]
    }
  ]
}
```

### Posibles errores
- **500**: Error interno (usualmente falta la tabla en BD)

---

## 2. PUT /sistema

Actualiza la configuración del sistema. Si no existe, se crea automáticamente.

### Request Body
```json
{
  "diasCuatrimestreBase": 120,
  "diasMesBase": 30,
  "comisionEmpresaPorcentaje": 5.0,
  "usarDiasExactosPrimerCuatrimestre": true,
  "aplicarReglaAnioBisiesto": true,
  "activo": true
}
```

### Validaciones
- `diasCuatrimestreBase`: Entero positivo (> 0)
- `diasMesBase`: Entero positivo (> 0)
- `comisionEmpresaPorcentaje`: Decimal >= 0
- `usarDiasExactosPrimerCuatrimestre`: Booleano
- `aplicarReglaAnioBisiesto`: Booleano
- `activo`: Booleano

### Respuesta exitosa (200)
Mismo formato que GET /sistema

---

## 3. GET /cortes

Obtiene todos los cortes de pago configurados.

### Respuesta exitosa (200)
```json
[
  {
    "id": 1,
    "nombre": "Corte Mensual",
    "activo": true,
    "fechas": [
      {
        "id": 1,
        "orden": 1,
        "dia": 15,
        "mes": 1
      },
      {
        "id": 2,
        "orden": 2,
        "dia": 30,
        "mes": 1
      }
    ]
  }
]
```

---

## 4. POST /cortes

Crea un nuevo corte de pago con sus fechas.

### Request Body
```json
{
  "nombre": "Corte Trimestral",
  "activo": true,
  "fechas": [
    {
      "orden": 1,
      "dia": 10,
      "mes": 1
    },
    {
      "orden": 2,
      "dia": 10,
      "mes": 4
    },
    {
      "orden": 3,
      "dia": 10,
      "mes": 7
    },
    {
      "orden": 4,
      "dia": 10,
      "mes": 10
    }
  ]
}
```

### Validaciones para FechasCorte
- `orden`: Entero >= 1, único por corte
- `dia`: 1-31
- `mes`: 1-12
- La combinación `mes` + `dia` debe ser única por corte

### Respuesta exitosa (201)
```json
{
  "id": 2,
  "nombre": "Corte Trimestral",
  "activo": true,
  "fechas": [...]
}
```

---

## 5. PUT /cortes/{id}

Actualiza un corte de pago existente.

### Request Body
Mismo formato que POST /cortes

### Respuesta exitosa (200)
Mismo formato que POST /cortes

### Errores
- **404**: Si el corte con el ID no existe

---

## 6. DELETE /cortes/{id}

Elimina un corte de pago y sus fechas asociadas.

### Respuesta exitosa (204)
Sin contenido

### Errores
- **404**: Si el corte con el ID no existe

---

## Ejemplos con curl

### Obtener configuración
```bash
curl -X GET "http://localhost:8080/api/Configuracion/sistema" \
  -H "Authorization: Bearer TU_TOKEN"
```

### Actualizar configuración
```bash
curl -X PUT "http://localhost:8080/api/Configuracion/sistema" \
  -H "Authorization: Bearer TU_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "diasCuatrimestreBase": 120,
    "diasMesBase": 30,
    "comisionEmpresaPorcentaje": 5.0,
    "usarDiasExactosPrimerCuatrimestre": true,
    "aplicarReglaAnioBisiesto": true,
    "activo": true
  }'
```

### Crear corte de pago
```bash
curl -X POST "http://localhost:8080/api/Configuracion/cortes" \
  -H "Authorization: Bearer TU_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "nombre": "Corte Quincenal",
    "activo": true,
    "fechas": [
      {"orden": 1, "dia": 15, "mes": 1},
      {"orden": 2, "dia": 30, "mes": 1}
    ]
  }'
```

### Eliminar corte
```bash
curl -X DELETE "http://localhost:8080/api/Configuracion/cortes/1" \
  -H "Authorization: Bearer TU_TOKEN"
```

---

## Notas importantes

1. **Inicialización automática**: Al iniciar la aplicación en desarrollo, se ejecuta `EnsureCreated()` para crear las tablas automáticamente si no existen.

2. **Configuración por defecto**: Si no existe configuración activa, se crea una con valores:
   - DiasCuatrimestreBase: 120
   - DiasMesBase: 30
   - ComisionEmpresaPorcentaje: 5%
   - UsarDiasExactosPrimerCuatrimestre: true
   - AplicarReglaAnioBisiesto: true

3. **Solo un sistema activo**: La consulta GET devuelve la configuración más reciente con `Activo = true`.
