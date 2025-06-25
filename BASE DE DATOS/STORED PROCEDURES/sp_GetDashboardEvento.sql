ALTER PROCEDURE sp_GetDashboardEvento
    @EventoID INT,
    @OrganizadorID INT
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @TituloEvento NVARCHAR(200);

	UPDATE Eventos
    SET Estado = 'Finalizado'
    WHERE EventoID = @EventoID AND FechaEvento < GETDATE() AND Estado = 'Activo';

    SELECT @TituloEvento = Titulo FROM Eventos WHERE EventoID = @EventoID AND OrganizadorID = @OrganizadorID;
    IF @TituloEvento IS NULL BEGIN RETURN; END

    SELECT 
        @TituloEvento AS TituloEvento,
        COALESCE(COUNT(i.InscripcionID), 0) AS TotalInscritos,
        COALESCE(SUM(CASE WHEN i.FechaCheckIn IS NOT NULL THEN 1 ELSE 0 END), 0) AS TotalAsistentes,
        COALESCE((SELECT SUM(te.Precio) 
                    FROM Inscripciones i
                    JOIN TiposEntrada te ON i.TipoEntradaID = te.TipoEntradaID
                    JOIN PagoInscripciones pi ON i.InscripcionID = pi.InscripcionID
                    JOIN Pagos p ON pi.PagoID = p.PagoID
                    WHERE i.ActividadID = @EventoID AND i.TipoActividad = 'Evento' AND p.EstadoPago = 'Completado'), 0) AS TotalRecaudado
    FROM Inscripciones i
    WHERE i.ActividadID = @EventoID AND i.TipoActividad = 'Evento' AND i.Estado = 'Confirmado';

    -- Tipo de Entrada
    SELECT 
        te.Nombre AS TipoEntrada,
        te.Precio,
        COUNT(i.InscripcionID) AS CantidadVendida,
        (COUNT(i.InscripcionID) * te.Precio) AS Subtotal
    FROM Inscripciones i
    JOIN TiposEntrada te ON i.TipoEntradaID = te.TipoEntradaID
    WHERE i.ActividadID = @EventoID AND i.TipoActividad = 'Evento' AND i.Estado = 'Confirmado' AND te.Precio > 0
    GROUP BY te.Nombre, te.Precio;

    -- Lista de Participantes
    SELECT 
        u.Nombre AS NombreParticipante,
        u.Email,
        i.CodigoCheckIn,
        i.FechaCheckIn
    FROM Inscripciones i
    JOIN Usuarios u ON i.UsuarioID = u.UsuarioID
    WHERE i.ActividadID = @EventoID AND i.TipoActividad = 'Evento' AND i.Estado = 'Confirmado'
    ORDER BY u.Nombre;
END
GO