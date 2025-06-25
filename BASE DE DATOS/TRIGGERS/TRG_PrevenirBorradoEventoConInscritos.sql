-- APROBADO
ALTER TRIGGER TRG_PrevenirBorradoEventoConInscritos
ON Eventos
INSTEAD OF DELETE
AS
BEGIN
    IF EXISTS (
        SELECT 1
        FROM deleted d
        JOIN Inscripciones i ON d.EventoID = i.ActividadID
        WHERE i.TipoActividad = 'Evento' AND i.Estado = 'Confirmado'
    )
    BEGIN
        RAISERROR ('No se puede eliminar el evento porque ya existen inscripciones confirmadas.', 16, 1);
        ROLLBACK TRANSACTION;
        RETURN;
    END
    ELSE
    BEGIN
        DELETE e
        FROM Eventos e
        JOIN deleted d ON e.EventoID = d.EventoID;
    END
END

