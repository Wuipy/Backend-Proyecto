using Microsoft.EntityFrameworkCore;

namespace Backend_Proyecto.Data;

public static class DatabaseSchemaEnsurer
{
    public static async Task EnsureLecturasMedidorColumnsAsync(AppDbContext context)
    {
        await context.Database.ExecuteSqlRawAsync("""
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
            """);

        await context.Database.ExecuteSqlRawAsync("""
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
            """);
    }
}
