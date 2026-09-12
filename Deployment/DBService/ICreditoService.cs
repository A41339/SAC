using System;
using System.Collections.Generic;
using System.ServiceModel;
using FGA.Models;

[ServiceContract]
public interface ICreditoService
{
    // --- Ofertas ---
    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<Creditos_Ofertas> GetOfertasActivas();

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    Creditos_Ofertas GetOfertaById(int id);

    [OperationContract]
    void PublicarOferta(Creditos_Ofertas oferta, List<int> requisitosIds);

    [OperationContract]
    void InactivarOferta(int id, int usuarioModificaId);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<Creditos_OfertaRequisitos> GetRequisitosOferta(int ofertaId);

    // --- Solicitudes ---
    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<Creditos_Solicitudes> GetSolicitudesPorEntidad(string entidadId);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<Creditos_Solicitudes> GetSolicitudesRecibidas(string entidadOferenteId);

    [OperationContract]
    void CrearSolicitud(Creditos_Solicitudes solicitud);

    [OperationContract]
    void ResolverSolicitud(int solicitudId, int estadoId, int usuarioId, string motivoRechazo = null);

    // --- Documentos ---
    [OperationContract]
    void CargarDocumento(Creditos_Documentos documento);

    [OperationContract]
    void ValidarDocumento(int documentoId, bool esValido, int usuarioSacId, string comentarios);

    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<Creditos_Documentos> GetDocumentosPorSolicitud(int solicitudId);

    // --- Catálogos ---
    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<Creditos_TipoDocumento> GetTiposDocumento();
    [OperationContract]
    [ReferencePreservingDataContractFormat]
    List<Creditos_Moneda> GetMonedas();
}
