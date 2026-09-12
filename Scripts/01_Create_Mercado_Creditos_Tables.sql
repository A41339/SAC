-- =========================================================================
-- SCRIPT DE BASE DE DATOS: MÓDULO OPORTUNIDADES/MERCADO DE CRÉDITO
-- VERSION: 2.2 (Auditoría, Log de SMS y Trazabilidad de Desembolso)
-- =========================================================================

-- 1. TABLA CATÁLOGO: MONEDAS
CREATE TABLE [dbo].[Creditos_Moneda] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [Codigo] NVARCHAR(3) NOT NULL, -- COL, USD
    [Nombre] NVARCHAR(50) NOT NULL,
    [Ind_Activo] BIT NOT NULL DEFAULT 1
);

INSERT INTO [dbo].[Creditos_Moneda] ([Codigo], [Nombre]) VALUES ('COL', 'Colones'), ('USD', 'Dólares');

-- 2. TABLA CATÁLOGO: ESTADOS DE SOLICITUD
CREATE TABLE [dbo].[Creditos_EstadoSolicitud] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [Descripcion] NVARCHAR(50) NOT NULL,
    [Ind_Activo] BIT NOT NULL DEFAULT 1
);

-- Insertar estados básicos (Versión Extendida)
INSERT INTO [dbo].[Creditos_EstadoSolicitud] ([Descripcion], [Ind_Activo]) VALUES 
('Pendiente', 1),          -- Solicitud recién creada
('Aceptada', 1),           -- Entidad Oferente aceptó la intención
('Rechazada', 1),          -- Entidad Oferente rechazó
('Firma Pendiente', 1),    -- Esperando que el Solicitante suba el Pagaré firmado
('Revisión SAC', 1),       -- Documento subido, esperando validación de FFC
('Aprobación Final SAC', 1),-- FFC valida firma digital y da luz verde
('Desembolsada', 1),       -- Entidad Oferente confirma que ya envió el dinero
('Cancelada', 1);          -- Por si el solicitante se arrepiente antes de firmar

-- 3. TABLA PARÁMETROS: COMISIONES DEL FONDO
CREATE TABLE [dbo].[Creditos_ComisionConfig] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [PorcentajeComision] DECIMAL(5, 2) NOT NULL, 
    [Fec_VigenciaDesde] DATETIME NOT NULL,
    [Fec_VigenciaHasta] DATETIME NULL,
    [Ind_Activa] BIT NOT NULL DEFAULT 1,
    [UsuarioActualiza_Id] INT NOT NULL 
);

-- 4. TABLA DE OFERTAS (Lo que publica la Entidad)
CREATE TABLE [dbo].[Creditos_Ofertas] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [EntidadOferente_Id] NVARCHAR(5) NOT NULL,     
    [UsuarioPublicador_Id] INT NOT NULL,          
    [Mon_Monto] DECIMAL(18, 2) NOT NULL,
    [Moneda_Id] INT NOT NULL,
    [TasaInteresAnual] DECIMAL(5, 2) NOT NULL,
    [PlazoMeses] INT NOT NULL,
    [Descripcion] NVARCHAR(500) NULL,
    [TelefonoNotificacionSMS] NVARCHAR(20) NOT NULL,
    [EmailNotificacion] NVARCHAR(100) NOT NULL,
    [Fec_Publicacion] DATETIME NOT NULL DEFAULT GETDATE(),
    [Fec_Vencimiento] DATETIME NULL,
    [Ind_EstadoActiva] BIT NOT NULL DEFAULT 1,
    
    -- Auditoría
    [Fec_UltimaModificacion] DATETIME NULL,
    [UsuarioModifica_Id] INT NULL,
    
    CONSTRAINT FK_Ofertas_Entidad FOREIGN KEY ([EntidadOferente_Id]) REFERENCES [dbo].[Entidad] ([Id]),
    CONSTRAINT FK_Ofertas_Usuario FOREIGN KEY ([UsuarioPublicador_Id]) REFERENCES [dbo].[Usuario] ([Id]),
    CONSTRAINT FK_Ofertas_Moneda FOREIGN KEY ([Moneda_Id]) REFERENCES [dbo].[Creditos_Moneda] ([Id])
);

