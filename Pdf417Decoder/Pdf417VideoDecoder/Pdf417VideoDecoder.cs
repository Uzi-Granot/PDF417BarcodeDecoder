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
//	Some portions of the Pdf417VideoDecoder are licensed under GNU Lesser
//	General Public License v3.0.
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

using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Windows.Forms;
using System.Diagnostics;
using Pdf417DecoderLibrary;
using DirectShowLib;

namespace Pdf417VideoDecoderDemo
{
/// <summary>
/// Pdf417 camera captuer using Direct Show Library
/// </summary>
public partial class Pdf417VideoDecoder : Form
	{
	private FrameSize FrameSize = new FrameSize(640, 480);
	private Camera VideoCamera;
	private Timer VideoDecoderTimer;
	private Pdf417Decoder Decoder;

	/// <summary>
	/// Constructor
	/// </summary>
	public Pdf417VideoDecoder()
		{
		InitializeComponent();
		return;
		}

	/// <summary>
	/// Program initialization
	/// </summary>
	/// <param name="sender">Sender</param>
	/// <param name="e">Event arguments</param>
	private void OnLoad(object sender, EventArgs e)
		{
		// program title
		Text = "Pdf417VideoDecoder - " + Pdf417Decoder.VersionNumber + " \u00a9 2019 Uzi Granot. All rights reserved.";

		#if DEBUG
		// current directory
		string CurDir = Environment.CurrentDirectory;
		string WorkDir = CurDir.Replace("bin\\Debug", "Work");
		if(WorkDir != CurDir && Directory.Exists(WorkDir)) Environment.CurrentDirectory = WorkDir;

		// open trace file
		Pdf417Trace.Open("Pdf417VideoDecoderTrace.txt");
		Pdf417Trace.Write(Text);
		#endif

		// disable reset button
		ResetButton.Enabled = false;
		GoToUriButton.Enabled = false;

		// get an array of web camera devices
		DsDevice[] CameraDevices = DsDevice.GetDevicesOfCat(FilterCategory.VideoInputDevice);

		// make sure at least one is available
		if(CameraDevices == null || CameraDevices.Length == 0)
			{
			MessageBox.Show("No video cameras in this computer");
			Close();
			return;
			}

		// select the first camera
		DsDevice CameraDevice = CameraDevices[0];

		// Device moniker
		IMoniker CameraMoniker = CameraDevice.Moniker;

		// get a list of frame sizes available
		FrameSize[] FrameSizes = Camera.GetFrameSizeList(CameraMoniker);

		// make sure there is at least one frame size
		if(FrameSizes == null || FrameSizes.Length == 0)
			{
			MessageBox.Show("No video cameras in this computer");
			Close();
			return;
			}

		// test if our frame size is available
		int Index;
		for(Index = 0; Index < FrameSizes.Length &&
			(FrameSizes[Index].Width != FrameSize.Width || FrameSizes[Index].Height != FrameSize.Height); Index++);

		// select first frame size
		if(Index == FrameSizes.Length) FrameSize = FrameSizes[0];

		// Set selected camera to camera control with default frame size
		// Create camera object
		VideoCamera = new Camera(PreviewPanel, CameraMoniker, FrameSize);

		// create QR code decoder
		Decoder = new Pdf417Decoder();

		// resize window
		OnResize(sender, e);

		// create timer
		VideoDecoderTimer = new Timer();
		VideoDecoderTimer.Interval = 200;
		VideoDecoderTimer.Tick += VideoDecoderTimer_Tick;
		VideoDecoderTimer.Enabled = true;
		return;
		}

