-- =====================================================
-- Script de semilla: Categorías base de LuxuryCo
-- Ejecutar en tu base de datos PostgreSQL (Supabase)
-- Tabla real: "categoria" (minúsculas, sin comillas o con comillas dobles)
-- =====================================================

INSERT INTO categoria (nombre, descripcion)
SELECT 'Hombre', 'Productos para hombre'
WHERE NOT EXISTS (SELECT 1 FROM categoria WHERE nombre = 'Hombre');

INSERT INTO categoria (nombre, descripcion)
SELECT 'Mujer', 'Productos para mujer'
WHERE NOT EXISTS (SELECT 1 FROM categoria WHERE nombre = 'Mujer');

INSERT INTO categoria (nombre, descripcion)
SELECT 'Accesorios', 'Accesorios en general'
WHERE NOT EXISTS (SELECT 1 FROM categoria WHERE nombre = 'Accesorios');

-- Verificar:
SELECT * FROM categoria ORDER BY nombre;

