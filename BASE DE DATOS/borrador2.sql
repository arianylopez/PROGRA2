SELECT * FROM Categorias
SELECT * FROM CompetenciaCategorias
SELECT * FROM Competencias
SELECT * FROM Equipos
SELECT * FROM EventoCategorias
SELECT * FROM Eventos
SELECT * FROM Competencias
SELECT * FROM FavoritosCompetencias
SELECT * FROM FavoritosEventos
SELECT * FROM Inscripciones
SELECT * FROM PagoInscripciones
SELECT * FROM Pagos
SELECT * FROM PreguntasSeguridad
SELECT * FROM TiposEntrada
SELECT * FROM TiposEntradaCompetencia
SELECT * FROM Usuarios -- 72 USUARIOS NUEVOS
SELECT * FROM Equipos
SELECT * FROM MiembrosEquipo -- FALTA QUE LLEGUEN LOS DATOS AQUI

ALTER TABLE Competencias
ADD Estado NVARCHAR(50) NOT NULL DEFAULT 'Activo';
GO

ALTER TABLE Eventos
ADD Estado NVARCHAR(50) NOT NULL DEFAULT 'Activo';
GO
PRINT 'Columna Estado añadida a la tabla Eventos.';

-- Creamos la nueva tabla con todas las columnas necesarias
CREATE TABLE TiposEntradaCompetencia (
    TipoEntradaID INT IDENTITY(1,1) PRIMARY KEY,
    CompetenciaID INT NOT NULL,
    Nombre NVARCHAR(100) NOT NULL,
    Precio DECIMAL(10, 2) NOT NULL,
    CantidadTotal INT NOT NULL,
    CantidadDisponible INT NOT NULL,
    FechaInicioVenta DATETIME2 NOT NULL,
    FechaFinVenta DATETIME2 NOT NULL,
    CONSTRAINT FK_TiposEntradaCompetencia_Competencias_Final FOREIGN KEY (CompetenciaID) REFERENCES Competencias(CompetenciaID) ON DELETE CASCADE
);
GO
PRINT 'Tabla TiposEntradaCompetencia re-creada con éxito.';
-- 1. Insertar un usuario de prueba
INSERT INTO Usuarios (Nombre, Email, Password, PreguntaID, RespuestaSeguridad) 
VALUES ('Usuario Prueba', 'prueba@test.com', '123456', 1, 'respuesta');
SELECT * FROM Usuarios
-- 2. Insertar un evento de prueba
INSERT INTO Eventos (Titulo, Descripcion, FechaEvento, Ubicacion, OrganizadorID) 
VALUES ('Evento de Prueba', 'Descripción del evento', '2024-12-31 18:00:00', 'Ubicación Prueba', 1);
SELECT * FROM Eventos
-- 3. Insertar un tipo de entrada con precio
INSERT INTO TiposEntrada (EventoID, Nombre, Precio, CantidadTotal, CantidadDisponible, FechaInicioVenta, FechaFinVenta) 
VALUES (8, 'Entrada General Prueba', 50.00, 100, 100, '2024-12-30 23:59:59', '2024-12-31 23:59:59');
SELECT * FROM TiposEntrada


SELECT 
    p.PagoID, p.EstadoPago, p.MontoTotal,
    i.InscripcionID, i.Estado as EstadoInscripcion,
    pi.PagoID as RelacionPago, pi.InscripcionID as RelacionInscripcion
FROM Pagos p
LEFT JOIN PagoInscripciones pi ON p.PagoID = pi.PagoID
LEFT JOIN Inscripciones i ON pi.InscripcionID = i.InscripcionID
WHERE p.PagoID = 7;
-- Verificar si existe el pago con ID 5
SELECT * FROM Pagos WHERE PagoID = 5;

-- Verificar las columnas de la tabla Pagos
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE 
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'Pagos';

-- Verificar si las columnas existen
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE, CHARACTER_MAXIMUM_LENGTH
FROM INFORMATION_SCHEMA.COLUMNS 
WHERE TABLE_NAME = 'MiembrosEquipo'
ORDER BY ORDINAL_POSITION;


-- Verificar el pago específico
SELECT PagoID, UsuarioID, MontoTotal, EstadoPago, FechaPago, MetodoPagoSimulado, TransaccionID
FROM Pagos 
WHERE PagoID = 5;

-- Ver todos los pagos para entender el contexto
SELECT * FROM Pagos ORDER BY PagoID DESC;

