namespace Historicos
{
    partial class Salidas
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.txt_folder = new System.Windows.Forms.TextBox();
            this.btn_browse = new System.Windows.Forms.Button();
            this.btn_Ejecutar = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.dt_fechaFinal = new System.Windows.Forms.DateTimePicker();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.ddl_salidas = new System.Windows.Forms.ComboBox();
            this.btn_buscar = new System.Windows.Forms.Button();
            this.progress_bar = new System.Windows.Forms.ProgressBar();
            this.dt_fechaInicial = new System.Windows.Forms.DateTimePicker();
            this.label4 = new System.Windows.Forms.Label();
            this.grid_Files = new System.Windows.Forms.DataGridView();
            this.ck_Excel = new System.Windows.Forms.CheckBox();
            ((System.ComponentModel.ISupportInitialize)(this.grid_Files)).BeginInit();
            this.SuspendLayout();
            // 
            // txt_folder
            // 
            this.txt_folder.Location = new System.Drawing.Point(450, 22);
            this.txt_folder.Name = "txt_folder";
            this.txt_folder.Size = new System.Drawing.Size(270, 20);
            this.txt_folder.TabIndex = 10;
            // 
            // btn_browse
            // 
            this.btn_browse.Location = new System.Drawing.Point(750, 20);
            this.btn_browse.Name = "btn_browse";
            this.btn_browse.Size = new System.Drawing.Size(62, 23);
            this.btn_browse.TabIndex = 11;
            this.btn_browse.Text = "Browse";
            this.btn_browse.UseVisualStyleBackColor = true;
            this.btn_browse.Click += new System.EventHandler(this.btn_browse_Click);
            // 
            // btn_Ejecutar
            // 
            this.btn_Ejecutar.Location = new System.Drawing.Point(752, 462);
            this.btn_Ejecutar.Name = "btn_Ejecutar";
            this.btn_Ejecutar.Size = new System.Drawing.Size(65, 23);
            this.btn_Ejecutar.TabIndex = 14;
            this.btn_Ejecutar.Text = "Ejecutar";
            this.btn_Ejecutar.UseVisualStyleBackColor = true;
            this.btn_Ejecutar.Click += new System.EventHandler(this.btn_Ejecutar_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(382, 65);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(62, 13);
            this.label1.TabIndex = 15;
            this.label1.Text = "Fecha Final";
            // 
            // dt_fechaFinal
            // 
            this.dt_fechaFinal.CustomFormat = "MM//YYYYY";
            this.dt_fechaFinal.Location = new System.Drawing.Point(450, 60);
            this.dt_fechaFinal.Name = "dt_fechaFinal";
            this.dt_fechaFinal.Size = new System.Drawing.Size(270, 20);
            this.dt_fechaFinal.TabIndex = 16;
            this.dt_fechaFinal.Value = new System.DateTime(2019, 9, 1, 0, 0, 0, 0);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(382, 29);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(44, 13);
            this.label2.TabIndex = 17;
            this.label2.Text = "Carpeta";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(26, 29);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(36, 13);
            this.label3.TabIndex = 18;
            this.label3.Text = "Salida";
            // 
            // ddl_salidas
            // 
            this.ddl_salidas.FormattingEnabled = true;
            this.ddl_salidas.Location = new System.Drawing.Point(94, 21);
            this.ddl_salidas.Name = "ddl_salidas";
            this.ddl_salidas.Size = new System.Drawing.Size(270, 21);
            this.ddl_salidas.TabIndex = 19;
            this.ddl_salidas.SelectedIndexChanged += new System.EventHandler(this.ddl_salidas_SelectedIndexChanged);
            // 
            // btn_buscar
            // 
            this.btn_buscar.Location = new System.Drawing.Point(750, 60);
            this.btn_buscar.Name = "btn_buscar";
            this.btn_buscar.Size = new System.Drawing.Size(62, 23);
            this.btn_buscar.TabIndex = 20;
            this.btn_buscar.Text = "Buscar";
            this.btn_buscar.UseVisualStyleBackColor = true;
            this.btn_buscar.Click += new System.EventHandler(this.btn_buscar_Click);
            // 
            // progress_bar
            // 
            this.progress_bar.Location = new System.Drawing.Point(29, 462);
            this.progress_bar.Name = "progress_bar";
            this.progress_bar.Size = new System.Drawing.Size(717, 23);
            this.progress_bar.TabIndex = 21;
            // 
            // dt_fechaInicial
            // 
            this.dt_fechaInicial.CustomFormat = "MM//YYYYY";
            this.dt_fechaInicial.Location = new System.Drawing.Point(94, 60);
            this.dt_fechaInicial.Name = "dt_fechaInicial";
            this.dt_fechaInicial.Size = new System.Drawing.Size(270, 20);
            this.dt_fechaInicial.TabIndex = 23;
            this.dt_fechaInicial.Value = new System.DateTime(2019, 2, 11, 12, 53, 51, 0);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(26, 65);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(67, 13);
            this.label4.TabIndex = 22;
            this.label4.Text = "Fecha Inicial";
            // 
            // grid_Files
            // 
            this.grid_Files.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grid_Files.Location = new System.Drawing.Point(29, 128);
            this.grid_Files.Name = "grid_Files";
            this.grid_Files.Size = new System.Drawing.Size(788, 318);
            this.grid_Files.TabIndex = 12;
            // 
            // ck_Excel
            // 
            this.ck_Excel.AutoSize = true;
            this.ck_Excel.Location = new System.Drawing.Point(29, 96);
            this.ck_Excel.Name = "ck_Excel";
            this.ck_Excel.Size = new System.Drawing.Size(52, 17);
            this.ck_Excel.TabIndex = 24;
            this.ck_Excel.Text = "Excel";
            this.ck_Excel.UseVisualStyleBackColor = true;
            // 
            // Salidas
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(849, 501);
            this.Controls.Add(this.ck_Excel);
            this.Controls.Add(this.dt_fechaInicial);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.progress_bar);
            this.Controls.Add(this.btn_buscar);
            this.Controls.Add(this.ddl_salidas);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.dt_fechaFinal);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btn_Ejecutar);
            this.Controls.Add(this.txt_folder);
            this.Controls.Add(this.grid_Files);
            this.Controls.Add(this.btn_browse);
            this.Name = "Salidas";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Salidas";
            ((System.ComponentModel.ISupportInitialize)(this.grid_Files)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txt_folder;
        private System.Windows.Forms.Button btn_browse;
        private System.Windows.Forms.Button btn_Ejecutar;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.DateTimePicker dt_fechaFinal;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ComboBox ddl_salidas;
        private System.Windows.Forms.Button btn_buscar;
        private System.Windows.Forms.ProgressBar progress_bar;
        private System.Windows.Forms.DateTimePicker dt_fechaInicial;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.DataGridView grid_Files;
        private System.Windows.Forms.CheckBox ck_Excel;
    }
}