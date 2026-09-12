using FGA.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Historicos
{
    public partial class Ordenar : Form
    {
        public Ordenar()
        {
            InitializeComponent();
        }

        private void Btn_ordenar_Click(object sender, EventArgs e)
        {
            int num = 0;
            lblAvance.Text = "Procesando archivos. Por favor espere ...: ";
            lblAvance.Refresh();

            DirectorySearch(txt_folderOrigen.Text, ref num);

            lblAvance.Text = "Archivos leidos. ";
            lblAvance.Refresh();
        }

        public void DirectorySearch(string dirOrigen, ref int num)
        {
            try
            {
                DateTime fecha = DateTime.Now;
                String entidad = string.Empty;
                String mensaje = string.Empty;
                FGA.Models.File file;

                foreach (string f in Directory.GetFiles(dirOrigen))
                {
                    try
                    {
                        file = new FGA.Models.File();
                        if (ProcessFile.GetInfoFile(f, ref file))
                        {
                            file.Ruta = f;
                            var path = Path.Combine(txt_folderDestino.Text, file.Entidad);
                            bool exists = System.IO.Directory.Exists(path);

                            if (!exists)
                                System.IO.Directory.CreateDirectory(path);

                            path = Path.Combine(path, file.Periodo.ToString("yyyy"));
                            exists = System.IO.Directory.Exists(path);

                            if (!exists)
                                System.IO.Directory.CreateDirectory(path);

                            path = Path.Combine(path, FGA.Utility.Utilitarios.toUpperFirstLetter(file.Periodo.ToString("MMMM")));
                            exists = System.IO.Directory.Exists(path);

                            if (!exists)
                                System.IO.Directory.CreateDirectory(path);

                            lblAvance.Text = "Procesando archivo: " + file.Archivo + ".xml Periodo: " + file.Periodo.ToShortDateString();
                            lblAvance.Refresh();

                            var xmlPath = Path.Combine(path, file.Archivo + ".xml");

                            try {
                                System.IO.File.Copy(f, xmlPath);
                            }
                            catch (Exception) {

                                path = Path.Combine(path, "duplicados");
                                exists = System.IO.Directory.Exists(path);

                                if (!exists)
                                    System.IO.Directory.CreateDirectory(path);

                                xmlPath = Path.Combine(path, file.Archivo + "_" + num.ToString() + ".xml");
                                System.IO.File.Copy(f, xmlPath);
                            }
                        }

                        num += 1;
                    }
                    catch (Exception ex) {
                        MessageBox.Show("Reporte el error: " + ex.Message, "FGA", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }

                }
                foreach (string d in Directory.GetDirectories(dirOrigen))
                {
                    DirectorySearch(d, ref num);
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show("Reporte el error: " + ex.Message, "FGA", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void Ordenar_Load(object sender, EventArgs e)
        {
        }

        private void Btn_browse_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderDlg = new FolderBrowserDialog();
            folderDlg.ShowNewFolderButton = true;
            DialogResult result = folderDlg.ShowDialog();
            if (result == DialogResult.OK)
            {
                txt_folderOrigen.Text = folderDlg.SelectedPath;
                Environment.SpecialFolder root = folderDlg.RootFolder;
            }
        }

        private void Btn_browse_destino_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog folderDlg = new FolderBrowserDialog();
            folderDlg.ShowNewFolderButton = true;
            DialogResult result = folderDlg.ShowDialog();
            if (result == DialogResult.OK)
            {
                txt_folderDestino.Text = folderDlg.SelectedPath;
                Environment.SpecialFolder root = folderDlg.RootFolder;
            }
        }
    }
}
