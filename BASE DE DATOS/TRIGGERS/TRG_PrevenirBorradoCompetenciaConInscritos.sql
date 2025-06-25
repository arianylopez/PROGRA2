ALTER TRIGGER TRG_PrevenirBorradoCompetenciaConInscritos
ON Competencias
INSTEAD OF DELETE
AS
BEGIN
    IF EXISTS (
        SELECT 1
        FROM deleted d
        JOIN Inscripciones i ON d.CompetenciaID = i.ActividadID
        WHERE i.TipoActividad = 'Competencia' AND i.Estado = 'Confirmado'
    )
    BEGIN
        RAISERROR ('No se puede eliminar la competencia porque ya existen inscripciones confirmadas.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END
    ELSE
    BEGIN
        DELETE c
        FROM Competencias c
        JOIN deleted d ON c.CompetenciaID = d.CompetenciaID;
    END
END