-- 5. TABLA DE SOLICITUDES (Trazabilidad completa)
CREATE TABLE [dbo].[Creditos_Solicitudes] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [Oferta_Id] INT NOT NULL,
    [EntidadSolicitante_Id] NVARCHAR(5) NOT NULL,  
    [UsuarioSolicitante_Id] INT NOT NULL,         
    [Mon_MontoSolicitado] DECIMAL(18, 2) NOT NULL,
    
    [Mon_MontoComisionSAC] DECIMAL(18, 2) NOT NULL DEFAULT 0, 
    [ComisionConfig_Id] INT NULL, 
    
    [Justificacion] NVARCHAR(500) NULL,
    [TelefonoNotificacionSMS] NVARCHAR(20) NOT NULL,
    [EmailNotificacion] NVARCHAR(100) NOT NULL,
    [Fec_Solicitud] DATETIME NOT NULL DEFAULT GETDATE(),
    
    [EstadoSolicitud_Id] INT NOT NULL, 
    
    -- Resolución de la Oferente
    [Fec_Resolucion] DATETIME NULL,
    [UsuarioResolucion_Id] INT NULL,
    [MotivoRechazo] NVARCHAR(500) NULL,

    -- Datos de Desembolso (Cierre de ciclo)
    [Fec_Desembolso] DATETIME NULL,
    [ReferenciaBancaria] NVARCHAR(100) NULL,
    [ComentariosDesembolso] NVARCHAR(500) NULL,
    
    CONSTRAINT FK_Solicitudes_Ofertas FOREIGN KEY ([Oferta_Id]) REFERENCES [dbo].[Creditos_Ofertas] ([Id]),
    CONSTRAINT FK_Solicitudes_Entidad FOREIGN KEY ([EntidadSolicitante_Id]) REFERENCES [dbo].[Entidad] ([Id]),
    CONSTRAINT FK_Solicitudes_Usuario FOREIGN KEY ([UsuarioSolicitante_Id]) REFERENCES [dbo].[Usuario] ([Id]),
    CONSTRAINT FK_Solicitudes_Estado FOREIGN KEY ([EstadoSolicitud_Id]) REFERENCES [dbo].[Creditos_EstadoSolicitud] ([Id]),
    CONSTRAINT FK_Solicitudes_Comision FOREIGN KEY ([ComisionConfig_Id]) REFERENCES [dbo].[Creditos_ComisionConfig] ([Id]),
    CONSTRAINT FK_Solicitudes_UsuarioRes FOREIGN KEY ([UsuarioResolucion_Id]) REFERENCES [dbo].[Usuario] ([Id])
);

-- 6. TABLA CATÁLOGO: TIPOS DE DOCUMENTOS
-- Permite que SAC defina qué documentos se pueden solicitar (Pagaré, EEFF, Certificación, etc.)
CREATE TABLE [dbo].[Creditos_TipoDocumento] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [Descripcion] NVARCHAR(100) NOT NULL,
    [Ind_Activo] BIT NOT NULL DEFAULT 1
);

-- Insertar algunos tipos por defecto
INSERT INTO [dbo].[Creditos_TipoDocumento] ([Descripcion]) VALUES ('Pagaré Firmado'), ('Estados Financieros'), ('Certificación de Personería'), ('Acuerdo de Junta Directiva');

-- 7. TABLA: REQUISITOS POR OFERTA (Many-to-Many)
-- Define qué documentos solicita el Banco específicamente para UNA oferta.
CREATE TABLE [dbo].[Creditos_OfertaRequisitos] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [Oferta_Id] INT NOT NULL,
    [TipoDocumento_Id] INT NOT NULL,

    CONSTRAINT FK_Requisitos_Oferta FOREIGN KEY ([Oferta_Id]) REFERENCES [dbo].[Creditos_Ofertas] ([Id]),
    CONSTRAINT FK_Requisitos_TipoDoc FOREIGN KEY ([TipoDocumento_Id]) REFERENCES [dbo].[Creditos_TipoDocumento] ([Id])
);

