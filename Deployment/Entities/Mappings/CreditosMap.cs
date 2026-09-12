using FGA.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;

namespace FGA.Maping
{
    public class Creditos_EstadoSolicitudMap : EntityTypeConfiguration<Creditos_EstadoSolicitud>
    {
        public Creditos_EstadoSolicitudMap()
        {
            HasKey(o => o.Id);
            Property(o => o.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(o => o.Descripcion).IsRequired().HasMaxLength(50);
            ToTable("Creditos_EstadoSolicitud");
        }
    }

    public class Creditos_ComisionConfigMap : EntityTypeConfiguration<Creditos_ComisionConfig>
    {
        public Creditos_ComisionConfigMap()
        {
            HasKey(o => o.Id);
            Property(o => o.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            
            HasRequired(o => o.UsuarioActualiza)
                .WithMany()
                .HasForeignKey(o => o.UsuarioActualiza_Id)
                .WillCascadeOnDelete(false);

            ToTable("Creditos_ComisionConfig");
        }
    }

    public class Creditos_MonedaMap : EntityTypeConfiguration<Creditos_Moneda>
    {
        public Creditos_MonedaMap()
        {
            HasKey(o => o.Id);
            Property(o => o.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            Property(o => o.Codigo).IsRequired().HasMaxLength(3);
            Property(o => o.Nombre).IsRequired().HasMaxLength(50);
            ToTable("Creditos_Moneda");
        }
    }

    public class Creditos_TipoDocumentoMap : EntityTypeConfiguration<Creditos_TipoDocumento>
    {
        public Creditos_TipoDocumentoMap()
        {
            HasKey(o => o.Id);
            Property(o => o.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);
            ToTable("Creditos_TipoDocumento");
        }
    }

    public class Creditos_OfertaRequisitosMap : EntityTypeConfiguration<Creditos_OfertaRequisitos>
    {
        public Creditos_OfertaRequisitosMap()
        {
            HasKey(o => o.Id);
            Property(o => o.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            HasRequired(o => o.Oferta)
                .WithMany(o => o.Requisitos)
                .HasForeignKey(o => o.Oferta_Id)
                .WillCascadeOnDelete(false);

            HasRequired(o => o.TipoDocumento)
                .WithMany(o => o.OfertaRequisitos)
                .HasForeignKey(o => o.TipoDocumento_Id)
                .WillCascadeOnDelete(false);

            ToTable("Creditos_OfertaRequisitos");
        }
    }

    public class Creditos_OfertasMap : EntityTypeConfiguration<Creditos_Ofertas>
    {
        public Creditos_OfertasMap()
        {
            HasKey(o => o.Id);
            Property(o => o.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            HasRequired(o => o.EntidadOferente)
                .WithMany()
                .HasForeignKey(o => o.EntidadOferente_Id)
                .WillCascadeOnDelete(false);

            HasRequired(o => o.UsuarioPublicador)
                .WithMany()
                .HasForeignKey(o => o.UsuarioPublicador_Id)
                .WillCascadeOnDelete(false);

            HasOptional(o => o.UsuarioModifica)
                .WithMany()
                .HasForeignKey(o => o.UsuarioModifica_Id)
                .WillCascadeOnDelete(false);

            HasRequired(o => o.Moneda)
                .WithMany(o => o.Ofertas)
                .HasForeignKey(o => o.Moneda_Id)
                .WillCascadeOnDelete(false);

            ToTable("Creditos_Ofertas");
        }
    }

    public class Creditos_SolicitudesMap : EntityTypeConfiguration<Creditos_Solicitudes>
    {
        public Creditos_SolicitudesMap()
        {
            HasKey(o => o.Id);
            Property(o => o.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            HasRequired(o => o.Oferta)
                .WithMany(o => o.Solicitudes)
                .HasForeignKey(o => o.Oferta_Id)
                .WillCascadeOnDelete(false);

            HasRequired(o => o.EntidadSolicitante)
                .WithMany()
                .HasForeignKey(o => o.EntidadSolicitante_Id)
                .WillCascadeOnDelete(false);

            HasRequired(o => o.UsuarioSolicitante)
                .WithMany()
                .HasForeignKey(o => o.UsuarioSolicitante_Id)
                .WillCascadeOnDelete(false);

            HasRequired(o => o.EstadoSolicitud)
                .WithMany(o => o.Solicitudes)
                .HasForeignKey(o => o.EstadoSolicitud_Id)
                .WillCascadeOnDelete(false);

            ToTable("Creditos_Solicitudes");
        }
    }

    public class Creditos_DocumentosMap : EntityTypeConfiguration<Creditos_Documentos>
    {
        public Creditos_DocumentosMap()
        {
            HasKey(o => o.Id);
            Property(o => o.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            HasRequired(o => o.Solicitud)
                .WithMany(o => o.Documentos)
                .HasForeignKey(o => o.Solicitud_Id)
                .WillCascadeOnDelete(false);

            HasRequired(o => o.TipoDocumento)
                .WithMany(o => o.DocumentosRecibidos)
                .HasForeignKey(o => o.TipoDocumento_Id)
                .WillCascadeOnDelete(false);

            HasRequired(o => o.UsuarioCarga)
                .WithMany()
                .HasForeignKey(o => o.UsuarioCarga_Id)
                .WillCascadeOnDelete(false);

            HasOptional(o => o.ValidadoPorSAC)
                .WithMany()
                .HasForeignKey(o => o.ValidadoPorSAC_Id)
                .WillCascadeOnDelete(false);

            ToTable("Creditos_Documentos");
        }
    }

    public class Creditos_SMSLogMap : EntityTypeConfiguration<Creditos_SMSLog>
    {
        public Creditos_SMSLogMap()
        {
            HasKey(o => o.Id);
            Property(o => o.Id).HasDatabaseGeneratedOption(DatabaseGeneratedOption.Identity);

            HasOptional(o => o.Solicitud)
                .WithMany(o => o.SMSLogs)
                .HasForeignKey(o => o.Solicitud_Id)
                .WillCascadeOnDelete(false);

            ToTable("Creditos_SMSLog");
        }
    }
}