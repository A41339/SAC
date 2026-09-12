using System;
using System.Collections.Generic;
using System.ServiceModel;
using FGA.Models;

namespace FGA_En_Linea.CreditoService
{
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    [System.ServiceModel.ServiceContractAttribute(ConfigurationName = "CreditoService.ICreditoService")]
    public interface ICreditoService
    {
        [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/ICreditoService/GetOfertasActivas", ReplyAction = "http://tempuri.org/ICreditoService/GetOfertasActivasResponse")]
        List<Creditos_Ofertas> GetOfertasActivas();

        [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/ICreditoService/GetOfertaById", ReplyAction = "http://tempuri.org/ICreditoService/GetOfertaByIdResponse")]
        Creditos_Ofertas GetOfertaById(int id);

        [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/ICreditoService/PublicarOferta", ReplyAction = "http://tempuri.org/ICreditoService/PublicarOfertaResponse")]
        void PublicarOferta(Creditos_Ofertas oferta, List<int> requisitosIds);

        [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/ICreditoService/InactivarOferta", ReplyAction = "http://tempuri.org/ICreditoService/InactivarOfertaResponse")]
        void InactivarOferta(int id, int usuarioModificaId);

        [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/ICreditoService/GetSolicitudesPorEntidad", ReplyAction = "http://tempuri.org/ICreditoService/GetSolicitudesPorEntidadResponse")]
        List<Creditos_Solicitudes> GetSolicitudesPorEntidad(string entidadId);

        [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/ICreditoService/GetSolicitudesRecibidas", ReplyAction = "http://tempuri.org/ICreditoService/GetSolicitudesRecibidasResponse")]
        List<Creditos_Solicitudes> GetSolicitudesRecibidas(string entidadOferenteId);

        [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/ICreditoService/CrearSolicitud", ReplyAction = "http://tempuri.org/ICreditoService/CrearSolicitudResponse")]
        void CrearSolicitud(Creditos_Solicitudes solicitud);

        [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/ICreditoService/ResolverSolicitud", ReplyAction = "http://tempuri.org/ICreditoService/ResolverSolicitudResponse")]
        void ResolverSolicitud(int solicitudId, int estadoId, int usuarioId, string motivoRechazo = null);

        [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/ICreditoService/CargarDocumento", ReplyAction = "http://tempuri.org/ICreditoService/CargarDocumentoResponse")]
        void CargarDocumento(Creditos_Documentos documento);

        [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/ICreditoService/ValidarDocumento", ReplyAction = "http://tempuri.org/ICreditoService/ValidarDocumentoResponse")]
        void ValidarDocumento(int documentoId, bool esValido, int usuarioSacId, string comentarios);

        [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/ICreditoService/GetRequisitosOferta", ReplyAction = "http://tempuri.org/ICreditoService/GetRequisitosOfertaResponse")]
        List<Creditos_OfertaRequisitos> GetRequisitosOferta(int ofertaId);

        [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/ICreditoService/GetDocumentosPorSolicitud", ReplyAction = "http://tempuri.org/ICreditoService/GetDocumentosPorSolicitudResponse")]
        List<Creditos_Documentos> GetDocumentosPorSolicitud(int solicitudId);

        [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/ICreditoService/GetTiposDocumento", ReplyAction = "http://tempuri.org/ICreditoService/GetTiposDocumentoResponse")]
        List<Creditos_TipoDocumento> GetTiposDocumento();

        [System.ServiceModel.OperationContractAttribute(Action = "http://tempuri.org/ICreditoService/GetMonedas", ReplyAction = "http://tempuri.org/ICreditoService/GetMonedasResponse")]
        List<Creditos_Moneda> GetMonedas();
    }

    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public interface ICreditoServiceChannel : ICreditoService, System.ServiceModel.IClientChannel
    {
    }

    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.ServiceModel", "4.0.0.0")]
    public partial class CreditoServiceClient : System.ServiceModel.ClientBase<ICreditoService>, ICreditoService
    {
        public CreditoServiceClient() { }

        public CreditoServiceClient(string endpointConfigurationName) : base(endpointConfigurationName) { }

        public CreditoServiceClient(string endpointConfigurationName, string remoteAddress) : base(endpointConfigurationName, remoteAddress) { }

        public CreditoServiceClient(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : base(endpointConfigurationName, remoteAddress) { }

        public CreditoServiceClient(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : base(binding, remoteAddress) { }

        public List<Creditos_Ofertas> GetOfertasActivas()
        {
            return base.Channel.GetOfertasActivas();
        }

        public Creditos_Ofertas GetOfertaById(int id)
        {
            return base.Channel.GetOfertaById(id);
        }

        public List<Creditos_OfertaRequisitos> GetRequisitosOferta(int ofertaId)
        {
            return base.Channel.GetRequisitosOferta(ofertaId);
        }

        public void PublicarOferta(Creditos_Ofertas oferta, List<int> requisitosIds)
        {
            base.Channel.PublicarOferta(oferta, requisitosIds);
        }

        public void InactivarOferta(int id, int usuarioModificaId)
        {
            base.Channel.InactivarOferta(id, usuarioModificaId);
        }

        public List<Creditos_Solicitudes> GetSolicitudesPorEntidad(string entidadId)
        {
            return base.Channel.GetSolicitudesPorEntidad(entidadId);
        }

        public List<Creditos_Solicitudes> GetSolicitudesRecibidas(string entidadOferenteId)
        {
            return base.Channel.GetSolicitudesRecibidas(entidadOferenteId);
        }

        public void CrearSolicitud(Creditos_Solicitudes solicitud)
        {
            base.Channel.CrearSolicitud(solicitud);
        }

        public void ResolverSolicitud(int solicitudId, int estadoId, int usuarioId, string motivoRechazo = null)
        {
            base.Channel.ResolverSolicitud(solicitudId, estadoId, usuarioId, motivoRechazo);
        }

        public void CargarDocumento(Creditos_Documentos documento)
        {
            base.Channel.CargarDocumento(documento);
        }

        public List<Creditos_Documentos> GetDocumentosPorSolicitud(int solicitudId)
        {
            return base.Channel.GetDocumentosPorSolicitud(solicitudId);
        }

        public void ValidarDocumento(int documentoId, bool esValido, int usuarioSacId, string comentarios)
        {
            base.Channel.ValidarDocumento(documentoId, esValido, usuarioSacId, comentarios);
        }

        public List<Creditos_TipoDocumento> GetTiposDocumento()
        {
            return base.Channel.GetTiposDocumento();
        }

        public List<Creditos_Moneda> GetMonedas()
        {
            return base.Channel.GetMonedas();
        }
    }
}
