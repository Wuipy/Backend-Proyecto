using Backend_Proyecto.Models.DTOs;

using Backend_Proyecto.Models.Entities;

using Backend_Proyecto.Utils;



namespace Backend_Proyecto.Services;



public static class Mappers

{

    public static AveriaResponseDto ToAveriaResponse(Averia averia)

    {

        var estado = EstadosAveria.Normalizar(averia.Estado);

        return new AveriaResponseDto

        {

            NumeroSeguimiento = averia.NumeroSeguimiento,

            Nombre = averia.Nombre,

            Telefono = averia.Telefono,

            Correo = averia.Correo,

            Direccion = averia.Direccion,

            Tipo = averia.Tipo,

            Descripcion = averia.Descripcion,

            Fecha = FechaFormatter.Formatear(averia.FechaCreacion),

            Estado = estado,

            Prioridad = averia.Prioridad,

            MensajeEstado = EstadoMensajes.ParaAveria(estado),

            FontaneroAsignado = averia.FontaneroAsignado?.NombreUsuario,

            NotasAtencion = averia.NotasAtencion,

            ObservacionesAdmin = averia.ObservacionesAdmin,

            DescripcionTrabajo = averia.DescripcionTrabajo,

            MaterialesUtilizados = averia.MaterialesUtilizados,

            FechaUltimaActualizacion = averia.FechaUltimaActualizacion.HasValue

                ? FechaFormatter.Formatear(averia.FechaUltimaActualizacion.Value)

                : null,

            Foto = CrearFoto(averia.FotoNombre, averia.FotoBase64),

            EvidenciaTrabajo = CrearFoto(averia.EvidenciaTrabajoNombre, averia.EvidenciaTrabajoBase64)

        };

    }



    public static AveriaHistorialDto ToHistorialResponse(AveriaHistorial historial) =>

        new()

        {

            Accion = historial.Accion,

            ValorAnterior = historial.ValorAnterior,

            ValorNuevo = historial.ValorNuevo,

            Usuario = historial.Usuario,

            Fecha = FechaFormatter.Formatear(historial.Fecha)

        };



    public static ActividadFontaneroResponseDto ToActividadFontaneroResponse(ActividadFontanero actividad) =>

        new()

        {

            Id = actividad.Id,

            Fontanero = actividad.Fontanero.NombreUsuario,

            FechaActividad = FechaFormatter.Formatear(actividad.FechaActividad),
            FechaActividadIso = actividad.FechaActividad.ToString("yyyy-MM-dd"),

            HoraInicio = actividad.HoraInicio,

            HoraFin = actividad.HoraFin,

            Tipo = actividad.Tipo,

            Descripcion = actividad.Descripcion,

            Ubicacion = actividad.Ubicacion,

            NumeroAveriaVinculada = actividad.NumeroAveriaVinculada,

            LecturaMedidorId = actividad.LecturaMedidorId,

            MaterialesUtilizados = actividad.MaterialesUtilizados,

            Observaciones = actividad.Observaciones,

            Estado = actividad.Estado,

            EstadoValidacion = actividad.EstadoValidacion,

            ObservacionValidacion = actividad.ObservacionValidacion,

            FechaCreacion = FechaFormatter.Formatear(actividad.FechaCreacion),

            FechaActualizacion = actividad.FechaActualizacion.HasValue

                ? FechaFormatter.Formatear(actividad.FechaActualizacion.Value)

                : null,

            AbonadoNumero = actividad.AbonadoNumero,
            NombreAbonado = actividad.NombreAbonado,
            LugarVisita = actividad.LugarVisita,
            MotivoVisita = actividad.MotivoVisita,
            LecturaAnteriorM3 = actividad.LecturaAnteriorM3,
            LecturaActualM3 = actividad.LecturaActualM3,
            ConsumoRegistradoM3 = actividad.ConsumoRegistradoM3,
            EstadoMedidor = actividad.EstadoMedidor,
            DetectoFuga = actividad.DetectoFuga,
            ResultadoInspeccion = actividad.ResultadoInspeccion,
            AccionRecomendada = actividad.AccionRecomendada,
            FotoMedidorNombre = actividad.FotoMedidorNombre,
            FotoMedidorBase64 = actividad.FotoMedidorBase64,
            AforoNumero = actividad.AforoNumero,
            LugarPrueba = actividad.LugarPrueba,
            HoraPrueba = actividad.HoraPrueba,
            ResultadoPsi = actividad.ResultadoPsi,
            DiametroTuberia = actividad.DiametroTuberia,
            ObservacionesPresion = actividad.ObservacionesPresion,
            PruebaNumero = actividad.PruebaNumero,
            LugarCasa = actividad.LugarCasa,
            HoraControl = actividad.HoraControl,
            CloroResidual = actividad.CloroResidual,
            Turbiedad = actividad.Turbiedad,
            Ph = actividad.Ph,
            Olor = actividad.Olor,
            Sabor = actividad.Sabor,
            ObservacionesControlOperativo = actividad.ObservacionesControlOperativo,
            DetalleTrabajoRealizado = actividad.DetalleTrabajoRealizado,
            ResultadoTrabajo = actividad.ResultadoTrabajo,
            RequiereSeguimiento = actividad.RequiereSeguimiento,
            PrioridadSeguimiento = actividad.PrioridadSeguimiento,
            FotoEvidenciaNombre = actividad.FotoEvidenciaNombre,
            FotoEvidenciaBase64 = actividad.FotoEvidenciaBase64

        };



    public static LecturaMedidorResponseDto ToLecturaResponse(LecturaMedidor lectura)

    {

        var alerta = lectura.ConsumoMesAnterior.HasValue &&

                     lectura.ConsumoMesAnterior.Value > 0 &&

                     lectura.Consumo > lectura.ConsumoMesAnterior.Value * 2;



        return new LecturaMedidorResponseDto

        {

            Id = lectura.Id,

            NombreAbonado = lectura.NombreAbonado,

            NumeroMedidor = lectura.NumeroMedidor,

            CedulaAbonado = lectura.CedulaAbonado,

            LecturaAnterior = lectura.LecturaAnterior,

            LecturaActual = lectura.LecturaActual,

            Consumo = lectura.Consumo,

            ConsumoMesAnterior = lectura.ConsumoMesAnterior,

            AlertaConsumoAlto = alerta,

            FechaLectura = FechaFormatter.Formatear(lectura.FechaLectura),

            Observaciones = lectura.Observaciones,

            Estado = lectura.Estado,

            Fontanero = lectura.Fontanero.NombreUsuario,

            FechaRegistro = FechaFormatter.Formatear(lectura.FechaRegistro)

        };

    }



    private static FotoAveriaDto? CrearFoto(string? nombre, string? base64) =>

        string.IsNullOrWhiteSpace(base64)

            ? null

            : new FotoAveriaDto

            {

                Nombre = nombre ?? "evidencia.jpg",

                VistaPrevia = base64

            };

}


