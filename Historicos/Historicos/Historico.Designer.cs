namespace Historicos
{
    partial class frm_historicos
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
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.txt_folder = new System.Windows.Forms.TextBox();
            this.btn_browse = new System.Windows.Forms.Button();
            this.grid_Files = new System.Windows.Forms.DataGridView();
            this.btn_leer = new System.Windows.Forms.Button();
            this.lbl_avance = new System.Windows.Forms.Label();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabInfo = new System.Windows.Forms.TabPage();
            this.dt_fechaFinal = new System.Windows.Forms.DateTimePicker();
            this.label2 = new System.Windows.Forms.Label();
            this.btn_Ejecutar = new System.Windows.Forms.Button();
            this.lblAvance = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.tabErrores = new System.Windows.Forms.TabPage();
            this.grid_Errores = new System.Windows.Forms.DataGridView();
            ((System.ComponentModel.ISupportInitialize)(this.grid_Files)).BeginInit();
            this.tabControl1.SuspendLayout();
            this.tabInfo.SuspendLayout();
            this.tabErrores.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.grid_Errores)).BeginInit();
            this.SuspendLayout();
            // 
            // txt_folder
            // 
            this.txt_folder.Location = new System.Drawing.Point(14, 9);
            this.txt_folder.Name = "txt_folder";
            this.txt_folder.Size = new System.Drawing.Size(400, 20);
            this.txt_folder.TabIndex = 5;
            // 
            // btn_browse
            // 
            this.btn_browse.Location = new System.Drawing.Point(420, 6);
            this.btn_browse.Name = "btn_browse";
            this.btn_browse.Size = new System.Drawing.Size(75, 23);
            this.btn_browse.TabIndex = 6;
            this.btn_browse.Text = "Browse";
            this.btn_browse.UseVisualStyleBackColor = true;
            this.btn_browse.Click += new System.EventHandler(this.btn_browse_Click);
            // 
            // grid_Files
            // 
            this.grid_Files.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grid_Files.Location = new System.Drawing.Point(14, 32);
            this.grid_Files.Name = "grid_Files";
            this.grid_Files.Size = new System.Drawing.Size(779, 362);
            this.grid_Files.TabIndex = 7;
            // 
            // btn_leer
            // 
            this.btn_leer.Location = new System.Drawing.Point(637, 400);
            this.btn_leer.Name = "btn_leer";
            this.btn_leer.Size = new System.Drawing.Size(75, 23);
            this.btn_leer.TabIndex = 9;
            this.btn_leer.Text = "Leer";
            this.btn_leer.UseVisualStyleBackColor = true;
            this.btn_leer.Click += new System.EventHandler(this.btn_leer_Click);
            // 
            // lbl_avance
            // 
            this.lbl_avance.AutoSize = true;
            this.lbl_avance.Location = new System.Drawing.Point(284, 386);
            this.lbl_avance.Name = "lbl_avance";
            this.lbl_avance.Size = new System.Drawing.Size(0, 13);
            this.lbl_avance.TabIndex = 10;
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabInfo);
            this.tabControl1.Controls.Add(this.tabErrores);
            this.tabControl1.Location = new System.Drawing.Point(12, 12);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(828, 472);
            this.tabControl1.TabIndex = 11;
            // 
            // tabInfo
            // 
            this.tabInfo.Controls.Add(this.dt_fechaFinal);
            this.tabInfo.Controls.Add(this.label2);
            this.tabInfo.Controls.Add(this.btn_Ejecutar);
            this.tabInfo.Controls.Add(this.lblAvance);
            this.tabInfo.Controls.Add(this.label1);
            this.tabInfo.Controls.Add(this.txt_folder);
            this.tabInfo.Controls.Add(this.grid_Files);
            this.tabInfo.Controls.Add(this.btn_leer);
            this.tabInfo.Controls.Add(this.btn_browse);
            this.tabInfo.Location = new System.Drawing.Point(4, 22);
            this.tabInfo.Name = "tabInfo";
            this.tabInfo.Padding = new System.Windows.Forms.Padding(3);
            this.tabInfo.Size = new System.Drawing.Size(820, 446);
            this.tabInfo.TabIndex = 0;
            this.tabInfo.Text = "Leer Archivos";
            this.tabInfo.UseVisualStyleBackColor = true;
            // 
            // dt_fechaFinal
            // 
            this.dt_fechaFinal.CustomFormat = "MM//YYYYY";
            this.dt_fechaFinal.Location = new System.Drawing.Point(585, 10);
            this.dt_fechaFinal.Name = "dt_fechaFinal";
            this.dt_fechaFinal.Size = new System.Drawing.Size(208, 20);
            this.dt_fechaFinal.TabIndex = 18;
            this.dt_fechaFinal.Value = new System.DateTime(2019, 9, 1, 0, 0, 0, 0);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(501, 12);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(78, 13);
            this.label2.TabIndex = 17;
            this.label2.Text = "Fecha máxima:";
            // 
            // btn_Ejecutar
            // 
            this.btn_Ejecutar.Location = new System.Drawing.Point(718, 400);
            this.btn_Ejecutar.Name = "btn_Ejecutar";
            this.btn_Ejecutar.Size = new System.Drawing.Size(75, 23);
            this.btn_Ejecutar.TabIndex = 12;
            this.btn_Ejecutar.Text = "Ejecutar";
            this.btn_Ejecutar.UseVisualStyleBackColor = true;
            this.btn_Ejecutar.Click += new System.EventHandler(this.btn_Ejecutar_Click_1);
            // 
            // lblAvance
            // 
            this.lblAvance.AutoSize = true;
            this.lblAvance.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.lblAvance.ForeColor = System.Drawing.Color.Red;
            this.lblAvance.Location = new System.Drawing.Point(18, 397);
            this.lblAvance.Name = "lblAvance";
            this.lblAvance.Size = new System.Drawing.Size(10, 15);
            this.lblAvance.TabIndex = 11;
            this.lblAvance.Text = ".";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.Red;
            this.label1.Location = new System.Drawing.Point(11, 397);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(0, 15);
            this.label1.TabIndex = 10;
            // 
            // tabErrores
            // 
            this.tabErrores.Controls.Add(this.grid_Errores);
            this.tabErrores.Location = new System.Drawing.Point(4, 22);
            this.tabErrores.Name = "tabErrores";
            this.tabErrores.Padding = new System.Windows.Forms.Padding(3);
            this.tabErrores.Size = new System.Drawing.Size(820, 446);
            this.tabErrores.TabIndex = 1;
            this.tabErrores.Text = "Errores";
            this.tabErrores.UseVisualStyleBackColor = true;
            // 
            // grid_Errores
            // 
            this.grid_Errores.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.grid_Errores.Location = new System.Drawing.Point(6, 6);
            this.grid_Errores.Name = "grid_Errores";
            this.grid_Errores.Size = new System.Drawing.Size(798, 437);
            this.grid_Errores.TabIndex = 8;
            // 
            // frm_historicos
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(852, 481);
            this.Controls.Add(this.tabControl1);
            this.Controls.Add(this.lbl_avance);
            this.Name = "frm_historicos";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Históricos";
            ((System.ComponentModel.ISupportInitialize)(this.grid_Files)).EndInit();
            this.tabControl1.ResumeLayout(false);
            this.tabInfo.ResumeLayout(false);
            this.tabInfo.PerformLayout();
            this.tabErrores.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.grid_Errores)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.TextBox txt_folder;
        private System.Windows.Forms.Button btn_browse;
        private System.Windows.Forms.DataGridView grid_Files;
        private System.Windows.Forms.Button btn_leer;
        private System.Windows.Forms.Label lbl_avance;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabInfo;
        private System.Windows.Forms.TabPage tabErrores;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblAvance;
        private System.Windows.Forms.DataGridView grid_Errores;
        private System.Windows.Forms.Button btn_Ejecutar;
        private System.Windows.Forms.DateTimePicker dt_fechaFinal;
        private System.Windows.Forms.Label label2;
    }
}

