namespace Historicos
{
    partial class Ordenar
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
            this.txt_folderOrigen = new System.Windows.Forms.TextBox();
            this.btn_ordenar = new System.Windows.Forms.Button();
            this.btn_browse = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.txt_folderDestino = new System.Windows.Forms.TextBox();
            this.btn_browse_destino = new System.Windows.Forms.Button();
            this.lblAvance = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // txt_folderOrigen
            // 
            this.txt_folderOrigen.Location = new System.Drawing.Point(136, 60);
            this.txt_folderOrigen.Name = "txt_folderOrigen";
            this.txt_folderOrigen.Size = new System.Drawing.Size(616, 20);
            this.txt_folderOrigen.TabIndex = 19;
            // 
            // btn_ordenar
            // 
            this.btn_ordenar.Location = new System.Drawing.Point(772, 157);
            this.btn_ordenar.Name = "btn_ordenar";
            this.btn_ordenar.Size = new System.Drawing.Size(75, 21);
            this.btn_ordenar.TabIndex = 22;
            this.btn_ordenar.Text = "Ordenar";
            this.btn_ordenar.UseVisualStyleBackColor = true;
            this.btn_ordenar.Click += new System.EventHandler(this.Btn_ordenar_Click);
            // 
            // btn_browse
            // 
            this.btn_browse.Location = new System.Drawing.Point(772, 58);
            this.btn_browse.Name = "btn_browse";
            this.btn_browse.Size = new System.Drawing.Size(75, 23);
            this.btn_browse.TabIndex = 20;
            this.btn_browse.Text = "Browse";
            this.btn_browse.UseVisualStyleBackColor = true;
            this.btn_browse.Click += new System.EventHandler(this.Btn_browse_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(24, 58);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(86, 17);
            this.label2.TabIndex = 23;
            this.label2.Text = "Ruta origen:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(24, 108);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(92, 17);
            this.label1.TabIndex = 24;
            this.label1.Text = "Ruta destino:";
            // 
            // txt_folderDestino
            // 
            this.txt_folderDestino.Location = new System.Drawing.Point(136, 110);
            this.txt_folderDestino.Name = "txt_folderDestino";
            this.txt_folderDestino.Size = new System.Drawing.Size(616, 20);
            this.txt_folderDestino.TabIndex = 25;
            // 
            // btn_browse_destino
            // 
            this.btn_browse_destino.Location = new System.Drawing.Point(772, 108);
            this.btn_browse_destino.Name = "btn_browse_destino";
            this.btn_browse_destino.Size = new System.Drawing.Size(75, 23);
            this.btn_browse_destino.TabIndex = 26;
            this.btn_browse_destino.Text = "Browse";
            this.btn_browse_destino.UseVisualStyleBackColor = true;
            this.btn_browse_destino.Click += new System.EventHandler(this.Btn_browse_destino_Click);
            // 
            // lblAvance
            // 
            this.lblAvance.AutoSize = true;
            this.lblAvance.Font = new System.Drawing.Font("Microsoft Sans Serif", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblAvance.ForeColor = System.Drawing.Color.White;
            this.lblAvance.Location = new System.Drawing.Point(24, 157);
            this.lblAvance.Name = "lblAvance";
            this.lblAvance.Size = new System.Drawing.Size(12, 18);
            this.lblAvance.TabIndex = 27;
            this.lblAvance.Text = ".";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 60F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.ForeColor = System.Drawing.SystemColors.ControlLight;
            this.label3.Location = new System.Drawing.Point(184, 239);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(526, 91);
            this.label3.TabIndex = 28;
            this.label3.Text = "FGA CONFIA";
            // 
            // Ordenar
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(39)))), ((int)(((byte)(57)))), ((int)(((byte)(80)))));
            this.ClientSize = new System.Drawing.Size(870, 481);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.lblAvance);
            this.Controls.Add(this.btn_browse_destino);
            this.Controls.Add(this.txt_folderDestino);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txt_folderOrigen);
            this.Controls.Add(this.btn_ordenar);
            this.Controls.Add(this.btn_browse);
            this.Name = "Ordenar";
            this.Text = "Ordenar";
            this.Load += new System.EventHandler(this.Ordenar_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.TextBox txt_folderOrigen;
        private System.Windows.Forms.Button btn_ordenar;
        private System.Windows.Forms.Button btn_browse;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txt_folderDestino;
        private System.Windows.Forms.Button btn_browse_destino;
        private System.Windows.Forms.Label lblAvance;
        private System.Windows.Forms.Label label3;
    }
}