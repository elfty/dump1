namespace MarketQADataProcessorApp
{
	partial class formMain
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
            this.buttonRun = new System.Windows.Forms.Button();
            this.listboxMemo = new System.Windows.Forms.ListBox();
            this.numericInsertRows = new System.Windows.Forms.NumericUpDown();
            this.checkboxTestMode = new System.Windows.Forms.CheckBox();
            this.label1 = new System.Windows.Forms.Label();
            this.id_imnt = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.numericInsertRows)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonRun
            // 
            this.buttonRun.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonRun.Location = new System.Drawing.Point(648, 72);
            this.buttonRun.Name = "buttonRun";
            this.buttonRun.Size = new System.Drawing.Size(75, 23);
            this.buttonRun.TabIndex = 1;
            this.buttonRun.Text = "Run";
            this.buttonRun.UseVisualStyleBackColor = true;
            this.buttonRun.Click += new System.EventHandler(this.buttonRun_Click);
            // 
            // listboxMemo
            // 
            this.listboxMemo.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.listboxMemo.FormattingEnabled = true;
            this.listboxMemo.Location = new System.Drawing.Point(0, 101);
            this.listboxMemo.Name = "listboxMemo";
            this.listboxMemo.Size = new System.Drawing.Size(735, 303);
            this.listboxMemo.TabIndex = 52;
            // 
            // numericInsertRows
            // 
            this.numericInsertRows.Location = new System.Drawing.Point(148, 12);
            this.numericInsertRows.Maximum = new decimal(new int[] {
            10000,
            0,
            0,
            0});
            this.numericInsertRows.Name = "numericInsertRows";
            this.numericInsertRows.Size = new System.Drawing.Size(62, 20);
            this.numericInsertRows.TabIndex = 54;
            this.numericInsertRows.Value = new decimal(new int[] {
            10,
            0,
            0,
            0});
            // 
            // checkboxTestMode
            // 
            this.checkboxTestMode.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.checkboxTestMode.AutoSize = true;
            this.checkboxTestMode.Location = new System.Drawing.Point(646, 49);
            this.checkboxTestMode.Name = "checkboxTestMode";
            this.checkboxTestMode.Size = new System.Drawing.Size(77, 17);
            this.checkboxTestMode.TabIndex = 55;
            this.checkboxTestMode.Text = "Test Mode";
            this.checkboxTestMode.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 14);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(130, 13);
            this.label1.TabIndex = 56;
            this.label1.Text = "Number of Rows to Import";
            // 
            // id_imnt
            // 
            this.id_imnt.Location = new System.Drawing.Point(646, 23);
            this.id_imnt.Name = "id_imnt";
            this.id_imnt.Size = new System.Drawing.Size(77, 20);
            this.id_imnt.TabIndex = 57;
            this.id_imnt.Text = "Enter id_imnt";
            // 
            // formMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(735, 409);
            this.Controls.Add(this.id_imnt);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.checkboxTestMode);
            this.Controls.Add(this.numericInsertRows);
            this.Controls.Add(this.listboxMemo);
            this.Controls.Add(this.buttonRun);
            this.Name = "formMain";
            this.Text = "MarketQA";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.formMain_FormClosing);
            this.Load += new System.EventHandler(this.formMain_Load);
            ((System.ComponentModel.ISupportInitialize)(this.numericInsertRows)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Button buttonRun;
		private System.Windows.Forms.ListBox listboxMemo;
		private System.Windows.Forms.NumericUpDown numericInsertRows;
		private System.Windows.Forms.CheckBox checkboxTestMode;
		private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox id_imnt;

	}
}

