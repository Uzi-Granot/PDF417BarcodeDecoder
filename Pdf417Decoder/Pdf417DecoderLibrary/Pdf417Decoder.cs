/////////////////////////////////////////////////////////////////////
//
//	PDF417 Barcode Decoder
//
//	Pdf417Decoder class
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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Numerics;
using System.Text;
using System.Runtime.InteropServices;

namespace Pdf417DecoderLibrary
{
/// <summary>
/// PDF417 barcode decoder class
/// </summary>
public class Pdf417Decoder
	{
	// encoding mode
	private enum EncodingMode
		{
		Byte,
		Text,
		Numeric,
		}

	// text encoding sub-mode
	private enum TextEncodingMode
		{
		Upper,
		Lower,
		Mixed,
		Punct,
		ShiftUpper,
		ShiftPunct,
		}

	/// <summary>
	/// program version number
	/// </summary>
	public const string VersionNumber = "Rev 1.0.0 - 2019-05-01";

	/// <summary>
	/// Returned array of barcodes binary data
	/// </summary>
	public byte[][] BarcodesData {get; internal set;}

	/// <summary>
	/// Returned array of barcodes binary data plus extra information
	/// </summary>
	public BarcodeInfo[] BarcodesInfo {get; internal set;}

	// width of symbol in bars
	private const int ModulesInCodeword = 17;

	// Control codewords
	private const int SwitchToTextMode = 900;
	private const int SwitchToByteMode = 901;
	private const int SwitchToNumericMode = 902;
	private const int ShiftToByteMode = 913;
	private const int SwitchToByteModeForSix = 924;

	// User-Defined GLis:
	// Codeword 925 followed by one codeword
	// The program allows for value of 0 to 899.
	// The documentation is not clear one codeword
	// cannot be 810,900 to 811,799.
	// (GLI values from 810,900 to 811,799). These GLis
	// should be used for closed-system applications
	private const int GliUserDefined = 925;

	// General Purpose GLis:
	// Codeword 926 followed by two codewords
	// representing GLI values from 900 to 810,899
	private const int GliGeneralPurpose = 926;

	// international character set
	// Codeword 927 followed by a single codeword
	// with a value ranging from O to 899. The GLI
	// value of 0 is the default interpretation
	// This value is probably ISO 8859 part number
	private const int GliCharacterSet = 927;

	private static readonly int[] StartSig = { 9, 2, 2, 2, 2, 2 };
	private static readonly int[] StopSig = { 8, 2, 4, 4, 2, 2 };

	internal bool[,] ImageMatrix;
	internal int ImageWidth;
	internal int ImageHeight;
	internal int IndControl;
	internal int DataColumns;
	internal int DataRows;
	internal int ErrorCorrectionLength;
	internal int ErrorCorrectionCount;
	internal byte[] BarcodeBinaryData;

	// Global Label Identifier character set (ISO-8859-n)
	// The n represent part number 1 to 9, 13 and 15
	internal string GlobalLabelIDCharacterSet;

	// Global Label Identifier character set number
	// This number is two more than the part number
	internal int GlobalLabelIDCharacterSetNo;

	// Global label identifier general purpose number
	// code word 926 value 900 to 810899
	internal int GlobalLabelIDGeneralPurpose;

	// Global label identifier user defined number
	// code word 925 value 810,900 to 811,799
	internal int GlobalLabelIDUserDefined;

	// locate barcodes
	// horizontal image scan to find black and white bars
	private int[] BarPos;
	private int BarEnd;

	private List<BarcodeArea> BarcodeList;
	private BarcodeArea BarcodeArea;

	private double AvgSymbolWidth;
	private double MaxSymbolError;

	internal int[] ScanX = new int[9];
	internal int[] ScanY = new int[9];

#if DEBUG
	private int StartTimer;
	private int ImageToMatrixTime;
	private int LocateBarcodesTime;
	private int BarcodesToDataTime;
	private int CodewordFix;
#endif

	private static readonly int[] YStep = {1, -1, 2, -2, 3, -3};

	// barcode four corners
	private int TopLeftX;
	private int TopLeftY;
	private int TopLeftCol;
	private int TopLeftRow;
	private int TopRightX;
	private int TopRightY;
	private int TopRightCol;
	private int TopRightRow;
	private int BottomLeftX;
	private int BottomLeftY;
	private int BottomLeftCol;
	private int BottomLeftRow;
	private int BottomRightX;
	private int BottomRightY;
	private int BottomRightCol;
	private int BottomRightRow;

	// transformation matrix
	private double Trans4a;
	private double Trans4b;
	private double Trans4c;
	private double Trans4d;
	private double Trans4e;
	private double Trans4f;
	private double Trans4g;
	private double Trans4h;

	// codewords array including data length, data, padding and error correction
	private int[] Codewords;
	private int CodewordsPtr;

	// text encoding mode
	private TextEncodingMode _TextEncodingMode;

