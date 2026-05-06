# Flujo de Vida de Contratos

Este flujo resume el comportamiento esperado para multiples contratos, unificacion/desunificacion y decisiones de pago.

## 1. Apertura de contratos

- Un cliente puede abrir varios contratos aunque ya tenga otros activos.
- Cada contrato nace con su `CapitalInicial`, `PorcentajeMensual`, modalidad (`Normal` o `InteresCompuesto`) y configuracion dinamica inicial.

## 2. Unificacion y desunificacion

- El sistema registra relaciones entre contratos en `ContratoRelaciones`.
- Una operacion de unificacion/desunificacion guarda:
  - contrato origen,
  - contrato destino,
  - tipo de relacion,
  - fecha/hora,
  - usuario ejecutor,
  - monto transferido,
  - observacion,
  - id de grupo de operacion para auditoria completa.

Con esto se puede reconstruir historicamente cualquier cambio y trazabilidad de fondos.

## 3. Calculo cuatrimestral por dias

- La base es por cuatrimestres (default 120 dias), configurable.
- Se contempla regla de dias exactos para contratos del primer tramo del anio.
- Se contempla regla de 30 dias por mes para contratos que entren a la modalidad estandar de la empresa.
- La configuracion permite contemplar anio bisiesto.

## 4. Fechas de corte y pago

- Las fechas de corte se parametrizan en `FechasCortePago`.
- Se pueden definir varias fechas por configuracion (por ejemplo, 3 fechas de pago).
- El sistema usa estas fechas para determinar cierre de calculo y generacion de pago.

## 5. Decisiones del cliente al pagar

Por cada pago, se guarda una o varias decisiones en `DecisionesPago`:

- `RetiroTotal`
- `ReinversionTotal`
- `ReinversionParcial`
- `TrasladoAInteresCompuesto`
- `SalidaDeInteresCompuesto`

Cada decision puede incluir montos retirados, reinvertidos o enviados a interes compuesto, metodo de retiro y observaciones.

## 6. Auditoria

Ademas de `ContratoRelaciones`, el sistema guarda:

- `MovimientosContrato` para aportes/retiros/reinversiones y efectos de unificacion.
- `ConfiguracionContrato` para cambios de capital base y porcentaje por periodos.

Esto permite trazabilidad contable y operativa punta a punta.
