SELECT * FROM Categorias
SELECT * FROM CompetenciaCategorias -- FALTA ESTO
SELECT * FROM Competencias
SELECT * FROM EventoCategorias -- FALTA ESTO
SELECT * FROM Eventos
SELECT * FROM FavoritosCompetencias -- FALTA ESTO
SELECT * FROM FavoritosEventos -- FALTA ESTO
SELECT * FROM InscripcionesCompetencia
SELECT * FROM InscripcionesEventos
SELECT * FROM PagoInscripciones -- VER ESTO
SELECT * FROM Pagos -- MANEJAR DISTINTOS METODOS DE PAGO: Qr y Tarjeta modificar eso, y guardar los datos de las tarjetas
SELECT * FROM PreguntasSeguridad -- AGREGAR MAS PREGUNTAS
SELECT * FROM TiposEntrada -- MODIFICAR PARA QUE SE AGREGUE FECHA INICIO Y FECHA FIN
SELECT * FROM Usuarios
SELECT * FROM TiposEntradaCompetencia
USE GestionEventosDB;
GO

PRINT 'Iniciando reestructuración final de la base de datos...';

-- --- PASO 1: Eliminar de forma segura la columna Precio de la tabla Competencias ---

-- Variable para guardar el nombre de la restricción
DECLARE @ConstraintName NVARCHAR(200)

-- Buscamos el nombre de la restricción por defecto para la columna 'Precio'
SELECT @ConstraintName = NAME
FROM SYS.DEFAULT_CONSTRAINTS
WHERE PARENT_OBJECT_ID = OBJECT_ID('Competencias')
AND PARENT_COLUMN_ID = (SELECT column_id FROM sys.columns WHERE Name = N'Precio' AND object_id = OBJECT_ID(N'Competencias'))

-- Si se encontró una restricción, la eliminamos dinámicamente
IF @ConstraintName IS NOT NULL
BEGIN
    EXEC('ALTER TABLE Competencias DROP CONSTRAINT ' + @ConstraintName)
    PRINT 'Restricción por defecto ' + @ConstraintName + ' eliminada.';
END

-- Ahora que no hay dependencias, eliminamos la columna
IF EXISTS (SELECT * FROM sys.columns WHERE Name = N'Precio' AND object_id = OBJECT_ID(N'Competencias'))
BEGIN
    ALTER TABLE Competencias DROP COLUMN Precio;
    PRINT 'Columna Precio eliminada de la tabla Competencias.';
END
GO

-- --- PASO 2: Crear la nueva tabla para las entradas de competencias ---

IF OBJECT_ID('dbo.TiposEntradaCompetencia', 'U') IS NOT NULL
	PRINT 'La tabla TiposEntradaCompetencia ya existe.'
ELSE
BEGIN
	CREATE TABLE TiposEntradaCompetencia (
		TipoEntradaID INT IDENTITY(1,1) PRIMARY KEY,
		CompetenciaID INT NOT NULL,
		Nombre NVARCHAR(100) NOT NULL,
		Precio DECIMAL(10, 2) NOT NULL,
		CONSTRAINT FK_TiposEntradaCompetencia_Competencias FOREIGN KEY (CompetenciaID) REFERENCES Competencias(CompetenciaID) ON DELETE CASCADE
	);
	PRINT 'Tabla TiposEntradaCompetencia creada exitosamente.';
END
GO

SELECT * FROM TiposEntradaCompetencia
SELECT * FROM TiposEntrada
ALTER TABLE Eventos
ADD ImagenUrl NVARCHAR(500) NULL;
GO

-- Añade la columna para la URL de la imagen a la tabla Competencias
ALTER TABLE Competencias
ADD ImagenUrl NVARCHAR(500) NULL;
GO

-- Eliminamos la tabla antigua para evitar conflictos.
-- El 'IF EXISTS' asegura que el script no falle si ya la habías borrado.
IF OBJECT_ID('dbo.InscripcionesCompetencia', 'U') IS NOT NULL
BEGIN
    DROP TABLE dbo.Equipos;
    PRINT 'Tabla antigua InscripcionesCompetencia eliminada.';
END
GO

-- Creamos la nueva tabla para definir un Equipo como una entidad propia.
-- Guardará el nombre del equipo y quién es el líder.
CREATE TABLE Equipos (
    EquipoID INT IDENTITY(1,1) PRIMARY KEY,
    CompetenciaID INT NOT NULL,
    NombreEquipo NVARCHAR(150) NOT NULL,
    LiderUsuarioID INT NOT NULL,
    FechaInscripcion DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT FK_Equipos_Competencias FOREIGN KEY (CompetenciaID) REFERENCES Competencias(CompetenciaID) ON DELETE CASCADE,
    CONSTRAINT FK_Equipos_Usuarios FOREIGN KEY (LiderUsuarioID) REFERENCES Usuarios(UsuarioID),
    -- Regla: Un equipo con el mismo nombre no puede inscribirse dos veces en la misma competencia.
    CONSTRAINT UQ_Equipo_Competencia UNIQUE (CompetenciaID, NombreEquipo)
);
GO
SELECT * FROM Equipos
-- Creamos la tabla de enlace para los miembros del equipo.
-- Esta tabla nos dice qué usuarios pertenecen a qué equipo.
CREATE TABLE MiembrosEquipo (
    EquipoID INT NOT NULL,
    UsuarioID INT NOT NULL,
    PRIMARY KEY (EquipoID, UsuarioID), -- Un usuario solo puede estar una vez en el mismo equipo.
    CONSTRAINT FK_MiembrosEquipo_Equipos FOREIGN KEY (EquipoID) REFERENCES Equipos(EquipoID) ON DELETE CASCADE,
    CONSTRAINT FK_MiembrosEquipo_Usuarios FOREIGN KEY (UsuarioID) REFERENCES Usuarios(UsuarioID)
);
SELECT * FROM MiembrosEquipo

ALTER TABLE Equipos
ADD Estado NVARCHAR(50) NOT NULL DEFAULT 'Reservado';
GO

SELECT * FROM Competencias
SELECT * FROM Equipos
SELECT * FROM MiembrosEquipo
SELECT * FROM PagoEquipos
