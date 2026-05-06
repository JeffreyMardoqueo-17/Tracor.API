# Documento Maestro de Reglas del Negocio
## Sistema de Gestión de Contratos de Inversión

> Este documento define las **reglas absolutas, inmutables y obligatorias** del negocio.
> Todo agente de IA, proceso automático o desarrollo humano debe **obedecer estrictamente** este documento.
>  
> **No se permiten interpretaciones creativas.**
> **No se permiten suposiciones.**
> **El dinero se calcula exactamente por días.**

---

## 1. Propósito del Documento

Este documento existe para que:

- Los **agentes de IA entiendan el dominio financiero real**
- Las reglas de negocio se apliquen **de forma consistente**
- No existan errores por fechas, montos o estados
- Se garantice **trazabilidad total y auditoría completa**

Este documento es la **fuente única de verdad (Single Source of Truth)**.

---

## 2. Principios Fundamentales del Sistema

1. El sistema maneja **dinero real**
2. **Nada se elimina**
3. Todo cambio debe quedar **registrado históricamente**
4. Los cálculos deben ser:
   - Exactos por días
   - Sin redondeos arbitrarios
   - Sin pagos extra ni pagos incompletos
5. Las reglas aquí descritas **no son configurables**
6. El sistema debe prevenir errores humanos, no permitirlos

---

## 3. Entidades Principales

### 3.1 Cliente

- Tiene **un único código de cliente**
- Puede tener **uno o varios contratos**
- Puede tener contratos activos, finalizados o unificados

### 3.2 Contrato

- Tiene **código de contrato único**
- Pertenece a un solo cliente
- Tiene:
  - Capital
  - Porcentaje
  - Fecha de inicio
  - Estado
- Puede:
  - Unificarse
  - Desunificarse
  - Finalizarse
  - Reinvertirse

---

## 4. Reglas de Contratos

### 4.1 Creación de Contratos

- Un cliente puede crear **nuevos contratos aunque tenga otros activos**
- Las ganancias empiezan a contar **el mismo día de creación**
- No existe período de gracia

---

### 4.2 Inyección de Capital

Si un cliente **inyecta capital**:

1. El contrato vigente **se finaliza**
2. Se crea **un nuevo contrato**
3. El historial del contrato anterior se conserva intacto

---

### 4.3 Reinversión de Ganancias

Si un cliente **reinverte ganancias**:

- El contrato **NO se finaliza**
- Solo se modifica el saldo
- El porcentaje **permanece igual**
- Existe una opción excepcional de cambiar porcentaje (6% – 8.50%)

---

## 5. Unificación y Desunificación de Contratos

### 5.1 Unificación

- Varios contratos pueden unificarse en uno solo
- El contrato resultante:
  - Suma capitales
  - Conserva trazabilidad de origen

### 5.2 Desunificación

- Un contrato unificado puede separarse nuevamente
- Los contratos originales **no se pierden**

### 5.3 Registro Histórico Obligatorio

Cada acción debe guardar:

- Fecha
- Usuario
- Tipo de acción
- Contratos origen
- Contrato destino
- Montos involucrados

> **Nada se borra. Nada se sobrescribe.**

---

## 6. Interés Compuesto

- Un contrato puede operar bajo interés compuesto
- El cliente puede:
  - Mantener interés compuesto
  - Salir del interés compuesto
- Al cambiar modalidad:
  - Se cierra el esquema anterior
  - Se crea un nuevo contrato
  - Se conserva el historial

---

## 7. Cálculo de Ganancias

### 7.1 Periodos

- El sistema paga por **cuatrimestres**
- Cada cuatrimestre = **120 días**
- Hay **3 pagos al año**

---

### 7.2 Fechas de Pago (Fijas e Inmutables)

- 30 de abril
- 30 de agosto
- 30 de diciembre

Estas fechas:
- No se configuran
- No se modifican
- No se recalculan

---

### 7.3 Reglas de Conteo de Días

- Las ganancias se calculan **por días exactos**
- Reglas:

| Mes        | Días contables |
|------------|----------------|
| Enero      | Días reales desde inicio |
| Febrero   | Siempre 28 días |
| Otros     | Siempre 30 días |

#### Regla Especial

- Contratos iniciados **antes del 1 de enero**:
  - Todos los cuatrimestres cuentan **120 días exactos**

---

## 8. Comisión de la Empresa

- Comisión fija del **5%**
- Se descuenta **solo de la ganancia**
- Nunca del capital
- No es configurable

---

## 9. Préstamos

### 9.1 Límite

- Préstamo máximo: **50% del capital total del cliente**
- Nunca puede superar el capital disponible

### 9.2 Impacto

- El préstamo se descuenta automáticamente
- Puede anular pagos de ganancia hasta cubrir deuda

---

## 10. Pagos

### 10.1 Estructura de Pagos

- Cada contrato tiene **3 columnas de pago**
- Cada columna corresponde a una fecha fija
- Reglas:
  - No se puede pagar fuera de fecha
  - No se puede saltar un pago anterior
  - No se pueden marcar pagos futuros

---

### 10.2 Estados del Contrato

- Pendiente
- En progreso
- Pagado
- Finalizado

> Último pago ⇒ **Pagado y Finalizado**

---

### 10.3 Modalidades de Pago

En cada pago el cliente puede elegir:

- Efectivo
- Transferencia
- Reinversión parcial
- Reinversión total

---

## 11. Recibos

- Todo pago genera recibo
- El recibo:
  - Está ligado a un ejecutivo
  - Se genera en PDF o DOC
  - Debe estar listo para impresión

---

## 12. Filtros del Sistema

El sistema debe filtrar por:

- Rango de ganancias
- País
- Banco
- Efectivo / Transferencia
- Estado del pago

---

## 13. Dashboards

### 13.1 Dashboard Ejecutivo

Debe mostrar:

- Total de clientes
- Clientes nacionales vs extranjeros
- Pagos por transferencia
- Pagos en efectivo
- Total de inversores

---

### 13.2 Dashboard Administrativo

Debe mostrar:

- Total global de clientes
- Total a pagar en la próxima fecha
- Desglose por modalidad
- Progreso de pagos en tiempo real
- Filtros por monto y estado

---

## 14. Reglas Finales para Agentes de IA

- No asumir reglas no escritas
- No modificar fechas
- No redondear días
- No eliminar datos
- No permitir estados inválidos
- Siempre registrar cambios

> **Si una acción no cumple este documento, no debe ejecutarse.**

---

## 15. Declaración Final

Este sistema existe para garantizar:

- Justicia financiera
- Precisión matemática
- Control total
- Confianza absoluta

El dinero no se negocia.
Las reglas no se interpretan.
Este documento se obedece.