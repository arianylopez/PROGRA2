/****************************************************************************************
 * SCRIPT DE CREACIÓN DE BASE DE DATOS PARA EL PROYECTO DE GESTIÓN DE EVENTOS (SPA)
 * Base de Datos: GestionEventosDB
 * Autor: Ariany Lopez
 * Fecha: 14 de Junio de 2025
 ****************************************************************************************/
-- Creación de la base de datos
CREATE DATABASE GestionEventosDB;

-- Cambiar al contexto de la nueva base de datos
USE GestionEventosDB;

-- SECCIÓN 1: CREACIÓN DE TABLAS

-- Tabla para almacenar las preguntas de seguridad predefinidas
CREATE TABLE PreguntasSeguridad (
    PreguntaID INT IDENTITY(1,1) PRIMARY KEY,
    TextoPregunta NVARCHAR(255) NOT NULL
);

CREATE TABLE Categorias (
    CategoriaID INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(100) NOT NULL UNIQUE
);

-- Tabla principal de Usuarios
CREATE TABLE Usuarios (
    UsuarioID INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(150) NOT NULL,
    Email NVARCHAR(150) NOT NULL UNIQUE,
    Password NVARCHAR(100) NOT NULL, -- SIN HASH, como se especificó para el proyecto académico
    PreguntaID INT NOT NULL,
    RespuestaSeguridad NVARCHAR(255) NOT NULL, -- SIN HASH
    CONSTRAINT FK_Usuarios_PreguntasSeguridad FOREIGN KEY (PreguntaID) REFERENCES PreguntasSeguridad(PreguntaID)
);

-- Tabla para Eventos
CREATE TABLE Eventos (
    EventoID INT IDENTITY(1,1) PRIMARY KEY,
    Titulo NVARCHAR(200) NOT NULL,
    Descripcion NVARCHAR(MAX) NOT NULL,
    FechaEvento DATETIME2 NOT NULL,
    Ubicacion NVARCHAR(255) NOT NULL,
    OrganizadorID INT NOT NULL,
    CONSTRAINT FK_Eventos_Usuarios FOREIGN KEY (OrganizadorID) REFERENCES Usuarios(UsuarioID)
);

-- Tabla para Competencias
CREATE TABLE Competencias (
    CompetenciaID INT IDENTITY(1,1) PRIMARY KEY,
    Titulo NVARCHAR(200) NOT NULL,
    Descripcion NVARCHAR(MAX) NOT NULL,
    FechaInicio DATETIME2 NOT NULL,
    FechaFin DATETIME2 NOT NULL,
    Tipo NVARCHAR(50) NOT NULL CHECK (Tipo IN ('Individual', 'Por Equipos')),
    TamanoMinEquipo INT,
    TamanoMaxEquipo INT,
    OrganizadorID INT NOT NULL,
    CONSTRAINT FK_Competencias_Usuarios FOREIGN KEY (OrganizadorID) REFERENCES Usuarios(UsuarioID)
);

-- Tablas de enlace para relaciones Muchos a Muchos (Categorías)
CREATE TABLE EventoCategorias (
    EventoID INT NOT NULL,
    CategoriaID INT NOT NULL,
    PRIMARY KEY (EventoID, CategoriaID),
    CONSTRAINT FK_EventoCategorias_Eventos FOREIGN KEY (EventoID) REFERENCES Eventos(EventoID) ON DELETE CASCADE,
    CONSTRAINT FK_EventoCategorias_Categorias FOREIGN KEY (CategoriaID) REFERENCES Categorias(CategoriaID) ON DELETE CASCADE
);
CREATE TABLE CompetenciaCategorias (
    CompetenciaID INT NOT NULL,
    CategoriaID INT NOT NULL,
    PRIMARY KEY (CompetenciaID, CategoriaID),
    CONSTRAINT FK_CompetenciaCategorias_Competencias FOREIGN KEY (CompetenciaID) REFERENCES Competencias(CompetenciaID) ON DELETE CASCADE,
    CONSTRAINT FK_CompetenciaCategorias_Categorias FOREIGN KEY (CategoriaID) REFERENCES Categorias(CategoriaID) ON DELETE CASCADE
);

