namespace CEGVN.TMP
{
    partial class FormSelectElements
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
            this.Cancel = new System.Windows.Forms.Button();
            this.OK = new System.Windows.Forms.Button();
            this.listView2 = new System.Windows.Forms.ListView();
            this.zzz = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Mark = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Number = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Name = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.listView1 = new System.Windows.Forms.ListView();
            this.STT1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.Assembly = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.Control_Mark = new System.Windows.Forms.TextBox();
            this.Assemblies = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // Cancel
            // 
            this.Cancel.AutoEllipsis = true;
            this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel.Location = new System.Drawing.Point(325, 520);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(80, 25);
            this.Cancel.TabIndex = 11;
            this.Cancel.Text = "Cancel";
            this.Cancel.UseVisualStyleBackColor = true;
            // 
            // OK
            // 
            this.OK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OK.Location = new System.Drawing.Point(234, 520);
            this.OK.Name = "OK";
            this.OK.Size = new System.Drawing.Size(80, 25);
            this.OK.TabIndex = 12;
            this.OK.Text = "OK";
            this.OK.UseVisualStyleBackColor = true;
            this.OK.Click += new System.EventHandler(this.OK_Click);
            // 
            // listView2
            // 
            this.listView2.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.zzz,
            this.Mark,
            this.Number,
            this.Name});
            this.listView2.FullRowSelect = true;
            this.listView2.HideSelection = false;
            this.listView2.Location = new System.Drawing.Point(212, 78);
            this.listView2.Name = "listView2";
            this.listView2.Size = new System.Drawing.Size(446, 422);
            this.listView2.TabIndex = 9;
            this.listView2.UseCompatibleStateImageBehavior = false;
            // 
            // zzz
            // 
            this.zzz.Text = "zzz";
            this.zzz.Width = 0;
            // 
            // Mark
            // 
            this.Mark.Text = "Mark";
            this.Mark.Width = 80;
            // 
            // Number
            // 
            this.Number.Text = "Number";
            this.Number.Width = 80;
            // 
            // Name
            // 
            this.Name.Text = "Name";
            this.Name.Width = 300;
            // 
            // listView1
            // 
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.STT1,
            this.Assembly});
            this.listView1.FullRowSelect = true;
            this.listView1.HideSelection = false;
            this.listView1.Location = new System.Drawing.Point(25, 78);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(154, 422);
            this.listView1.TabIndex = 10;
            this.listView1.UseCompatibleStateImageBehavior = false;
            // 
            // STT1
            // 
            this.STT1.Text = "STT";
            this.STT1.Width = 0;
            // 
            // Assembly
            // 
            this.Assembly.Text = "Assembly";
            this.Assembly.Width = 150;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(419, 11);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(70, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Control_Mark";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(73, 11);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(59, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "Assemblies";
            // 
            // Control_Mark
            // 
            this.Control_Mark.Location = new System.Drawing.Point(212, 35);
            this.Control_Mark.Name = "Control_Mark";
            this.Control_Mark.Size = new System.Drawing.Size(446, 20);
            this.Control_Mark.TabIndex = 5;
            this.Control_Mark.TextChanged += new System.EventHandler(this.Control_Mark_TextChanged);
            // 
            // Assemblies
            // 
            this.Assemblies.Location = new System.Drawing.Point(25, 35);
            this.Assemblies.Name = "Assemblies";
            this.Assemblies.Size = new System.Drawing.Size(154, 20);
            this.Assemblies.TabIndex = 6;
            this.Assemblies.TextChanged += new System.EventHandler(this.Assemblies_TextChanged);
            // 
            // FormSelectElements
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(683, 559);
            this.Controls.Add(this.Cancel);
            this.Controls.Add(this.OK);
            this.Controls.Add(this.listView2);
            this.Controls.Add(this.listView1);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.Control_Mark);
            this.Controls.Add(this.Assemblies);
            //this.Name = "FormSelectElements";
            this.Text = "FormSelectElements";
            this.Load += new System.EventHandler(this.FormSelectElements_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button Cancel;
        private System.Windows.Forms.Button OK;
        private System.Windows.Forms.ListView listView2;
        private System.Windows.Forms.ColumnHeader zzz;
        private System.Windows.Forms.ColumnHeader Mark;
        private System.Windows.Forms.ColumnHeader Number;
        private System.Windows.Forms.ColumnHeader Name;
        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader STT1;
        private System.Windows.Forms.ColumnHeader Assembly;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox Control_Mark;
        private System.Windows.Forms.TextBox Assemblies;
    }
}