	private void VideoDecoderTimer_Tick(object sender, EventArgs e)
		{
		VideoDecoderTimer.Enabled = false;
		Bitmap Pdf417Image;
		try
			{
			Pdf417Image = VideoCamera.SnapshotSourceImage();

			// trace
			#if DEBUG
			Pdf417Trace.Format("Image width: {0}, Height: {1}", Pdf417Image.Width, Pdf417Image.Height);
			#endif
			}

		catch (Exception EX)
			{
			DataTextBox.Text = "Decode exception.\r\n" + EX.Message;
			VideoDecoderTimer.Enabled = true;
			return;
			}

		// decode image to byte array (Pdf417Decoder.BarcodeBinaryData)
		string BarcodeText = null;

		// decode barcodes
		int BarcodesCount = Decoder.Decode(Pdf417Image);

		// dispose bitmap
		Pdf417Image.Dispose();

		// we have no Pdf417 barcode
		if(BarcodesCount == 0)
			{
			VideoDecoderTimer.Enabled = true;
			return;
			}

		// decoding was successful
		// convert binary data to text string
		BarcodeText = Decoder.BinaryDataToString(0);

		// we have no Pdf417 barcode
		if(BarcodeText == null || BarcodeText.Length == 0)
			{
			VideoDecoderTimer.Enabled = true;
			return;
			}

		VideoCamera.PauseGraph();

		DataTextBox.Text = BarcodeText;
		ResetButton.Enabled = true;
		if(IsValidUri(DataTextBox.Text)) GoToUriButton.Enabled = true;
		return;
		}

	private static bool IsValidUri(string Uri)
		{
		if(!System.Uri.IsWellFormedUriString(Uri, UriKind.Absolute)) return false;

		if(!System.Uri.TryCreate(Uri, UriKind.Absolute, out Uri TempUri)) return false;

		return TempUri.Scheme == System.Uri.UriSchemeHttp || TempUri.Scheme == System.Uri.UriSchemeHttps;
		}

	/// <summary>
	/// Reset button was pressed
	/// </summary>
	/// <param name="sender">Sender</param>
	/// <param name="e">Event arguments</param>
	private void OnResetButton(object sender, EventArgs e)
		{
		VideoCamera.RunGraph();
		VideoDecoderTimer.Enabled = true;
		ResetButton.Enabled = false;
		GoToUriButton.Enabled = false;
		DataTextBox.Text = string.Empty;
		return;
		}

	/// <summary>
	/// 
	/// </summary>
	/// <param name="sender">Sender</param>
	/// <param name="e">Event arguments</param>
	private void OnGoToUri(object sender, EventArgs e)
		{
		Process.Start(DataTextBox.Text);
		return;
		}

	/// <summary>
	/// Resize window
	/// </summary>
	/// <param name="sender">Sender</param>
	/// <param name="e">Event arguments</param>
	private void OnResize(object sender, EventArgs e)
		{
		// minimize
		if(ClientSize.Width == 0) return;

		// put reset button at bottom center
		ResetButton.Left = ClientSize.Width / 2 - ResetButton.Width - 8;
		ResetButton.Top = ClientSize.Height - ResetButton.Height - 8;
		GoToUriButton.Left = ResetButton.Right + 16;
		GoToUriButton.Top = ResetButton.Top;

		// data text box
		DataTextBox.Top = ResetButton.Top - DataTextBox.Height - 8;
		DataTextBox.Width = ClientSize.Width - 8 - SystemInformation.VerticalScrollBarWidth;

		// decoded data label
		DecodedDataLabel.Top = DataTextBox.Top - DecodedDataLabel.Height - 4;

		// preview area
		int AreaWidth = ClientSize.Width - 4;
		int AreaHeight = DecodedDataLabel.Top - 4;
		if(AreaHeight > FrameSize.Height * AreaWidth / FrameSize.Width)
			AreaHeight = FrameSize.Height * AreaWidth / FrameSize.Width;
		else
			AreaWidth = FrameSize.Width * AreaHeight / FrameSize.Height;

		// preview panel
		PreviewPanel.Left = (ClientSize.Width - AreaWidth) / 2;
		PreviewPanel.Top = (DecodedDataLabel.Top - 4 - AreaHeight) / 2;
		PreviewPanel.Width = AreaWidth;
		PreviewPanel.Height = AreaHeight;
		return;
		}

	private void OnClosing(object sender, FormClosingEventArgs e)
		{
		if(VideoCamera != null) VideoCamera.Dispose();
		return;
		}
	}
}