-- Tabla para los diferentes tipos de entrada de un evento
ALTER TABLE TiposEntrada (
    TipoEntradaID INT IDENTITY(1,1) PRIMARY KEY,
    EventoID INT NOT NULL,
    Nombre NVARCHAR(100) NOT NULL,
    Precio DECIMAL(10, 2) NOT NULL,
    CantidadTotal INT NOT NULL,
    CantidadDisponible INT NOT NULL,
    FechaInicioVenta DATETIME2 NOT NULL,
    FechaFinVenta DATETIME2 NOT NULL,
    CONSTRAINT FK_TiposEntrada_Eventos FOREIGN KEY (EventoID) REFERENCES Eventos(EventoID) ON DELETE CASCADE
);

-- Tabla para registrar las inscripciones de usuarios a eventos
CREATE TABLE InscripcionesEventos (
    InscripcionID INT IDENTITY(1,1) PRIMARY KEY,
    UsuarioID INT NOT NULL,
    TipoEntradaID INT NOT NULL,
    FechaInscripcion DATETIME2 DEFAULT GETDATE(),
    CodigoCheckIn NVARCHAR(20) NOT NULL UNIQUE,
    CONSTRAINT FK_InscripcionesEventos_Usuarios FOREIGN KEY (UsuarioID) REFERENCES Usuarios(UsuarioID),
    CONSTRAINT FK_InscripcionesEventos_TiposEntrada FOREIGN KEY (TipoEntradaID) REFERENCES TiposEntrada(TipoEntradaID) -- NO CASCADE, para no borrar inscripciones si se borra el tipo de entrada
);

-- Tabla para registrar las inscripciones a competencias
CREATE TABLE InscripcionesCompetencia (
    InscripcionID INT IDENTITY(1,1) PRIMARY KEY,
    CompetenciaID INT NOT NULL,
    UsuarioID INT NOT NULL, -- Líder del equipo o participante individual
    NombreEquipo NVARCHAR(150), -- Nulo si es individual
    FechaInscripcion DATETIME2 DEFAULT GETDATE(),
    CONSTRAINT FK_InscripcionesCompetencia_Competencias FOREIGN KEY (CompetenciaID) REFERENCES Competencias(CompetenciaID),
    CONSTRAINT FK_InscripcionesCompetencia_Usuarios FOREIGN KEY (UsuarioID) REFERENCES Usuarios(UsuarioID)
);

-- Tablas para la funcionalidad de Favoritos
CREATE TABLE FavoritosEventos (
    FavoritoID INT IDENTITY(1,1) PRIMARY KEY,
    UsuarioID INT NOT NULL,
    EventoID INT NOT NULL,
    CONSTRAINT UQ_Usuario_Evento_Favorito UNIQUE (UsuarioID, EventoID),
    CONSTRAINT FK_FavoritosEventos_Usuarios FOREIGN KEY (UsuarioID) REFERENCES Usuarios(UsuarioID) ON DELETE CASCADE,
    CONSTRAINT FK_FavoritosEventos_Eventos FOREIGN KEY (EventoID) REFERENCES Eventos(EventoID) ON DELETE CASCADE
);
CREATE TABLE FavoritosCompetencias (
    FavoritoID INT IDENTITY(1,1) PRIMARY KEY,
    UsuarioID INT NOT NULL,
    CompetenciaID INT NOT NULL,
    CONSTRAINT UQ_Usuario_Competencia_Favorita UNIQUE (UsuarioID, CompetenciaID),
    CONSTRAINT FK_FavoritosCompetencias_Usuarios FOREIGN KEY (UsuarioID) REFERENCES Usuarios(UsuarioID) ON DELETE CASCADE,
    CONSTRAINT FK_FavoritosCompetencias_Competencias FOREIGN KEY (CompetenciaID) REFERENCES Competencias(CompetenciaID) ON DELETE CASCADE
);

-- Tabla de auditoría para cambios de precios en las entradas
CREATE TABLE AuditoriaPrecios (
    AuditoriaID INT IDENTITY(1,1) PRIMARY KEY,
    TipoEntradaID INT NOT NULL,
    PrecioAnterior DECIMAL(10, 2) NOT NULL,
    PrecioNuevo DECIMAL(10, 2) NOT NULL,
    UsuarioModificador NVARCHAR(150) NOT NULL,
    FechaModificacion DATETIME2 DEFAULT GETDATE()
);
