-- APROBADO
CREATE VIEW vw_EventosDisponibles 
AS
SELECT DISTINCT
    e.EventoID,
    e.Titulo,
    e.Descripcion,
    e.FechaEvento,
    e.Ubicacion,
    u.Nombre AS Organizador
FROM Eventos e
JOIN Usuarios u ON e.OrganizadorID = u.UsuarioID
WHERE e.FechaEvento >= GETDATE() AND EXISTS (
    SELECT 1
    FROM TiposEntrada te
    WHERE te.EventoID = e.EventoID AND te.CantidadDisponible > 0
);

SELECT * FROM vw_EventosDisponibles
