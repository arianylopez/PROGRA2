-- --- Índices de Búsqueda de Usuarios por Email
CREATE INDEX IX_Usuarios_Email 
ON Usuarios(Email);

SELECT UsuarioID, Nombre, Email 
FROM Usuarios 
WHERE Email IN (
    'mateo.a@example.com', 
    'sebas.moreno@example.com',
    'hector.ramos.h@gmail.com',
    'cristian.cabrera.c@gmail.com'
);

-- --- Índices de Búsqueda de Actividades por Organizador 
CREATE NONCLUSTERED INDEX IX_Eventos_OrganizadorID 
ON Eventos(OrganizadorID);

CREATE NONCLUSTERED INDEX IX_Competencias_OrganizadorID 
ON Competencias(OrganizadorID);

SELECT 
    E.Titulo AS TituloEvento,
    U.Nombre AS NombreParticipante
FROM Eventos E
JOIN Inscripciones I ON E.EventoID = I.ActividadID AND I.TipoActividad = 'Evento'
JOIN Usuarios U ON I.UsuarioID = U.UsuarioID
WHERE E.OrganizadorID = 1; 

-- --- Índices de Búsqueda de Inscripciones por Actividad 
CREATE NONCLUSTERED INDEX IX_Inscripciones_Actividad 
ON Inscripciones(TipoActividad, ActividadID);

SELECT 
    U.Nombre,
    PS.TextoPregunta
FROM Inscripciones I
JOIN Usuarios U ON I.UsuarioID = U.UsuarioID
JOIN PreguntasSeguridad PS ON U.PreguntaID = PS.PreguntaID
WHERE I.TipoActividad = 'Evento' AND I.ActividadID = 1;

-- --- Índices de Aceleración los Filtros por Categoría 
CREATE NONCLUSTERED INDEX IX_EventoCategorias_CategoriaID 
ON EventoCategorias(CategoriaID);

CREATE NONCLUSTERED INDEX IX_CompetenciaCategorias_CategoriaID 
ON CompetenciaCategorias(CategoriaID);

SELECT 
    U.Nombre AS NombreUsuario,
    C.Titulo AS TituloCompetencia
FROM Categorias Cat
JOIN CompetenciaCategorias CC ON Cat.CategoriaID = CC.CategoriaID
JOIN Competencias C ON CC.CompetenciaID = C.CompetenciaID
JOIN Inscripciones I ON C.CompetenciaID = I.ActividadID AND I.TipoActividad = 'Competencia'
JOIN Usuarios U ON I.UsuarioID = U.UsuarioID
WHERE Cat.CategoriaID = 3; 

-- --- Índices de Búsqueda de Entradas por Actividad ---
CREATE NONCLUSTERED INDEX IX_TiposEntrada_EventoID 
ON TiposEntrada(EventoID);

CREATE NONCLUSTERED INDEX IX_TiposEntradaCompetencia_CompetenciaID 
ON TiposEntradaCompetencia(CompetenciaID);

SELECT
    E.Titulo,
    TE.Nombre AS NombreEntrada,
    TE.Precio
FROM Eventos AS E
JOIN TiposEntrada AS TE ON E.EventoID = TE.EventoID
