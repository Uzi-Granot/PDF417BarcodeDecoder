/////////////////////////////////////////////////////////////////////
//
//	PDF417 Barcode Decoder
//
//	Pdf417DecoderDemo class
//
//	Author: Uzi Granot
//	Version: 1.0
//	Date: May 1, 2019
//	Copyright (C) 2019 Uzi Granot. All Rights Reserved
//
//	PDF417 barcode decoder class library and the attached test/demo
//  applications are free software.
//	Software developed by this author is licensed under CPOL 1.02.
//
//	The main points of CPOL 1.02 subject to the terms of the License are:
//
//	Source Code and Executable Files can be used in commercial applications;
//	Source Code and Executable Files can be redistributed; and
//	Source Code can be modified to create derivative works.
//	No claim of suitability, guarantee, or any warranty whatsoever is
//	provided. The software is provided "as-is".
//	The Article accompanying the Work may not be distributed or republished
//	without the Author's consent
//
//	Version History
//	---------------
//
//	Version 1.0 2019/05/01
//		Original version
/////////////////////////////////////////////////////////////////////

using Pdf417DecoderLibrary;
using System;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace Pdf417DecoderDemo
{
/// <summary>
/// PDF 417 Decoder demo/test program
/// </summary>
public partial class Pdf417DecoderDemo : Form
	{
	private Pdf417Decoder Pdf417Decoder;
	private Bitmap Pdf417InputImage;
	private Rectangle ImageArea = new Rectangle();

	/// <summary>
	/// Constructor
	/// </summary>
	public Pdf417DecoderDemo()
		{
		InitializeComponent();
		return;
		}

	// initialization
	private void OnLoad(object sender, EventArgs e)
		{
		// program title
		Text = "Pdf417DecoderDemo - " + Pdf417Decoder.VersionNumber + " \u00a9 2019 Uzi Granot. All rights reserved.";

		#if DEBUG
		// current directory
		string CurDir = Environment.CurrentDirectory;
		string WorkDir = CurDir.Replace("bin\\Debug", "Work");
		if(WorkDir != CurDir && Directory.Exists(WorkDir)) Environment.CurrentDirectory = WorkDir;

		// open trace file
		Pdf417Trace.Open("Pdf417DecoderTrace.txt");
		Pdf417Trace.Write("Pdf417DecoderDemo");
		#endif

		// create decoder
		Pdf417Decoder = new Pdf417Decoder();

		// resize window
		OnResize(sender, e);
		return;
		}

	// user pressed load image button
	private void OnLoadImage(object sender, EventArgs e)
		{
		// get file name to decode
		OpenFileDialog Dialog = new OpenFileDialog
			{
			Filter = "Image Files(*.png;*.jpg;*.gif;*.tif)|*.png;*.jpg;*.gif;*.tif;*.bmp)|All files (*.*)|*.*",
			Title = "Load PDF417 Barcode Image",
			InitialDirectory = Directory.GetCurrentDirectory(),
			RestoreDirectory = true,
			FileName = string.Empty
			};

		// display dialog box
		if(Dialog.ShowDialog() != DialogResult.OK) return;

		// display file name
		int Ptr = Dialog.FileName.LastIndexOf('\\');
		int Ptr1 = Dialog.FileName.LastIndexOf('\\', Ptr - 1);
		ImageFileLabel.Text = Dialog.FileName.Substring(Ptr1 + 1);

		// disable buttons
		LoadImageButton.Enabled = false;

		// dispose previous image
		if(Pdf417InputImage != null) Pdf417InputImage.Dispose();

		// load image to bitmap
		Pdf417InputImage = new Bitmap(Dialog.FileName);

		// trace
		#if DEBUG
		Pdf417Trace.Format("****");
		Pdf417Trace.Format("Decode image: {0} ", Dialog.FileName);
		Pdf417Trace.Format("Image width: {0}, Height: {1}", Pdf417InputImage.Width, Pdf417InputImage.Height);
		#endif

		// decode barcodes
		int BarcodesCount = Pdf417Decoder.Decode(Pdf417InputImage);

		// no barcodes were found
		if(BarcodesCount == 0)
			{
			#if DEBUG
			Pdf417Trace.Format("Image has no barcodes: {0}", Dialog.FileName);
			#endif

			// clear barcode data text box 
			DataTextBox.Text = null;
			MessageBox.Show(string.Format("Image has no barcodes: {0}", Dialog.FileName));
			}

		// one barcodes was found
		else if(BarcodesCount == 1)
			{ 
			// decoding was successful
			// convert binary data to text string
			DataTextBox.Text = Pdf417Decoder.BinaryDataToString(0);
			}

		// more than one barcode
		else
			{
			StringBuilder Str = new StringBuilder();

			for(int Count = 0; Count < BarcodesCount; Count++)
				{
				Str.AppendFormat("Barcode No. {0}\r\n{1}\r\n", Count + 1, Pdf417Decoder.BinaryDataToString(Count));	
				}
			DataTextBox.Text = Str.ToString();
			}

		// enable buttons
		LoadImageButton.Enabled = true;

		// force repaint
		Invalidate();
		return;
		}

	// paint PDF417 barcode image
	private void OnPaint(object sender, PaintEventArgs e)
		{
		// no image
		if(Pdf417InputImage == null) return;

		// displayed image size
		int ImageWidth = ImageArea.Width;
		int ImageHeight = (ImageWidth * Pdf417InputImage.Height) / Pdf417InputImage.Width;

		// change to preserve aspect ratio
		if(ImageHeight > ImageArea.Height)
			{
			ImageHeight = ImageArea.Height;
			ImageWidth = (ImageHeight * Pdf417InputImage.Width) / Pdf417InputImage.Height;
			}

		// draw image
		e.Graphics.DrawImage(Pdf417InputImage, new Rectangle(ImageArea.X + (ImageArea.Width - ImageWidth) / 2,
			ImageArea.Y + (ImageArea.Height - ImageHeight) / 2, ImageWidth, ImageHeight));
		return;
		}

	////////////////////////////////////////////////////////////////////
	// Resize demo program window
	////////////////////////////////////////////////////////////////////

	private void OnResize(object sender, EventArgs e)
		{
		// minimize
		if(ClientSize.Width == 0) return;

		// center header label
		HeaderLabel.Left = (ClientSize.Width - HeaderLabel.Width) / 2;

		// put button at bottom left
		LoadImageButton.Top = ClientSize.Height - LoadImageButton.Height - 8;

		// image file label
		ImageFileLabel.Top = LoadImageButton.Top + (LoadImageButton.Height - ImageFileLabel.Height) / 2;
		ImageFileLabel.Width = ClientSize.Width - ImageFileLabel.Left - 16;

		// data text box
		DataTextBox.Top = LoadImageButton.Top - DataTextBox.Height - 8;
		DataTextBox.Width = ClientSize.Width - 8 - SystemInformation.VerticalScrollBarWidth;

		// decoded data label
		DecodedDataLabel.Top = DataTextBox.Top - DecodedDataLabel.Height - 4;

		// image area
		ImageArea.X = 4;
		ImageArea.Y = HeaderLabel.Bottom + 4;
		ImageArea.Width = ClientSize.Width - ImageArea.X - 4;
		ImageArea.Height = DecodedDataLabel.Top - ImageArea.Y - 4;

		if(Pdf417InputImage != null) Invalidate();
		return;
		}
	}
}
