using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace Historicos
{
    public partial class FormMenu : Form
    {
        public FormMenu()
        {
            InitializeComponent();
        }
        private void iconCerrar_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Está seguro de cerrar el sistema?", "FGA", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                //tus codigos
                Application.Exit();
            }
            else { 
            //tus codigos
            }
        }
        
        private void AbrirFormInPanel(object formHijo)
        {
            if (this.panelContenedor.Controls.Count > 0)
                this.panelContenedor.Controls.RemoveAt(0);
            Form fh = formHijo as Form;
            fh.TopLevel = false;
            fh.FormBorderStyle = FormBorderStyle.None;
            fh.Dock = DockStyle.Fill;
            this.panelContenedor.Controls.Add(fh);
            this.panelContenedor.Tag = fh;
            fh.Show();
        }
        int LX, LY,SW,SH;
        private void iconmaximizar_Click(object sender, EventArgs e)
        {
            //this.WindowState = FormWindowState.Maximized;
            LX = this.Location.X;
            LY = this.Location.Y;
            SW = this.Size.Width;
            SH = this.Size.Height;
            this.Size = Screen.PrimaryScreen.WorkingArea.Size;
            this.Location = Screen.PrimaryScreen.WorkingArea.Location;
            iconmaximizar.Visible = false;
            iconrestaurar.Visible = true;
        }

        private void iconrestaurar_Click(object sender, EventArgs e)
        {
            //this.WindowState = FormWindowState.Normal;
            this.Size = new Size(SW, SH);
            this.Location = new Point(LX,LY);
            iconmaximizar.Visible = true;
            iconrestaurar.Visible = false;
        }

        private void iconMinimizar_Click(object sender, EventArgs e)
        {
            this.WindowState = FormWindowState.Minimized;
        }

        private void btnmenu_Click(object sender, EventArgs e)
        {
            if (MenuVertical.Width == 57)
            {
                MenuVertical.Width = 250;
            }
            else

                MenuVertical.Width = 57;
        }

        public void btnINICIO_Click(object sender, EventArgs e)
        {
            //AbrirFormInPanel(new FormLogo());
        }
         

        private void Form1_Load(object sender, EventArgs e)
        {
            mostrarlogo();
        }
        private void mostrarlogo() {
            AbrirFormInPanel(new FormLogo());
        }

        private void Ordenar_Click(object sender, EventArgs e)
        {
            Historicos.Ordenar frm = new Historicos.Ordenar();
            frm.FormClosed += new FormClosedEventHandler(mostrarlogoAlCerrarForm);
            AbrirFormInPanel(frm);
        }

        private void btnREPORTES_Click(object sender, EventArgs e)
        {
            Historicos.Salidas frm = new Historicos.Salidas();
            frm.FormClosed += new FormClosedEventHandler(mostrarlogoAlCerrarForm);
            AbrirFormInPanel(frm);
        }

        private void btnCarga_Click(object sender, EventArgs e)
        {
            Historicos.frm_historicos frm = new Historicos.frm_historicos();
            frm.FormClosed += new FormClosedEventHandler(mostrarlogoAlCerrarForm);
            AbrirFormInPanel(frm);
        }

        private void mostrarlogoAlCerrarForm(object sender, FormClosedEventArgs e) {
            mostrarlogo();
        }
                   
    }
}