-- 8. TABLA DE DOCUMENTOS RECIBIDOS
CREATE TABLE [dbo].[Creditos_Documentos] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [Solicitud_Id] INT NOT NULL,
    [TipoDocumento_Id] INT NOT NULL, -- Ahora FK al catálogo
    [RutaArchivoPDF] NVARCHAR(500) NOT NULL,
    [NombreArchivoOriginal] NVARCHAR(255) NOT NULL,
    [Fec_Carga] DATETIME NOT NULL DEFAULT GETDATE(),
    [UsuarioCarga_Id] INT NOT NULL,
    
    [Ind_FirmaValida] BIT NOT NULL DEFAULT 0,
    [ValidadoPorSAC_Id] INT NULL,
    [Fec_ValidacionSAC] DATETIME NULL,
    [ComentariosRevision] NVARCHAR(500) NULL,

    CONSTRAINT FK_Docs_Solicitudes FOREIGN KEY ([Solicitud_Id]) REFERENCES [dbo].[Creditos_Solicitudes] ([Id]),
    CONSTRAINT FK_Docs_TipoDoc FOREIGN KEY ([TipoDocumento_Id]) REFERENCES [dbo].[Creditos_TipoDocumento] ([Id]),
    CONSTRAINT FK_Docs_UsuarioCarga FOREIGN KEY ([UsuarioCarga_Id]) REFERENCES [dbo].[Usuario] ([Id]),
    CONSTRAINT FK_Docs_UsuarioSAC FOREIGN KEY ([ValidadoPorSAC_Id]) REFERENCES [dbo].[Usuario] ([Id])
);

-- 9. LOG DE NOTIFICACIONES SMS (Para auditoría de envíos)
CREATE TABLE [dbo].[Creditos_SMSLog] (
    [Id] INT IDENTITY(1,1) PRIMARY KEY,
    [Solicitud_Id] INT NULL,
    [TelefonoDestino] NVARCHAR(20) NOT NULL,
    [Mensaje] NVARCHAR(500) NOT NULL,
    [Fec_Envio] DATETIME NOT NULL DEFAULT GETDATE(),
    [Ind_Exitoso] BIT NOT NULL DEFAULT 0,
    [RespuestaProveedor] NVARCHAR(MAX) NULL
);


-- =========================================================================
-- INTEGRACIÓN CON MENÚ DINÁMICO
-- =========================================================================

-- 1. Cabecera del Módulo
DECLARE @MenuId INT;
SELECT @MenuId = ISNULL(MAX(Id), 0) + 1 FROM [dbo].[Menu];

INSERT INTO [dbo].[Menu] ([Id], [MenuText], [MenuURL], [ParentId], [MenuIcon], [SortOrder])
VALUES (@MenuId, 'Créditos', '#', NULL, '<i class="fa fa-university"></i>', 1);

-- 2. Sub-opciones
DECLARE @ParentId INT = @MenuId;
DECLARE @NextId INT;

SELECT @NextId = ISNULL(MAX(Id), 0) + 1 FROM [dbo].[Menu];
INSERT INTO [dbo].[Menu] ([Id], [MenuText], [MenuURL], [ParentId], [MenuIcon], [SortOrder]) VALUES 
(@NextId, 'Mercado de Créditos', 'Creditos/Index', @ParentId, '<i class="fa fa-circle-o text-aqua"></i>', 1);

SELECT @NextId = ISNULL(MAX(Id), 0) + 1 FROM [dbo].[Menu];
INSERT INTO [dbo].[Menu] ([Id], [MenuText], [MenuURL], [ParentId], [MenuIcon], [SortOrder]) VALUES 
(@NextId, 'Mis Solicitudes', 'Creditos/MisSolicitudes', @ParentId, '<i class="fa fa-circle-o text-yellow"></i>', 2);

SELECT @NextId = ISNULL(MAX(Id), 0) + 1 FROM [dbo].[Menu];
INSERT INTO [dbo].[Menu] ([Id], [MenuText], [MenuURL], [ParentId], [MenuIcon], [SortOrder]) VALUES 
(@NextId, 'Publicar Oferta', 'Creditos/Publicar', @ParentId, '<i class="fa fa-circle-o text-green"></i>', 3);

SELECT @NextId = ISNULL(MAX(Id), 0) + 1 FROM [dbo].[Menu];
INSERT INTO [dbo].[Menu] ([Id], [MenuText], [MenuURL], [ParentId], [MenuIcon], [SortOrder]) VALUES 
(@NextId, 'Gestión Recibidas', 'Creditos/Gestion', @ParentId, '<i class="fa fa-circle-o text-red"></i>', 4);

-- 3. Permisos (Ejemplo para el Rol Administrador / Cooperativa con RoleId 1 y 2)
INSERT INTO [dbo].[MenuPermission] ([MenuId], [RoleId], [SortOrder], [IsCreate], [IsRead], [IsUpdate], [IsDelete])
SELECT Id, 1, 1, 1, 1, 1, 1 FROM [dbo].[Menu] WHERE ParentId = @ParentId OR Id = @ParentId;

GO
