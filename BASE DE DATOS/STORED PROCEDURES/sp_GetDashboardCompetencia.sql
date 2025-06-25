ALTER PROCEDURE sp_GetDashboardCompetencia
    @CompetenciaID INT,
    @OrganizadorID INT
AS
BEGIN
    SET NOCOUNT ON;

	UPDATE Competencias
    SET Estado = 'Finalizado'
    WHERE CompetenciaID = @CompetenciaID AND FechaFin < GETDATE() AND Estado = 'Activo';

    DECLARE @TipoCompetencia NVARCHAR(50), @ModalidadPago NVARCHAR(50), @TituloCompetencia NVARCHAR(200);

    SELECT @TipoCompetencia = Tipo, @ModalidadPago = ModalidadPago, @TituloCompetencia = Titulo 
    FROM Competencias WHERE CompetenciaID = @CompetenciaID AND OrganizadorID = @OrganizadorID;
    IF @TipoCompetencia IS NULL BEGIN RETURN; END

    SELECT 
        @TituloCompetencia AS TituloCompetencia,
        @TipoCompetencia AS TipoCompetencia,
        @ModalidadPago AS ModalidadPago,
        COALESCE((SELECT COUNT(1) FROM Inscripciones WHERE ActividadID = @CompetenciaID AND TipoActividad = 'Competencia' AND Estado = 'Confirmado'), 0) AS TotalInscritos,
        COALESCE((SELECT COUNT(1) FROM Inscripciones WHERE ActividadID = @CompetenciaID AND TipoActividad = 'Competencia' AND Estado = 'Confirmado' AND FechaCheckIn IS NOT NULL), 0) AS TotalAsistentes,
        COALESCE((SELECT SUM(tec.Precio)
                    FROM Inscripciones i
                    JOIN TiposEntradaCompetencia tec ON i.TipoEntradaID = tec.TipoEntradaID
                    JOIN PagoInscripciones pi ON i.InscripcionID = pi.InscripcionID
                    JOIN Pagos p ON pi.PagoID = p.PagoID
                    WHERE i.ActividadID = @CompetenciaID AND i.TipoActividad = 'Competencia' AND p.EstadoPago = 'Completado'), 0) AS TotalRecaudado;

    SELECT 
        tec.Nombre AS TipoEntrada,
        tec.Precio,
        COUNT(i.InscripcionID) AS CantidadVendida,
        (COUNT(i.InscripcionID) * tec.Precio) AS Subtotal
    FROM Inscripciones i
    JOIN TiposEntradaCompetencia tec ON i.TipoEntradaID = tec.TipoEntradaID
    WHERE i.ActividadID = @CompetenciaID 
      AND i.TipoActividad = 'Competencia' 
      AND i.Estado = 'Confirmado' 
      AND tec.Precio > 0
    GROUP BY tec.Nombre, tec.Precio;

    IF @TipoCompetencia = 'Por Equipos'
    BEGIN
        SELECT 
            eq.EquipoID,
            eq.NombreEquipo,
            lider.Nombre AS NombreLider,
            miembro.Nombre AS NombreMiembro,
            miembro.Email AS EmailMiembro,
            i.CodigoCheckIn,
            i.FechaCheckIn,
            tec.Nombre as TipoEntrada 
        FROM Equipos eq
        JOIN Usuarios lider ON eq.LiderUsuarioID = lider.UsuarioID
        JOIN MiembrosEquipo me ON eq.EquipoID = me.EquipoID
        JOIN Usuarios miembro ON me.UsuarioID = miembro.UsuarioID
        JOIN Inscripciones i ON miembro.UsuarioID = i.UsuarioID AND i.ActividadID = @CompetenciaID AND i.TipoActividad = 'Competencia'
        LEFT JOIN TiposEntradaCompetencia tec ON i.TipoEntradaID = tec.TipoEntradaID -- Se añade el JOIN
        WHERE eq.CompetenciaID = @CompetenciaID AND eq.Estado = 'Confirmado'
        ORDER BY eq.NombreEquipo, miembro.Nombre;
    END
    ELSE -- Competencia Individual
    BEGIN
        SELECT 
            u.Nombre AS NombreParticipante,
            u.Email,
            i.CodigoCheckIn,
            i.FechaCheckIn,
            tec.Nombre AS TipoEntrada 
        FROM Inscripciones i
        JOIN Usuarios u ON i.UsuarioID = u.UsuarioID
        LEFT JOIN TiposEntradaCompetencia tec ON i.TipoEntradaID = tec.TipoEntradaID -- Se añade el JOIN
        WHERE i.ActividadID = @CompetenciaID AND i.TipoActividad = 'Competencia' AND i.Estado = 'Confirmado'
        ORDER BY u.Nombre;
    END
END
