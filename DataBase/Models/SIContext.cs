using Entities.Entities.Evaluacion;
using System.Data.Entity;

namespace FGA.Models
{
    public class SIContext : DbContext
    {
        public SIContext()
            : base("name=SIConnectionString")
        {
            this.Configuration.LazyLoadingEnabled = false;
            this.Configuration.ProxyCreationEnabled = false;
        }


        public virtual DbSet<XML_ICL> XML_ICLs { get; set; }
        public virtual DbSet<CargaAsincronica> CargaAsincronica { get; set; }
        public virtual DbSet<PerfilEntidad> PerfilesEntidad { get; set; }
        public virtual DbSet<EvalResponsableCategoria> ResponsableCategorias { get; set; }
        public virtual DbSet<EvalSubCategoria> SubCategorias { get; set; }
        public virtual DbSet<EvalCategoria> Categorias { get; set; }
        public virtual DbSet<EvalPregunta> Preguntas { get; set; }
        public virtual DbSet<EvalRespuestaUsuario> RespuestasUsuario { get; set; }
        public virtual DbSet<Proy_EEFF> Proy_EEFF { get; set; }
        public virtual DbSet<CatalogoCuenta> CatalogoCuentas { get; set; }
        public virtual DbSet<Role> Roles { get; set; }
        public virtual DbSet<Usuario> Usuarios { get; set; }
        public virtual DbSet<Menu> Menus { get; set; }
        public virtual DbSet<MenuPermission> MenuPermissions { get; set; }
        public virtual DbSet<Entidad> Entidades { get; set; }
        public virtual DbSet<UsuarioEstado> UsuarioEstados { get; set; }
        public virtual DbSet<TipoXML> TipoXMLs { get; set; }
        public virtual DbSet<Log> Logs { get; set; }
        public virtual DbSet<ArchivoEstado> ArchivoEstados { get; set; }
        public virtual DbSet<XML_Encabezado> XML_Encabezados { get; set; }
        public virtual DbSet<XML_Errores> XML_Errores { get; set; }
        public virtual DbSet<XML_Credito_Cuota_Atrasada> XML_Credito_Cuotas_Atrasadas { get; set; }
        public virtual DbSet<XML_Credito_Deudor> XML_Credito_Deudores { get; set; }
        public virtual DbSet<XML_Contable_Estado> XML_Contable_Estados { get; set; }
        public virtual DbSet<XML_Credito_Operacion_DirInd> XML_Credito_Operaciones_DirInds { get; set; }
        public virtual DbSet<XML_Pasivo_Cuenta_Contable_210> XML_Pasivo_Cuenta_Contable_210s { get; set; }
        public virtual DbSet<XML_Inversion_Activa> XML_Inversion_Activas { get; set; }
        public virtual DbSet<XML_Capital_Social> XML_Capital_Sociales { get; set; }
        public virtual DbSet<XML_Rango_Calce_Plazo> XML_Rango_Calce_Plazo { get; set; }
        public virtual DbSet<XML_Contable_Brecha> XML_Contable_Brecha { get; set; }
        public virtual DbSet<XML_Contable_DatosAdicionales> XML_Contable_DatosAdicionales { get; set; }
        public virtual DbSet<XML_Flujo_EfectivoReal> XML_Flujo_EfectivoReal { get; set; }
        public virtual DbSet<XML_Suficiencia_Patrimonial> XML_Suficiencia_Patrimonial { get; set; }
        public virtual DbSet<XML_Indicadores_Financieros> XML_Indicadores_Financieros { get; set; }
        public virtual DbSet<Hist_Libor> Hist_Libor { get; set; }
        public virtual DbSet<Hist_TBP> Hist_TBP { get; set; }
        public virtual DbSet<BCCR_TipoIndicador> BCCR_TipoIndicador { get; set; }
        public virtual DbSet<BCCR_Indicadores> BCCR_Indicadores { get; set; }
        public virtual DbSet<Hist_TipoCambio> Hist_TipoCambio { get; set; }
        public virtual DbSet<Hist_IPC> Hist_IPC { get; set; }
        public virtual DbSet<Rpt_EstructuraCamel> Rpt_EstructuraCamel { get; set; }
        public virtual DbSet<Rpt_Analisis_VH> Rpt_Analisis_VH { get; set; }
        public virtual DbSet<Rpt_Graph> Rpt_Graph { get; set; }
        public virtual DbSet<Calendario> Calendarios { get; set; }
        public virtual DbSet<Tipo_Cartera> Tipos_Cartera { get; set; }
        public virtual DbSet<Categoria_Riesgo> Categorias_Riesgo { get; set; }
        public virtual DbSet<Formulas> Formulas { get; set; }
        public virtual DbSet<Message> Message { get; set; }
        public virtual DbSet<Sexo> Sexo { get; set; }
        public virtual DbSet<XML_Excepcion> XML_Excepcion { get; set; }
        public virtual DbSet<Proyecto> Proyecto { get; set; }
        public virtual DbSet<SolicitudCambio> SolicitudCambio { get; set; }
        public virtual DbSet<TipoTarea> TipoTarea { get; set; }
        public virtual DbSet<Tarea> Tarea { get; set; }
        public virtual DbSet<Proyeccion> Proyeccion { get; set; }
        public virtual DbSet<TipoProyeccion> TipoProyeccion { get; set; }
        public virtual DbSet<Exclusion_Periodos_Proyectar> Exclusion_Periodo_Proyectar { get; set; }
        public virtual DbSet<TipoInforme> TipoInforme { get; set; }
        public virtual DbSet<InformeMail> InformeMail { get; set; }
        public virtual DbSet<Parametros> Parametros { get; set; }
        public virtual DbSet<Bit_Sessiones> Bit_Sessiones { get; set; }
        public virtual DbSet<Sector> Sector { get; set; }
        public virtual DbSet<Sugef_Encabezado> Sugef_Encabezdo { get; set; }
        public virtual DbSet<Sugef_Info_Contable> Sugef_Info_Contable { get; set; }
        public virtual DbSet<Sugef_Cartera> Sugef_Cartera { get; set; }
        public virtual DbSet<Sugef_Errores> Sugef_Error { get; set; }
        public virtual DbSet<Salida_Sugef_Indicadores_Cartera> Salida_Sugef_Indicadores_Cartera { get; set; }
        public virtual DbSet<Notificaciones> Notificaciones { get; set; }
        public virtual DbSet<Creditos_EstadoSolicitud> Creditos_EstadoSolicitud { get; set; }
        public virtual DbSet<Creditos_ComisionConfig> Creditos_ComisionConfig { get; set; }
        public virtual DbSet<Creditos_Ofertas> Creditos_Ofertas { get; set; }
        public virtual DbSet<Creditos_Solicitudes> Creditos_Solicitudes { get; set; }
        public virtual DbSet<Creditos_Documentos> Creditos_Documentos { get; set; }
        public virtual DbSet<Creditos_SMSLog> Creditos_SMSLog { get; set; }
        public virtual DbSet<Creditos_TipoDocumento> Creditos_TipoDocumento { get; set; }
        public virtual DbSet<Creditos_OfertaRequisitos> Creditos_OfertaRequisitos { get; set; }
        public virtual DbSet<Creditos_Moneda> Creditos_Moneda { get; set; }
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.Add(new Maping.XML_ICLMap());
            modelBuilder.Configurations.Add(new Maping.CargaAsincronicaMap());
            modelBuilder.Configurations.Add(new Maping.PerfilEntidadMap());
            modelBuilder.Configurations.Add(new Maping.EvalResponsableCategoriaMap());
            modelBuilder.Configurations.Add(new Maping.EvalSubCategoriaMap());
            modelBuilder.Configurations.Add(new Maping.EvalCategoriaMap());
            modelBuilder.Configurations.Add(new Maping.EvalPreguntaMap());
            modelBuilder.Configurations.Add(new Maping.EvalRespuestaUsuarioMap());
            modelBuilder.Configurations.Add(new Maping.NotificacionesMap());
            modelBuilder.Configurations.Add(new Maping.Sugef_CarteraMap());
            modelBuilder.Configurations.Add(new Maping.Sugef_EncabezadoMap());
            modelBuilder.Configurations.Add(new Maping.Sugef_Info_ContableMap());
            modelBuilder.Configurations.Add(new Maping.Sugef_ErroresMap());
            modelBuilder.Configurations.Add(new Maping.SectorMap());
            modelBuilder.Configurations.Add(new Maping.Bit_SessionesMap());
            modelBuilder.Configurations.Add(new Maping.ParametrosMap());
            modelBuilder.Configurations.Add(new Maping.Exclusion_Periodos_ProyectarMap());
            modelBuilder.Configurations.Add(new Maping.TipoProyeccionMap());
            modelBuilder.Configurations.Add(new Maping.ProyeccionMap());
            modelBuilder.Configurations.Add(new Maping.CatalogoCuentaMap());
            modelBuilder.Configurations.Add(new Maping.TipoTareaMap());
            modelBuilder.Configurations.Add(new Maping.TareaMap());
            modelBuilder.Configurations.Add(new Maping.ProyectoMap());
            modelBuilder.Configurations.Add(new Maping.SolicitudCambioMap());
            modelBuilder.Configurations.Add(new Maping.SexoMap());
            modelBuilder.Configurations.Add(new Maping.XML_ExcepcionMap());
            modelBuilder.Configurations.Add(new Maping.MessageMap());
            modelBuilder.Configurations.Add(new Maping.FormulasMap());
            modelBuilder.Configurations.Add(new Maping.Categoria_RiesgoMap());
            modelBuilder.Configurations.Add(new Maping.Tipo_CarteraMap());
            modelBuilder.Configurations.Add(new Maping.CalendarioMap());
            modelBuilder.Configurations.Add(new Maping.Rpt_GraphMap());
            modelBuilder.Configurations.Add(new Maping.Rpt_Analisis_VHMap());
            modelBuilder.Configurations.Add(new Maping.Rpt_EstructuraCamelMap());
            modelBuilder.Configurations.Add(new Maping.Hist_LiborMap());
            modelBuilder.Configurations.Add(new Maping.Hist_TBPMap());
            modelBuilder.Configurations.Add(new Maping.Hist_IPCMap());
            modelBuilder.Configurations.Add(new Maping.Hist_TipoCambioMap());
            modelBuilder.Configurations.Add(new Maping.XML_Suficiencia_PatrimonialMap());
            modelBuilder.Configurations.Add(new Maping.XML_Indicadores_FinancierosMap());
            modelBuilder.Configurations.Add(new Maping.XML_Contable_BrechaMap());
            modelBuilder.Configurations.Add(new Maping.XML_Contable_DatosAdicionalesMap());
            modelBuilder.Configurations.Add(new Maping.XML_Flujo_EfectivoRealMap());
            modelBuilder.Configurations.Add(new Maping.XML_Rango_Calce_PlazoMap());
            modelBuilder.Configurations.Add(new Maping.RoleMap());
            modelBuilder.Configurations.Add(new Maping.UsuarioMap());
            modelBuilder.Configurations.Add(new Maping.LogMap());
            modelBuilder.Configurations.Add(new Maping.MenuMap());
            modelBuilder.Configurations.Add(new Maping.MenuPermissionMap());
            modelBuilder.Configurations.Add(new Maping.EntidadMap());
            modelBuilder.Configurations.Add(new Maping.UsuarioEstadoMap());
            modelBuilder.Configurations.Add(new Maping.ArchivoEstadoMap());
            modelBuilder.Configurations.Add(new Maping.XML_Contable_EstadoMap());
            modelBuilder.Configurations.Add(new Maping.XML_Credito_Cuota_AtrasadaMap());
            modelBuilder.Configurations.Add(new Maping.XML_Credito_DeudorMap());
            modelBuilder.Configurations.Add(new Maping.XML_Inveresion_ActivaMap());
            modelBuilder.Configurations.Add(new Maping.XML_Credito_Operacion_DirIndMap());
            modelBuilder.Configurations.Add(new Maping.XML_Pasivo_Cuenta_Contable_210Map());
            modelBuilder.Configurations.Add(new Maping.XML_Capital_SocialMap());
            modelBuilder.Configurations.Add(new Maping.XML_EncabezadoMap());
            modelBuilder.Configurations.Add(new Maping.TipoXMLMap());
            modelBuilder.Configurations.Add(new Maping.TipoInformeMap());
            modelBuilder.Configurations.Add(new Maping.InformeMailMap());
            modelBuilder.Configurations.Add(new Maping.Salida_Sugef_Indicadores_CarteraMap());
            modelBuilder.Configurations.Add(new Maping.Creditos_EstadoSolicitudMap());
            modelBuilder.Configurations.Add(new Maping.Creditos_ComisionConfigMap());
            modelBuilder.Configurations.Add(new Maping.Creditos_OfertasMap());
            modelBuilder.Configurations.Add(new Maping.Creditos_SolicitudesMap());
            modelBuilder.Configurations.Add(new Maping.Creditos_DocumentosMap());
            modelBuilder.Configurations.Add(new Maping.Creditos_SMSLogMap());
            modelBuilder.Configurations.Add(new Maping.Creditos_TipoDocumentoMap());
            modelBuilder.Configurations.Add(new Maping.Creditos_OfertaRequisitosMap());
            modelBuilder.Configurations.Add(new Maping.Creditos_MonedaMap());
            base.OnModelCreating(modelBuilder);
        }       
    }
}