	/// <summary>
	/// Decode PDF417 barcode image into binary array
	/// </summary>
	/// <param name="InputImage">Barcode image bitmap</param>
	/// <returns>Count of decoded barcodes or zero</returns>
	public int Decode
			(
			Bitmap InputImage
			)
		{
		#if DEBUG
		StartTimer = Environment.TickCount;
		ImageToMatrixTime = 0;
		LocateBarcodesTime = 0;
		BarcodesToDataTime = 0;
		#endif

		// convert grapics image to black and white matrix
		if(!ConvertImage(InputImage))
			{
			#if DEBUG
			Pdf417Trace.Write("Image conversion to black and white matrix failed");
			#endif
			return 0;
			}

		#if DEBUG
		int Timer = Environment.TickCount;
		ImageToMatrixTime = Timer - StartTimer;
		StartTimer = Timer;
		#endif

		// locate one or more barcodes in the image
		if(!LocateBarcodes())
			{
			#if DEBUG
			Pdf417Trace.Write("No barcodes were found in the image.");
			#endif
			return 0;
			}

		#if DEBUG
		Timer = Environment.TickCount;
		LocateBarcodesTime = Timer - StartTimer;
		StartTimer = Timer;
		#endif

		// reset results list
		List<BarcodeInfo> BarcodeExtraInfoList = new List<BarcodeInfo>();

		// loop for all barcodes found
		for(int Index = 0; Index < BarcodeList.Count; Index++)
			{
			// current barcode area
			BarcodeArea = BarcodeList[Index];

			// reset some variables
			IndControl = 0;
			DataRows = 0;
			DataColumns = 0;
			ErrorCorrectionLength = 0;
			ErrorCorrectionCount = 0;
			BarcodeBinaryData = null;
			BarcodesData = null;
			BarcodesInfo = null;

			// average symbol width and maximum error allowed
			AvgSymbolWidth = BarcodeArea.AvgSymbolWidth;
			MaxSymbolError = BarcodeArea.MaxSymbolError;

			// test left indicators columns
			if(!LeftIndicators())
				{
				#if DEBUG
				Pdf417Trace.Write("Left indicators error");
				#endif
				continue;
				}

			// test right indicators columns
			if(!RightIndicators())
				{
				#if DEBUG
				Pdf417Trace.Write("Right indicators error");
				#endif
				continue;
				}

			#if DEBUG
			Pdf417Trace.Format("Data columns: {0}, Data rows: {1}, Error correction length: {2}",
				DataColumns, DataRows, ErrorCorrectionLength);
			#endif

			// set conversion matrix
			if(!SetTransMatrix())
				{
				#if DEBUG
				Pdf417Trace.Write("TransMatrix SetMatrix Solve linear equations failed");
				#endif
				continue;
				}

			// extract codewords from image
			if(!GetCodewords())
				{
				#if DEBUG
				Pdf417Trace.Write("Conversion of BW image to codewords failed");
				#endif
				continue;
				}

			// convert codewords to bytes and text
			if(!CodewordsToData())
				{
				#if DEBUG
				Pdf417Trace.Write("Conversion of codewords to data failed");
				#endif
				continue;
				}

			// save results
			BarcodeInfo Result = new BarcodeInfo();
			Result.BarcodeData = BarcodeBinaryData;
			Result.CharacterSet = GlobalLabelIDCharacterSet;
			Result.GliCharacterSetNo = GlobalLabelIDCharacterSetNo;
			Result.GliGeneralPurpose = GlobalLabelIDGeneralPurpose;
			Result.GliUserDefined = GlobalLabelIDUserDefined;
			Result.DataColumns = DataColumns;
			Result.DataRows = DataRows;
			Result.ErrorCorrectionLength = ErrorCorrectionLength;
			Result.ErrorCorrectionCount = ErrorCorrectionCount;
			BarcodeExtraInfoList.Add(Result);
			}

		#if DEBUG
		Timer = Environment.TickCount;
		BarcodesToDataTime = Timer - StartTimer;
		StartTimer = Timer;

		Pdf417Trace.Format("Image to matrix time: {0:0.00}", 0.001 * ImageToMatrixTime);
		Pdf417Trace.Format("Locate barcode time: {0:0.00}", 0.001 * LocateBarcodesTime);
		Pdf417Trace.Format("Codewords to data time: {0:0.00}", 0.001 * BarcodesToDataTime);
		Pdf417Trace.Format("Total time: {0:0.00}", 0.001 * (ImageToMatrixTime + LocateBarcodesTime + BarcodesToDataTime));
		#endif

		// count of decoded barcods
		int BarcodesCount = BarcodeExtraInfoList.Count;

		// no valid barcode found
		if(BarcodesCount == 0) return 0;

		// convert to array
		BarcodesInfo = BarcodeExtraInfoList.ToArray();

		// extract binary data arrays
		BarcodesData = new byte[BarcodesCount][];
		for(int Index = 0; Index < BarcodesCount; Index++)
			{
			BarcodesData[Index] = BarcodesInfo[Index].BarcodeData;
			}

		// exit with binary data
		return BarcodesCount;
		}

	/// <summary>
	/// Convert binary data to string for one result
	/// </summary>
	/// <returns>Text string</returns>
	public string BinaryDataToString
			(
			int Index
			)
		{
		return BinaryDataToString(BarcodesInfo[Index].BarcodeData,
			 BarcodesInfo[Index].CharacterSet ?? "ISO-8859-1");
		}

	/// <summary>
	/// Convert binary data array to text string
	/// </summary>
	/// <param name="BarcodeBinaryData">Binary byte array</param>
	/// <param name="IsoStandard">ISO standard "ISO-8859-part"</param>
	/// <returns>Text string</returns>
	public static string BinaryDataToString
			(
			byte[] BarcodeBinaryData,
			string IsoStandard = "ISO-8859-1"
			)
		{
		try
			{
			// convert byte array to string
			Encoding ISO = Encoding.GetEncoding(IsoStandard);
			byte[] Utf8Bytes = Encoding.Convert(ISO, Encoding.UTF8, BarcodeBinaryData);
			char[] Utf8Chars = Encoding.UTF8.GetChars(Utf8Bytes);
		
			// save result
			return new string(Utf8Chars);
			}

		catch(Exception Ex)
			{
			#if DEBUG
			Pdf417Trace.Write("Conversion of binary data to string failed\r\n" + Ex.Message);
			#endif
			return null;
			}
		}

	////////////////////////////////////////////////////////////////////
	// Convert image to black and white boolean matrix
	////////////////////////////////////////////////////////////////////

