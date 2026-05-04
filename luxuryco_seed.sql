-- ============================================================
--  LUXURYCO — DATOS SEMILLA COMPLETOS
--  Parte 2: SEED DATA — Ejecutar DESPUÉS de luxuryco_schema.sql
-- ============================================================

-- ── 1. ROLES ─────────────────────────────────────────────────
INSERT INTO rol (id_rol, nombre_rol, descripcion) VALUES
  (1, 'ADMIN',   'Administrador del sistema'),
  (2, 'CLIENTE', 'Cliente de la tienda'),
  (3, 'BODEGA',  'Encargado de bodega/inventario')
ON CONFLICT (id_rol) DO UPDATE SET nombre_rol = EXCLUDED.nombre_rol;

-- ── 2. SEDES ─────────────────────────────────────────────────
INSERT INTO sede (id_sede, nombre, ciudad, direccion, telefono, activa) VALUES
  (1, 'LuxuryCo Bogotá',    'Bogotá',    'Cra 7 #72-41 Local 201', '3001234567', true),
  (2, 'LuxuryCo Medellín',  'Medellín',  'Calle 10 #43-12 El Poblado', '3009876543', true)
ON CONFLICT (id_sede) DO UPDATE SET nombre = EXCLUDED.nombre;

-- ── 3. USUARIOS ──────────────────────────────────────────────
-- password_hash = bcrypt de "Admin123!"
INSERT INTO usuario (id_usuario, nombre, apellido, email, password_hash, telefono, id_rol, id_sede, activo) VALUES
  (1, 'Admin', 'LuxuryCo', 'admin@luxuryco.com',
   '$2a$11$K8GpJYzLcERYj3HMcGFR8eKm0gVNEhN4sB5oQxXkvRjJfBN3q1Fru',
   '3001234567', 1, 1, true),
  (2, 'Carlos', 'Martínez', 'carlos@email.com',
   '$2a$11$K8GpJYzLcERYj3HMcGFR8eKm0gVNEhN4sB5oQxXkvRjJfBN3q1Fru',
   '3115551234', 2, NULL, true),
  (3, 'Valentina', 'Restrepo', 'vale@email.com',
   '$2a$11$K8GpJYzLcERYj3HMcGFR8eKm0gVNEhN4sB5oQxXkvRjJfBN3q1Fru',
   '3209998877', 2, NULL, true)
ON CONFLICT (id_usuario) DO UPDATE SET nombre = EXCLUDED.nombre;

-- ── 4. DEPARTAMENTOS Y CIUDADES ──────────────────────────────
INSERT INTO departamento (id_departamento, nombre_departamento) VALUES
  (1, 'Cundinamarca'), (2, 'Antioquia'), (3, 'Valle del Cauca'),
  (4, 'Atlántico'), (5, 'Santander')
ON CONFLICT (id_departamento) DO UPDATE SET nombre_departamento = EXCLUDED.nombre_departamento;

INSERT INTO ciudad (id_ciudad, nombre_ciudad, id_departamento) VALUES
  (1, 'Bogotá', 1), (2, 'Medellín', 2), (3, 'Cali', 3),
  (4, 'Barranquilla', 4), (5, 'Bucaramanga', 5),
  (6, 'Envigado', 2), (7, 'Soacha', 1)
ON CONFLICT (id_ciudad) DO UPDATE SET nombre_ciudad = EXCLUDED.nombre_ciudad;

-- ── 5. DIRECCIONES ───────────────────────────────────────────
INSERT INTO direccion_usuario (id_direccion, id_usuario, direccion, referencia, id_ciudad) VALUES
  (1, 2, 'Calle 85 #15-10 Apto 302', 'Edificio Torres del Parque', 1),
  (2, 3, 'Cra 43A #1-50 El Poblado', 'Cerca al Parque Lleras', 2)
ON CONFLICT (id_direccion) DO NOTHING;

-- ── 6. CATEGORÍAS ────────────────────────────────────────────
INSERT INTO categoria (id_categoria, nombre, descripcion) VALUES
  (1, 'Hombre',     'Moda masculina premium'),
  (2, 'Mujer',      'Moda femenina exclusiva'),
  (3, 'Accesorios', 'Gorras, gafas, relojes y más')
ON CONFLICT (id_categoria) DO UPDATE SET nombre = EXCLUDED.nombre;