-- Verificar las relaciones del pago
SELECT pi.PagoID, pi.InscripcionID, i.Estado as EstadoInscripcion
FROM PagoInscripciones pi
JOIN Inscripciones i ON pi.InscripcionID = i.InscripcionID
WHERE pi.PagoID = 5;


select * from equipos 
select * from MiembrosEquipo
-- MODIFICACIONES

-- ====================================================================
-- PASO B: CREACIÓN DE LA NUEVA ESTRUCTURA
-- ====================================================================

-- 1. Tabla de Pagos (Revisada)
-- Almacena únicamente transacciones monetarias.
CREATE TABLE Pagos (
    PagoID INT IDENTITY(1,1) PRIMARY KEY,
    UsuarioID INT NOT NULL, -- El usuario que realizó el pago
    MontoTotal DECIMAL(10, 2) NOT NULL,
    FechaPago DATETIME2 NOT NULL,
    EstadoPago NVARCHAR(50) NOT NULL, -- 'Pendiente', 'Completado', 'Fallido'
    MetodoPagoSimulado NVARCHAR(100),
    TransaccionID NVARCHAR(100) NULL,
    CONSTRAINT FK_Pagos_Usuarios FOREIGN KEY (UsuarioID) REFERENCES Usuarios(UsuarioID)
);
GO
PRINT 'Tabla [Pagos] creada.';

-- 2. Tabla de Equipos (Revisada)
-- Define un equipo y lo asocia a una competencia y a un líder.
CREATE TABLE Equipos (
    EquipoID INT IDENTITY(1,1) PRIMARY KEY,
    CompetenciaID INT NOT NULL,
    NombreEquipo NVARCHAR(150) NOT NULL,
    LiderUsuarioID INT NOT NULL,
    FechaCreacion DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT FK_Equipos_Competencias FOREIGN KEY (CompetenciaID) REFERENCES Competencias(CompetenciaID) ON DELETE CASCADE,
    CONSTRAINT FK_Equipos_Lider FOREIGN KEY (LiderUsuarioID) REFERENCES Usuarios(UsuarioID),
    CONSTRAINT UQ_Equipo_En_Competencia UNIQUE (CompetenciaID, NombreEquipo)
);
GO
PRINT 'Tabla [Equipos] creada.';

-- 3. Tabla Universal de Inscripciones (NUEVA Y CENTRAL)
-- Esta es la tabla más importante. Registra a CADA participante individual.
CREATE TABLE Inscripciones (
    InscripcionID INT IDENTITY(1,1) PRIMARY KEY,
    UsuarioID INT NOT NULL,                 -- ¿Quién es el inscrito?
    TipoActividad NVARCHAR(50) NOT NULL,    -- 'Evento' o 'Competencia'
    ActividadID INT NOT NULL,               -- El ID del Evento o de la Competencia
    
    -- ID del ticket o tipo de inscripción seleccionado (puede ser nulo si la inscripción es directa y gratuita)
    TipoEntradaID INT NULL, 
    
    -- ID del equipo al que pertenece esta inscripción (solo para competencias grupales)
    EquipoID INT NULL,

    CodigoCheckIn NVARCHAR(20) NOT NULL UNIQUE, -- Código único universal para CADA persona
    Estado NVARCHAR(50) NOT NULL,           -- 'Confirmado' (si es gratis) o 'Reservado' (si está pendiente de pago)
    FechaInscripcion DATETIME2 DEFAULT GETDATE(),
    FechaCheckIn DATETIME2 NULL,

    CONSTRAINT FK_Inscripciones_Usuarios FOREIGN KEY (UsuarioID) REFERENCES Usuarios(UsuarioID),
    CONSTRAINT FK_Inscripciones_Equipos FOREIGN KEY (EquipoID) REFERENCES Equipos(EquipoID)
);
GO
PRINT 'Tabla universal [Inscripciones] creada.';

-- 4. Tabla de Enlace para Pagos (Revisada)
-- Esta única tabla enlaza un Pago con una o más Inscripciones universales.
CREATE TABLE PagoInscripciones (
    PagoID INT NOT NULL,
    InscripcionID INT NOT NULL,
    PRIMARY KEY (PagoID, InscripcionID),
    CONSTRAINT FK_PagoInscripciones_Pagos FOREIGN KEY (PagoID) REFERENCES Pagos(PagoID) ON DELETE CASCADE,
    CONSTRAINT FK_PagoInscripciones_Inscripciones FOREIGN KEY (InscripcionID) REFERENCES Inscripciones(InscripcionID) ON DELETE CASCADE
);
GO

