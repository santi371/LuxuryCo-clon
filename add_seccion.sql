-- =====================================================
-- Script para añadir la columna "seccion" a la tabla de productos
-- Ejecutar en Supabase (SQL Editor)
-- =====================================================

ALTER TABLE producto ADD COLUMN IF NOT EXISTS seccion VARCHAR(50);

-- Verificar que se añadió correctamente
SELECT column_name, data_type, character_maximum_length 
FROM information_schema.columns 
WHERE table_name = 'producto' AND column_name = 'seccion';
