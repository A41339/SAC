-- =====================================================================================
-- Script 30: Registrar parámetros para el Asistente Virtual Inteligente (Chatbot Copilot)
-- Base de Datos: FGA
-- =====================================================================================
USE [FGA];
GO

-- 1. Parámetro global para habilitar o apagar el Chatbot en caliente sin reiniciar IIS
IF NOT EXISTS (SELECT 1 FROM [dbo].[Parametros] WHERE [Llave] = 'CHATBOT_HABILITADO')
BEGIN
    INSERT INTO [dbo].[Parametros] ([Llave], [Valor], [Descripcion])
    VALUES ('CHATBOT_HABILITADO', '1', 'Habilita (1) o deshabilita (0) el Asistente Virtual Inteligente Copilot');
    PRINT 'Parámetro CHATBOT_HABILITADO insertado correctamente.';
END
ELSE
BEGIN
    PRINT 'Parámetro CHATBOT_HABILITADO ya existe.';
END
GO

-- 2. Proveedor de Inteligencia Artificial configurado (GEMINI, LOCAL, SIMULADO)
IF NOT EXISTS (SELECT 1 FROM [dbo].[Parametros] WHERE [Llave] = 'CHATBOT_PROVEEDOR')
BEGIN
    INSERT INTO [dbo].[Parametros] ([Llave], [Valor], [Descripcion])
    VALUES ('CHATBOT_PROVEEDOR', 'GEMINI', 'Proveedor de IA activo (GEMINI, SIMULADO)');
    PRINT 'Parámetro CHATBOT_PROVEEDOR insertado correctamente.';
END
ELSE
BEGIN
    PRINT 'Parámetro CHATBOT_PROVEEDOR ya existe.';
END
GO

-- 3. Roles permitidos (ALL para todos los usuarios con sesión, o IDs separados por coma)
IF NOT EXISTS (SELECT 1 FROM [dbo].[Parametros] WHERE [Llave] = 'CHATBOT_ROLES_PERMITIDOS')
BEGIN
    INSERT INTO [dbo].[Parametros] ([Llave], [Valor], [Descripcion])
    VALUES ('CHATBOT_ROLES_PERMITIDOS', 'ALL', 'Roles autorizados para usar el Asistente (ALL o ej. 1,2,4,5)');
    PRINT 'Parámetro CHATBOT_ROLES_PERMITIDOS insertado correctamente.';
END
ELSE
BEGIN
    PRINT 'Parámetro CHATBOT_ROLES_PERMITIDOS ya existe.';
END
GO
