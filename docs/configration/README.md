# Configuracion Base del Sistema

Esta guia describe los parametros que gobiernan calculo de ganancias, cortes y comision de empresa.

## Objetivo

Permitir que la empresa cambie reglas globales sin desplegar codigo:

- Dias por cuatrimestre.
- Dias base por mes.
- Comision de empresa.
- Uso de dias exactos para contratos iniciados en el primer tramo del anio.
- Regla de anio bisiesto.
- Fechas de corte/pago configurables.

## Entidades principales

1. `ConfiguracionSistema`
- `DiasCuatrimestreBase`: default 120.
- `DiasMesBase`: default 30.
- `ComisionEmpresaPorcentaje`: default 5.
- `UsarDiasExactosPrimerCuatrimestre`: habilita prorrateo real para contratos iniciados en el primer tramo del anio.
- `AplicarReglaAnioBisiesto`: permite tratar correctamente los dias de febrero en anios bisiestos.

2. `ConfiguracionCortesPago`
- Agrupa una lista de fechas de corte activas.
- Se pueden manejar varias configuraciones y activar/desactivar.

3. `FechasCortePago`
- Define cada fecha de corte por `Mes` + `Dia` y `Orden`.
- Permite configurar el calendario de pagos de forma explicita.

## Endpoints (seguros)

Todos requieren JWT valido y rol `Admin`.

- `GET /api/configuracion/sistema`
- `PUT /api/configuracion/sistema`
- `GET /api/configuracion/cortes`
- `POST /api/configuracion/cortes`
- `PUT /api/configuracion/cortes/{id}`
- `DELETE /api/configuracion/cortes/{id}`

## Recomendaciones operativas

- Mantener una sola configuracion activa por periodo de operacion.
- Registrar cambios de comision y cortes con proceso interno de aprobacion.
- Usar UTC en base de datos y servidores para evitar desfaces de fecha.
