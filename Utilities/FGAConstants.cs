using System;
using System.Globalization;

namespace FGA.Utility
{
    /// <summary>
    /// Constantes globales del sistema FFC / SAC
    /// Centraliza roles, entidades especiales, formatos, culturas, parámetros y llaves de sesión
    /// </summary>
    public static class FGAConstants
    {
        #region Culturas y Formatos
        public static readonly CultureInfo CulturaCR = new CultureInfo("es-CR");

        public static class Formatos
        {
            public const string Moneda = "#,##0.00";
            public const string MonedaEntero = "#,##0";
            public const string Porcentaje = "N1";
            public const string PorcentajeDosDecimales = "N2";
            public const string FechaCorta = "dd/MM/yyyy";
            public const string FechaIso = "yyyy-MM-dd";
            public const string PeriodoMesAno = "MM/yyyy";
            public const string PeriodoAnoMes = "yyyyMM";
            public const string PrefijoColones = "₡ ";
            public const string PrefijoDolares = "$ ";
        }
        #endregion

        #region Roles de Usuario
        public static class Roles
        {
            public const int SuperAdmin = 1;
            public const int ConsultaAuditor = 4;
            public const int AdminEntidad = 5;
            public const int DirectorFGA = 6;
            public const int Gerencial = 7;
            public const int MaestroEntidad = 10;

            public static bool EsAdministrador(int roleId)
            {
                return roleId == SuperAdmin || roleId == DirectorFGA || roleId == Gerencial;
            }

            public static bool EsSoloConsulta(int roleId)
            {
                return roleId == ConsultaAuditor;
            }
        }
        #endregion

        #region Entidades Especiales
        public static class Entidades
        {
            public const string Todas = "-1";
            public const string Administradora = "0";
            public const string Comodin = "99";
            public const string NombreAdministradora = "FFC";
            public const string TextoTodasCooperativas = "TODAS LAS COOPERATIVAS";
            public const string TextoTodasEntidades = "TODAS LAS ENTIDADES";

            public static bool EsFiltroTodas(string entidadId)
            {
                return string.IsNullOrEmpty(entidadId) || entidadId == Todas || entidadId == "0";
            }
        }
        #endregion

        #region Alertas Financieras
        public static class Alertas
        {
            public const decimal UmbralVariacionPorcGlobal = 15.0m;
            public const decimal UmbralVariacionMontoGlobal = 10000000.0m;

            public const string ModoTodas = "TODAS";
            public const string ModoSoloWatchlist = "SOLO_WATCHLIST";
            public const string ModoExcluirBlacklist = "EXCLUIR_BLACKLIST";

            public const string SeveridadCritica = "CRITICA";
            public const string SeveridadAdvertencia = "ADVERTENCIA";
            public const string SeveridadTodas = "TODAS";

            public const string ModalidadAcumulado = "Acumulado";
            public const string ModalidadMensual = "Mensual";

            public const string ComparacionInteranual = "Interanual";
            public const string ComparacionMesAnterior = "MesAnterior";
            public const string ComparacionDiciembreAnterior = "DiciembreAnterior";
        }
        #endregion

        #region Llaves de Sesión y Contexto
        public static class Sesion
        {
            public const string IdEntidad = "IdEntidad";
            public const string RoleId = "roleid";
            public const string UserId = "userid";
            public const string Usuario = "username";
            public const string Nombre = "name";
            public const string Entidad = "identidad";
            public const string ModuloActivo = "ModuloActivo";
            public const string SubModuloActivo = "SubModuloActivo";
            public const string CurrentUserObj = "CurrentUserObj";
            public const string AllEntidades = "AllEntidades";
            public const string NomEntidad = "NomEntidad";
            public const string Logo = "Logo";
        }
        #endregion

        #region Paleta de Colores Institucional FFC
        public static class Colores
        {
            public const string AzulCorporativo = "#2F5597";
            public const string AzulSecundario = "#1e3a8a";
            public const string AzulOscuro = "#143750";
            public const string AzulClaro = "#0284c7";
            public const string AzulFondo = "#f0f9ff";
            public const string VerdeExito = "#10b981";
            public const string VerdeOscuro = "#059669";
            public const string VerdeFondo = "#ecfdf5";
            public const string NaranjaInstitucional = "#ea580c";
            public const string NaranjaFondo = "#fff7ed";
            public const string NaranjaBorde = "#fed7aa";
            public const string AmbarAlerta = "#f59e0b";
            public const string RojoPeligro = "#dc2626";
            public const string Purpura = "#8b5cf6";
            public const string Cian = "#06b6d4";
            public const string GrisBorde = "#cbd5e1";
            public const string GrisTexto = "#64748b";
            public const string GrisOscuro = "#1e293b";
            public const string GrisFondo = "#f8fafc";
        }
        #endregion
    }
}