-- ── 7. MARCAS ────────────────────────────────────────────────
INSERT INTO marca (id_marca, nombre, descripcion, logo_url) VALUES
  (1, 'MONEY MAKERS', 'Marca colombiana de ropa urbana con diseños atrevidos e íconos del streetwear local.', ''),
  (2, 'Y/OUT',        'Streetwear premium colombiano. Prendas urbanas con identidad propia y alta calidad.', ''),
  (3, 'BLOW UP',      'Marca urbana premium con línea masculina, femenina y accesorios de alto impacto.',     ''),
  (4, 'CULTUREWISE',  'Lifestyle brand que fusiona arte, moda y cultura urbana contemporánea.',               '')
ON CONFLICT (id_marca) DO UPDATE SET nombre = EXCLUDED.nombre, descripcion = EXCLUDED.descripcion;

-- ── 8. PRODUCTOS REALES (Capistore CDN) ──────────────────────
-- Limpieza en cascada
DELETE FROM detalle_carrito WHERE id_producto IN (SELECT id_producto FROM producto);
DELETE FROM detalle_pedido  WHERE id_producto IN (SELECT id_producto FROM producto);
DELETE FROM inventario_sede WHERE id_producto IN (SELECT id_producto FROM producto);
DELETE FROM imagen_producto WHERE id_producto IN (SELECT id_producto FROM producto);
DELETE FROM resena          WHERE id_producto IN (SELECT id_producto FROM producto);
DELETE FROM producto;

INSERT INTO producto (id_producto, id_categoria, id_marca, nombre, descripcion, precio, stock, activo, seccion, fecha_creacion) VALUES
-- Y/OUT HOMBRE
(101, 1, 2, 'BUZO Y/OUT AIRLINES',
 'Hoodie Airlines de Y/OUT. Tela de alta calidad con acabado suave al tacto. Estampado gráfico de la colección. Perfecto para el día a día streetwear premium.',
 550000, 20, true, 'Hombre', NOW()),
(102, 1, 2, 'CAMISA Y/OUT HARVEST',
 'Camisa manga larga colección Harvest. Tela premium con diseño bordado. Corte regular, perfecta para combinar con cualquier outfit urbano.',
 490000, 18, true, 'Hombre', NOW()),
-- MONEY MAKERS HOMBRE
(103, 1, 1, 'BUZO MONEY MAKERS CHAMPS NEGRO',
 'Buzo colección Champs en negro. Algodón premium con estampado en alta densidad. Silueta oversize y acabados de lujo urbano.',
 380000, 15, true, 'Hombre', NOW()),
(104, 1, 1, 'CAMISETA MONEY MAKERS OVERSIZE GODS HAND',
 'Camiseta oversized Gods Hand. Algodón 100% ultrasiliconado. Estampado alta densidad frente y espalda. La pieza statement de la temporada.',
 320000, 25, true, 'Hombre', NOW()),
-- BLOW UP HOMBRE
(105, 1, 3, 'BERMUDA BLOW UP DESERT BLACK SWIMWEAR',
 'Bermuda de baño línea Desert en negro. Tela secado rápido, bolsillos laterales con malla interior. Perfecta para playa o día urbano.',
 350000, 20, true, 'Hombre', NOW()),
(106, 1, 3, '3 PACK CAMISETAS BLOW UP CLASIC',
 'Pack de 3 camisetas clásicas Blow Up en colores neutros. 100% algodón peinado. Silueta regular con cuello redondo en rib.',
 565000, 12, true, 'Hombre', NOW()),
-- Y/OUT MUJER
(107, 2, 2, 'BODY Y/OUT AIRWAY BLACK',
 'Body strapless línea Airway. Tela elástica 4 vías con acabado mate premium. Diseño minimalista que se adapta perfecto al cuerpo.',
 310000, 15, true, 'Mujer', NOW()),
(108, 2, 2, 'BODY Y/OUT CHARTER BLACK',
 'Body colección Charter en negro. Escote pronunciado con tirantes finos. Tela de segunda piel en lycra premium.',
 350000, 12, true, 'Mujer', NOW()),
(109, 2, 2, 'BUZO Y/OUT CHANGI HOODIE',
 'Hoodie oversized colección Changi. Tela fleece suave con forro interior premium. Capucha ajustable y estampado gráfico.',
 450000, 18, true, 'Mujer', NOW()),
