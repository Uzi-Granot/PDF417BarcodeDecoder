namespace Pdf417VideoDecoderDemo
{
	partial class Pdf417VideoDecoder
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
			this.PreviewPanel = new System.Windows.Forms.Panel();
			this.DataTextBox = new System.Windows.Forms.TextBox();
			this.DecodedDataLabel = new System.Windows.Forms.Label();
			this.ResetButton = new System.Windows.Forms.Button();
			this.GoToUriButton = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// PreviewPanel
			// 
			this.PreviewPanel.Location = new System.Drawing.Point(14, 12);
			this.PreviewPanel.Name = "PreviewPanel";
			this.PreviewPanel.Size = new System.Drawing.Size(640, 360);
			this.PreviewPanel.TabIndex = 0;
			// 
			// DataTextBox
			// 
			this.DataTextBox.AcceptsReturn = true;
			this.DataTextBox.BackColor = System.Drawing.SystemColors.Info;
			this.DataTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.DataTextBox.Cursor = System.Windows.Forms.Cursors.Default;
			this.DataTextBox.Location = new System.Drawing.Point(12, 394);
			this.DataTextBox.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.DataTextBox.Multiline = true;
			this.DataTextBox.Name = "DataTextBox";
			this.DataTextBox.ReadOnly = true;
			this.DataTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.DataTextBox.Size = new System.Drawing.Size(637, 74);
			this.DataTextBox.TabIndex = 2;
			this.DataTextBox.TabStop = false;
			this.DataTextBox.Text = "\r\n";
			// 
			// DecodedDataLabel
			// 
			this.DecodedDataLabel.AutoSize = true;
			this.DecodedDataLabel.Location = new System.Drawing.Point(11, 374);
			this.DecodedDataLabel.Name = "DecodedDataLabel";
			this.DecodedDataLabel.Size = new System.Drawing.Size(88, 16);
			this.DecodedDataLabel.TabIndex = 1;
			this.DecodedDataLabel.Text = "Decoded data";
			// 
			// ResetButton
			// 
			this.ResetButton.Location = new System.Drawing.Point(213, 487);
			this.ResetButton.Name = "ResetButton";
			this.ResetButton.Size = new System.Drawing.Size(110, 39);
			this.ResetButton.TabIndex = 3;
			this.ResetButton.Text = "Reset";
			this.ResetButton.UseVisualStyleBackColor = true;
			this.ResetButton.Click += new System.EventHandler(this.OnResetButton);
			// 
			// GoToUriButton
			// 
			this.GoToUriButton.Location = new System.Drawing.Point(348, 487);
			this.GoToUriButton.Name = "GoToUriButton";
			this.GoToUriButton.Size = new System.Drawing.Size(110, 39);
			this.GoToUriButton.TabIndex = 4;
			this.GoToUriButton.Text = "Go to URI";
			this.GoToUriButton.UseVisualStyleBackColor = true;
			this.GoToUriButton.Click += new System.EventHandler(this.OnGoToUri);
			// 
			// Pdf417VideoDecoder
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = System.Drawing.SystemColors.GradientInactiveCaption;
			this.ClientSize = new System.Drawing.Size(670, 538);
			this.Controls.Add(this.GoToUriButton);
			this.Controls.Add(this.DataTextBox);
			this.Controls.Add(this.DecodedDataLabel);
			this.Controls.Add(this.ResetButton);
			this.Controls.Add(this.PreviewPanel);
			this.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.Name = "Pdf417VideoDecoder";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Form1";
			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.OnClosing);
			this.Load += new System.EventHandler(this.OnLoad);
			this.Resize += new System.EventHandler(this.OnResize);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Panel PreviewPanel;
		private System.Windows.Forms.TextBox DataTextBox;
		private System.Windows.Forms.Label DecodedDataLabel;
		private System.Windows.Forms.Button ResetButton;
		private System.Windows.Forms.Button GoToUriButton;
	}
}

