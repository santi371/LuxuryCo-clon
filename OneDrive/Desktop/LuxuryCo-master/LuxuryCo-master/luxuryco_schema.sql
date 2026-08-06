-- ============================================================
--  LUXURYCO — ESQUEMA COMPLETO DE BASE DE DATOS
--  PostgreSQL (Supabase) — Parte 1: ESTRUCTURA
--  Ejecutar en: Supabase Dashboard > SQL Editor
-- ============================================================

-- ══════════════════════════════════════════════════════════════
-- 1. ROLES
-- ══════════════════════════════════════════════════════════════
CREATE TABLE IF NOT EXISTS rol (
    id_rol          SERIAL PRIMARY KEY,
    nombre_rol      VARCHAR(50)  NOT NULL,
    descripcion     VARCHAR(150)
);

-- ══════════════════════════════════════════════════════════════
-- 2. SEDES
-- ══════════════════════════════════════════════════════════════
CREATE TABLE IF NOT EXISTS sede (
    id_sede         SERIAL PRIMARY KEY,
    nombre          VARCHAR(100) NOT NULL,
    ciudad          VARCHAR(100) NOT NULL,
    direccion       VARCHAR(255),
    telefono        VARCHAR(50),
    activa          BOOLEAN DEFAULT true,
    fecha_creacion  TIMESTAMP DEFAULT NOW()
);

-- ══════════════════════════════════════════════════════════════
-- 3. USUARIOS
-- ══════════════════════════════════════════════════════════════
CREATE TABLE IF NOT EXISTS usuario (
    id_usuario            SERIAL PRIMARY KEY,
    nombre                VARCHAR(100) NOT NULL,
    apellido              VARCHAR(100) NOT NULL,
    email                 VARCHAR(150) NOT NULL UNIQUE,
    password_hash         VARCHAR(255) NOT NULL,
    telefono              VARCHAR(20),
    fecha_registro        TIMESTAMP DEFAULT NOW(),
    activo                BOOLEAN DEFAULT true,
    id_rol                INT REFERENCES rol(id_rol),
    id_sede               INT REFERENCES sede(id_sede),
    foto_perfil_url       TEXT,
    two_factor_enabled    BOOLEAN DEFAULT false,
    two_factor_secret     TEXT,
    reset_password_code   VARCHAR(6),
    reset_password_expires TIMESTAMP
);

-- ══════════════════════════════════════════════════════════════
-- 4. DEPARTAMENTOS Y CIUDADES
-- ══════════════════════════════════════════════════════════════
CREATE TABLE IF NOT EXISTS departamento (
    id_departamento     SERIAL PRIMARY KEY,
    nombre_departamento VARCHAR(100) NOT NULL
);

CREATE TABLE IF NOT EXISTS ciudad (
    id_ciudad        SERIAL PRIMARY KEY,
    nombre_ciudad    VARCHAR(100) NOT NULL,
    id_departamento  INT REFERENCES departamento(id_departamento)
);

-- ══════════════════════════════════════════════════════════════
-- 5. DIRECCIONES DE USUARIO
-- ══════════════════════════════════════════════════════════════
CREATE TABLE IF NOT EXISTS direccion_usuario (
    id_direccion  SERIAL PRIMARY KEY,
    id_usuario    INT REFERENCES usuario(id_usuario),
    direccion     VARCHAR(200) NOT NULL,
    referencia    VARCHAR(200),
    id_ciudad     INT REFERENCES ciudad(id_ciudad)
);

-- ══════════════════════════════════════════════════════════════
-- 6. CATEGORÍAS
-- ══════════════════════════════════════════════════════════════
CREATE TABLE IF NOT EXISTS categoria (
    id_categoria  SERIAL PRIMARY KEY,
    nombre        VARCHAR(100) NOT NULL,
    descripcion   VARCHAR(200)
);

-- ══════════════════════════════════════════════════════════════
-- 7. MARCAS
-- ══════════════════════════════════════════════════════════════
CREATE TABLE IF NOT EXISTS marca (
    id_marca     SERIAL PRIMARY KEY,
    nombre       VARCHAR(100) NOT NULL,
    descripcion  VARCHAR(200),
    logo_url     VARCHAR(500)
);

