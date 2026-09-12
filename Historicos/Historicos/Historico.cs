using FGA.Models;
using Entities.Entities.Procedures;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using FGA.Utility;

namespace Historicos
{
    public partial class frm_historicos : Form
    {
        public frm_historicos()
        {
            InitializeComponent();
        }

        private void btn_browse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderDlg = new FolderBrowserDialog();
            folderDlg.ShowNewFolderButton = true;
            DialogResult result = folderDlg.ShowDialog();
            if (result == DialogResult.OK)
            {
                txt_folder.Text = folderDlg.SelectedPath;
                Environment.SpecialFolder root = folderDlg.RootFolder;
            }
        }

        /*Debe leer todos los archivos del subdirectorio y mostrarlos*/
        private async void btn_leer_Click(object sender, EventArgs e)
        {
            List<FGA.Models.File> listaArchivos = new List<FGA.Models.File>();
            int num = 0;
            List<FGA.Models.Error> listaErrores = new List<FGA.Models.Error>();

            lblAvance.Text = "Procesando archivos. Por favor espere ...: ";
            lblAvance.Refresh();

            await Task.Run(() => DirectorySearch(txt_folder.Text, ref num, ref listaArchivos, ref listaErrores));

            lblAvance.Text = "Archivos leidos. Validando errores ...";
            lblAvance.Refresh();

            IEnumerable<FGA.Models.File> files = listaArchivos.OrderBy(o => o.Periodo);
            var bindingList = new BindingList<FGA.Models.File>(files.ToList());
            var source = new BindingSource(bindingList, null);

            grid_Files.DataSource = source;
            grid_Files.Columns[0].Width = 200;
            grid_Files.Columns[4].Width = 400;
            await Task.Run(() => ValidarErrores(files.ToList(), listaErrores));

            lblAvance.Text = "Proceso finalizado ...";
            lblAvance.Refresh();
        }

        private void ValidarErrores(List<FGA.Models.File> listaArchivos, List<FGA.Models.Error> listaErrores)
        {
            FGA.Models.Error registro;

            try
            {
                try
                {
                    DateTime minPeriodo = listaArchivos.Min(o => o.Periodo);
                    DateTime maxPeriodo = listaArchivos.Max(o => o.Periodo);
                    List<FGA.Models.File> listaPeriodo;

                    List<string> archivosObligatorios = new List<string>();
                    archivosObligatorios.Add(Utilitarios.xml_contable_estado);
                    archivosObligatorios.Add(Utilitarios.xml_contable_brecha);
                    archivosObligatorios.Add(Utilitarios.xml_contable_datos_adicionales);
                    archivosObligatorios.Add(Utilitarios.xml_flujo_efectivo);
                    archivosObligatorios.Add(Utilitarios.xml_calce_plazo);
                    archivosObligatorios.Add(Utilitarios.xml_credito_bienes_realizables);
                    archivosObligatorios.Add(Utilitarios.xml_credito_cuentas_cobrar);
                    archivosObligatorios.Add(Utilitarios.xml_credito_cuota_atrasada);
                    archivosObligatorios.Add(Utilitarios.xml_credito_deudor);
                    archivosObligatorios.Add(Utilitarios.xml_credito_garantia_operaciones);
                    archivosObligatorios.Add(Utilitarios.xml_credito_informacion_oper_no_reportadas);
                    archivosObligatorios.Add(Utilitarios.xml_credito_oper_dirind);
                    archivosObligatorios.Add(Utilitarios.xml_icl);
                    archivosObligatorios.Add(Utilitarios.xml_inversiones_activas);
                    archivosObligatorios.Add(Utilitarios.xml_pasivos_cuentas_contables_210);
                    archivosObligatorios.Add(Utilitarios.xml_suficiencia_patrimonial);
                    archivosObligatorios.Add(Utilitarios.xml_indicadores_financieros);

                    while (minPeriodo <= maxPeriodo)
                    {
                        listaPeriodo = listaArchivos.Where(o => o.Periodo == minPeriodo).ToList();

                        //revisar que tenga los 11 archivo    
                        var archivosFaltantes = from p in archivosObligatorios
                                                where listaPeriodo.All(a => a.Codigo != p)
                                                select p;

                        foreach (string t in archivosFaltantes)
                        {
                            registro = new FGA.Models.Error();
                            registro.Periodo = minPeriodo;
                            registro.Validacion = "Falta el archivo: " + Utilitarios.GetFileName(t);
                            listaErrores.Add(registro);
                        }

                        //revisar duplicados
                        var duplicates = listaPeriodo
                        .GroupBy(i => i.Codigo)
                        .Where(g => g.Count() > 1)
                        .Select(g => g.Key);

                        foreach (var t in duplicates)
                        {
                            registro = new FGA.Models.Error();
                            registro.Periodo = minPeriodo;
                            registro.Validacion = "El archivo esta duplicado: " + Utilitarios.GetFileName(t);
                            listaErrores.Add(registro);
                        }

                        minPeriodo = minPeriodo.AddMonths(1);
                    }
                }
                catch (Exception)
                {
                }

                IEnumerable<FGA.Models.Error> errores = listaErrores.OrderBy(o => o.Periodo);
                var bindingList = new BindingList<FGA.Models.Error>(errores.ToList());
                var source = new BindingSource(bindingList, null);
                grid_Errores.DataSource = source;
                grid_Errores.Columns[1].Width = 700;
            }
            catch (Exception e)
            {
                registro = new FGA.Models.Error();
                registro.Periodo = DateTime.Now;
                registro.Validacion = "Error al validar los archivos: " + e.Message;
            }
        }

