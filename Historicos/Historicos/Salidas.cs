using Entities.Entities.Procedures;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;

namespace Historicos
{
    public partial class Salidas : Form
    {
        public Salidas()
        {
            InitializeComponent();
            this.dt_fechaInicial.Value = DateTime.Now.AddMonths(-1);
            dt_fechaInicial.Format = DateTimePickerFormat.Custom;
            dt_fechaInicial.CustomFormat = "MM/yyyy";

            this.dt_fechaFinal.Value = DateTime.Now.AddMonths(-1);
            dt_fechaFinal.Format = DateTimePickerFormat.Custom;
            dt_fechaFinal.CustomFormat = "MM/yyyy";

            this.CargarSalidas();
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

        private void btn_Ejecutar_Click(object sender, EventArgs e)
        {
            FGAEntities db = new FGAEntities();

            try
            {
                if (this.dt_fechaInicial.Value > this.dt_fechaFinal.Value)
                    MessageBox.Show("La fecha inicial no puede ser mayor a la fecha final", "FGA", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else
                {

                    if (string.IsNullOrEmpty(this.txt_folder.Text))
                        MessageBox.Show("Debe seleccionar un directorio donde generar la información", "FGA", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    else
                    {
                        string dir = this.txt_folder.Text;
                        string dirActual = string.Empty;
                        DateTime periodo = new DateTime(dt_fechaInicial.Value.Year, dt_fechaInicial.Value.Month, 1); ;
                        DateTime fechaFinal = new DateTime(this.dt_fechaFinal.Value.Year, this.dt_fechaFinal.Value.Month, 1);
                        var diffMonths = (fechaFinal.Month + fechaFinal.Year * 12) - (periodo.Month + periodo.Year * 12);
                        progress_bar.Value = 0;
                        progress_bar.Maximum = diffMonths + 1;
                        progress_bar.Step = 1;

                        while (periodo <= fechaFinal)
                        {
                            progress_bar.PerformStep();
                            dirActual = dir + "\\" + periodo.Year.ToString();
                            System.IO.Directory.CreateDirectory(dirActual);

                            if (this.ddl_salidas.SelectedValue.ToString().Equals("0"))
                            {
                                dirActual = dirActual + "\\" + periodo.Month.ToString();
                                System.IO.Directory.CreateDirectory(dirActual);
                            }

                            switch (this.ddl_salidas.SelectedValue)
                            {
                                case "1":
                                    dirActual = dirActual + "\\Balance_Comprobacion";
                                    System.IO.Directory.CreateDirectory(dirActual);
                                    GenerarBalance(db, periodo, dirActual, ck_Excel.Checked);
                                    break;
                                case "2":
                                    dirActual = dirActual + "\\Cartera_Credito";
                                    System.IO.Directory.CreateDirectory(dirActual);
                                    GenerarCarteraCredito(db, periodo, dirActual, ck_Excel.Checked);
                                    break;
                                case "3":
                                    dirActual = dirActual + "\\Maduracion";
                                    System.IO.Directory.CreateDirectory(dirActual);
                                    GenerarMaduracionCartera(db, periodo, dirActual, ck_Excel.Checked);
                                    break;
                                case "4":
                                    dirActual = dirActual + "\\Maduracion_Resumen";
                                    System.IO.Directory.CreateDirectory(dirActual);
                                    GenerarMaduracionCarteraResumen(db, periodo, dirActual, ck_Excel.Checked);
                                    break;
                                case "5":
                                    dirActual = dirActual + "\\Riesgo_Liquidez";
                                    System.IO.Directory.CreateDirectory(dirActual);
                                    GenerarRiesgoLiquidez(db, periodo, dirActual, ck_Excel.Checked);
                                    break;
                                case "6":
                                    dirActual = dirActual + "\\Tasa_Ponderada";
                                    System.IO.Directory.CreateDirectory(dirActual);
                                    //GenerarTasaPonderada(db, periodo, dirActual, ck_Excel.Checked);
                                    break;
                                case "7":
                                    dirActual = dirActual + "\\Composicion_Cartera";
                                    System.IO.Directory.CreateDirectory(dirActual);
                                    GenerarComposicionCartera(db, periodo, dirActual, ck_Excel.Checked);
                                    break;
                                default:
                                    break;
                            }
                            periodo = periodo.AddMonths(1);
                        }
                        progress_bar.PerformStep();
                        MessageBox.Show("La información se generó con éxito", "FGA", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Reporte el error: " + ex.Message, "FGA", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                db.Dispose();
                this.grid_Files.DataSource = null;
            }
        }
        private void GenerarExcel<T>(DateTime periodo, String encabezado, String nombreHoja, String dir, String nomEntidad, String codEntidad, Func<string, DateTime?, IEnumerable<T>> mDetalle)
        {
            ExcelWorksheet ws;
            using (ExcelPackage xlPackage = new ExcelPackage())
            {
                ws = xlPackage.Workbook.Worksheets.Add(nombreHoja);
                var result = mDetalle(codEntidad, periodo);
                int i = 1;
                int j = 1;
                string texto = encabezado;
                string[] columnas = texto.Split(';');

                foreach (string columna in columnas)
                {
                    ws.Cells[i, j].Value = columna.ToString();
                    j++;
                }

                i = 2;
                j = 1;

                foreach (var b in result)
                {
                    texto = string.Empty;
                    if (b.GetType() == typeof(String))
                        texto = b.ToString();
                    else
                    {
                        foreach (PropertyInfo d in b.GetType().GetProperties().Where(p => p.GetIndexParameters().Length == 0))
                        {
                            texto += d.GetValue(b) + ";";
                        }
                        texto += codEntidad;
                    }

                    string[] filas = texto.Split(';');
                    foreach (string valor in filas)
                    {
                        ws.Cells[i, j].Value = valor;
                        j++;
                    }

                    j = 1;
                    i++;
                }

                string path = dir + "//" + nomEntidad + ".xlsx";
                Stream stream = File.Create(path);
                xlPackage.SaveAs(stream);
                stream.Close();
            }
        }

        private void GenerarTxt<T>(DateTime periodo, String encabezado, String nombreHoja, String dir, String nomEntidad, String codEntidad, Func<string, DateTime?, IEnumerable<T>> mDetalle)
        {
            var result = mDetalle(codEntidad, periodo);
            System.IO.StreamWriter sw = new System.IO.StreamWriter(dir + "\\" + nomEntidad + ".txt", false);
            string texto = string.Empty;
            bool primeraLinea = true;

            foreach (var b in result)
            {
                if (b.GetType() == typeof(String))
                    texto = b.ToString();
                else
                {
                    if (primeraLinea)
                    {
                        sw.WriteLine(encabezado);
                        primeraLinea = false;
                    }

                    foreach (PropertyInfo d in b.GetType().GetProperties().Where(p => p.GetIndexParameters().Length == 0))
                    {
                        texto += d.GetValue(b) + ";";
                    }
                    texto += codEntidad;
                }

                sw.WriteLine(texto);
                texto = string.Empty;
            }

            sw.Close();
            texto = string.Empty;
        }

        private void GenerarComposicionCartera(FGAEntities db, DateTime periodo, string dir, bool excel)
        {
            var entidades = db.FGA_Consultar_Operaciones_Nuevas_Masiva(periodo).Where(o => o.GENERADO.Contains("GENERADO")).ToList();
            string texto = string.Empty;
            string encabezado = "Id;Entidad;Periodo;Operacion;Deudor;Fecha Vencimiento;Tasa;Cuota;Saldo Principal;Saldo Productos;Estado;Cuenta Contable Principal;Cuenta Contable Producto";
            foreach (var a in entidades)
            {
                if (excel)
                    GenerarExcel<FGA_Consultar_Operaciones_Nuevas_Result>(periodo, encabezado, "Balance", dir, a.NOMBRE + "_" + periodo.Month + "_" + periodo.Year, a.ID, db.FGA_Consultar_Operaciones_Nuevas);
                else
                    GenerarTxt<FGA_Consultar_Operaciones_Nuevas_Result>(periodo, encabezado, "Balance", dir, a.NOMBRE + "_" + periodo.Month + "_" + periodo.Year, a.ID, db.FGA_Consultar_Operaciones_Nuevas);
            }
        }

        private void GenerarBalance(FGAEntities db, DateTime periodo, string dir, bool excel)
        {
            var entidades = db.FGA_Consultar_Balance_Comprobacion_Masiva(periodo).Where(o => o.GENERADO.Contains("GENERADO")).ToList();
            string texto = string.Empty;
            string encabezado = "Periodo;Cuenta;Credito;Debito;Saldo Final;Entidad";
            foreach (var a in entidades)
            {
                if (excel)
                    GenerarExcel<FGA_Consultar_Balance_Comprobacion_Result>(periodo, encabezado, "Balance", dir, a.NOMBRE + "_" + periodo.Month + "_" + periodo.Year, a.ID, db.FGA_Consultar_Balance_Comprobacion);
                else
                    GenerarTxt<FGA_Consultar_Balance_Comprobacion_Result>(periodo, encabezado, "Balance", dir, a.NOMBRE + "_" + periodo.Month + "_" + periodo.Year, a.ID, db.FGA_Consultar_Balance_Comprobacion);
            }
        }

        private void GenerarCarteraCredito(FGAEntities db, DateTime periodo, string dir, bool excel)
        {
            var entidades = db.FGA_Consultar_Cartera_Credito_Masiva(periodo).Where(o => o.GENERADO.Contains("GENERADO")).ToList();
            string texto = string.Empty;

            foreach (var a in entidades)
            {
                if (excel)
                    GenerarExcel<FGA_Consultar_Cartera_Credito_Result>(periodo, "Periodo;Operacion;Deudor;Tipo Cartera;Categoria Riesgo;Dias Atraso Alerta;Dias Atraso Matris;Saldo;Entidad",
                                                                                           "CarteraCredito", dir, a.NOMBRE + "_" + periodo.Month + "_" + periodo.Year, a.ID, db.FGA_Consultar_Cartera_Credito);
                else
                    GenerarTxt<FGA_Consultar_Cartera_Credito_Result>(periodo, "Periodo;Operacion;Deudor;Tipo Cartera;Categoria Riesgo;Dias Atraso Alerta;Dias Atraso Matris;Saldo;Entidad",
                                                                                            "CarteraCredito", dir, a.NOMBRE + "_" + periodo.Month + "_" + periodo.Year, a.ID, db.FGA_Consultar_Cartera_Credito);
            }
        }

        private void GenerarMaduracionCartera(FGAEntities db, DateTime periodo, string dir, bool excel)
        {
            var entidades = db.FGA_Consultar_Maduracion_Cartera_Masiva(periodo).Where(o => o.GENERADO.Contains("GENERADO")).ToList();
            string texto = string.Empty;
            string encabezado = "Periodo;Operacion;Tipo Cartera;Categoria;Tasa;Plazo Restante;Saldo;Cuota;Entidad";
            foreach (var a in entidades)
            {
                if (excel)
                    GenerarExcel<FGA_Consultar_Maduracion_Cartera_Result>(periodo, encabezado, "Maduracion", dir, a.NOMBRE + "_" + periodo.Month + "_" + periodo.Year, a.ID, db.FGA_Consultar_Maduracion_Cartera);
                else
                    GenerarTxt<FGA_Consultar_Maduracion_Cartera_Result>(periodo, encabezado, "Maduracion", dir, a.NOMBRE + "_" + periodo.Month + "_" + periodo.Year, a.ID, db.FGA_Consultar_Maduracion_Cartera);
            }
        }

        private void GenerarMaduracionCarteraResumen(FGAEntities db, DateTime periodo, string dir, bool excel)
        {
            var entidades = db.FGA_Consultar_Maduracion_Cartera_Det_Masiva(periodo).Where(o => o.GENERADO.Contains("GENERADO")).ToList();
            string texto = string.Empty;
            string encabezado = "Periodo;Fecha;Total;0-A;0-B;0-C;0-D;0-E;1-A;1-B;1-C;1-D;1-E;2-A;2-B;2-C;2-D;2-E;3-A;3-B;3-C;3-D;3-E;4-A;4-B;4-C;4-D;4-E;5-A;5-B;5-C;5-D;5-E;6-A;" +
                "6-B;6-C;6-D;6-E;7-A;7-B;7-C;7-D;7-E;8-A;8-B;8-C;8-D;8-E;9-A;9-B;9-C;9-D;9-E;10-A;10-B;10-C;10-D;10-E;11-A;11-B;11-C;11-D;11-E;Entidad";
            foreach (var a in entidades)
            {
                if (excel)
                    GenerarExcel<FGA_Consultar_Maduracion_Cartera_Det_Result>(periodo, encabezado, "MaduracionResumen", dir, a.NOMBRE + "_" + periodo.Month + "_" + periodo.Year, a.ID, db.FGA_Consultar_Maduracion_Cartera_Det);
                else
                    GenerarTxt<FGA_Consultar_Maduracion_Cartera_Det_Result>(periodo, encabezado, "MaduracionResumen", dir, a.NOMBRE + "_" + periodo.Month + "_" + periodo.Year, a.ID, db.FGA_Consultar_Maduracion_Cartera_Det);
            }
        }

        private void GenerarRiesgoLiquidez(FGAEntities db, DateTime periodo, string dir, bool excel)
        {
            var entidades = db.FGA_Consultar_Riesgo_Liquidez_Masiva(periodo).Where(o => o.GENERADO.Contains("GENERADO")).ToList();
            string texto = string.Empty;
            string encabezado = "Periodo;Contractual Aumenta;Contractual Disminuye;Contractual Mantiene;Contractual Nuevo;Contractual Cancelado;Deposito Aumenta;Deposito Disminuye;Deposito Mantiene;Deposito Nuevo;Deposito Cancelado;CDP Nuevo;CDP Cancelado;CDP Mantiene";
            foreach (var a in entidades)
            {
                if (excel)
                    GenerarExcel<FGA_Consultar_Riesgo_Liquidez_Result>(periodo, encabezado, "RiesgoLiquidez", dir, a.NOMBRE + "_" + periodo.Month + "_" + periodo.Year, a.ID, db.FGA_Consultar_Riesgo_Liquidez);
                else
                    GenerarTxt<FGA_Consultar_Riesgo_Liquidez_Result>(periodo, encabezado, "RiesgoLiquidez", dir, a.NOMBRE + "_" + periodo.Month + "_" + periodo.Year, a.ID, db.FGA_Consultar_Riesgo_Liquidez);
            }
        }

        //private void GenerarTasaPonderada(FGAEntities db, DateTime periodo, string dir, bool excel)
        //{
        //    var entidades = db.FGA_Consultar_Tasa_Ponderada_Masiva(periodo).Where(o => o.GENERADO.Contains("GENERADO")).ToList();
        //    string texto = string.Empty;
        //    string encabezado = "Periodo;Moneda;Total;Entidad";
        //    foreach (var a in entidades)
        //    {
        //        if (excel)
        //            GenerarExcel<FGA_Consultar_Tasa_Ponderada_Result>(periodo, encabezado, "TasaPonderada", dir, a.NOMBRE + "_" + periodo.Month + "_" + periodo.Year, a.ID, db.FGA_Consultar_Tasa_Ponderada);
        //        else
        //            GenerarTxt<FGA_Consultar_Tasa_Ponderada_Result>(periodo, encabezado, "TasaPonderada", dir, a.NOMBRE + "_" + periodo.Month + "_" + periodo.Year, a.ID, db.FGA_Consultar_Tasa_Ponderada);
        //    }
        //}

        private void CargarSalidas()
        {
            var items = new[] {
                new { Text = "Balance de Comprobación", Value = "1" },
                new { Text = "Cartera de Crédito", Value = "2" },
                new { Text = "Composición de la Cartera", Value = "7" },
                new { Text = "Maduración Cartera", Value = "3" },
                new { Text = "Maduración Resumen", Value = "4" },
                new { Text = "Riesgo de Liquidez", Value = "5" },
                new { Text = "Tasa Ponderada", Value = "6" }
            };

            ddl_salidas.DisplayMember = "Text";
            ddl_salidas.ValueMember = "Value";
            ddl_salidas.DataSource = items;
        }

        private void btn_buscar_Click(object sender, EventArgs e)
        {
            FGAEntities db = new FGAEntities();

            try
            {
                DateTime periodo = this.dt_fechaInicial.Value;
                periodo = new DateTime(periodo.Year, periodo.Month, 1);

                switch (this.ddl_salidas.SelectedValue)
                {
                    case "1":
                        this.grid_Files.DataSource = db.FGA_Consultar_Balance_Comprobacion_Masiva(periodo).Where(o => o.GENERADO.Contains("GENERADO")).ToList();
                        break;
                    case "2":
                        this.grid_Files.DataSource = db.FGA_Consultar_Cartera_Credito_Masiva(periodo).Where(o => o.GENERADO.Contains("GENERADO")).ToList();
                        break;
                    case "3":
                        this.grid_Files.DataSource = db.FGA_Consultar_Maduracion_Cartera_Masiva(periodo).Where(o => o.GENERADO.Contains("GENERADO")).ToList();
                        break;
                    case "4":
                        this.grid_Files.DataSource = db.FGA_Consultar_Maduracion_Cartera_Det_Masiva(periodo).Where(o => o.GENERADO.Contains("GENERADO")).ToList();
                        break;
                    case "5":
                        this.grid_Files.DataSource = db.FGA_Consultar_Riesgo_Liquidez_Masiva(periodo).Where(o => o.GENERADO.Contains("GENERADO")).ToList();
                        break;
                    case "6":
                        this.grid_Files.DataSource = db.FGA_Consultar_Tasa_Ponderada_Masiva(periodo).Where(o => o.GENERADO.Contains("GENERADO")).ToList();
                        break;
                    case "7":
                        this.grid_Files.DataSource = db.FGA_Consultar_Operaciones_Nuevas_Masiva(periodo).Where(o => o.GENERADO.Contains("GENERADO")).ToList();
                        break;
                    default:
                        break;
                }

                this.grid_Files.Columns[1].Width = 300;
                this.grid_Files.Columns[2].Width = 200;
            }
            catch (Exception)
            {
            }
            finally
            {
                db.Dispose();
            }
        }

        private void ddl_salidas_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.ck_Excel.Checked = this.ddl_salidas.SelectedValue.ToString() == "0" ? false : true;
        }
    }
}