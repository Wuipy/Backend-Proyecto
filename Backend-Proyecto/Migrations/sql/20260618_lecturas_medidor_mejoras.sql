-- Migracion compatible con Supabase PostgreSQL
-- Mejoras al modulo de lecturas de medidores SIGASJ
-- NO elimina datos existentes

ALTER TABLE "LecturasMedidor"
    ADD COLUMN IF NOT EXISTS "NumeroAbonado" character varying(50),
    ADD COLUMN IF NOT EXISTS "Ubicacion" character varying(250),
    ADD COLUMN IF NOT EXISTS "HoraLectura" character varying(10),
    ADD COLUMN IF NOT EXISTS "EstadoMedidor" character varying(50),
    ADD COLUMN IF NOT EXISTS "EvidenciaNombre" character varying(255),
    ADD COLUMN IF NOT EXISTS "EvidenciaBase64" text,
    ADD COLUMN IF NOT EXISTS "ConsumoAlto" boolean NOT NULL DEFAULT false,
    ADD COLUMN IF NOT EXISTS "ObservacionAdmin" character varying(2000),
    ADD COLUMN IF NOT EXISTS "MotivoVisita" character varying(500),
    ADD COLUMN IF NOT EXISTS "ResultadoInspeccion" character varying(1000),
    ADD COLUMN IF NOT EXISTS "RevisadaPorAdminId" integer,
    ADD COLUMN IF NOT EXISTS "FechaActualizacion" timestamp with time zone;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint WHERE conname = 'FK_LecturasMedidor_Usuarios_RevisadaPorAdminId'
    ) THEN
        ALTER TABLE "LecturasMedidor"
            ADD CONSTRAINT "FK_LecturasMedidor_Usuarios_RevisadaPorAdminId"
            FOREIGN KEY ("RevisadaPorAdminId") REFERENCES "Usuarios" ("Id") ON DELETE SET NULL;
    END IF;
END $$;

CREATE TABLE IF NOT EXISTS "HistorialLecturas" (
    "Id" serial PRIMARY KEY,
    "LecturaMedidorId" integer NOT NULL,
    "UsuarioId" integer,
    "UsuarioNombre" character varying(150),
    "Accion" character varying(200) NOT NULL,
    "EstadoAnterior" character varying(50),
    "EstadoNuevo" character varying(50),
    "Observacion" character varying(2000),
    "Fecha" timestamp with time zone NOT NULL DEFAULT NOW(),
    CONSTRAINT "FK_HistorialLecturas_LecturasMedidor_LecturaMedidorId"
        FOREIGN KEY ("LecturaMedidorId") REFERENCES "LecturasMedidor" ("Id") ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS "IX_HistorialLecturas_LecturaMedidorId"
    ON "HistorialLecturas" ("LecturaMedidorId");

CREATE INDEX IF NOT EXISTS "IX_LecturasMedidor_RevisadaPorAdminId"
    ON "LecturasMedidor" ("RevisadaPorAdminId");

-- Actualizar consumo alto en registros existentes (opcional, no destructivo)
UPDATE "LecturasMedidor"
SET "ConsumoAlto" = true
WHERE "Consumo" > 50
   OR (
        "ConsumoMesAnterior" IS NOT NULL
        AND "ConsumoMesAnterior" > 0
        AND "Consumo" > "ConsumoMesAnterior" * 2
      );
