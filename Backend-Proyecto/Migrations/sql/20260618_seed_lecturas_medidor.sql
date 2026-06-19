-- Datos de prueba para el modulo de Lecturas de Medidores SIGASJ
-- Ejecutar en Supabase SQL Editor solo si la tabla esta vacia.
-- No elimina datos existentes.

INSERT INTO "LecturasMedidor" (
    "NombreAbonado", "NumeroAbonado", "NumeroMedidor", "CedulaAbonado", "Ubicacion",
    "LecturaAnterior", "LecturaActual", "Consumo", "ConsumoMesAnterior", "ConsumoAlto",
    "FechaLectura", "HoraLectura", "Observaciones", "Estado", "EstadoMedidor",
    "MotivoVisita", "ResultadoInspeccion", "ObservacionAdmin",
    "FontaneroId", "RevisadaPorAdminId", "FechaRegistro"
)
SELECT * FROM (VALUES
    (
        'Maria Fernandez Solano', 'A-1024', 'M-45821', '1-0234-0567',
        'Palmares Centro, 150 m norte de la iglesia',
        1245.50, 1268.30, 22.80, 21.50, false,
        NOW() - INTERVAL '1 month', '08:30',
        'Lectura normal. Medidor en buen estado.', 'Validada', 'Bueno',
        'Lectura mensual', NULL, NULL,
        f."Id", a."Id", NOW() - INTERVAL '1 month'
    ),
    (
        'Carlos Mora Jimenez', 'A-1087', 'M-45822', '2-0456-0789',
        'Barrio El Carmen, contiguo al pulperia La Union',
        890.00, 942.50, 52.50, 24.00, true,
        NOW() - INTERVAL '28 days', '09:15',
        'Consumo elevado. Se inspecciono el medidor, no presenta fuga visible.',
        'Con inconsistencia', 'Bueno',
        'Inspeccion por consumo alto', 'Medidor no presenta fuga', NULL,
        f."Id", NULL, NOW() - INTERVAL '28 days'
    ),
    (
        'Roberto Chaves Castro', 'A-1203', 'M-45824', NULL,
        'Urbanizacion Los Laureles, casa 12',
        2100.00, 2135.75, 35.75, 32.00, false,
        NOW() - INTERVAL '3 days', '07:45',
        'Lectura registrada sin novedad.', 'Registrada', 'Bueno',
        'Lectura mensual', NULL, NULL,
        f."Id", NULL, NOW() - INTERVAL '3 days'
    ),
    (
        'Elena Rojas Mora', 'A-1245', 'M-45825', NULL,
        'Barrio San Martin, 200 m oeste de la escuela',
        1780.00, 1780.00, 0, 19.00, false,
        NOW(), NULL,
        'Pendiente de lectura en campo.', 'Pendiente', 'Bueno',
        NULL, NULL, NULL,
        f."Id", NULL, NOW()
    ),
    (
        'Francisco Navarro', 'A-1402', 'M-45828', NULL,
        'San Juan, contiguo al colegio',
        675.00, 675.00, 0, NULL, false,
        NOW() + INTERVAL '2 days', NULL,
        'Asignada para lectura del mes en curso.', 'Pendiente', 'Bueno',
        NULL, NULL, NULL,
        f."Id", NULL, NOW()
    )
) AS seed(
    "NombreAbonado", "NumeroAbonado", "NumeroMedidor", "CedulaAbonado", "Ubicacion",
    "LecturaAnterior", "LecturaActual", "Consumo", "ConsumoMesAnterior", "ConsumoAlto",
    "FechaLectura", "HoraLectura", "Observaciones", "Estado", "EstadoMedidor",
    "MotivoVisita", "ResultadoInspeccion", "ObservacionAdmin",
    "FontaneroId", "RevisadaPorAdminId", "FechaRegistro"
)
CROSS JOIN (SELECT "Id" FROM "Usuarios" WHERE "NombreUsuario" = 'fontanero' LIMIT 1) f
CROSS JOIN (SELECT "Id" FROM "Usuarios" WHERE "Rol" = 'admin' LIMIT 1) a
WHERE NOT EXISTS (SELECT 1 FROM "LecturasMedidor" LIMIT 1);