-- BLOW UP MUJER
(110, 2, 3, 'BLUSA BLOW UP SHORT SLEEVE BLACK',
 'Blusa manga corta en jersey premium en negro. Silueta semifitted con caída perfecta. Cuello crew y bajo recto.',
 249000, 20, true, 'Mujer', NOW()),
(111, 2, 3, 'BLUSA BLOW UP SHORT SLEEVE IVORY',
 'Blusa manga corta en tono ivory. Tejido alta textura con acabado premium mate. Perfecta para combinar con cualquier prenda.',
 249000, 15, true, 'Mujer', NOW()),
-- ACCESORIOS
(112, 3, 1, 'GORRA MONEY MAKERS NEGRO',
 'Gorra 6 paneles en negro total. Bordado frontal con logo insignia. Visera curva con cierre ajustable en correa.',
 180000, 40, true, 'Accesorios', NOW());

-- ── 9. IMÁGENES REALES (CDN Shopify/Capistore) ──────────────
INSERT INTO imagen_producto (id_producto, url_imagen, principal, orden) VALUES
(101, 'https://cdn.shopify.com/s/files/1/0915/1624/0183/files/02994968-57A7-49A6-834C-65AFD1F05EAB.webp?v=1765748644', true, 1),
(102, 'https://cdn.shopify.com/s/files/1/0915/1624/0183/files/CAMISAYOUTHARVEST1.webp?v=1758829344', true, 1),
(103, 'https://cdn.shopify.com/s/files/1/0915/1624/0183/files/01_3a03180a-a7f8-4f78-a83f-175c6506ac81.webp?v=1775921478', true, 1),
(104, 'https://cdn.shopify.com/s/files/1/0915/1624/0183/files/CAMISETAMONEYMAKERSOVERSIZEGODSHAND1.webp?v=1756423135', true, 1),
(105, 'https://cdn.shopify.com/s/files/1/0915/1624/0183/files/BERMUDABLOWUPDESERTBLACKSWIMWEAR1.webp?v=1756409666', true, 1),
(106, 'https://cdn.shopify.com/s/files/1/0915/1624/0183/files/E2532C66-C23B-4C5C-8FF3-BA0E8FAD108C.jpg?v=1768083091', true, 1),
(107, 'https://cdn.shopify.com/s/files/1/0915/1624/0183/files/267AFBFE-3D11-40AA-985C-6106DDFF591D.webp?v=1765731123', true, 1),
(108, 'https://cdn.shopify.com/s/files/1/0915/1624/0183/files/7A30EB0E-28AC-4654-9DD8-97D52E47198B.webp?v=1765215353', true, 1),
(109, 'https://cdn.shopify.com/s/files/1/0915/1624/0183/files/3E2B57D0-78B2-4D4F-BCA2-C6E847244E3E.webp?v=1765940467', true, 1),
(110, 'https://cdn.shopify.com/s/files/1/0915/1624/0183/files/8171946D-3601-483A-B7D8-8EB773963777.webp?v=1768071530', true, 1),
(111, 'https://cdn.shopify.com/s/files/1/0915/1624/0183/files/88217D53-8236-4026-AD29-89AF3D4A5FD4.webp?v=1768070977', true, 1),
(112, 'https://cdn.shopify.com/s/files/1/0915/1624/0183/files/GORRAMONEYMAKERSNEGRONEGRO1.webp?v=1750525101', true, 1)
ON CONFLICT DO NOTHING;

-- ── 10. ESTADOS DE PEDIDO ────────────────────────────────────
INSERT INTO estado_pedido (id_estado_pedido, nombre_estado) VALUES
  (1, 'PENDIENTE'),
  (2, 'CONFIRMADO'),
  (3, 'EN PREPARACIÓN'),
  (4, 'ENVIADO'),
  (5, 'ENTREGADO'),
  (6, 'CANCELADO')
ON CONFLICT (id_estado_pedido) DO UPDATE SET nombre_estado = EXCLUDED.nombre_estado;

-- ── 11. ESTADOS DE ENVÍO ─────────────────────────────────────
INSERT INTO estado_envio (id_estado_envio, nombre_estado) VALUES
  (1, 'POR DESPACHAR'),
  (2, 'EN TRÁNSITO'),
  (3, 'EN CIUDAD DESTINO'),
  (4, 'ENTREGADO'),
  (5, 'DEVUELTO')
ON CONFLICT (id_estado_envio) DO UPDATE SET nombre_estado = EXCLUDED.nombre_estado;