-- ══════════════════════════════════════════════════════════════
-- 8. PRODUCTOS
-- ══════════════════════════════════════════════════════════════
CREATE TABLE IF NOT EXISTS producto (
    id_producto    SERIAL PRIMARY KEY,
    nombre         VARCHAR(150) NOT NULL,
    descripcion    TEXT,
    precio         DECIMAL(10,2) NOT NULL DEFAULT 0,
    stock          INT DEFAULT 0,
    activo         BOOLEAN DEFAULT true,
    fecha_creacion TIMESTAMP DEFAULT NOW(),
    id_categoria   INT REFERENCES categoria(id_categoria),
    id_marca       INT REFERENCES marca(id_marca),
    seccion        VARCHAR(50)  -- 'Hombre', 'Mujer', 'Accesorios'
);

-- ══════════════════════════════════════════════════════════════
-- 9. IMÁGENES DE PRODUCTO
-- ══════════════════════════════════════════════════════════════
CREATE TABLE IF NOT EXISTS imagen_producto (
    id_imagen    SERIAL PRIMARY KEY,
    id_producto  INT REFERENCES producto(id_producto),
    url_imagen   TEXT,
    principal    BOOLEAN DEFAULT false,
    orden        INT DEFAULT 0
);

-- ══════════════════════════════════════════════════════════════
-- 10. RESEÑAS
-- ══════════════════════════════════════════════════════════════
CREATE TABLE IF NOT EXISTS resena (
    id_resena    SERIAL PRIMARY KEY,
    id_usuario   INT REFERENCES usuario(id_usuario),
    id_producto  INT REFERENCES producto(id_producto),
    calificacion INT CHECK (calificacion BETWEEN 1 AND 5) NOT NULL,
    comentario   TEXT,
    fecha        TIMESTAMP DEFAULT NOW()
);

-- ══════════════════════════════════════════════════════════════
-- 11. CARRITO
-- ══════════════════════════════════════════════════════════════
CREATE TABLE IF NOT EXISTS carrito (
    id_carrito     SERIAL PRIMARY KEY,
    id_usuario     INT REFERENCES usuario(id_usuario),
    fecha_creacion TIMESTAMP DEFAULT NOW()
);

CREATE TABLE IF NOT EXISTS detalle_carrito (
    id_detalle_carrito SERIAL PRIMARY KEY,
    id_carrito         INT REFERENCES carrito(id_carrito),
    id_producto        INT REFERENCES producto(id_producto),
    cantidad           INT DEFAULT 1
);

-- ══════════════════════════════════════════════════════════════
-- 12. ESTADOS DE PEDIDO
-- ══════════════════════════════════════════════════════════════
CREATE TABLE IF NOT EXISTS estado_pedido (
    id_estado_pedido SERIAL PRIMARY KEY,
    nombre_estado    VARCHAR(50) NOT NULL
);

-- ══════════════════════════════════════════════════════════════
-- 13. PEDIDOS
-- ══════════════════════════════════════════════════════════════
CREATE TABLE IF NOT EXISTS pedido (
    id_pedido        SERIAL PRIMARY KEY,
    id_usuario       INT REFERENCES usuario(id_usuario),
    id_direccion     INT REFERENCES direccion_usuario(id_direccion),
    fecha_pedido     TIMESTAMP DEFAULT NOW(),
    total            DECIMAL(10,2),
    id_estado_pedido INT REFERENCES estado_pedido(id_estado_pedido)
);

CREATE TABLE IF NOT EXISTS detalle_pedido (
    id_detalle_pedido SERIAL PRIMARY KEY,
    id_pedido         INT REFERENCES pedido(id_pedido),
    id_producto       INT REFERENCES producto(id_producto),
    cantidad          INT,
    precio_unitario   DECIMAL(10,2)
);

-- ══════════════════════════════════════════════════════════════
-- 14. ESTADOS DE ENVÍO Y ENVÍOS
-- ══════════════════════════════════════════════════════════════
CREATE TABLE IF NOT EXISTS estado_envio (
    id_estado_envio SERIAL PRIMARY KEY,
    nombre_estado   VARCHAR(50) NOT NULL
);

