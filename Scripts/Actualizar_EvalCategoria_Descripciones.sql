-- ============================================================
-- Actualizacion de descripciones en dbo.EvalCategoria
-- Ejecutar en la base de datos FGA (VSRV-SQL-PROD)
-- Fecha: 2026-09-17
-- ============================================================

USE FGA
GO

-- Verificar valores actuales antes de actualizar
SELECT Id, Titulo, Descripcion
FROM dbo.EvalCategoria
WHERE Id IN (1, 8, 9, 10, 11)
ORDER BY Id;

GO

-- Id 1: Calidad de Gobierno Corporativo
UPDATE dbo.EvalCategoria
SET Descripcion = N'Evalúa la estructura, funcionamiento y prácticas de gobierno de la entidad, incluyendo la conformación y desempeño de los órganos de dirección, la gestión de conflictos de interés, la transparencia y la rendición de cuentas.'
WHERE Id = 1;

-- Id 8: Calidad de la Gestion de Riesgos
UPDATE dbo.EvalCategoria
SET Descripcion = N'Analiza la estructura y efectividad del marco de gestión de riesgos, incluyendo la identificación, medición, monitoreo, control y mitigación de los riesgos, así como la suficiencia de políticas, procesos y recursos.'
WHERE Id = 8;

-- Id 9: Evaluacion de la Situacion Economica Financiera
UPDATE dbo.EvalCategoria
SET Descripcion = N'Evalúa la sostenibilidad financiera de la entidad, considerando la calidad de los activos, la rentabilidad, la liquidez, la adecuación de capital y la capacidad para hacer frente a sus obligaciones.'
WHERE Id = 9;

-- Id 10: Calidad del Ambiente de Cumplimiento Legal y Regulatorio
UPDATE dbo.EvalCategoria
SET Descripcion = N'Analiza el grado de cumplimiento del marco normativo aplicable, incluyendo leyes, reglamentos, normas y disposiciones emitidas por el supervisor, así como la efectividad de los sistemas de control para garantizar su observancia.'
WHERE Id = 10;

-- Id 11: Nivel y la Calidad del Capital Base - Suficiencia Patrimonial
UPDATE dbo.EvalCategoria
SET Descripcion = N'Evalúa la suficiencia del capital en relación con los riesgos asumidos, la calidad de los fondos propios y la capacidad de la entidad para mantener niveles de capital acordes con su perfil de riesgos.'
WHERE Id = 11;

GO

-- Verificar resultado
SELECT Id, Titulo, Descripcion
FROM dbo.EvalCategoria
WHERE Id IN (1, 8, 9, 10, 11)
ORDER BY Id;
