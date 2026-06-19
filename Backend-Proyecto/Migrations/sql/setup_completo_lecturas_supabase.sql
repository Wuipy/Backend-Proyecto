-- =============================================================================
-- SIGASJ — Setup completo modulo Lecturas de Medidores (Supabase PostgreSQL)
-- Ejecutar en Supabase > SQL Editor. No elimina datos existentes.
-- =============================================================================

-- 1) Columnas adicionales en LecturasMedidor (si faltan)
ALTER TABLE "LecturasMedidor"
    ADD COLUMN IF NOT EXISTS "NumeroAbonado" text,
    ADD COLUMN IF NOT EXISTS "Ubicacion" text,
    ADD COLUMN IF NOT EXISTS "HoraLectura" text,
    ADD COLUMN IF NOT EXISTS "EstadoMedidor" text,
    ADD COLUMN IF NOT EXISTS "EvidenciaNombre" text,
    ADD COLUMN IF NOT EXISTS "EvidenciaBase64" text,
    ADD COLUMN IF NOT EXISTS "ConsumoAlto" boolean NOT NULL DEFAULT false,
    ADD COLUMN IF NOT EXISTS "ObservacionAdmin" text,
    ADD COLUMN IF NOT EXISTS "MotivoVisita" text,
    ADD COLUMN IF NOT EXISTS "ResultadoInspeccion" text,
    ADD COLUMN IF NOT EXISTS "RevisadaPorAdminId" integer,
    ADD COLUMN IF NOT EXISTS "FechaActualizacion" timestamp with time zone;

CREATE TABLE IF NOT EXISTS "HistorialLecturas" (
    "Id" serial PRIMARY KEY,
    "LecturaMedidorId" integer NOT NULL REFERENCES "LecturasMedidor"("Id") ON DELETE CASCADE,
    "UsuarioId" integer,
    "UsuarioNombre" text,
    "Accion" text NOT NULL,
    "EstadoAnterior" text,
    "EstadoNuevo" text,
    "Observacion" text,
    "Fecha" timestamp with time zone NOT NULL DEFAULT NOW()
);

-- 2) Datos de prueba (solo si la tabla esta vacia)
DO $$
DECLARE
    v_fontanero_id integer;
    v_admin_id integer;