        public void DirectorySearch(string dir, ref int num, ref List<FGA.Models.File> listaArchivos, ref List<FGA.Models.Error> listaErrores)
        {
            try
            {
                DateTime fecha = DateTime.Now;
                String entidad = string.Empty;
                String mensaje = string.Empty;
                FGA.Models.File file;
                FGA.Models.Error error;

                foreach (string f in Directory.GetFiles(dir))
                {
                    file = new FGA.Models.File();
                    if (ProcessFile.GetInfoFile(f, ref file))
                    {
                        file.Ruta = f;
                        listaArchivos.Add(file);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(file.Error))
                        {
                            error = new Error();
                            error.Periodo = file.Periodo;
                            error.Validacion = file.Error;
                            listaErrores.Add(error);
                        }
                    }

                    num += 1;
                  

                }
                foreach (string d in Directory.GetDirectories(dir))
                {
                    DirectorySearch(d, ref num, ref listaArchivos, ref listaErrores);
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Reporte el error: " + ex.Message, "FGA", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private async void btn_Ejecutar_Click_1(object sender, EventArgs e)
        {
            /*Recorrer periodo a periodo*/
            try
            {
                String periodo = grid_Files.Rows[0].Cells[2].Value.ToString().Substring(0, grid_Files.Rows[0].Cells[2].Value.ToString().IndexOf(" "));
                String entidad = string.Empty;
                SIContext db = new SIContext();
                FGAEntities proc = new FGAEntities();
                Usuario ObjUser = db.Usuarios.Where(o => o.Id == 1).FirstOrDefault();
                DateTime fechaFinal = new DateTime(this.dt_fechaFinal.Value.Year, this.dt_fechaFinal.Value.Month, 1);

                for (int i = 0; i < this.grid_Files.Rows.Count; i++)
                {
                    if (grid_Files.Rows[i].Cells[0].Value != null)
                    {
                        this.lblAvance.Text = "Procesando " + grid_Files.Rows[i].Cells[3].Value.ToString() + " periodo: " +
                        periodo + " archivo: " + grid_Files.Rows[i].Cells[0].Value.ToString();
                        this.lblAvance.Refresh();

                        if (!periodo.Equals(grid_Files.Rows[i].Cells[2].Value.ToString().Substring(0, grid_Files.Rows[i].Cells[2].Value.ToString().IndexOf(" "))))
                        {
                            /*Solicitar ejecutar el cierre*/
                           /* if(!string.IsNullOrEmpty(entidad))
                                proc.FGA_Cierre_Mensual(entidad, DateTime.Parse(periodo));*/

                            periodo = grid_Files.Rows[i].Cells[2].Value.ToString().Substring(0, grid_Files.Rows[i].Cells[2].Value.ToString().IndexOf(" "));
                            this.lblAvance.Text = "Procesando " + grid_Files.Rows[i].Cells[3].Value.ToString() + " periodo: " +
                            periodo + " archivo: " + grid_Files.Rows[i].Cells[0].Value.ToString();
                            this.lblAvance.Refresh();
                        }

                        if (DateTime.Parse(periodo) <= new DateTime(fechaFinal.Year, fechaFinal.Month, 1) && DateTime.Parse(periodo) >= new DateTime(2016, 1, 1))
                        {
                            string resultado = await Task.Run(() => ProcessFile.Add(grid_Files.Rows[i].Cells[4].Value.ToString(), ObjUser, db));

                            if (!string.IsNullOrEmpty(resultado))
                                entidad = resultado;
                        }
                    }
                }

               // proc.FGA_Cierre_Mensual(entidad, DateTime.Parse(periodo));
                MessageBox.Show("Proceso finalizado con éxito", "FGA", MessageBoxButtons.OK, MessageBoxIcon.Information);
                db.Dispose();
            }
            catch (Exception ex) {
                MessageBox.Show("Asegurese de haber seleccionado la opción leer, antes de ejecutar el proceso: " + ex.Message, "FGA", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
    }
}
