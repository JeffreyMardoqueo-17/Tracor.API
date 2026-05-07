```mermaid
flowchart TD
    %% =====================
    %% INICIO
    %% =====================
    A([Inicio])

    %% =====================
    %% VALIDAR CLIENTE
    %% =====================
    A --> B{¿Cliente existe?}

    %% =====================
    %% CLIENTE NO EXISTE
    %% =====================
    B -- No --> C[Ingresar datos del cliente]
    C --> D[Crear registro en Clientes]

    %% =====================
    %% REGISTRAR DIRECCIÓN
    %% =====================
    D --> E[Ingresar dirección del cliente]
    E --> F[Seleccionar país]
    F --> G[Guardar en ClienteDirecciones]

    %% =====================
    %% REGISTRAR CUENTAS BANCARIAS
    %% =====================
    G --> H{¿Desea registrar cuenta bancaria?}

    H -- Sí --> I[Seleccionar banco]
    I --> J[Ingresar datos de la cuenta]
    J --> K{¿Es cuenta principal?}

    K -- Sí --> L[Marcar como principal]
    K -- No --> M[Guardar como secundaria]

    L --> N[Guardar en ClienteCuentas]
    M --> N

    %% =====================
    %% MÚLTIPLES CUENTAS
    %% =====================
    N --> O{¿Registrar otra cuenta bancaria?}
    O -- Sí --> I
    O -- No --> Z([Fin])

    %% =====================
    %% CLIENTE EXISTE
    %% =====================
    B -- Sí --> P[Seleccionar cliente existente]
    P --> Q{¿Agregar nueva cuenta bancaria?}
    Q -- Sí --> I
    Q -- No --> Z

    %% =====================
    %% ESTILOS
    %% =====================
    classDef inicioFin fill:#0f172a,color:#ffffff,stroke:#1e293b,stroke-width:2px
    classDef proceso fill:#1e40af,color:#ffffff,stroke:#1e3a8a
    classDef decision fill:#f59e0b,color:#000000,stroke:#b45309
    classDef almacenamiento fill:#16a34a,color:#ffffff,stroke:#166534

    class A,Z inicioFin
    class C,D,E,F,I,J,L,M,P proceso
    class B,H,K,O,Q decision
    class G,N almacenamiento
````