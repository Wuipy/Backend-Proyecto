using System.Globalization;

namespace Backend_Proyecto.Utils;

public static class FechaFormatter
{
    private static readonly CultureInfo CostaRica = CultureInfo.GetCultureInfo("es-CR");
    private static readonly TimeZoneInfo ZonaHoraria = ObtenerZonaCostaRica();

    public static string Formatear(DateTime fechaUtc)
    {
        var local = TimeZoneInfo.ConvertTimeFromUtc(fechaUtc, ZonaHoraria);
        return local.ToString("dd MMM yyyy, h:mm tt", CostaRica);
    }

    private static TimeZoneInfo ObtenerZonaCostaRica()
    {
        foreach (var id in new[] { "Central America Standard Time", "America/Costa_Rica" })
        {
            try
            {
                return TimeZoneInfo.FindSystemTimeZoneById(id);
            }
            catch (TimeZoneNotFoundException)
            {
                // Intenta el siguiente identificador disponible en el sistema operativo.
            }
        }

        return TimeZoneInfo.Utc;
    }
}

public static class SeguimientoGenerator
{
    public static string Crear(string prefijo, int consecutivo) =>
        $"{prefijo}-{consecutivo:D4}";
}

public static class EstadoMensajes
{
    public static string ParaAveria(string estadoInterno) =>
        EstadosAveria.Normalizar(estadoInterno) switch
        {
            "Pendiente" => "Estado: Reporte recibido, pendiente de revision por la ASADA.",
            "En revision" => "Estado: En revision por el equipo administrativo.",
            "Asignada" => "Estado: Asignada a un fontanero para atencion.",
            "En proceso" => "Estado: Un fontanero esta atendiendo el reporte.",
            "Finalizada" => "Estado: Averia atendida y finalizada.",
            "Cancelada" => "Estado: Reporte cancelado por la administracion.",
            "No se pudo atender" => "Estado: No fue posible atender el reporte.",
            _ => "Estado: Reporte recibido, pendiente de revision por la ASADA."
        };

    public static string ParaSolicitud(string estadoInterno) =>
        estadoInterno switch
        {
            "En revision" => "Estado: En revision por la Secretaria Ejecutiva",
            "Aprobada" => "Estado: Solicitud aprobada por la ASADA.",
            "Rechazada" => "Estado: Solicitud rechazada. Contacte la ASADA para mas detalle.",
            _ => "Estado: En revision por la Secretaria Ejecutiva"
        };
}
