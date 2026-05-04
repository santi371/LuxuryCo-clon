-- ==========================================================
-- LuxuryCo - Script de datos iniciales para Supabase
-- Ejecuta este script en el SQL Editor de Supabase
-- ==========================================================

-- ============================================================
-- 1. LIMPIAR datos existentes (orden correcto por FK)
-- ============================================================
-- Si ya tienes productos vinculados, comenta las líneas DELETE
-- y sólo ejecuta los INSERT con ON CONFLICT DO NOTHING

DELETE FROM imagen_producto;
DELETE FROM detalle_carrito;
DELETE FROM detalle_pedido;
DELETE FROM resena;
DELETE FROM producto;
DELETE FROM categoria;
DELETE FROM marca;

-- ============================================================
-- 2. CATEGORÍAS de ropa (tienda de streetwear / moda urbana)
-- ============================================================
INSERT INTO categoria (id_categoria, nombre, descripcion) VALUES
(1,  'Camisetas',          'Camisetas básicas, gráficas y de temporada'),
(2,  'Camisetas Oversize', 'Camisetas con corte oversized y fit holgado'),
(3,  'Tops',               'Tops y crop tops para mujer'),
(4,  'Hoodies',            'Sudaderas con capucha, algodón premium'),
(5,  'Crewneck',           'Sudaderas cuello redondo sin capucha'),
(6,  'Pantalones',         'Pantalones casuales y de temporada'),
(7,  'Cargo Pants',        'Pantalones cargo con bolsillos laterales'),
(8,  'Joggers',            'Joggers deportivos y urbanos'),
(9,  'Shorts',             'Shorts y bermudas para hombre y mujer'),
(10, 'Chaquetas',          'Chaquetas ligeras, cortavientos y bomber'),
(11, 'Abrigos',            'Abrigos y prendas de abrigo premium'),
(12, 'Vestidos',           'Vestidos casuales y de ocasión'),
(13, 'Faldas',             'Faldas mini, midi y maxi'),
(14, 'Bodys',              'Bodys y enterizos cortos'),
(15, 'Conjuntos',          'Conjuntos coordinados top + pantalón o falda'),
(16, 'Gorras',             'Gorras snapback, dad hat y bucket hat'),
(17, 'Accesorios',         'Cinturones, bolsos, bufandas y más'),
(18, 'Calzado',            'Tenis, sneakers y zapatillas'),
(19, 'Calcetines',         'Calcetines y medias de moda'),
(20, 'Underwear',          'Ropa interior y básicos');

-- ============================================================
-- 3. MARCAS de la tienda
-- ============================================================
INSERT INTO marca (id_marca, nombre, descripcion, logo_url) VALUES
(1, 'MONEY MAKERS',  'Marca urbana premium enfocada en streetwear de lujo', ''),
(2, 'Y/OUT',         'Marca minimalista con prendas de alta calidad',         ''),
(3, 'BLOW UP',       'Streetwear explosivo con diseños gráficos únicos',       ''),
(4, 'CULTUREWISE',   'Moda cultural con raíces en la música y el arte',        ''),
(5, 'LUXE BASICS',   'Básicos de lujo, materiales premium y cortes perfectos', ''),
(6, 'NEON DISTRICT', 'Colores vibrantes y estética urbana nocturna',           ''),
(7, 'RAW FORM',      'Ropa sin género, diseño raw y atemporal',                ''),
(8, 'VELVET EDGE',   'Elegancia urbana con texturas de terciopelo y seda',     '');

-- ============================================================
-- 4. REINICIAR las secuencias para que el próximo INSERT
--    auto-incremente desde el valor correcto
-- ============================================================
SELECT setval(pg_get_serial_sequence('categoria', 'id_categoria'), 20);
SELECT setval(pg_get_serial_sequence('marca', 'id_marca'), 8);

-- ============================================================
-- Verificación final
-- ============================================================
SELECT 'Categorías insertadas: ' || COUNT(*) AS resultado FROM categoria
UNION ALL
SELECT 'Marcas insertadas: '     || COUNT(*) AS resultado FROM marca;