BEGIN
    IF EXISTS (SELECT 1 FROM "LecturasMedidor" LIMIT 1) THEN
        RAISE NOTICE 'LecturasMedidor ya tiene datos. Seed omitido.';
        RETURN;
    END IF;

    SELECT "Id" INTO v_fontanero_id FROM "Usuarios" WHERE "NombreUsuario" = 'fontanero' LIMIT 1;
    IF v_fontanero_id IS NULL THEN
        SELECT "Id" INTO v_fontanero_id FROM "Usuarios" WHERE "Rol" = 'fontanero' LIMIT 1;
    END IF;

    IF v_fontanero_id IS NULL THEN
        RAISE EXCEPTION 'No existe usuario fontanero. Inicie el backend al menos una vez con la conexion configurada, o cree el usuario manualmente.';
    END IF;

    SELECT "Id" INTO v_admin_id FROM "Usuarios" WHERE "Rol" = 'admin' LIMIT 1;

    INSERT INTO "LecturasMedidor" (
        "NombreAbonado", "NumeroAbonado", "NumeroMedidor", "CedulaAbonado", "Ubicacion",
        "LecturaAnterior", "LecturaActual", "Consumo", "ConsumoMesAnterior", "ConsumoAlto",
        "FechaLectura", "HoraLectura", "Observaciones", "Estado", "EstadoMedidor",
        "MotivoVisita", "ResultadoInspeccion", "ObservacionAdmin",
        "FontaneroId", "RevisadaPorAdminId", "FechaRegistro"
    ) VALUES
    (
        'Maria Fernandez Solano', 'A-1024', 'M-45821', '1-0234-0567',
        'Palmares Centro, 150 m norte de la iglesia',
        1245.50, 1268.30, 22.80, 21.50, false,
        NOW() - INTERVAL '1 month', '08:30',
        'Lectura normal. Medidor en buen estado.', 'Validada', 'Bueno',
        'Lectura mensual', NULL, NULL,
        v_fontanero_id, v_admin_id, NOW() - INTERVAL '1 month'
    ),
    (
        'Carlos Mora Jimenez', 'A-1087', 'M-45822', '2-0456-0789',
        'Barrio El Carmen, contiguo al pulperia La Union',
        890.00, 942.50, 52.50, 24.00, true,
        NOW() - INTERVAL '28 days', '09:15',
        'Consumo elevado. Se inspecciono el medidor, no presenta fuga visible.',
        'Con inconsistencia', 'Bueno',
        'Inspeccion por consumo alto', 'Medidor no presenta fuga', NULL,
        v_fontanero_id, NULL, NOW() - INTERVAL '28 days'
    ),
    (
        'Ana Lucia Vargas', 'A-1156', 'M-45823', NULL,
        'San Juan de Dios, frente al parque',
        560.25, 560.25, 0, 18.75, false,
        NOW() - INTERVAL '25 days', '10:00',
        'Consumo cero por ausencia prolongada del abonado.',
        'Revisada', 'Inaccesible',
        'Lectura mensual', NULL, NULL,
        v_fontanero_id, v_admin_id, NOW() - INTERVAL '25 days'
    ),
    (
        'Roberto Chaves Castro', 'A-1203', 'M-45824', NULL,
        'Urbanizacion Los Laureles, casa 12',
        2100.00, 2135.75, 35.75, 32.00, false,
        NOW() - INTERVAL '3 days', '07:45',
        'Lectura registrada sin novedad.', 'Registrada', 'Bueno',
        'Lectura mensual', NULL, NULL,
        v_fontanero_id, NULL, NOW() - INTERVAL '3 days'
    ),
    (
        'Jose Alberto Quesada', 'A-1301', 'M-45826', NULL,
        'Comunidad El Roble, calle principal',
        450.00, 520.00, 70.00, 22.00, true,
        NOW() - INTERVAL '1 day', '11:30',
        'Consumo muy alto. Se recomienda revision administrativa.',
        'Con inconsistencia', 'Con posible fuga',
        'Inspeccion por consumo alto', 'Se recomienda revision administrativa', NULL,
        v_fontanero_id, NULL, NOW() - INTERVAL '1 day'
    ),
    (
        'Patricia Solis Brenes', 'A-1350', 'M-45827', NULL,
        'Palmares, Barrio La Granja',
        320.50, 298.00, -22.50, 15.00, false,
        NOW() - INTERVAL '5 days', '16:20',
        'Lectura menor a la anterior. Posible error de digitacion.',
        'Rechazada', 'Bueno',
        NULL, NULL, 'Rechazada: solicitar nueva visita.',
        v_fontanero_id, v_admin_id, NOW() - INTERVAL '5 days'
    ),
    (
        'Elena Rojas Mora', 'A-1245', 'M-45825', NULL,
        'Barrio San Martin, 200 m oeste de la escuela',
        1780.00, 1780.00, 0, 19.00, false,
        NOW(), NULL,
        'Pendiente de lectura en campo.', 'Pendiente', 'Bueno',
        NULL, NULL, NULL,
        v_fontanero_id, NULL, NOW()
    ),
    (
        'Francisco Navarro', 'A-1402', 'M-45828', NULL,
        'San Juan, contiguo al colegio',
        675.00, 675.00, 0, NULL, false,
        NOW() + INTERVAL '2 days', NULL,
        'Asignada para lectura del mes en curso.', 'Pendiente', 'Bueno',
        NULL, NULL, NULL,
        v_fontanero_id, NULL, NOW()
    );

    RAISE NOTICE 'Seed completado: 8 lecturas de prueba insertadas.';
END $$;

-- 3) Verificacion
SELECT COUNT(*) AS total_lecturas FROM "LecturasMedidor";
SELECT "Id", "NombreAbonado", "NumeroMedidor", "Consumo", "Estado" FROM "LecturasMedidor" ORDER BY "Id";
