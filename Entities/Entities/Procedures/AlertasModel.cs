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
        public string idEntidad { get => IdEntidad; set => IdEntidad = value; }

        public string NombreEntidad { get; set; }
        public string nombreEntidad { get => NombreEntidad; set => NombreEntidad = value; }

        public decimal UmbralVariacionPorcentaje { get; set; }
        public decimal umbralVariacionPorcentaje { get => UmbralVariacionPorcentaje; set => UmbralVariacionPorcentaje = value; }
        public decimal UmbralPorc => UmbralVariacionPorcentaje;
        public decimal umbralPorc => UmbralVariacionPorcentaje;

        public decimal UmbralVariacionMonto { get; set; }
        public decimal umbralVariacionMonto { get => UmbralVariacionMonto; set => UmbralVariacionMonto = value; }
        public decimal UmbralMonto => UmbralVariacionMonto;
        public decimal umbralMonto => UmbralVariacionMonto;

        public string ModoMonitoreo { get; set; } = "TODAS";
        public string modoMonitoreo { get => ModoMonitoreo; set => ModoMonitoreo = value; }

        public bool EsPersonalizado { get; set; }
        public bool esPersonalizado { get => EsPersonalizado; set => EsPersonalizado = value; }

        public bool Activo { get; set; }
        public bool activo { get => Activo; set => Activo = value; }

        public decimal GlobalPorc { get; set; } = 15.0m;
        public decimal globalPorc { get => GlobalPorc; set => GlobalPorc = value; }

        public decimal GlobalMonto { get; set; } = 10000000.0m;
        public decimal globalMonto { get => GlobalMonto; set => GlobalMonto = value; }

        public string FechaModificacion { get; set; }
        public string fechaModificacion { get => FechaModificacion; set => FechaModificacion = value; }

        public string UsuarioModificacion { get; set; }
        public string usuarioModificacion { get => UsuarioModificacion; set => UsuarioModificacion = value; }
    }

    /// <summary>
    /// Modelo de datos tipado para una cuenta contable en la lista de monitoreo (Watchlist o Blacklist)
    /// </summary>
    public class AlertaCuentaMonitoreadaDTO
    {
        public int Id { get; set; }
        public int id { get => Id; set => Id = value; }

        public string IdEntidad { get; set; }
        public string idEntidad { get => IdEntidad; set => IdEntidad = value; }

        public string NombreEntidad { get; set; }
        public string nombreEntidad { get => NombreEntidad; set => NombreEntidad = value; }

        public string CuentaContable { get; set; }
        public string cuentaContable { get => CuentaContable; set => CuentaContable = value; }
        public string Cuenta => CuentaContable;
        public string cuenta => CuentaContable;

        public string NombreCuenta { get; set; }
        public string nombreCuenta { get => NombreCuenta; set => NombreCuenta = value; }

        public int Nivel { get; set; } = 3;
        public int nivel { get => Nivel; set => Nivel = value; }

        public string TipoMonitoreo { get; set; } = "WATCHLIST";
        public string tipoMonitoreo { get => TipoMonitoreo; set => TipoMonitoreo = value; }
        public string TipoRegla => TipoMonitoreo;
        public string tipoRegla => TipoMonitoreo;

        public string Justificacion { get; set; }
        public string justificacion { get => Justificacion; set => Justificacion = value; }

        public decimal? UmbralPorcPersonalizado { get; set; }
        public decimal? umbralPorcPersonalizado { get => UmbralPorcPersonalizado; set => UmbralPorcPersonalizado = value; }
        public decimal? UmbralPorc => UmbralPorcPersonalizado;
        public decimal? umbralPorc => UmbralPorcPersonalizado;

        public decimal? UmbralMontoPersonalizado { get; set; }
        public decimal? umbralMontoPersonalizado { get => UmbralMontoPersonalizado; set => UmbralMontoPersonalizado = value; }
        public decimal? UmbralMonto => UmbralMontoPersonalizado;
        public decimal? umbralMonto => UmbralMontoPersonalizado;

        public bool Activo { get; set; } = true;
        public bool activo { get => Activo; set => Activo = value; }

        public string FechaRegistro { get; set; }
        public string fechaRegistro { get => FechaRegistro; set => FechaRegistro = value; }
        public string Fecha => FechaRegistro;
        public string fecha => FechaRegistro;

        public string UsuarioRegistro { get; set; }
        public string usuarioRegistro { get => UsuarioRegistro; set => UsuarioRegistro = value; }
    }

    /// <summary>
    /// Modelo tipado para elementos de sugerencia en el catálogo de cuentas
    /// </summary>
    public class AlertaCuentaCatalogoItemDTO
    {
        public string Cuenta { get; set; }
        public string cuenta { get => Cuenta; set => Cuenta = value; }

        public string Descripcion { get; set; }
        public string descripcion { get => Descripcion; set => Descripcion = value; }
        public string Nombre => Descripcion;
        public string nombre => Descripcion;

        public int Nivel { get; set; } = 3;
        public int nivel { get => Nivel; set => Nivel = value; }
    }

    /// <summary>
    /// Respuesta tipada para la consulta de configuración de alertas por entidad
    /// </summary>
    public class AlertaConfiguracionResponse
    {
        public bool Success { get; set; }
        public bool success { get => Success; set => Success = value; }

        public string Message { get; set; }
        public string message { get => Message; set => Message = value; }

        public AlertaConfiguracionEntidadDTO Config { get; set; }
        public AlertaConfiguracionEntidadDTO config { get => Config; set => Config = value; }

        public decimal GlobalUmbralPorc { get; set; } = 15.0m;
        public decimal globalUmbralPorc { get => GlobalUmbralPorc; set => GlobalUmbralPorc = value; }

        public decimal GlobalUmbralMonto { get; set; } = 10000000.0m;
        public decimal globalUmbralMonto { get => GlobalUmbralMonto; set => GlobalUmbralMonto = value; }

        public bool EsAdmin { get; set; }
        public bool esAdmin { get => EsAdmin; set => EsAdmin = value; }

        public string EntidadId { get; set; }
        public string entidadId { get => EntidadId; set => EntidadId = value; }
    }

    /// <summary>
    /// Respuesta tipada para la consulta de cuentas monitoreadas de una entidad
    /// </summary>
    public class AlertaCuentasMonitoreadasResponse
    {
        public bool Success { get; set; }
        public bool success { get => Success; set => Success = value; }

        public string Message { get; set; }
        public string message { get => Message; set => Message = value; }

        public List<AlertaCuentaMonitoreadaDTO> Cuentas { get; set; } = new List<AlertaCuentaMonitoreadaDTO>();
        public List<AlertaCuentaMonitoreadaDTO> cuentas { get => Cuentas; set => Cuentas = value; }

        public List<AlertaCuentaMonitoreadaDTO> Items => Cuentas;
        public List<AlertaCuentaMonitoreadaDTO> items => Cuentas;
    }

    /// <summary>
    /// Respuesta tipada para la búsqueda en el catálogo contable
    /// </summary>
    public class AlertaCatalogoCuentasResponse
    {
        public bool Success { get; set; }
        public bool success { get => Success; set => Success = value; }

        public string Message { get; set; }
        public string message { get => Message; set => Message = value; }

        public List<AlertaCuentaCatalogoItemDTO> Items { get; set; } = new List<AlertaCuentaCatalogoItemDTO>();
        public List<AlertaCuentaCatalogoItemDTO> items { get => Items; set => Items = value; }
    }

    /// <summary>
    /// Respuesta tipada general para operaciones de actualización o eliminación
    /// </summary>
    public class AlertaOperacionResponse
    {
        public bool Success { get; set; }
        public bool success { get => Success; set => Success = value; }

        public string Message { get; set; }
        public string message { get => Message; set => Message = value; }

        public int? AffectedId { get; set; }
        public int? affectedId { get => AffectedId; set => AffectedId = value; }
    }
}