CREATE TABLE IF NOT EXISTS envio (
    id_envio        SERIAL PRIMARY KEY,
    id_pedido       INT REFERENCES pedido(id_pedido),
    fecha_envio     TIMESTAMP,
    numero_guia     VARCHAR(100),
    id_estado_envio INT REFERENCES estado_envio(id_estado_envio)
);

-- ══════════════════════════════════════════════════════════════
-- 15. MÉTODOS DE PAGO Y FACTURAS
-- ══════════════════════════════════════════════════════════════
CREATE TABLE IF NOT EXISTS metodo_pago (
    id_metodo_pago SERIAL PRIMARY KEY,
    nombre         VARCHAR(50) NOT NULL,
    descripcion    VARCHAR(150)
);

CREATE TABLE IF NOT EXISTS factura (
    id_factura     SERIAL PRIMARY KEY,
    id_pedido      INT REFERENCES pedido(id_pedido),
    fecha_factura  TIMESTAMP DEFAULT NOW(),
    total          DECIMAL(10,2),
    id_metodo_pago INT REFERENCES metodo_pago(id_metodo_pago)
);

-- ══════════════════════════════════════════════════════════════
-- 16. PROVEEDORES
-- ══════════════════════════════════════════════════════════════
CREATE TABLE IF NOT EXISTS proveedor (
    id_proveedor SERIAL PRIMARY KEY,
    nombre       VARCHAR(150) NOT NULL,
    contacto     VARCHAR(100),
    telefono     VARCHAR(50),
    email        VARCHAR(150),
    activo       BOOLEAN DEFAULT true
);

CREATE TABLE IF NOT EXISTS producto_proveedor (
    id_producto        INT REFERENCES producto(id_producto),
    id_proveedor       INT REFERENCES proveedor(id_proveedor),
    precio_costo       DECIMAL(10,2) NOT NULL DEFAULT 0,
    tiempo_entrega_dias INT,
    PRIMARY KEY (id_producto, id_proveedor)
);

-- ══════════════════════════════════════════════════════════════
-- 17. INVENTARIO POR SEDE
-- ══════════════════════════════════════════════════════════════
CREATE TABLE IF NOT EXISTS inventario_sede (
    id_inventario       SERIAL PRIMARY KEY,
    id_producto         INT REFERENCES producto(id_producto),
    id_sede             INT REFERENCES sede(id_sede),
    cantidad_disponible INT DEFAULT 0,
    umbral_minimo       INT DEFAULT 5
);

-- ══════════════════════════════════════════════════════════════
-- 18. HISTORIAL DE ABASTECIMIENTO
-- ══════════════════════════════════════════════════════════════
CREATE TABLE IF NOT EXISTS historial_abastecimiento (
    id_abastecimiento   SERIAL PRIMARY KEY,
    id_producto         INT REFERENCES producto(id_producto),
    id_proveedor        INT REFERENCES proveedor(id_proveedor),
    id_sede             INT REFERENCES sede(id_sede),
    id_usuario_registra INT REFERENCES usuario(id_usuario),
    cantidad            INT NOT NULL,
    costo_unitario      DECIMAL(10,2) NOT NULL DEFAULT 0,
    fecha_ingreso       TIMESTAMP DEFAULT NOW(),
    notas               TEXT
);

-- ══════════════════════════════════════════════════════════════
-- 19. TRANSFERENCIAS DE STOCK
-- ══════════════════════════════════════════════════════════════
CREATE TABLE IF NOT EXISTS transferencia_stock (
    id_transferencia    SERIAL PRIMARY KEY,
    id_producto         INT REFERENCES producto(id_producto),
    id_sede_origen      INT REFERENCES sede(id_sede),
    id_sede_destino     INT REFERENCES sede(id_sede),
    cantidad            INT NOT NULL,
    id_usuario_solicita INT REFERENCES usuario(id_usuario),
    estado              VARCHAR(20) DEFAULT 'PENDIENTE',
    fecha_solicitud     TIMESTAMP DEFAULT NOW(),
    fecha_completada    TIMESTAMP
);

-- ✅ ESQUEMA CREADO — 19 TABLAS
-- Ahora ejecuta luxuryco_seed.sql para insertar los datos.
