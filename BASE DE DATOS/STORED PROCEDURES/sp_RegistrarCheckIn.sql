ALTER PROCEDURE sp_RegistrarCheckIn
    @CodigoCheckIn NVARCHAR(20),
    @ActividadID INT, 
    @TipoActividad NVARCHAR(50) 
AS
BEGIN
    SET NOCOUNT ON;
    DECLARE @InscripcionID INT;
    DECLARE @FechaCheckInExistente DATETIME2;

    SELECT 
        @InscripcionID = i.InscripcionID, 
        @FechaCheckInExistente = i.FechaCheckIn
    FROM 
        Inscripciones i
    WHERE 
        i.CodigoCheckIn = @CodigoCheckIn 
        AND i.TipoActividad = @TipoActividad 
        AND i.ActividadID = @ActividadID
        AND i.Estado = 'Confirmado';

    IF @InscripcionID IS NULL
    BEGIN
        SELECT 0 AS Exito, 'C�digo no v�lido, de otra actividad, o de una inscripci�n no confirmada.' AS Mensaje;
        RETURN;
    END

    IF @FechaCheckInExistente IS NOT NULL
    BEGIN
        SELECT 0 AS Exito, 'Este c�digo ya fue utilizado para un check-in el ' + CONVERT(NVARCHAR, @FechaCheckInExistente, 120) AS Mensaje;
        RETURN;
    END

    UPDATE Inscripciones SET FechaCheckIn = GETDATE() WHERE InscripcionID = @InscripcionID;
    
    SELECT 1 AS Exito, 'Check-in registrado exitosamente.' AS Mensaje;
END
