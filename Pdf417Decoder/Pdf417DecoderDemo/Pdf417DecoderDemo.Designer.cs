namespace Pdf417DecoderDemo
{
	partial class Pdf417DecoderDemo
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
			if(disposing && (components != null))
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
			this.ImageFileLabel = new System.Windows.Forms.Label();
			this.LoadImageButton = new System.Windows.Forms.Button();
			this.DataTextBox = new System.Windows.Forms.TextBox();
			this.DecodedDataLabel = new System.Windows.Forms.Label();
			this.HeaderLabel = new System.Windows.Forms.Label();
			this.SuspendLayout();
			// 
			// ImageFileLabel
			// 
			this.ImageFileLabel.BackColor = System.Drawing.SystemColors.Info;
			this.ImageFileLabel.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.ImageFileLabel.Location = new System.Drawing.Point(154, 500);
			this.ImageFileLabel.Name = "ImageFileLabel";
			this.ImageFileLabel.Size = new System.Drawing.Size(474, 24);
			this.ImageFileLabel.TabIndex = 4;
			this.ImageFileLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// LoadImageButton
			// 
			this.LoadImageButton.Location = new System.Drawing.Point(14, 487);
			this.LoadImageButton.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
			this.LoadImageButton.Name = "LoadImageButton";
			this.LoadImageButton.Size = new System.Drawing.Size(124, 47);
			this.LoadImageButton.TabIndex = 3;
			this.LoadImageButton.Text = "PDF417 Image";
			this.LoadImageButton.UseVisualStyleBackColor = true;
			this.LoadImageButton.Click += new System.EventHandler(this.OnLoadImage);
			// 
			// DataTextBox
			// 
			this.DataTextBox.AcceptsReturn = true;
			this.DataTextBox.BackColor = System.Drawing.SystemColors.Info;
			this.DataTextBox.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
			this.DataTextBox.Cursor = System.Windows.Forms.Cursors.Default;
			this.DataTextBox.Location = new System.Drawing.Point(14, 352);
			this.DataTextBox.Margin = new System.Windows.Forms.Padding(3, 5, 3, 5);
			this.DataTextBox.Multiline = true;
			this.DataTextBox.Name = "DataTextBox";
			this.DataTextBox.ReadOnly = true;
			this.DataTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.DataTextBox.Size = new System.Drawing.Size(667, 124);
			this.DataTextBox.TabIndex = 2;
			this.DataTextBox.TabStop = false;
			this.DataTextBox.Text = "\r\n";
			// 
			// DecodedDataLabel
			// 
			this.DecodedDataLabel.AutoSize = true;
			this.DecodedDataLabel.Location = new System.Drawing.Point(13, 328);
			this.DecodedDataLabel.Name = "DecodedDataLabel";
			this.DecodedDataLabel.Size = new System.Drawing.Size(88, 16);
			this.DecodedDataLabel.TabIndex = 1;
			this.DecodedDataLabel.Text = "Decoded data";
			// 
			// HeaderLabel
			// 
			this.HeaderLabel.BackColor = System.Drawing.SystemColors.ActiveCaption;
			this.HeaderLabel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.HeaderLabel.Font = new System.Drawing.Font("Arial", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.HeaderLabel.Location = new System.Drawing.Point(278, 9);
			this.HeaderLabel.Name = "HeaderLabel";
			this.HeaderLabel.Size = new System.Drawing.Size(210, 39);
			this.HeaderLabel.TabIndex = 0;
			this.HeaderLabel.Text = "Pdf417 Decoder";
			this.HeaderLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// Pdf417DecoderDemo
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 16F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(717, 549);
			this.Controls.Add(this.ImageFileLabel);
			this.Controls.Add(this.LoadImageButton);
			this.Controls.Add(this.DataTextBox);
			this.Controls.Add(this.DecodedDataLabel);
			this.Controls.Add(this.HeaderLabel);
			this.Font = new System.Drawing.Font("Arial", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
			this.Name = "Pdf417DecoderDemo";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Form1";
			this.Load += new System.EventHandler(this.OnLoad);
			this.Paint += new System.Windows.Forms.PaintEventHandler(this.OnPaint);
			this.Resize += new System.EventHandler(this.OnResize);
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private System.Windows.Forms.Label ImageFileLabel;
		private System.Windows.Forms.Button LoadImageButton;
		private System.Windows.Forms.TextBox DataTextBox;
		private System.Windows.Forms.Label DecodedDataLabel;
		private System.Windows.Forms.Label HeaderLabel;
	}
}

