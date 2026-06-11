namespace Backend_Proyecto.Utils;

public static class Roles
{
    public const string Admin = "admin";
    public const string Fontanero = "fontanero";

    public static readonly string[] Todos = [Admin, Fontanero];

    public static bool EsValido(string rol) =>
        Todos.Contains(rol, StringComparer.Ordinal);
}

public static class EstadosAveria
{
    public static readonly string[] Todos =
    [
        "Pendiente",
        "En revision",
        "Asignada",
        "En proceso",
        "Finalizada",
        "Cancelada",
        "No se pudo atender"
    ];

    public static readonly string[] Admin =
    [
        "Pendiente",
        "En revision",
        "Asignada",
        "En proceso",
        "Finalizada",
        "Cancelada",
        "No se pudo atender"
    ];

    public static readonly string[] Fontanero =
    [
        "En proceso",
        "Finalizada",
        "No se pudo atender"
    ];

    public static bool EsValido(string estado) =>
        Todos.Contains(estado, StringComparer.Ordinal) ||
        EsEstadoLegacy(estado);

    public static bool EsValidoAdmin(string estado) =>
        Admin.Contains(estado, StringComparer.Ordinal) || EsEstadoLegacy(estado);

    public static bool EsValidoFontanero(string estado) =>
        Fontanero.Contains(estado, StringComparer.Ordinal);

    public static string Normalizar(string estado) =>
        estado switch
        {
            "Recibido" => "Pendiente",
            "En atencion" => "En proceso",
            "Atendido" => "Finalizada",
            _ => estado
        };

    private static bool EsEstadoLegacy(string estado) =>
        estado is "Recibido" or "En atencion" or "Atendido";
}

public static class PrioridadesAveria
{
    public static readonly string[] Todas = ["Baja", "Media", "Alta", "Urgente"];

    public static bool EsValida(string prioridad) =>
        Todas.Contains(prioridad, StringComparer.Ordinal);
}

public static class EstadosActividadFontanero
{
    public static readonly string[] Todos = ["Pendiente", "En proceso", "Finalizada"];

    public static bool EsValido(string estado) =>
        Todos.Contains(estado, StringComparer.Ordinal);
}

public static class EstadosValidacionActividad
{
    public static readonly string[] Todos = ["Pendiente", "Validada", "Rechazada"];

    public static bool EsValido(string estado) =>
        Todos.Contains(estado, StringComparer.Ordinal);
}

public static class EstadosLecturaMedidor
{
    public static readonly string[] Todos = ["Pendiente", "Registrada", "Con inconsistencia", "Revisada"];

    public static bool EsValido(string estado) =>
        Todos.Contains(estado, StringComparer.Ordinal);
}
