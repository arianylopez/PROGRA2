ALTER PROCEDURE sp_RegistrarUsuario
    @Nombre NVARCHAR(100),
    @Email NVARCHAR(100),
    @Password NVARCHAR(100),
    @PreguntaID INT,
    @RespuestaSeguridad NVARCHAR(200)
AS
BEGIN
    SET NOCOUNT ON;
    IF EXISTS (SELECT 1 FROM Usuarios WHERE Email = @Email)
    BEGIN
        SELECT 0 AS Exito, 'El correo electrónico ya está registrado.' AS Mensaje, -1 AS NuevoUsuarioID;
        RETURN;
    END

    INSERT INTO Usuarios (Nombre, Email, Password, PreguntaID, RespuestaSeguridad)
    VALUES (@Nombre, @Email, @Password, @PreguntaID, @RespuestaSeguridad);

    SELECT 1 AS Exito, 'Usuario registrado exitosamente.' AS Mensaje, SCOPE_IDENTITY() AS NuevoUsuarioID;
END