	private bool ConvertImage
			(
			Bitmap	InputImage
			)
		{
		// save image dimension
		ImageWidth = InputImage.Width;
		ImageHeight = InputImage.Height;

		// lock image bits
		BitmapData BitmapData = InputImage.LockBits(new Rectangle(0, 0, ImageWidth, ImageHeight),
			ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

		// address of first line
		IntPtr BitArrayPtr = BitmapData.Scan0;

		// length in bytes of one scan line
		int ScanLineWidth = BitmapData.Stride;

		// Image conversion to black and white failed (upside down image)
		if(ScanLineWidth < 0)
			{
			#if DEBUG
			Pdf417Trace.Write("Image conversion to black and white failed (upside down image)");
			#endif
			return false;
			}

		// image total bytes
		int TotalBytes = ScanLineWidth * ImageHeight;
		byte[] BitmapArray = new byte[TotalBytes];

		// Copy the RGB values into the array.
        Marshal.Copy(BitArrayPtr, BitmapArray, 0, TotalBytes);

		// unlock image
		InputImage.UnlockBits(BitmapData);

		// allocate gray image 
		byte[,] GrayImage = new byte[ImageHeight, ImageWidth];
		int[] GrayLevel = new int[256];
 
		// convert to gray
		int Delta = ScanLineWidth - 3 * ImageWidth;
		int BitmapPtr = 0;
		for(int Row = 0; Row < ImageHeight; Row++)
			{
			for(int Col = 0; Col < ImageWidth; Col++)
				{
				int Module = (30 * BitmapArray[BitmapPtr] + 59 * BitmapArray[BitmapPtr + 1] + 11 * BitmapArray[BitmapPtr + 2]) / 100;
				GrayLevel[Module]++;
				GrayImage[Row, Col] = (byte) Module;
				BitmapPtr += 3;
				}
			BitmapPtr += Delta;
			}

		// gray level cutoff between black and white
		int LevelStart;
		int LevelEnd;
		for(LevelStart = 0; LevelStart < 256 && GrayLevel[LevelStart] == 0; LevelStart++);
		for(LevelEnd = 255; LevelEnd >= LevelStart && GrayLevel[LevelEnd] == 0; LevelEnd--);
		LevelEnd++;

		// Image conversion to black and white failed (no color variations)
		if(LevelEnd - LevelStart < 2)
			{
			#if DEBUG
			Pdf417Trace.Write("Image conversion to black and white failed (no color variations)");
			#endif
			return false;
			}

		// all gray values less than cutoff are black
		// all gray values greater than or equal to cutoff are white
		int CutoffLevel = (LevelStart + LevelEnd) / 2;

		// create boolean image white = false, black = true
		ImageMatrix = new bool[ImageHeight, ImageWidth];
		for(int Row = 0; Row < ImageHeight; Row++)
			for(int Col = 0; Col < ImageWidth; Col++)
				ImageMatrix[Row, Col] = GrayImage[Row, Col] < CutoffLevel;

		// save BW image for debugging
		#if DEBUG && SAVEBWIMAGE
		Bitmap BWImage = CreateBitmap();
		BWImage.Save("BlackWhiteImage.png", ImageFormat.Png);
		BWImage.Dispose();
		#endif

		// exit;
		return true;
		}

	internal bool LocateBarcodes()
		{
		BarPos = new int[ImageWidth];
		List<List<BorderSymbol>> StartSymbols = new List<List<BorderSymbol>>();
		List<List<BorderSymbol>> StopSymbols = new List<List<BorderSymbol>>();
		BarcodeList = new List<BarcodeArea>();

		for(int Scan = 0;; Scan++)
			{
			// scan one row at a time
			for(int Row = 0; Row < ImageHeight; Row++)
				{
				// scan the line for array of bars
				if(!ScanLine(Row)) continue;

				// look for start signature
				BorderSignature(StartSymbols, StartSig, Row);

				// look for stop signature
				BorderSignature(StopSymbols, StopSig, Row);
				}

			// remove all lists with less than 18 symbols
			for(int Index = 0; Index < StartSymbols.Count; Index++)
				{
				if(StartSymbols[Index].Count < 18)
					{
					StartSymbols.RemoveAt(Index);
					Index--;
					}
				}

			// remove all lists with less than 18 symbols
			for(int Index = 0; Index < StopSymbols.Count; Index++)
				{
				if(StopSymbols[Index].Count < 18)
					{
					StopSymbols.RemoveAt(Index);
					Index--;
					}
				}

			// match start and stop patterns
			if(StartSymbols.Count != 0 && StopSymbols.Count != 0)
				{
				foreach(List<BorderSymbol> StartList in StartSymbols)
					{
					foreach(List<BorderSymbol> StopList in StopSymbols)
						{
						MatchStartAndStop(StartList, StopList);
						}
					}
				}

			if(BarcodeList.Count > 0 || Scan == 1) break;

			// rotate image by 180 and try again
			RotateImageBy180();
			StartSymbols.Clear();
			StopSymbols.Clear();
			BarcodeList.Clear();

			#if DEBUG
			Pdf417Trace.Write("Rotate image by 180 Deg");
			#endif
			}

		// exit
		return BarcodeList.Count > 0;
		}

	// rotate image by 180 degrees
	internal void RotateImageBy180()
			{
			bool[,] RevImageMatrix = new bool[ImageHeight, ImageWidth];
			int LastRow = ImageHeight;
			int LastCol = ImageWidth - 1;
			for(int Row = 0; Row < ImageHeight; Row++)
				{
				LastRow--;
				for(int Col = 0; Col < ImageWidth; Col++) RevImageMatrix[Row, Col] = ImageMatrix[LastRow, LastCol - Col];
				}
			ImageMatrix = RevImageMatrix;
			return;
			}

	// convert image line to black and white bars
	internal bool ScanLine
			(
			int Row
			)
		{
		// look for first white pixel
		int Col;
		for(Col = 0; Col < ImageWidth && ImageMatrix[Row, Col]; Col++);

		// no transition found
		if(Col == ImageWidth) return false;

		// look for first white to black transition
		for(Col++; Col < ImageWidth && !ImageMatrix[Row, Col]; Col++);

		// no transition found
		if(Col == ImageWidth) return false;

		// save first black pixel
		BarEnd = 0;
		BarPos[BarEnd++] = Col;

		// loop for pairs
		for(;;)
			{
			// look for end of black bar
			for(; Col < ImageWidth && ImageMatrix[Row, Col]; Col++);
			if(Col == ImageWidth)
				{
				// make sure last transition was black to white
				BarEnd--;
				break;
				}

			// save white bar position
			BarPos[BarEnd++] = Col;

			// look for end of white bar
			for(; Col < ImageWidth && !ImageMatrix[Row, Col]; Col++);
			if(Col == ImageWidth) break;

			// save black bar position
			BarPos[BarEnd++] = Col;
			}

		// make sure there are at least 8 black and white bars
		return BarEnd > 8;
		}

	internal void BorderSignature
			(
			List<List<BorderSymbol>> BorderSymbols,
			int[] Signature,
			int Row
			)
		{
		// search for start or stop signature
		int BarPtrEnd = BarEnd - 8;
		for(int BarPtr = 0; BarPtr < BarPtrEnd; BarPtr += 2)
			{
			// width of 8 bars
			int Width = BarPos[BarPtr + 8] - BarPos[BarPtr];

			// test for signature
			int Index;
			for(Index = 0; Index < 6; Index++)
				{
				if((34 * (BarPos[BarPtr + Index + 2] - BarPos[BarPtr + Index]) + Width) / (2 * Width) != Signature[Index]) break;
				}

			// no start or stop signature
			if(Index < 6) continue;

			BorderSymbol NewSymbol = new BorderSymbol(BarPos[BarPtr], Row, BarPos[BarPtr + 8]);

			// this is the first start or stop signature
			if(BorderSymbols.Count == 0)
				{
				BorderSymbols.Add(new List<BorderSymbol>{NewSymbol});
				}

			else
				{
				// try to match it to one of the existing lists
				foreach(List<BorderSymbol> List in BorderSymbols)
					{
					// compare to last symbol
					BorderSymbol LastSymbol = List[List.Count - 1];

					// not part of current list
					if(Row - LastSymbol.Y1 >= 18 || 
						Math.Abs(NewSymbol.X1 - LastSymbol.X1) >= 5 ||
							Math.Abs(NewSymbol.X2 - LastSymbol.X2) >= 5) continue;

					// add to current list
					List.Add(NewSymbol);
					NewSymbol = null;
					break;
					}

				// start a new list
				if(NewSymbol != null)
					{
					BorderSymbols.Add(new List<BorderSymbol>{NewSymbol});
					}
				}
		
			// continue search after start signature
			BarPtr += 6;
			}
		return;
		}

	private bool MatchStartAndStop
			(
			List<BorderSymbol> StartList,
			List<BorderSymbol> StopList
			)
		{
		// calculate start and stop patterns relative to image coordinates
		BorderPattern StartBorder = new BorderPattern(false, StartList);
		BorderPattern StopBorder = new BorderPattern(true, StopList);

		// borders slopes must be less than 45 deg
		if(StartBorder.DeltaY <= Math.Abs(StartBorder.DeltaX) || StopBorder.DeltaY <= Math.Abs(StopBorder.DeltaX)) return false; 

		// stop must be to the right of start
		if(StopBorder.CenterX <= StartBorder.CenterX) return false;

		// center line
		int CenterDeltaX = StopBorder.CenterX - StartBorder.CenterX;
		int CenterDeltaY = StopBorder.CenterY - StartBorder.CenterY;
		double CenterLength = Math.Sqrt(CenterDeltaX * CenterDeltaX + CenterDeltaY * CenterDeltaY);

		// angle bewteen start line and center line must be about 84 to 96
		double Cos = (StartBorder.DeltaX * CenterDeltaX + StartBorder.DeltaY * CenterDeltaY) / (CenterLength * StartBorder.BorderLength);
		if(Math.Abs(Cos) > 0.1) return false;

		// angle bewteen start line and center line must be about 85 to 95
		Cos = (StopBorder.DeltaX * CenterDeltaX + StopBorder.DeltaY * CenterDeltaY) / (CenterLength * StopBorder.BorderLength);
		if(Math.Abs(Cos) > 0.1) return false;

		// add to the list
		BarcodeList.Add(new BarcodeArea(StartBorder, StopBorder));
		return true;
		}

	private bool LeftIndicators()
		{
		// get mid column codeword
		int PosX = BarcodeArea.LeftCenterX;
		int PosY = BarcodeArea.LeftCenterY;
		int MidCodeword = GetCodeword(PosX, PosY, BarcodeArea.LeftDeltaY, -BarcodeArea.LeftDeltaX);
		int LastCodeword = MidCodeword;
		int TopCodeword = -1;
		int BottomCodeword = -1;

		// move up from center
		int ErrorCount = 0;
		for(PosY--; PosY > 0; PosY--)
			{
			// get cluster plus codeword
			PosX = BarcodeArea.LeftXFuncY(PosY);
			int Codeword = GetCodeword(PosX, PosY, BarcodeArea.LeftDeltaY, -BarcodeArea.LeftDeltaX);

			// valid codeword
			if(Codeword >= 0)
				{
				// the same as last codeword
				if(Codeword == LastCodeword)
					{
					if(IndControl != 7) SetInfo(Codeword);

					// save position
					TopLeftX = ScanX[0];
					TopLeftY = ScanY[0];
					TopCodeword = Codeword;
					}
				else
					{
					LastCodeword = Codeword;
					}
				ErrorCount = 0;
				continue;
				}

			// error
			ErrorCount++;
			if(ErrorCount > 20) break;
			}

		// move down from center
		PosX = BarcodeArea.LeftCenterX;
		PosY = BarcodeArea.LeftCenterY;
		LastCodeword = MidCodeword;
		ErrorCount = 0;
		for(PosY++; PosY < ImageHeight; PosY++)
			{
			// get cluster plus codeword
			PosX = BarcodeArea.LeftXFuncY(PosY);
			int Codeword = GetCodeword(PosX, PosY, BarcodeArea.LeftDeltaY, -BarcodeArea.LeftDeltaX);

			// valid codeword
			if(Codeword >= 0)
				{
				// the same as last codeword
				if(Codeword == LastCodeword)
					{
					if(IndControl != 7) SetInfo(Codeword);

					// save position
					BottomLeftX = ScanX[0];
					BottomLeftY = ScanY[0];
					BottomCodeword = Codeword;
					}
				else
					{
					LastCodeword = Codeword;
					}
				ErrorCount = 0;
				continue;
				}

			// error
			ErrorCount++;
			if(ErrorCount > 20) break;
			}

		if(TopCodeword < 0 || BottomCodeword < 0) return false;

		// top left corner
		int Cluster = TopCodeword >> 10;
		TopLeftRow = 3 * ((TopCodeword & 0x3ff) / 30) + Cluster;
		TopLeftCol = -1;

		// bottom left corner
		Cluster = BottomCodeword >> 10;
		BottomLeftRow = 3 * ((BottomCodeword & 0x3ff) / 30) + Cluster;
		BottomLeftCol = -1;
		
		#if DEBUG
		FormatCorner("Top Left", TopLeftX, TopLeftY, TopCodeword);
		FormatCorner("Bottom Left", BottomLeftX, BottomLeftY, BottomCodeword);
		#endif
		return true;
		}

	private bool RightIndicators()
		{
		int PosX = BarcodeArea.RightCenterX;
		int PosY = BarcodeArea.RightCenterY;
		int MidCodeword = RevGetCodeword(PosX, PosY, BarcodeArea.RightDeltaY, -BarcodeArea.RightDeltaX);
		int LastCodeword = MidCodeword;
		int TopCodeword = -1;
		int BottomCodeword = -1;

		// move up from center
		int ErrorCount = 0;
		for(PosY--; PosY > 0; PosY--)
			{
			// get cluster plus codeword
			PosX = BarcodeArea.RightXFuncY(PosY);
			int Codeword = RevGetCodeword(PosX, PosY, BarcodeArea.RightDeltaY, -BarcodeArea.RightDeltaX);

			// valid codeword
			if(Codeword >= 0)
				{
				// the same as last codeword
				if(Codeword == LastCodeword)
					{
					if(IndControl != 7) SetInfo(Codeword);

					// save position
					TopRightX = ScanX[0];
					TopRightY = ScanY[0];
					TopCodeword = Codeword;
					}
				else
					{
					LastCodeword = Codeword;
					}
				ErrorCount = 0;
				continue;
				}

			// error
			ErrorCount++;
			if(ErrorCount > 20) break;
			}

		// move down from center
		PosX = BarcodeArea.RightCenterX;
		PosY = BarcodeArea.RightCenterY;
		LastCodeword = MidCodeword;
		ErrorCount = 0;
		for(PosY++; PosY < ImageHeight; PosY++)
			{
			// get cluster plus codeword
			PosX = BarcodeArea.RightXFuncY(PosY);
			int Codeword = RevGetCodeword(PosX, PosY, BarcodeArea.RightDeltaY, -BarcodeArea.RightDeltaX);

			// valid codeword
			if(Codeword >= 0)
				{
				// the same as last codeword
				if(Codeword == LastCodeword)
					{
					if(IndControl != 7) SetInfo(Codeword);

					// save position
					BottomRightX = ScanX[0];
					BottomRightY = ScanY[0];
					BottomCodeword = Codeword;
					}
				else
					{
					LastCodeword = Codeword;
					}
				ErrorCount = 0;
				continue;
				}

			// error
			ErrorCount++;
			if(ErrorCount > 20) break;
			}

		if(IndControl != 7 || TopCodeword < 0 || BottomCodeword < 0) return false;

		// top Right corner
		int Cluster = TopCodeword >> 10;
		TopRightRow = 3 * ((TopCodeword & 0x3ff) / 30) + Cluster;
		TopRightCol = DataColumns;

		// bottom Right corner
		Cluster = BottomCodeword >> 10;
		BottomRightRow = 3 * ((BottomCodeword & 0x3ff) / 30) + Cluster;
		BottomRightCol = DataColumns;
		
		#if DEBUG
		FormatCorner("Top Right", TopRightX, TopRightY, TopCodeword);
		FormatCorner("Bottom Right", BottomRightX, BottomRightY, BottomCodeword);
		#endif
		return true;
		}

	#if DEBUG
	private void FormatCorner
			(
			string Name,
			int X,
			int Y,
			int CW
			)
		{
		int Cluster = CW >> 10;
		CW &= 0x3ff;
		int Row = 3 * Math.DivRem(CW, 30, out int Info) + Cluster;
		Pdf417Trace.Format("{0} X={1}, Y={2}, Cluster={3}, Row={4}, Info={5}", Name, X, Y, Cluster, Row, Info);
		}
	#endif

	private void SetInfo
			(
			int Codeword
			)
		{
		int Cluster = Codeword >> 10;
		int Info = (Codeword & 0x3ff) % 30;

		// switch for cluster
		switch(Cluster)
			{
			case 0:
				// data rows (partial)
				if((IndControl & 1) == 0)
					{
					DataRows += Info * 3 + 1;
					IndControl |= 1;
					}
				break;

			case 1:
				// error correction and data rows extra
				if((IndControl & 2) == 0)
					{
					ErrorCorrectionLength = 1 << (Math.DivRem(Info, 3, out int DataRowsExtra) + 1);
					DataRows += DataRowsExtra;
					IndControl |= 2;
					}
				break;

			case 2:
				// save data columns
				if((IndControl & 4) == 0)
					{
					DataColumns = Info + 1;
					IndControl |= 4;
					}
				break;
			}

		return;
		}

	private bool SetTransMatrix()
		{
		double[,] Matrix = new double[8, 9];

		Matrix[0, 0] = TopLeftCol;
		Matrix[0, 1] = TopLeftRow;
		Matrix[0, 2] = 1.0;
		Matrix[0, 6] = -TopLeftCol * TopLeftX;
		Matrix[0, 7] = -TopLeftRow * TopLeftX;
		Matrix[0, 8] = TopLeftX;

		Matrix[1, 0] = TopRightCol;
		Matrix[1, 1] = TopRightRow;
		Matrix[1, 2] = 1.0;
		Matrix[1, 6] = -TopRightCol * TopRightX;
		Matrix[1, 7] = -TopRightRow * TopRightX;
		Matrix[1, 8] = TopRightX;

		Matrix[2, 0] = BottomLeftCol;
		Matrix[2, 1] = BottomLeftRow;
		Matrix[2, 2] = 1.0;
		Matrix[2, 6] = -BottomLeftCol * BottomLeftX;
		Matrix[2, 7] = -BottomLeftRow * BottomLeftX;
		Matrix[2, 8] = BottomLeftX;

		Matrix[3, 0] = BottomRightCol;
		Matrix[3, 1] = BottomRightRow;
		Matrix[3, 2] = 1.0;
		Matrix[3, 6] = -BottomRightCol * BottomRightX;
		Matrix[3, 7] = -BottomRightRow * BottomRightX;
		Matrix[3, 8] = BottomRightX;

		Matrix[4, 3] = TopLeftCol;
		Matrix[4, 4] = TopLeftRow;
		Matrix[4, 5] = 1.0;
		Matrix[4, 6] = -TopLeftCol * TopLeftY;
		Matrix[4, 7] = -TopLeftRow * TopLeftY;
		Matrix[4, 8] = TopLeftY;

		Matrix[5, 3] = TopRightCol;
		Matrix[5, 4] = TopRightRow;
		Matrix[5, 5] = 1.0;
		Matrix[5, 6] = -TopRightCol * TopRightY;
		Matrix[5, 7] = -TopRightRow * TopRightY;
		Matrix[5, 8] = TopRightY;

		Matrix[6, 3] = BottomLeftCol;
		Matrix[6, 4] = BottomLeftRow;
		Matrix[6, 5] = 1.0;
		Matrix[6, 6] = -BottomLeftCol * BottomLeftY;
		Matrix[6, 7] = -BottomLeftRow * BottomLeftY;
		Matrix[6, 8] = BottomLeftY;

		Matrix[7, 3] = BottomRightCol;
		Matrix[7, 4] = BottomRightRow;
		Matrix[7, 5] = 1.0;
		Matrix[7, 6] = -BottomRightCol * BottomRightY;
		Matrix[7, 7] = -BottomRightRow * BottomRightY;
		Matrix[7, 8] = BottomRightY;

		for(int Row = 0; Row < 8; Row++)
			{
		    // If the element is zero, make it non zero by adding another row
		    if(Matrix[Row, Row] == 0)
			    {
		        int Row1;
				for (Row1 = Row + 1; Row1 < 8 && Matrix[Row1, Row] == 0; Row1++);
			    if (Row1 == 8) return false;

                for(int Col = Row; Col < 9; Col++) Matrix[Row, Col] += Matrix[Row1, Col];
	            }

			// make the diagonal element 1.0
			for(int Col = 8; Col > Row; Col--) Matrix[Row, Col] /= Matrix[Row, Row];

			// subtract current row from next rows to eliminate one value
			for(int Row1 = Row + 1; Row1 < 8; Row1++)
				{
				for (int Col = 8; Col > Row; Col--) Matrix[Row1, Col] -= Matrix[Row, Col] * Matrix[Row1, Row];
				}
			}

		// go up from last row and eliminate all solved values
		for(int Col = 7; Col > 0; Col--) for(int Row = Col - 1; Row >= 0; Row--)
		    {
		    Matrix[Row, 8] -= Matrix[Row, Col] * Matrix[Col, 8];
			}

		// save transformation matrix coefficients
		Trans4a = Matrix[0, 8];
		Trans4b = Matrix[1, 8];
		Trans4c = Matrix[2, 8];
		Trans4d = Matrix[3, 8];
		Trans4e = Matrix[4, 8];
		Trans4f = Matrix[5, 8];
		Trans4g = Matrix[6, 8];
		Trans4h = Matrix[7, 8];
		return true;
		}

	private bool GetCodewords()
		{
		try
			{
			// codewords array
			Codewords = new int[DataColumns * DataRows];
			int CWPtr = 0;

			// erasures
			int ErasuresCount = 0;

			#if DEBUG && CODEWORDS
			StringBuilder Str = new StringBuilder();
 			#endif

			// convert modules to codewords
			for(int BarcodeY = 0; BarcodeY < DataRows; BarcodeY++)
				{
				#if DEBUG && CODEWORDS
				Str.Clear();
				Str.AppendFormat("{0}: ", BarcodeY);
				#endif

				for(int BarcodeX = 0; BarcodeX < DataColumns; BarcodeX++)
					{
					int Codeword = DataCodeword(BarcodeX, BarcodeY);
					if(Codeword < 0)
						{
						#if DEBUG && CODEWORDS
						Str.Append("Err, ");
						#endif

						Codewords[CWPtr++] = 0;
						ErasuresCount++;
						if(ErasuresCount > ErrorCorrectionLength / 2)
							{
							#if DEBUG
							Pdf417Trace.Format("Erasures count {0} > Error correction length {1} / 2",
								ErasuresCount, ErrorCorrectionLength);
							#endif
							return false;
							}
						}
					else
						{
						Codewords[CWPtr++] = Codeword;
						#if DEBUG && CODEWORDS
						if(CodewordFix == 0)
							Str.AppendFormat("{0}, ", Codeword);
						else
							Str.AppendFormat("{0}({1}), ", Codeword, CodewordFix);
						#endif
						}
					}

				#if DEBUG && CODEWORDS
				Pdf417Trace.Write(Str.ToString());
				#endif
				}

			#if DEBUG
			Pdf417Trace.Format("Erasures count {0}, ", ErasuresCount);
			ErrorCorrection.DataRows = DataRows;
			ErrorCorrection.DataColumns = DataColumns;
			#endif

			// test for errors
			ErrorCorrectionCount = ErrorCorrection.TestCodewords(Codewords, ErrorCorrectionLength);

			// too many errors decode failed
			if(ErrorCorrectionCount < 0)
				{
				#if DEBUG
				Pdf417Trace.Write("Error correction failed");
				#endif
				return false;
				}

			#if DEBUG
			if(ErrorCorrectionCount == 0)
				Pdf417Trace.Write("Decode successful. No errors");
			else
				Pdf417Trace.Format("Error count: {0}", ErrorCorrectionCount);
			#endif

			// return codewords array
			return true;
			}

		catch (Exception Ex)
			{
			#if DEBUG
			Pdf417Trace.Write("Image to codewords decode failed\r\n" + Ex.Message);
			#endif
			return false;
			}
		}

	////////////////////////////////////////////////////////////////////
	// Get data codeword
	////////////////////////////////////////////////////////////////////

	private int DataCodeword
			(
			int DataMatrixX,
			int DataMatrixY
			)
		{
		double W = Trans4g * DataMatrixX + Trans4h * DataMatrixY + 1.0;
		int OrigX = (int) Math.Round((Trans4a * DataMatrixX + Trans4b * DataMatrixY + Trans4c) / W, 0, MidpointRounding.AwayFromZero);
		int OrigY = (int) Math.Round((Trans4d * DataMatrixX + Trans4e * DataMatrixY + Trans4f) / W, 0, MidpointRounding.AwayFromZero);
		
		DataMatrixX++;
		W = Trans4g * DataMatrixX + Trans4h * DataMatrixY + 1.0;
		int DeltaX = (int) Math.Round((Trans4a * DataMatrixX + Trans4b * DataMatrixY + Trans4c) / W, 0, MidpointRounding.AwayFromZero) - OrigX;
		int DeltaY = (int) Math.Round((Trans4d * DataMatrixX + Trans4e * DataMatrixY + Trans4f) / W, 0, MidpointRounding.AwayFromZero) - OrigY;

		#if DEBUG
		CodewordFix = 0;
		#endif

		// get codeword
		int Codeword = GetCodeword(OrigX, OrigY, DeltaX, DeltaY);
		if(Codeword >= 0 && Codeword >> 10 == DataMatrixY % 3) return Codeword & 0x3ff;

		// try to fix the problem
		for(int Index = 0; Index < YStep.Length; Index++)
			{
			int Y = OrigY + YStep[Index];
			int X = OrigX - (Y - OrigY) * DeltaY / DeltaX; 
			Codeword = GetCodeword(X, Y, DeltaX, DeltaY);
			if(Codeword >= 0 && Codeword >> 10 == DataMatrixY % 3)
				{
				#if DEBUG
				CodewordFix = YStep[Index];
				#endif
				return Codeword & 0x3ff;
				}

			}

		// error return
		return -1;
		}

	internal int GetCodeword
			(
			int LeftX,
			int LeftY,
			int DeltaX,
			int DeltaY
			)
		{
		try
			{
			// make sure we are on a white to black transition
			WhiteToBlackTransition(ref LeftX, ref LeftY, DeltaX, DeltaY);
	
			// go right looking for color transition
			ScanX[0] = LeftX;
			ScanY[0] = LeftY;
			bool DotColor = true;
			int T = 1;
			for(int X = LeftX + 1; T < 9; X++)
				{
				int Y = LeftY + (X - LeftX) * DeltaY / DeltaX;
				if(ImageMatrix[Y, X] == DotColor) continue;
				DotColor = !DotColor;
				ScanX[T] = X;
				ScanY[T++] = Y;
				}

			return ScanToCodeword();
			}
		catch
			{
			return -2;
			}
		}


	internal int RevGetCodeword
			(
			int RightX,
			int RightY,
			int DeltaX,
			int DeltaY
			)
		{
		try
			{
			// make sure we are on a white to black transition
			WhiteToBlackTransition(ref RightX, ref RightY, DeltaX, DeltaY);

			// go left looking for color transition
			ScanX[8] = RightX;
			ScanY[8] = RightY;
			bool DotColor = false;
			int T = 7;
			for(int X = RightX - 1; T >= 0; X--)
				{
				int Y = RightY + (X - RightX) * DeltaY / DeltaX;
				if(ImageMatrix[Y, X] == DotColor) continue;
				DotColor = !DotColor;
				ScanX[T] = X;
				ScanY[T--] = Y;
				}
			return ScanToCodeword();
			}

		catch
			{
			return -1;
			}
		}

	private void WhiteToBlackTransition
			(
			ref int PosX,
			ref int PosY,
			int DeltaX,
			int DeltaY
			)
		{
		// current pixel is black
		if(ImageMatrix[PosY, PosX])
			{
			// pixel on the left is white
			if(!ImageMatrix[PosY, PosX - 1]) return;

			// go left to find first white pixel
			for(int X = PosX - 1;; X--)
				{
				// matching y coordinate
				int Y = PosY + (X - PosX) * DeltaY / DeltaX;

				// pixel is white
				if(!ImageMatrix[Y, X]) return;

				// move current pixel one to the left
				PosX = X;
				PosY = Y;
				}
			}

		// current pixel is white
		// go right to the next transition from white to black
		for(int X = PosX + 1;; X++)
			{
			// matching y coordinate
			int Y = PosY + (X - PosX) * DeltaY / DeltaX;

			// pixel is white
			if(!ImageMatrix[Y, X]) continue;

			// return black point
			PosX = X;
			PosY = Y;
			return;
			}
		}

	private int ScanToCodeword()
		{
		// line slope
		int ScanDeltaX = ScanX[8] - ScanX[0];
		int ScanDeltaY = ScanY[8] - ScanY[0];

		// line length
		double Length = Math.Sqrt(ScanDeltaX * ScanDeltaX + ScanDeltaY * ScanDeltaY);

		if(Math.Abs(Length - AvgSymbolWidth) > MaxSymbolError) return -1;

		// one over one bar width
		double InvWidth = ModulesInCodeword / Length;

		// reset symbol
		int Symbol = 0;

		// reset mode
		int Mode = 9;

		// loop for two bars
		for(int BarIndex = 0; BarIndex < 6; BarIndex++)
			{
			// two bars slope
			int BDX = ScanX[BarIndex + 2] - ScanX[BarIndex];
			int BDY = ScanY[BarIndex + 2] - ScanY[BarIndex];

			// two bars width must be 2 to 9
			int TwoBars = (int) Math.Round(InvWidth *  Math.Sqrt(BDX * BDX + BDY * BDY), 0, MidpointRounding.AwayFromZero);

			// error
			if(TwoBars < 2 || TwoBars > 9) return -1;

			// accumulate symbol
			// symbol is made of 6 two bars width
			// we subtract 2 to make the range of 0 to 7 (3 bits)
			// we pack 6 two bar width into 18 bits
			Symbol |= (TwoBars - 2) << 3 * (5 - BarIndex);

			if(BarIndex == 0 || BarIndex == 4) Mode += TwoBars;
			else if(BarIndex == 1 || BarIndex == 5) Mode -= TwoBars;
			}

		// test mode
		Mode = Mode % 9;
		if(Mode != 0 && Mode != 3 && Mode != 6) return -1;

		// translate symbol to cluster plus codeword
		int Index = Array.BinarySearch(StaticTables.SymbolTable, Symbol << 12, SymbolComparer);

		// symbol not found
		if(Index < 0) return -1;

		// symbol found
		return StaticTables.SymbolTable[Index] & 0xfff;
		}

	// declare symbol comparer class
	private static readonly SymbolComp SymbolComparer = new SymbolComp();

	/// <summary>
	/// define comparer class
	/// compare symbol bits only (bits 29 to bit 12)
	/// </summary>
	public class SymbolComp : IComparer<int>
		{
		/// <summary>
		/// Compare two barcodes symbols
		/// </summary>
		/// <param name="One">First symbol</param>
		/// <param name="Two">Second symbol</param>
		/// <returns></returns>
		public int Compare(int One, int Two)
			{
			return (One & 0x7ffff000) - (Two & 0x7ffff000);
			}
		}

	/////////////////////////////////////////////////////////////////
	// Convert codewords to data
	/////////////////////////////////////////////////////////////////

	private bool CodewordsToData()
		{
		// data codewords pointer and end
		CodewordsPtr = 1;
		int CodewordsEnd = Codewords[0];

		// make sure data length make sense
		if(CodewordsEnd + ErrorCorrectionLength != DataColumns * DataRows)
			{
			#if DEBUG
			Pdf417Trace.Format("Codewords length error: Data Len {0}, Err Len {1}, Columns (2}, Rows {3}",
				CodewordsEnd, ErrorCorrectionLength, DataColumns, DataRows);
			#endif
			return false;
			}
	
		// initialize encoding modes
		//_EncodingMode = EncodingMode.Text;
		_TextEncodingMode = TextEncodingMode.Upper;

		// binary data result
		List<byte> BinaryData = new List<byte>();

		while(CodewordsPtr < CodewordsEnd)
			{
			// load codeword at current pointer
			int Command = Codewords[CodewordsPtr++];

			// for the first time this codeword can be data
			if(Command < 900)
				{
				Command = SwitchToTextMode;
				CodewordsPtr--;
				}

			// count codewords data
			int SegEnd;
			for(SegEnd = CodewordsPtr; SegEnd < CodewordsEnd && Codewords[SegEnd] < 900; SegEnd++);

			// segment length
			int SegLen = SegEnd - CodewordsPtr;

			// segment is empty
			if(SegLen == 0) continue;

			// process command
			switch(Command)
				{
				case SwitchToByteMode:
					//_EncodingMode = EncodingMode.Byte;
					_TextEncodingMode = TextEncodingMode.Upper;
					CodewordsToBytes(BinaryData, SegLen, false);
					break;

				case SwitchToByteModeForSix:
					//_EncodingMode = EncodingMode.Byte;
					_TextEncodingMode = TextEncodingMode.Upper;
					CodewordsToBytes(BinaryData, SegLen, true);
					break;

				case ShiftToByteMode:
					int ShiftByte = Codewords[CodewordsPtr++];
					if(ShiftByte >= 900)
						{
						#if DEBUG
						Pdf417Trace.Write("Decode codewords error. Shift to byte mode");
						#endif
						return false;
						}
					BinaryData.Add((byte) ShiftByte);
					break;

				case SwitchToTextMode:
					//_EncodingMode = EncodingMode.Text;
					CodewordsToText(BinaryData, SegLen);
					break;

				case SwitchToNumericMode:
					//_EncodingMode = EncodingMode.Numeric;
					_TextEncodingMode = TextEncodingMode.Upper;
					CodewordsToNumeric(BinaryData, SegLen);
					break;

				case GliCharacterSet:
					if(BinaryData.Count > 0) 
						{
						#if DEBUG
						Pdf417Trace.Write("Decode codewords error. Character set value must be before data.");
						#endif
						return false;
						}

					int G1 = Codewords[CodewordsPtr++];
					if(G1 >= 900) 
						{
						#if DEBUG
						Pdf417Trace.Write("Decode codewords error. Character set value");
						#endif
						return false;
						}

					GlobalLabelIDCharacterSetNo = G1;
					int Part = G1 - 2;
					if(Part < 1 || Part > 9 && Part != 13 && Part != 15) Part = 1;
					GlobalLabelIDCharacterSet = string.Format("ISO-8859-{0}", Part);
					#if DEBUG
					Pdf417Trace.Format("Character set: G1={0}, {1}", G1, GlobalLabelIDCharacterSet);
					#endif
					break;

				case GliGeneralPurpose:
					if(BinaryData.Count > 0) 
						{
						#if DEBUG
						Pdf417Trace.Write("Decode codewords error. General purpose value must be before data.");
						#endif
						return false;
						}

					int G2 = Codewords[CodewordsPtr++];
					int G3 = Codewords[CodewordsPtr++];
					if(G2 >= 900 || G3 >= 900)
						{
						#if DEBUG
						Pdf417Trace.Write("Decode codewords error. General purpose value");
						#endif
						return false;
						}

					GlobalLabelIDGeneralPurpose = 900 * (G2 + 1) + G3;
					#if DEBUG
					Pdf417Trace.Format("GLID General purpose: G2={0}, G3={1}", G2, G3);
					#endif
					break;

				case GliUserDefined:
					if(BinaryData.Count > 0)
						{
						#if DEBUG
						Pdf417Trace.Write("Decode codewords error. User define value must be before data.");
						#endif
						return false;
						}

					int G4 = Codewords[CodewordsPtr++];
					if(G4 >= 900)
						{
						#if DEBUG
						Pdf417Trace.Write("Decode codewords error. User defined value");
						#endif
						return false;
						}

					GlobalLabelIDUserDefined = 810900 + G4;
					#if DEBUG
					Pdf417Trace.Format("GLID User Defined: G4={0}", G4);
					#endif
					break;

				default:
					#if DEBUG
					Pdf417Trace.Write("Decode codewords error. Unsupported command codeword [" + Command.ToString() + "]");
					#endif
					return false;
				}
			}

		// convert list to array
		BarcodeBinaryData = BinaryData.ToArray();

		// return binary bytes array
		return true;
		}

	/////////////////////////////////////////////////////////////////
	// Convert codewords to bytes
	/////////////////////////////////////////////////////////////////

	private void CodewordsToBytes
			(
			List<byte> BinaryData,
			int SegLen,
			bool SixFlag
			)
		{
		// number of whole 5 codewords blocks
		int Blocks = SegLen / 5;

		// if number of blocks is one or more and SixFlag is false, the last block is not converted 5 to 6
		if((SegLen % 5) == 0 && Blocks >= 1 && !SixFlag) Blocks--;

		// loop for blocks 
		for(int Block = 0; Block < Blocks; Block++)
			{
			long Temp = StaticTables.Fact900[4] * Codewords[CodewordsPtr++] +
						StaticTables.Fact900[3] * Codewords[CodewordsPtr++] +
						StaticTables.Fact900[2] * Codewords[CodewordsPtr++] +
						StaticTables.Fact900[1] * Codewords[CodewordsPtr++] +
						Codewords[CodewordsPtr++];

			// convert to bytes
			for(int Index = 0; Index < 6; Index++)
				{
				BinaryData.Add((byte) (Temp >> (40 - 8 * Index)));
				}
			}

		// left over
		SegLen -= 5 * Blocks;
		while(SegLen > 0)
			{
			BinaryData.Add((byte) Codewords[CodewordsPtr++]);
			SegLen--;
			}
		return;
		}

	/////////////////////////////////////////////////////////////////
	// Convert codewords to numeric characters
	/////////////////////////////////////////////////////////////////

	private void CodewordsToNumeric
			(
			List<byte> BinaryData,
			int SegLen
			)
		{
		// loop for blocks of 15 or less codewords
		int BlockLen;
		for(; SegLen > 0; SegLen -= BlockLen)
			{
			// block length
			BlockLen = Math.Min(SegLen, 15);

			// convert block to big integer number
			BigInteger Temp = BigInteger.Zero;
			for(int Index = BlockLen - 1; Index >= 0; Index--)
				{
				Temp += StaticTables.FactBigInt900[Index] * Codewords[CodewordsPtr++];
				}

			// convert number to a string
			string NumStr = Temp.ToString();

			// convert string to bytes (skip first digit, it is 1)
			for(int Index = 1; Index < NumStr.Length; Index++)
				{
				BinaryData.Add((byte) NumStr[Index]);
				}
			}
		return;
		}

	/////////////////////////////////////////////////////////////////
	// Convert codewords to text
	/////////////////////////////////////////////////////////////////

	private void CodewordsToText
			(
			List<byte> BinaryData,
			int SegLen
			)
		{
		int TextLen = 2 * SegLen;
		int Code;
		int Next = 0;
		TextEncodingMode SaveMode = TextEncodingMode.Upper;
		byte AsciiChar = 0;
		for(int Index = 0; Index < TextLen; Index++)
			{
			if((Index & 1) == 0)
				{
				int Codeword = Codewords[CodewordsPtr++];
				Code = Codeword / 30;
				Next = Codeword % 30;
				}
			else
				{
				Code = Next;
				if(Code == 29 && Index == TextLen - 1) break;
				}

			switch(_TextEncodingMode)
				{
				case TextEncodingMode.Upper:
					if((AsciiChar = StaticTables.UpperToText[Code]) != 0) break;
					if(Code == 27) _TextEncodingMode = TextEncodingMode.Lower;
					else if(Code == 28) _TextEncodingMode = TextEncodingMode.Mixed;
					else
						{
						SaveMode = _TextEncodingMode;
						_TextEncodingMode = TextEncodingMode.ShiftPunct;
						}
					continue;

				case TextEncodingMode.Lower:
					if((AsciiChar = StaticTables.LowerToText[Code]) != 0) break;
					if(Code == 27) _TextEncodingMode = TextEncodingMode.ShiftUpper;
					else if(Code == 28) _TextEncodingMode = TextEncodingMode.Mixed;
					else
						{
						SaveMode = _TextEncodingMode;
						_TextEncodingMode = TextEncodingMode.ShiftPunct;
						}
					continue;

				case TextEncodingMode.Mixed:
					if((AsciiChar = StaticTables.MixedToText[Code]) != 0) break;
					if(Code == 25) _TextEncodingMode = TextEncodingMode.Punct;
					else if(Code == 27) _TextEncodingMode = TextEncodingMode.Lower;
					else if(Code == 28) _TextEncodingMode = TextEncodingMode.Upper;
					else
						{
						SaveMode = _TextEncodingMode;
						_TextEncodingMode = TextEncodingMode.ShiftPunct;
						}
					continue;

				case TextEncodingMode.Punct:
					if((AsciiChar = StaticTables.PunctToText[Code]) != 0) break;
					_TextEncodingMode = TextEncodingMode.Upper;
					continue;

				case TextEncodingMode.ShiftUpper:
					_TextEncodingMode = TextEncodingMode.Lower;
					if((AsciiChar = StaticTables.UpperToText[Code]) != 0) break;
					throw new ApplicationException("Text decoding error. Shift to upper case.");

				case TextEncodingMode.ShiftPunct:
					_TextEncodingMode = SaveMode;
					if((AsciiChar = StaticTables.PunctToText[Code]) != 0) break;
					throw new ApplicationException("Text decoding error. Shift to punctuation.");
				}
			BinaryData.Add(AsciiChar);
			}
		return;
		}

	#if DEBUG && SAVEBWIMAGE
	private Bitmap CreateBitmap()
		{
		// create picture object and make it white
		Bitmap ImageBitmap = new Bitmap(ImageWidth, ImageHeight);
		Graphics Graphics = Graphics.FromImage(ImageBitmap);
		Graphics.FillRectangle(Brushes.White, 0, 0, ImageWidth, ImageHeight);

		// convert bool image matrix to bitmap
		for(int Row = 0; Row < ImageHeight; Row++)
			{
			for(int Col = 0; Col < ImageWidth; Col++)
				{
				if(ImageMatrix[Row, Col]) ImageBitmap.SetPixel(Col, Row, Color.Black);
				}
			}

		// return bitmap
		return ImageBitmap;
		}
	#endif
	}
}
