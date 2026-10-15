using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using Entities.Entities.Procedures;
using FGA.Utility;

namespace FGA.Services
{
    /// <summary>
    /// Servicio de acceso a datos y reglas de negocio para el módulo de Alertas Financieras
    /// Desacopla la lógica ADO.NET, WCF y caché del controlador web AlertasController (SRP)
    /// </summary>
    public class AlertasDataService
    {
        private static string _cachedConnString = null;
        private static bool _tablasAseguradas = false;
        private static readonly object _lockTablas = new object();

        private static List<AlertaCuentaCatalogoItemDTO> _cacheCatalogoCuentas = null;
        private static readonly object _lockCatalogo = new object();
        private static DateTime _cacheCatalogoExpira = DateTime.MinValue;

        #region Conexión a Base de Datos

        /// <summary>
        /// Obtiene de forma segura la cadena de conexión a la base de datos sin credenciales quemadas
        /// </summary>
        public string ObtenerCadenaConexion()
        {
            if (!string.IsNullOrEmpty(_cachedConnString)) return _cachedConnString;

            try
            {
                var cs = System.Configuration.ConfigurationManager.ConnectionStrings["FGAConnection"];
                if (cs != null && !string.IsNullOrEmpty(cs.ConnectionString))
                {
                    _cachedConnString = cs.ConnectionString;
                    return _cachedConnString;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceWarning("AlertasDataService: Error al leer FGAConnection: " + ex.Message);
            }

            try
            {
                var csContent = System.Configuration.ConfigurationManager.ConnectionStrings["ContentConnectionString"];
                if (csContent != null && !string.IsNullOrEmpty(csContent.ConnectionString))
                {
                    _cachedConnString = csContent.ConnectionString;
                    return _cachedConnString;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceWarning("AlertasDataService: Error al leer ContentConnectionString: " + ex.Message);
            }

            try
            {
                string dbServiceConfig = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"..\DBService\Web.config");
                if (System.IO.File.Exists(dbServiceConfig))
                {
                    var doc = new System.Xml.XmlDocument();
                    doc.Load(dbServiceConfig);
                    var node = doc.SelectSingleNode("//connectionStrings/add[@name='ContentConnectionString']");
                    if (node != null && node.Attributes["connectionString"] != null)
                    {
                        _cachedConnString = node.Attributes["connectionString"].Value;
                        return _cachedConnString;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceWarning("AlertasDataService: Error al leer DBService Web.config: " + ex.Message);
            }

            throw new System.Configuration.ConfigurationErrorsException("No se encontró una cadena de conexión válida ('FGAConnection' o 'ContentConnectionString') configurada en el sistema.");
        }

        #endregion

        #region Garantía de Esquema (Idempotente)

        /// <summary>
        /// Verifica de forma segura e idempotente la existencia de las tablas de alertas personalizadas
        /// </summary>
        public void AsegurarTablasAlertasPersonalizadas(SqlConnection conn)
        {
            if (_tablasAseguradas) return;
            lock (_lockTablas)
            {
                if (_tablasAseguradas) return;
                try
                {
                    string sqlCheck = @"
                        IF OBJECT_ID('dbo.AlertaParametrosEntidad', 'U') IS NULL
                        BEGIN
                            CREATE TABLE dbo.AlertaParametrosEntidad (
                                Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                                IdEntidad NVARCHAR(5) NOT NULL UNIQUE,
                                UmbralVariacionPorc DECIMAL(18,2) NULL,
                                UmbralVariacionMonto DECIMAL(18,2) NULL,
                                ModoMonitoreo NVARCHAR(20) NOT NULL DEFAULT 'SOLO_WATCHLIST',
                                Activo BIT NOT NULL DEFAULT 1,
                                FechaModificacion DATETIME NOT NULL DEFAULT GETDATE(),
                                UsuarioModificacion NVARCHAR(100) NULL
                            );
                        END;

                        IF OBJECT_ID('dbo.AlertaCuentaMonitoreada', 'U') IS NULL
                        BEGIN
                            CREATE TABLE dbo.AlertaCuentaMonitoreada (
                                Id INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
                                IdEntidad NVARCHAR(5) NOT NULL,
                                Cuenta NVARCHAR(50) NOT NULL,
                                NombreCuenta NVARCHAR(200) NULL,
                                TipoRegla NVARCHAR(20) NOT NULL DEFAULT 'MONITOREAR',
                                UmbralPorcPersonalizado DECIMAL(18,2) NULL,
                                UmbralMontoPersonalizado DECIMAL(18,2) NULL,
                                Activo BIT NOT NULL DEFAULT 1,
                                Justificacion NVARCHAR(500) NULL,
                                FechaRegistro DATETIME NOT NULL DEFAULT GETDATE(),
                                UsuarioRegistro NVARCHAR(100) NULL,
                                CONSTRAINT UQ_AlertaCuenta_Entidad_Cuenta UNIQUE (IdEntidad, Cuenta)
                            );
                        END;";

                    using (var cmd = new SqlCommand(sqlCheck, conn))
                    {
                        cmd.CommandTimeout = 20;
                        cmd.ExecuteNonQuery();
                    }
                    _tablasAseguradas = true;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.TraceWarning("AlertasDataService: AsegurarTablas error: " + ex.Message);
                }
            }
        }

        #endregion

        #region Catálogo Contable con Caché en Memoria

        /// <summary>
        /// Obtiene el catálogo completo de cuentas contables desde caché en memoria o carga directa ultra rápida
        /// </summary>
        public List<AlertaCuentaCatalogoItemDTO> ObtenerCatalogoCuentas()
        {
            if (_cacheCatalogoCuentas != null && DateTime.UtcNow < _cacheCatalogoExpira)
            {
                return _cacheCatalogoCuentas;
            }

            lock (_lockCatalogo)
            {
                if (_cacheCatalogoCuentas != null && DateTime.UtcNow < _cacheCatalogoExpira)
                {
                    return _cacheCatalogoCuentas;
                }

                var lista = new List<AlertaCuentaCatalogoItemDTO>(12000);
                try
                {
                    string connStr = ObtenerCadenaConexion();
                    using (var conn = new SqlConnection(connStr))
                    {
                        conn.Open();
                        string sql = @"
                            SELECT 
                                CUENTA, 
                                NOMBRE, 
                                CASE 
                                    WHEN LEN(RTRIM(CUENTA)) <= 2 THEN 1
                                    WHEN LEN(RTRIM(CUENTA)) <= 4 THEN 2
                                    WHEN LEN(RTRIM(CUENTA)) <= 6 THEN 3
                                    ELSE 4
                                END AS NIVEL 
                            FROM dbo.CATALOGOCUENTA WITH(NOLOCK) 
                            ORDER BY CUENTA";

                        using (var cmd = new SqlCommand(sql, conn))
                        {
                            cmd.CommandTimeout = 15;
                            using (var reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    lista.Add(new AlertaCuentaCatalogoItemDTO
                                    {
                                        Cuenta = reader["CUENTA"].ToString().Trim(),
                                        Descripcion = reader["NOMBRE"] != DBNull.Value ? reader["NOMBRE"].ToString().Trim() : "",
                                        Nivel = Convert.ToInt32(reader["NIVEL"])
                                    });
                                }
                            }
                        }
                    }

                    if (lista.Count > 0)
                    {
                        _cacheCatalogoCuentas = lista;
                        _cacheCatalogoExpira = DateTime.UtcNow.AddHours(4);
                    }
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Trace.TraceWarning("AlertasDataService: Error al cargar catálogo de cuentas: " + ex.Message);
                    if (_cacheCatalogoCuentas != null) return _cacheCatalogoCuentas;
                }

                return lista;
            }
        }

        /// <summary>
        /// Realiza una búsqueda optimizada de cuentas en catálogo en memoria con fallback SQL
        /// </summary>
        public List<AlertaCuentaCatalogoItemDTO> BuscarCuentasCatalogo(string query)
        {
            string q = (query ?? "").Trim();
            if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
            {
                return new List<AlertaCuentaCatalogoItemDTO>();
            }

            var catalogo = ObtenerCatalogoCuentas();
            if (catalogo != null && catalogo.Count > 0)
            {
                bool esDigito = char.IsDigit(q[0]);

                if (esDigito)
                {
                    return catalogo
                        .Where(c => c.Cuenta != null && (c.Cuenta.StartsWith(q, StringComparison.OrdinalIgnoreCase) || c.Cuenta.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0))
                        .OrderBy(c => c.Cuenta.StartsWith(q, StringComparison.OrdinalIgnoreCase) ? 0 : 1)
                        .ThenBy(c => c.Cuenta)
                        .Take(30)
                        .ToList();
                }
                else
                {
                    return catalogo
                        .Where(c => (c.Descripcion != null && c.Descripcion.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0) ||
                                    (c.Cuenta != null && c.Cuenta.IndexOf(q, StringComparison.OrdinalIgnoreCase) >= 0))
                        .OrderBy(c => (c.Descripcion != null && c.Descripcion.StartsWith(q, StringComparison.OrdinalIgnoreCase)) ? 0 :
                                      (c.Cuenta != null && c.Cuenta.StartsWith(q, StringComparison.OrdinalIgnoreCase)) ? 1 : 2)
                        .ThenBy(c => c.Cuenta)
                        .Take(30)
                        .ToList();
                }
            }

            // Fallback directo SQL
            var resultado = new List<AlertaCuentaCatalogoItemDTO>();
            try
            {
                string connStr = ObtenerCadenaConexion();
                using (var conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    string sqlCatalog = @"
                        SELECT TOP 30 
                            CUENTA, 
                            NOMBRE, 
                            CASE 
                                WHEN LEN(RTRIM(CUENTA)) <= 2 THEN 1
                                WHEN LEN(RTRIM(CUENTA)) <= 4 THEN 2
                                WHEN LEN(RTRIM(CUENTA)) <= 6 THEN 3
                                ELSE 4
                            END AS NIVEL 
                        FROM dbo.CATALOGOCUENTA WITH(NOLOCK) 
                        WHERE CUENTA LIKE @qLike OR NOMBRE LIKE @qLike 
                        ORDER BY 
                            CASE WHEN CUENTA LIKE @qStart THEN 0 WHEN NOMBRE LIKE @qStart THEN 1 ELSE 2 END,
                            CUENTA";

                    using (var cmd = new SqlCommand(sqlCatalog, conn))
                    {
                        cmd.CommandTimeout = 15;
                        cmd.Parameters.AddWithValue("@qLike", "%" + q + "%");
                        cmd.Parameters.AddWithValue("@qStart", q + "%");
                        using (var reader = cmd.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                resultado.Add(new AlertaCuentaCatalogoItemDTO
                                {
                                    Cuenta = reader["CUENTA"].ToString().Trim(),
                                    Descripcion = reader["NOMBRE"] != DBNull.Value ? reader["NOMBRE"].ToString().Trim() : "",
                                    Nivel = Convert.ToInt32(reader["NIVEL"])
                                });
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceWarning("AlertasDataService: BuscarCuentasCatalogo fallback SQL error: " + ex.Message);
            }

            return resultado;
        }

        #endregion

        #region Umbrales y Configuración por Entidad

        /// <summary>
        /// Obtiene la cantidad de cuentas actualmente registradas para monitoreo activo
        /// </summary>
        public int ObtenerCantidadCuentasMonitoreadas(string idEntidad)
        {
            if (FGAConstants.Entidades.EsFiltroTodas(idEntidad)) return 0;
            try
            {
                string connStr = ObtenerCadenaConexion();
                using (var conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand("SELECT COUNT(1) FROM dbo.AlertaCuentaMonitoreada WITH(NOLOCK) WHERE IdEntidad = @idEntidad AND Activo = 1 AND TipoRegla = @tipoRegla", conn))
                    {
                        cmd.Parameters.AddWithValue("@idEntidad", idEntidad);
                        cmd.Parameters.AddWithValue("@tipoRegla", FGAConstants.Alertas.ReglaMonitorear);
                        var obj = cmd.ExecuteScalar();
                        return obj != null && obj != DBNull.Value ? Convert.ToInt32(obj) : 0;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceWarning("AlertasDataService: Error al obtener cantidad de cuentas monitoreadas: " + ex.Message);
                return 0;
            }
        }

        /// <summary>
        /// Obtiene los umbrales globales configurados en la tabla de parámetros
        /// </summary>
        public void ObtenerUmbralesConfigurados(out decimal globalPorc, out decimal globalMonto)
        {
            globalPorc = FGAConstants.Alertas.UmbralVariacionPorcGlobal;
            globalMonto = FGAConstants.Alertas.UmbralVariacionMontoGlobal;

            try
            {
                var paramClient = new FGA_En_Linea.ParametrosService.ParametrosServiceClient();
                try
                {
                    var lista = paramClient.GetAll();
                    if (lista != null)
                    {
                        var pPorc = lista.FirstOrDefault(p => p.Llave != null && (
                            p.Llave.Trim().Equals(FGAConstants.Alertas.ParametroPorc, StringComparison.OrdinalIgnoreCase) ||
                            p.Llave.Trim().Equals("ALERTA_PORCENTAJE", StringComparison.OrdinalIgnoreCase) ||
                            p.Llave.Trim().Equals("ALERTA_PORC", StringComparison.OrdinalIgnoreCase) ||
                            (p.Llave.IndexOf("ALERTA", StringComparison.OrdinalIgnoreCase) >= 0 && p.Llave.IndexOf("PORC", StringComparison.OrdinalIgnoreCase) >= 0)
                        ));
                        if (pPorc != null && !string.IsNullOrWhiteSpace(pPorc.Valor))
                        {
                            if (decimal.TryParse(pPorc.Valor.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal valP))
                                globalPorc = valP;
                        }

                        var pMonto = lista.FirstOrDefault(p => p.Llave != null && (
                            p.Llave.Trim().Equals(FGAConstants.Alertas.ParametroMonto, StringComparison.OrdinalIgnoreCase) ||
                            p.Llave.Trim().Equals("ALERTA_MONTO", StringComparison.OrdinalIgnoreCase) ||
                            p.Llave.Trim().Equals("MONTO_ALERTA", StringComparison.OrdinalIgnoreCase) ||
                            (p.Llave.IndexOf("ALERTA", StringComparison.OrdinalIgnoreCase) >= 0 && p.Llave.IndexOf("MONTO", StringComparison.OrdinalIgnoreCase) >= 0)
                        ));
                        if (pMonto != null && !string.IsNullOrWhiteSpace(pMonto.Valor))
                        {
                            if (decimal.TryParse(pMonto.Valor.Replace(',', '.'), System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out decimal valM))
                                globalMonto = valM;
                        }
                    }
                }
                finally
                {
                    paramClient.SafeClose();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceWarning("AlertasDataService: Error al leer umbrales globales: " + ex.Message);
            }
        }

        /// <summary>
        /// Obtiene los umbrales efectivos aplicables a una entidad (propios o globales)
        /// </summary>
        public void ObtenerUmbralesEfectivos(
            string idEntidad,
            out decimal globalPorc,
            out decimal globalMonto,
            out decimal umbralPorc,
            out decimal umbralMonto,
            out string modoMonitoreo,
            out bool esPersonalizado,
            out DateTime? fechaModif,
            out string usuarioModif)
        {
            ObtenerUmbralesConfigurados(out globalPorc, out globalMonto);
            umbralPorc = globalPorc;
            umbralMonto = globalMonto;
            modoMonitoreo = FGAConstants.Alertas.ModoDefecto;
            esPersonalizado = false;
            fechaModif = null;
            usuarioModif = null;

            if (string.IsNullOrEmpty(idEntidad) || idEntidad == FGAConstants.Entidades.Todas)
            {
                return;
            }

            // 1. Intentar obtener configuración mediante WCF SPClient
            bool obtenidoWcf = false;
            try
            {
                var spClient = new FGA_En_Linea.SPService.SPClient();
                try
                {
                    var cfg = spClient.FGA_Obtener_Configuracion_Alerta_Entidad(idEntidad);
                    if (cfg != null && cfg.Length > 0)
                    {
                        var item = cfg[0];
                        umbralPorc = item.UmbralVariacionPorc;
                        umbralMonto = item.UmbralVariacionMonto;
                        modoMonitoreo = string.IsNullOrEmpty(item.ModoMonitoreo) ? FGAConstants.Alertas.ModoDefecto : item.ModoMonitoreo;
                        esPersonalizado = item.EsPersonalizado;
                        fechaModif = item.FechaModificacion;
                        usuarioModif = item.UsuarioModificacion;
                        obtenidoWcf = true;
                    }
                }
                finally
                {
                    spClient.SafeClose();
                }
            }
            catch (Exception exWcf)
            {
                obtenidoWcf = false;
                System.Diagnostics.Trace.TraceWarning("AlertasDataService: ObtenerUmbrales WCF error: " + exWcf.Message);
            }

            if (obtenidoWcf) return;

            // 2. Fallback directo ADO.NET
            try
            {
                string connStr = ObtenerCadenaConexion();
                if (!string.IsNullOrEmpty(connStr))
                {
                    using (var conn = new SqlConnection(connStr))
                    {
                        conn.Open();
                        AsegurarTablasAlertasPersonalizadas(conn);

                        bool spExitoso = false;
                        try
                        {
                            using (var cmd = new SqlCommand("[dbo].[FGA_Obtener_Configuracion_Alerta_Entidad]", conn))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.AddWithValue("@IDENTIDAD", idEntidad);

                                using (var r = cmd.ExecuteReader())
                                {
                                    if (r.Read())
                                    {
                                        if (r["UmbralVariacionPorc"] != DBNull.Value)
                                            umbralPorc = Convert.ToDecimal(r["UmbralVariacionPorc"]);
                                        if (r["UmbralVariacionMonto"] != DBNull.Value)
                                            umbralMonto = Convert.ToDecimal(r["UmbralVariacionMonto"]);
                                        if (r["ModoMonitoreo"] != DBNull.Value && !string.IsNullOrEmpty(r["ModoMonitoreo"].ToString()))
                                            modoMonitoreo = r["ModoMonitoreo"].ToString();
                                        if (r["EsPersonalizado"] != DBNull.Value)
                                            esPersonalizado = Convert.ToBoolean(r["EsPersonalizado"]);
                                        if (r["FechaModificacion"] != DBNull.Value)
                                            fechaModif = Convert.ToDateTime(r["FechaModificacion"]);
                                        if (r["UsuarioModificacion"] != DBNull.Value)
                                            usuarioModif = r["UsuarioModificacion"].ToString();
                                    }
                                }
                                spExitoso = true;
                            }
                        }
                        catch (Exception exSp)
                        {
                            spExitoso = false;
                            System.Diagnostics.Trace.TraceWarning("AlertasDataService: ObtenerUmbrales SP error: " + exSp.Message);
                        }

                        if (!spExitoso)
                        {
                            try
                            {
                                using (var cmdDirect = new SqlCommand(@"
                                    SELECT 
                                        UmbralVariacionPorc, 
                                        UmbralVariacionMonto, 
                                        ModoMonitoreo, 
                                        Activo, 
                                        FechaModificacion, 
                                        UsuarioModificacion 
                                    FROM dbo.AlertaParametrosEntidad WITH(NOLOCK) 
                                    WHERE IdEntidad = @idEntidad AND Activo = 1", conn))
                                {
                                    cmdDirect.Parameters.AddWithValue("@idEntidad", idEntidad);
                                    using (var r = cmdDirect.ExecuteReader())
                                    {
                                        if (r.Read())
                                        {
                                            if (r["UmbralVariacionPorc"] != DBNull.Value)
                                                umbralPorc = Convert.ToDecimal(r["UmbralVariacionPorc"]);
                                            if (r["UmbralVariacionMonto"] != DBNull.Value)
                                                umbralMonto = Convert.ToDecimal(r["UmbralVariacionMonto"]);
                                            if (r["ModoMonitoreo"] != DBNull.Value && !string.IsNullOrEmpty(r["ModoMonitoreo"].ToString()))
                                                modoMonitoreo = r["ModoMonitoreo"].ToString();
                                            esPersonalizado = true;
                                            if (r["FechaModificacion"] != DBNull.Value)
                                                fechaModif = Convert.ToDateTime(r["FechaModificacion"]);
                                            if (r["UsuarioModificacion"] != DBNull.Value)
                                                usuarioModif = r["UsuarioModificacion"].ToString();
                                        }
                                    }
                                }
                            }
                            catch (Exception exDirect)
                            {
                                System.Diagnostics.Trace.TraceWarning("AlertasDataService: ObtenerUmbrales direct SQL error: " + exDirect.Message);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceWarning("AlertasDataService: ObtenerUmbralesEfectivos error general: " + ex.Message);
            }
        }

        #endregion

        #region Consulta y Mantenimiento de Cuentas Monitoreadas

        /// <summary>
        /// Obtiene la lista de cuentas monitoreadas o excluidas para la entidad especificada
        /// </summary>
        public List<AlertaCuentaMonitoreadaDTO> ObtenerCuentasMonitoreadas(string idEntidad)
        {
            var lista = new List<AlertaCuentaMonitoreadaDTO>();

            // 1. Intentar mediante WCF SPClient
            bool obtenido = false;
            try
            {
                var spClient = new FGA_En_Linea.SPService.SPClient();
                try
                {
                    var cuentasSp = spClient.FGA_Obtener_Cuentas_Monitoreadas_Entidad(idEntidad, true);
                    if (cuentasSp != null)
                    {
                        foreach (var c in cuentasSp)
                        {
                            lista.Add(new AlertaCuentaMonitoreadaDTO
                            {
                                Id = c.Id,
                                IdEntidad = c.IdEntidad,
                                NombreEntidad = c.NombreEntidad,
                                CuentaContable = c.Cuenta,
                                NombreCuenta = c.NombreCuenta ?? "",
                                Nivel = c.Nivel,
                                TipoMonitoreo = (c.TipoRegla == FGAConstants.Alertas.ReglaIgnorar || c.TipoRegla == FGAConstants.Alertas.ReglaBlacklist) ? FGAConstants.Alertas.ReglaBlacklist : FGAConstants.Alertas.ReglaWatchlist,
                                UmbralPorcPersonalizado = c.UmbralPorcPersonalizado,
                                UmbralMontoPersonalizado = c.UmbralMontoPersonalizado,
                                Activo = c.Activo,
                                FechaRegistro = c.FechaRegistro.ToString("dd/MM/yyyy"),
                                UsuarioRegistro = c.UsuarioRegistro
                            });
                        }
                        obtenido = true;
                    }
                }
                finally
                {
                    spClient.SafeClose();
                }
            }
            catch (Exception exWcf)
            {
                obtenido = false;
                System.Diagnostics.Trace.TraceWarning("AlertasDataService: ObtenerCuentasMonitoreadas WCF error: " + exWcf.Message);
            }

            if (obtenido) return lista;

            // 2. Fallback directo ADO.NET si WCF no estuviera disponible
            try
            {
                string connStr = ObtenerCadenaConexion();
                if (!string.IsNullOrEmpty(connStr))
                {
                    using (var conn = new SqlConnection(connStr))
                    {
                        conn.Open();
                        AsegurarTablasAlertasPersonalizadas(conn);

                        bool spExitoso = false;
                        try
                        {
                            using (var cmd = new SqlCommand("[dbo].[FGA_Obtener_Cuentas_Monitoreadas_Entidad]", conn))
                            {
                                cmd.CommandType = CommandType.StoredProcedure;
                                cmd.Parameters.AddWithValue("@IDENTIDAD", idEntidad);
                                cmd.Parameters.AddWithValue("@SOLO_ACTIVAS", true);

                                using (var r = cmd.ExecuteReader())
                                {
                                    while (r.Read())
                                    {
                                        lista.Add(new AlertaCuentaMonitoreadaDTO
                                        {
                                            Id = Convert.ToInt32(r["Id"]),
                                            IdEntidad = r["IdEntidad"].ToString(),
                                            NombreEntidad = r["NombreEntidad"] != DBNull.Value ? r["NombreEntidad"].ToString() : "",
                                            CuentaContable = r["Cuenta"].ToString(),
                                            NombreCuenta = r["NombreCuenta"] != DBNull.Value ? r["NombreCuenta"].ToString() : "",
                                            Nivel = r["Nivel"] != DBNull.Value ? Convert.ToInt32(r["Nivel"]) : 3,
                                            TipoMonitoreo = (r["TipoRegla"].ToString() == FGAConstants.Alertas.ReglaIgnorar || r["TipoRegla"].ToString() == FGAConstants.Alertas.ReglaBlacklist) ? FGAConstants.Alertas.ReglaBlacklist : FGAConstants.Alertas.ReglaWatchlist,
                                            UmbralPorcPersonalizado = r["UmbralPorcPersonalizado"] != DBNull.Value ? Convert.ToDecimal(r["UmbralPorcPersonalizado"]) : (decimal?)null,
                                            UmbralMontoPersonalizado = r["UmbralMontoPersonalizado"] != DBNull.Value ? Convert.ToDecimal(r["UmbralMontoPersonalizado"]) : (decimal?)null,
                                            Activo = r["Activo"] != DBNull.Value && Convert.ToBoolean(r["Activo"]),
                                            FechaRegistro = r["FechaRegistro"] != DBNull.Value ? Convert.ToDateTime(r["FechaRegistro"]).ToString("dd/MM/yyyy") : "",
                                            UsuarioRegistro = r["UsuarioRegistro"] != DBNull.Value ? r["UsuarioRegistro"].ToString() : ""
                                        });
                                    }
                                    spExitoso = true;
                                }
                            }
                        }
                        catch (Exception exSp)
                        {
                            spExitoso = false;
                            System.Diagnostics.Trace.TraceWarning("AlertasDataService: ObtenerCuentasMonitoreadas SP error: " + exSp.Message);
                        }

                        if (!spExitoso)
                        {
                            try
                            {
                                string directSql = @"
                                    SELECT 
                                        acm.Id,
                                        acm.IdEntidad,
                                        ISNULL(e.Nombre, 'Entidad ' + acm.IdEntidad) AS NombreEntidad,
                                        acm.Cuenta,
                                        ISNULL(acm.NombreCuenta, ISNULL(c.NOMBRE, 'Cuenta ' + acm.Cuenta)) AS NombreCuenta,
                                        ISNULL(c.NIVEL, 3) AS Nivel,
                                        acm.TipoRegla,
                                        acm.UmbralPorcPersonalizado,
                                        acm.UmbralMontoPersonalizado,
                                        acm.Activo,
                                        acm.FechaRegistro,
                                        acm.UsuarioRegistro
                                    FROM dbo.AlertaCuentaMonitoreada acm WITH(NOLOCK)
                                    LEFT JOIN dbo.CATALOGOCUENTA c WITH(NOLOCK) ON acm.Cuenta = c.CUENTA
                                    LEFT JOIN dbo.Entidad e WITH(NOLOCK) ON acm.IdEntidad = e.Id
                                    WHERE (@idEntidad = '-1' OR acm.IdEntidad = @idEntidad)
                                      AND acm.Activo = 1
                                    ORDER BY acm.Cuenta";

                                using (var cmd = new SqlCommand(directSql, conn))
                                {
                                    cmd.Parameters.AddWithValue("@idEntidad", idEntidad);
                                    using (var r = cmd.ExecuteReader())
                                    {
                                        while (r.Read())
                                        {
                                            lista.Add(new AlertaCuentaMonitoreadaDTO
                                            {
                                                Id = Convert.ToInt32(r["Id"]),
                                                IdEntidad = r["IdEntidad"].ToString(),
                                                NombreEntidad = r["NombreEntidad"] != DBNull.Value ? r["NombreEntidad"].ToString() : "",
                                                CuentaContable = r["Cuenta"].ToString(),
                                                NombreCuenta = r["NombreCuenta"] != DBNull.Value ? r["NombreCuenta"].ToString() : "",
                                                Nivel = r["Nivel"] != DBNull.Value ? Convert.ToInt32(r["Nivel"]) : 3,
                                                TipoMonitoreo = (r["TipoRegla"].ToString() == FGAConstants.Alertas.ReglaIgnorar || r["TipoRegla"].ToString() == FGAConstants.Alertas.ReglaBlacklist) ? FGAConstants.Alertas.ReglaBlacklist : FGAConstants.Alertas.ReglaWatchlist,
                                                UmbralPorcPersonalizado = r["UmbralPorcPersonalizado"] != DBNull.Value ? Convert.ToDecimal(r["UmbralPorcPersonalizado"]) : (decimal?)null,
                                                UmbralMontoPersonalizado = r["UmbralMontoPersonalizado"] != DBNull.Value ? Convert.ToDecimal(r["UmbralMontoPersonalizado"]) : (decimal?)null,
                                                Activo = r["Activo"] != DBNull.Value && Convert.ToBoolean(r["Activo"]),
                                                FechaRegistro = r["FechaRegistro"] != DBNull.Value ? Convert.ToDateTime(r["FechaRegistro"]).ToString("dd/MM/yyyy") : "",
                                                UsuarioRegistro = r["UsuarioRegistro"] != DBNull.Value ? r["UsuarioRegistro"].ToString() : ""
                                            });
                                        }
                                    }
                                }
                            }
                            catch (Exception exDirect)
                            {
                                System.Diagnostics.Trace.TraceWarning("AlertasDataService: ObtenerCuentasMonitoreadas direct SQL error: " + exDirect.Message);
                            }
                        }
                    }
                }
            }
            catch (Exception exConn)
            {
                System.Diagnostics.Trace.TraceWarning("AlertasDataService: ObtenerCuentasMonitoreadas conn error: " + exConn.Message);
            }

            return lista;
        }

        #endregion

        #region Persistencia de Configuración y Cuentas Monitoreadas

        /// <summary>
        /// Guarda o actualiza la configuración de umbrales y modo de monitoreo para una entidad
        /// </summary>
        public bool GuardarConfiguracionEntidad(string idEntidad, decimal? porcFinal, decimal? montoFinal, string modoFinal, bool esActivo, string usuario)
        {
            // 1. Intentar mediante WCF SPClient
            try
            {
                var spClient = new FGA_En_Linea.SPService.SPClient();
                try
                {
                    var resSp = spClient.FGA_Guardar_Configuracion_Alerta_Entidad(idEntidad, porcFinal, montoFinal, modoFinal, esActivo, usuario);
                    if (resSp != null) return true;
                }
                finally
                {
                    spClient.SafeClose();
                }
            }
            catch (Exception exWcf)
            {
                System.Diagnostics.Trace.TraceWarning("AlertasDataService: GuardarConfiguracion WCF error: " + exWcf.Message);
            }

            // 2. Fallback directo ADO.NET
            try
            {
                string connStr = ObtenerCadenaConexion();
                using (var conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    AsegurarTablasAlertasPersonalizadas(conn);

                    bool spExitoso = false;
                    try
                    {
                        using (var cmd = new SqlCommand("[dbo].[FGA_Guardar_Configuracion_Alerta_Entidad]", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@IDENTIDAD", idEntidad);
                            cmd.Parameters.AddWithValue("@UMBRAL_PORC", (object)porcFinal ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@UMBRAL_MONTO", (object)montoFinal ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@MODO_MONITOREO", modoFinal);
                            cmd.Parameters.AddWithValue("@ACTIVO", esActivo);
                            cmd.Parameters.AddWithValue("@USUARIO", usuario);

                            cmd.ExecuteNonQuery();
                            spExitoso = true;
                        }
                    }
                    catch (Exception exSp)
                    {
                        spExitoso = false;
                        System.Diagnostics.Trace.TraceWarning("AlertasDataService: GuardarConfiguracion SP error: " + exSp.Message);
                    }

                    if (!spExitoso)
                    {
                        string upsertSql = @"
                            IF EXISTS (SELECT 1 FROM dbo.AlertaParametrosEntidad WHERE IdEntidad = @idEntidad)
                            BEGIN
                                UPDATE dbo.AlertaParametrosEntidad
                                SET UmbralVariacionPorc = @umbralPorc,
                                    UmbralVariacionMonto = @umbralMonto,
                                    ModoMonitoreo = @modoMonitoreo,
                                    Activo = @activo,
                                    FechaModificacion = GETDATE(),
                                    UsuarioModificacion = @usuario
                                WHERE IdEntidad = @idEntidad;
                            END
                            ELSE
                            BEGIN
                                INSERT INTO dbo.AlertaParametrosEntidad (IdEntidad, UmbralVariacionPorc, UmbralVariacionMonto, ModoMonitoreo, Activo, FechaModificacion, UsuarioModificacion)
                                VALUES (@idEntidad, @umbralPorc, @umbralMonto, @modoMonitoreo, @activo, GETDATE(), @usuario);
                            END";

                        using (var cmd = new SqlCommand(upsertSql, conn))
                        {
                            cmd.Parameters.AddWithValue("@idEntidad", idEntidad);
                            cmd.Parameters.AddWithValue("@umbralPorc", (object)porcFinal ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@umbralMonto", (object)montoFinal ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@modoMonitoreo", modoFinal);
                            cmd.Parameters.AddWithValue("@activo", esActivo);
                            cmd.Parameters.AddWithValue("@usuario", usuario);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceWarning("AlertasDataService: GuardarConfiguracion error general: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Guarda o actualiza una cuenta contable en la lista de monitoreo o exclusión
        /// </summary>
        public bool GuardarCuentaMonitoreada(string idEntidad, string codCuenta, string nombreCuenta, string regla, decimal? umbralPorc, decimal? umbralMonto, string usuario)
        {
            // 1. Intentar mediante WCF SPClient
            try
            {
                var spClient = new FGA_En_Linea.SPService.SPClient();
                try
                {
                    var res = spClient.FGA_Guardar_Cuenta_Monitoreada_Entidad(idEntidad, codCuenta, nombreCuenta, regla, umbralPorc, umbralMonto, usuario);
                    if (res != null) return true;
                }
                finally
                {
                    spClient.SafeClose();
                }
            }
            catch (Exception exWcf)
            {
                System.Diagnostics.Trace.TraceWarning("AlertasDataService: GuardarCuentaMonitoreada WCF error: " + exWcf.Message);
            }

            // 2. Fallback directo ADO.NET
            try
            {
                string connStr = ObtenerCadenaConexion();
                using (var conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    AsegurarTablasAlertasPersonalizadas(conn);

                    bool spExitoso = false;
                    try
                    {
                        using (var cmd = new SqlCommand("[dbo].[FGA_Guardar_Cuenta_Monitoreada_Entidad]", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@IDENTIDAD", idEntidad);
                            cmd.Parameters.AddWithValue("@CUENTA", codCuenta);
                            cmd.Parameters.AddWithValue("@NOMBRE_CUENTA", (object)nombreCuenta ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@TIPO_REGLA", regla);
                            cmd.Parameters.AddWithValue("@UMBRAL_PORC", (object)umbralPorc ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@UMBRAL_MONTO", (object)umbralMonto ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@USUARIO", usuario);

                            cmd.ExecuteNonQuery();
                            spExitoso = true;
                        }
                    }
                    catch (Exception exSp)
                    {
                        spExitoso = false;
                        System.Diagnostics.Trace.TraceWarning("AlertasDataService: GuardarCuentaMonitoreada SP error: " + exSp.Message);
                    }

                    if (!spExitoso)
                    {
                        string upsertSql = @"
                            IF EXISTS (SELECT 1 FROM dbo.AlertaCuentaMonitoreada WHERE IdEntidad = @idEntidad AND Cuenta = @cuenta)
                            BEGIN
                                UPDATE dbo.AlertaCuentaMonitoreada
                                SET NombreCuenta = @nombre,
                                    TipoRegla = @tipo,
                                    UmbralPorcPersonalizado = @umbralPorc,
                                    UmbralMontoPersonalizado = @umbralMonto,
                                    Activo = 1,
                                    FechaRegistro = GETDATE(),
                                    UsuarioRegistro = @usuario
                                WHERE IdEntidad = @idEntidad AND Cuenta = @cuenta;
                            END
                            ELSE
                            BEGIN
                                INSERT INTO dbo.AlertaCuentaMonitoreada (IdEntidad, Cuenta, NombreCuenta, TipoRegla, UmbralPorcPersonalizado, UmbralMontoPersonalizado, Activo, FechaRegistro, UsuarioRegistro)
                                VALUES (@idEntidad, @cuenta, @nombre, @tipo, @umbralPorc, @umbralMonto, 1, GETDATE(), @usuario);
                            END";

                        using (var cmd = new SqlCommand(upsertSql, conn))
                        {
                            cmd.Parameters.AddWithValue("@idEntidad", idEntidad);
                            cmd.Parameters.AddWithValue("@cuenta", codCuenta);
                            cmd.Parameters.AddWithValue("@nombre", (object)nombreCuenta ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@tipo", regla);
                            cmd.Parameters.AddWithValue("@umbralPorc", (object)umbralPorc ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@umbralMonto", (object)umbralMonto ?? DBNull.Value);
                            cmd.Parameters.AddWithValue("@usuario", usuario);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceWarning("AlertasDataService: GuardarCuentaMonitoreada error general: " + ex.Message);
                return false;
            }
        }

        /// <summary>
        /// Desactiva o retira una cuenta monitoreada para una entidad
        /// </summary>
        public bool EliminarCuentaMonitoreada(int id, string idEntidad, string usuario)
        {
            // 1. Intentar mediante WCF SPClient
            try
            {
                var spClient = new FGA_En_Linea.SPService.SPClient();
                try
                {
                    spClient.FGA_Eliminar_Cuenta_Monitoreada_Entidad(id, idEntidad, usuario);
                    return true;
                }
                finally
                {
                    spClient.SafeClose();
                }
            }
            catch (Exception exWcf)
            {
                System.Diagnostics.Trace.TraceWarning("AlertasDataService: EliminarCuentaMonitoreada WCF error: " + exWcf.Message);
            }

            // 2. Fallback directo ADO.NET
            try
            {
                string connStr = ObtenerCadenaConexion();
                using (var conn = new SqlConnection(connStr))
                {
                    conn.Open();
                    AsegurarTablasAlertasPersonalizadas(conn);

                    bool spExitoso = false;
                    try
                    {
                        using (var cmd = new SqlCommand("[dbo].[FGA_Eliminar_Cuenta_Monitoreada_Entidad]", conn))
                        {
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.Parameters.AddWithValue("@ID", id);
                            cmd.Parameters.AddWithValue("@IDENTIDAD", idEntidad);
                            cmd.Parameters.AddWithValue("@USUARIO", usuario);

                            cmd.ExecuteNonQuery();
                            spExitoso = true;
                        }
                    }
                    catch (Exception exSp)
                    {
                        spExitoso = false;
                        System.Diagnostics.Trace.TraceWarning("AlertasDataService: EliminarCuentaMonitoreada SP error: " + exSp.Message);
                    }

                    if (!spExitoso)
                    {
                        using (var cmd = new SqlCommand("UPDATE dbo.AlertaCuentaMonitoreada SET Activo = 0 WHERE Id = @id AND (@idEntidad = '-1' OR IdEntidad = @idEntidad)", conn))
                        {
                            cmd.Parameters.AddWithValue("@id", id);
                            cmd.Parameters.AddWithValue("@idEntidad", idEntidad);
                            cmd.ExecuteNonQuery();
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.TraceWarning("AlertasDataService: EliminarCuentaMonitoreada error general: " + ex.Message);
                return false;
            }
        }

        #endregion
    }
}
