namespace MarketQADataProcessorApp
{
	partial class BaseMainForm
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
			this.grpbxMemo = new System.Windows.Forms.GroupBox();
			this.listviewMemo = new System.Windows.Forms.ListView();
			this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
			this.buttonRun = new System.Windows.Forms.Button();
			this.grpbxMemo.SuspendLayout();
			this.SuspendLayout();
			// 
			// grpbxMemo
			// 
			this.grpbxMemo.Controls.Add(this.listviewMemo);
			this.grpbxMemo.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.grpbxMemo.Location = new System.Drawing.Point(0, 154);
			this.grpbxMemo.Name = "grpbxMemo";
			this.grpbxMemo.Size = new System.Drawing.Size(687, 174);
			this.grpbxMemo.TabIndex = 48;
			this.grpbxMemo.TabStop = false;
			this.grpbxMemo.Text = "Memo";
			// 
			// listviewMemo
			// 
			this.listviewMemo.BackColor = System.Drawing.SystemColors.InactiveCaptionText;
			this.listviewMemo.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.listviewMemo.CausesValidation = false;
			this.listviewMemo.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
			this.listviewMemo.Dock = System.Windows.Forms.DockStyle.Fill;
			this.listviewMemo.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
			this.listviewMemo.Location = new System.Drawing.Point(3, 16);
			this.listviewMemo.MultiSelect = false;
			this.listviewMemo.Name = "listviewMemo";
			this.listviewMemo.ShowGroups = false;
			this.listviewMemo.Size = new System.Drawing.Size(681, 155);
			this.listviewMemo.TabIndex = 1;
			this.listviewMemo.UseCompatibleStateImageBehavior = false;
			this.listviewMemo.View = System.Windows.Forms.View.Details;
			// 
			// columnHeader1
			// 
			this.columnHeader1.Width = 420;
			// 
			// buttonRun
			// 
			this.buttonRun.Location = new System.Drawing.Point(600, 125);
			this.buttonRun.Name = "buttonRun";
			this.buttonRun.Size = new System.Drawing.Size(75, 23);
			this.buttonRun.TabIndex = 50;
			this.buttonRun.Text = "Run";
			this.buttonRun.UseVisualStyleBackColor = true;
			// 
			// BaseMainForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(687, 328);
			this.Controls.Add(this.buttonRun);
			this.Controls.Add(this.grpbxMemo);
			this.Name = "BaseMainForm";
			this.Text = "BaseForm";
			this.grpbxMemo.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private System.Windows.Forms.GroupBox grpbxMemo;
		private System.Windows.Forms.ListView listviewMemo;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.Button buttonRun;

	}
}