-- SIGASJ / Supabase — referencia manual (opcional)
--
-- El esquema completo (incluida LecturasMedidor) se crea con EF Core:
--   dotnet ef database update
-- o automaticamente al iniciar la API en desarrollo (MigrateAsync).
--
-- Use este script solo si prefiere revisar o ejecutar pasos en el SQL Editor de Supabase.

-- Verificar que exista la tabla de lecturas (creada por migracion InitialCreate)
SELECT EXISTS (
    SELECT 1
    FROM information_schema.tables
    WHERE table_schema = 'public'
      AND table_name = 'LecturasMedidor'
) AS tabla_lecturas_existe;

-- Secuencias de numeracion (AV / SOL) — el seeder las crea si faltan
INSERT INTO "SecuenciasContador" ("Prefijo", "UltimoValor")
VALUES ('AV', 0), ('SOL', 0)
ON CONFLICT ("Prefijo") DO NOTHING;