-- CAMBIOOOOOOS 
PRINT 'Paso 3: Creando la nueva estructura de tablas...';

-- Tabla de Equipos
CREATE TABLE Equipos (
    EquipoID INT IDENTITY(1,1) PRIMARY KEY,
    CompetenciaID INT NOT NULL,
    NombreEquipo NVARCHAR(150) NOT NULL,
    LiderUsuarioID INT NOT NULL,
    FechaCreacion DATETIME2 DEFAULT GETDATE(),
    Estado NVARCHAR(50) NOT NULL DEFAULT 'Reservado', -- 'Reservado' o 'Confirmado'
    CONSTRAINT FK_Equipos_Competencias_Final FOREIGN KEY (CompetenciaID) REFERENCES Competencias(CompetenciaID) ON DELETE CASCADE,
    CONSTRAINT FK_Equipos_Usuarios_Final FOREIGN KEY (LiderUsuarioID) REFERENCES Usuarios(UsuarioID),
    CONSTRAINT UQ_Equipo_En_Competencia_Final UNIQUE (CompetenciaID, NombreEquipo)
);
GO
PRINT 'Tabla [Equipos] creada.';

-- Tabla de Miembros del Equipo (LA QUE FALTABA)
CREATE TABLE MiembrosEquipo (
    EquipoID INT NOT NULL,
    UsuarioID INT NOT NULL,
    PRIMARY KEY (EquipoID, UsuarioID),
    CONSTRAINT FK_MiembrosEquipo_Equipos_Final FOREIGN KEY (EquipoID) REFERENCES Equipos(EquipoID) ON DELETE CASCADE,
    CONSTRAINT FK_MiembrosEquipo_Usuarios_Final FOREIGN KEY (UsuarioID) REFERENCES Usuarios(UsuarioID)
);
GO
PRINT 'Tabla [MiembrosEquipo] creada.';

-- Tabla Universal de Inscripciones
CREATE TABLE Inscripciones (
    InscripcionID INT IDENTITY(1,1) PRIMARY KEY,
    UsuarioID INT NOT NULL,
    TipoActividad NVARCHAR(50) NOT NULL,
    ActividadID INT NOT NULL,
    TipoEntradaID INT NULL,
    EquipoID INT NULL,
    CodigoCheckIn NVARCHAR(20) NOT NULL UNIQUE,
    Estado NVARCHAR(50) NOT NULL,
    FechaInscripcion DATETIME2 DEFAULT GETDATE(),
    FechaCheckIn DATETIME2 NULL,
    CONSTRAINT FK_Inscripciones_Usuarios_Final FOREIGN KEY (UsuarioID) REFERENCES Usuarios(UsuarioID),
    CONSTRAINT FK_Inscripciones_Equipos_Final FOREIGN KEY (EquipoID) REFERENCES Equipos(EquipoID)
);
GO
PRINT 'Tabla universal [Inscripciones] creada.';

-- Tabla de Pagos (sin cambios, pero la incluimos para asegurar que exista)
IF OBJECT_ID('dbo.Pagos', 'U') IS NULL
BEGIN
    CREATE TABLE Pagos (
        PagoID INT IDENTITY(1,1) PRIMARY KEY,
        UsuarioID INT NOT NULL,
        MontoTotal DECIMAL(10, 2) NOT NULL,
        FechaPago DATETIME2 NOT NULL,
        EstadoPago NVARCHAR(50) NOT NULL,
        MetodoPagoSimulado NVARCHAR(100),
        TransaccionID NVARCHAR(100) NULL,
        CONSTRAINT FK_Pagos_Usuarios_Final FOREIGN KEY (UsuarioID) REFERENCES Usuarios(UsuarioID)
    );
    PRINT 'Tabla [Pagos] creada.';
END
GO

-- Tabla de Enlace para Pagos e Inscripciones
CREATE TABLE PagoInscripciones (
    PagoID INT NOT NULL,
    InscripcionID INT NOT NULL,
    PRIMARY KEY (PagoID, InscripcionID),
    CONSTRAINT FK_PagoInscripciones_Pagos_Final FOREIGN KEY (PagoID) REFERENCES Pagos(PagoID) ON DELETE CASCADE,
    CONSTRAINT FK_PagoInscripciones_Inscripciones_Final FOREIGN KEY (InscripcionID) REFERENCES Inscripciones(InscripcionID) ON DELETE CASCADE
);
GO
PRINT 'Tabla de enlace [PagoInscripciones] creada.';
PRINT 'Nueva estructura de tablas creada exitosamente.';