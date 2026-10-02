using System;
using System.Collections.Generic;

namespace Entities.Entities.Procedures
{
    /// <summary>
    /// Modelo de datos tipado para la configuración de umbrales y reglas de alerta por entidad
    /// </summary>
    public class AlertaConfiguracionEntidadDTO
    {
        public string IdEntidad { get; set; }
        public string NombreEntidad { get; set; }
        public decimal UmbralVariacionPorcentaje { get; set; }
        public decimal UmbralPorc => UmbralVariacionPorcentaje;
        public decimal UmbralVariacionMonto { get; set; }
        public decimal UmbralMonto => UmbralVariacionMonto;
        public string ModoMonitoreo { get; set; } = "TODAS";
        public bool EsPersonalizado { get; set; }
        public bool Activo { get; set; }
        public decimal GlobalPorc { get; set; } = 15.0m;
        public decimal GlobalMonto { get; set; } = 10000000.0m;
        public string FechaModificacion { get; set; }
        public string UsuarioModificacion { get; set; }
    }

    /// <summary>
    /// Modelo de datos tipado para una cuenta contable en la lista de monitoreo (Watchlist o Blacklist)
    /// </summary>
    public class AlertaCuentaMonitoreadaDTO
    {
        public int Id { get; set; }
        public string IdEntidad { get; set; }
        public string NombreEntidad { get; set; }
        public string CuentaContable { get; set; }
        public string Cuenta => CuentaContable;
        public string NombreCuenta { get; set; }
        public int Nivel { get; set; } = 3;
        public string TipoMonitoreo { get; set; } = "WATCHLIST";
        public string TipoRegla => TipoMonitoreo;
        public string Justificacion { get; set; }
        public decimal? UmbralPorcPersonalizado { get; set; }
        public decimal? UmbralPorc => UmbralPorcPersonalizado;
        public decimal? UmbralMontoPersonalizado { get; set; }
        public decimal? UmbralMonto => UmbralMontoPersonalizado;
        public bool Activo { get; set; } = true;
        public string FechaRegistro { get; set; }
        public string Fecha => FechaRegistro;
        public string UsuarioRegistro { get; set; }
    }

    /// <summary>
    /// Modelo tipado para elementos de sugerencia en el catálogo de cuentas
    /// </summary>
    public class AlertaCuentaCatalogoItemDTO
    {
        public string Cuenta { get; set; }
        public string Descripcion { get; set; }
        public string Nombre => Descripcion;
        public int Nivel { get; set; } = 3;
    }

    /// <summary>
    /// Respuesta tipada para la consulta de configuración de alertas por entidad
    /// </summary>
    public class AlertaConfiguracionResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public AlertaConfiguracionEntidadDTO Config { get; set; }
        public decimal GlobalUmbralPorc { get; set; } = 15.0m;
        public decimal GlobalUmbralMonto { get; set; } = 10000000.0m;
        public bool EsAdmin { get; set; }
        public string EntidadId { get; set; }
    }

    /// <summary>
    /// Respuesta tipada para la consulta de cuentas monitoreadas de una entidad
    /// </summary>
    public class AlertaCuentasMonitoreadasResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public List<AlertaCuentaMonitoreadaDTO> Cuentas { get; set; } = new List<AlertaCuentaMonitoreadaDTO>();
        public List<AlertaCuentaMonitoreadaDTO> Items => Cuentas;
    }

    /// <summary>
    /// Respuesta tipada para la búsqueda en el catálogo contable
    /// </summary>
    public class AlertaCatalogoCuentasResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public List<AlertaCuentaCatalogoItemDTO> Items { get; set; } = new List<AlertaCuentaCatalogoItemDTO>();
    }

    /// <summary>
    /// Respuesta tipada general para operaciones de actualización o eliminación
    /// </summary>
    public class AlertaOperacionResponse
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public int? AffectedId { get; set; }
    }
}
