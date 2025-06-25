ALTER VIEW vw_MisInscripcionesCompletas AS
SELECT 
    i.InscripcionID,
    i.UsuarioID,
    i.TipoActividad,
    i.ActividadID,
    i.CodigoCheckIn,
    i.FechaInscripcion,
    i.Estado,
    ev.Titulo AS TituloEvento,
    ev.FechaEvento,
    ev.Ubicacion,
    te.Nombre AS TipoEntrada,
    te.Precio AS PrecioEvento,
    c.Titulo AS TituloCompetencia,
    c.FechaInicio,
    c.FechaFin,
    eq.NombreEquipo,
    tec.Nombre AS TipoEntradaCompetencia,
    tec.Precio AS PrecioCompetencia,
    pi.PagoID
FROM Inscripciones i
LEFT JOIN Eventos ev ON i.ActividadID = ev.EventoID AND i.TipoActividad = 'Evento'
LEFT JOIN TiposEntrada te ON i.TipoEntradaID = te.TipoEntradaID
LEFT JOIN Competencias c ON i.ActividadID = c.CompetenciaID AND i.TipoActividad = 'Competencia'
LEFT JOIN TiposEntradaCompetencia tec ON i.TipoEntradaID = tec.TipoEntradaID AND i.TipoActividad = 'Competencia'
LEFT JOIN Equipos eq ON i.EquipoID = eq.EquipoID
LEFT JOIN PagoInscripciones pi ON i.InscripcionID = pi.InscripcionID;

SELECT * FROM vw_MisInscripcionesCompletas