-- ── 12. MÉTODOS DE PAGO ──────────────────────────────────────
INSERT INTO metodo_pago (id_metodo_pago, nombre, descripcion) VALUES
  (1, 'Tarjeta de Crédito',  'Visa, Mastercard, Amex'),
  (2, 'PSE',                 'Débito bancario PSE'),
  (3, 'Nequi',               'Pago con Nequi'),
  (4, 'Efectivo',            'Pago contra entrega'),
  (5, 'Transferencia',       'Transferencia bancaria')
ON CONFLICT (id_metodo_pago) DO UPDATE SET nombre = EXCLUDED.nombre;

-- ── 13. PROVEEDORES ──────────────────────────────────────────
INSERT INTO proveedor (id_proveedor, nombre, contacto, telefono, email, activo) VALUES
  (1, 'Distribuidora Y/OUT Colombia',   'Andrés López',  '3101112233', 'ventas@yout.co',        true),
  (2, 'Money Makers Distribuciones',     'Laura Gómez',   '3204445566', 'dist@moneymakers.co',   true),
  (3, 'Blow Up Wholesale',              'Julián Ríos',   '3157778899', 'mayorista@blowup.co',   true)
ON CONFLICT (id_proveedor) DO UPDATE SET nombre = EXCLUDED.nombre;

-- ── 14. INVENTARIO POR SEDE ──────────────────────────────────
INSERT INTO inventario_sede (id_producto, id_sede, cantidad_disponible, umbral_minimo) VALUES
  (101,1,20,4),(102,1,18,4),(103,1,15,3),(104,1,25,5),
  (105,1,20,4),(106,1,12,3),(107,1,15,3),(108,1,12,3),
  (109,1,18,4),(110,1,20,4),(111,1,15,3),(112,1,40,8),
  -- Sede 2 (Medellín)
  (101,2,10,3),(103,2,8,2),(104,2,12,3),(112,2,20,5)
ON CONFLICT DO NOTHING;

-- ── 15. RESEÑAS INICIALES ────────────────────────────────────
INSERT INTO resena (id_usuario, id_producto, calificacion, comentario, fecha) VALUES
  (2, 101, 5, 'Calidad brutal, el material se siente premium desde que abres la caja. Recomendado 100%.', NOW() - INTERVAL '5 days'),
  (3, 101, 5, 'Lo vi en Instagram y al tenerlo entendí por qué Y/OUT es diferente. La caída es perfecta.', NOW() - INTERVAL '3 days'),
  (2, 104, 4, 'La camiseta Gods Hand es increíble, el estampado tiene un nivel de detalle impresionante.', NOW() - INTERVAL '2 days'),
  (3, 107, 5, 'El body Airway se adapta perfecto al cuerpo, tela de segunda piel. Lo uso todos los fines.', NOW() - INTERVAL '1 day'),
  (2, 112, 5, 'Gorra de calidad top, el bordado es perfecto. Llegó en 2 días a Bogotá.', NOW())
ON CONFLICT DO NOTHING;

-- ── VERIFICACIÓN FINAL ───────────────────────────────────────
SELECT '✅ ROLES'          AS tabla, COUNT(*) AS registros FROM rol
UNION ALL SELECT '✅ SEDES',         COUNT(*) FROM sede
UNION ALL SELECT '✅ USUARIOS',      COUNT(*) FROM usuario
UNION ALL SELECT '✅ CATEGORÍAS',    COUNT(*) FROM categoria
UNION ALL SELECT '✅ MARCAS',        COUNT(*) FROM marca
UNION ALL SELECT '✅ PRODUCTOS',     COUNT(*) FROM producto
UNION ALL SELECT '✅ IMÁGENES',      COUNT(*) FROM imagen_producto
UNION ALL SELECT '✅ RESEÑAS',       COUNT(*) FROM resena
UNION ALL SELECT '✅ INVENTARIO',    COUNT(*) FROM inventario_sede
UNION ALL SELECT '✅ EST. PEDIDO',   COUNT(*) FROM estado_pedido
UNION ALL SELECT '✅ EST. ENVÍO',    COUNT(*) FROM estado_envio
UNION ALL SELECT '✅ MÉTODOS PAGO',  COUNT(*) FROM metodo_pago
UNION ALL SELECT '✅ PROVEEDORES',   COUNT(*) FROM proveedor
ORDER BY tabla;
