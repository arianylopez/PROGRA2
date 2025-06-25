ALTER TRIGGER TRG_ActualizarStockInscripcion
ON Inscripciones
AFTER INSERT, UPDATE
AS
BEGIN
    IF NOT EXISTS (SELECT 1 FROM inserted WHERE Estado = 'Confirmado')
    BEGIN
        RETURN;
    END

    UPDATE te
    SET te.CantidadDisponible = te.CantidadDisponible - i.CantidadInscrita
    FROM TiposEntrada te
    JOIN (
        SELECT 
            TipoEntradaID, 
            COUNT(*) as CantidadInscrita 
        FROM inserted 
        WHERE TipoActividad = 'Evento' AND Estado = 'Confirmado' AND TipoEntradaID IS NOT NULL
        GROUP BY TipoEntradaID
    ) AS i ON te.TipoEntradaID = i.TipoEntradaID;

    UPDATE tec
    SET tec.CantidadDisponible = tec.CantidadDisponible - i.CantidadInscrita
    FROM TiposEntradaCompetencia tec
    JOIN (
        SELECT 
            TipoEntradaID, 
            COUNT(*) as CantidadInscrita 
        FROM inserted 
        WHERE TipoActividad = 'Competencia' AND Estado = 'Confirmado' AND TipoEntradaID IS NOT NULL
        GROUP BY TipoEntradaID
    ) AS i ON tec.TipoEntradaID = i.TipoEntradaID;

END
