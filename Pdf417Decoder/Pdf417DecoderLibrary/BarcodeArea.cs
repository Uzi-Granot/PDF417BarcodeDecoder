/////////////////////////////////////////////////////////////////////
//
//	PDF417 Barcode Decoder
//
//	BarcodeArea class
//	Barcode area coordinates
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

namespace Pdf417DecoderLibrary
{
internal class BarcodeArea
	{
	// left border line of PDF 417 barcode excluding start border
	internal int LeftCenterX;
	internal int LeftCenterY;
	internal int LeftDeltaX;
	internal int LeftDeltaY;

	// right border line of PDF 417 barcode excluding stop border
	internal int RightCenterX;
	internal int RightCenterY;
	internal int RightDeltaX;
	internal int RightDeltaY;

	// average symbol width of start and stop borders
	internal double AvgSymbolWidth;
	internal const double MAX_SYMBOL_ERROR = 0.08;
	internal double MaxSymbolError;

	internal BarcodeArea
			(
			BorderPattern StartBorder,
			BorderPattern StopBorder
			)
		{
		// left border line of PDF 417 barcode excluding start border
		LeftCenterX = StartBorder.CenterX;
		LeftCenterY = StartBorder.CenterY;
		LeftDeltaX = StartBorder.DeltaX;
		LeftDeltaY = StartBorder.DeltaY;

		// right border line of PDF 417 barcode excluding stop border
		RightCenterX = StopBorder.CenterX;
		RightCenterY = StopBorder.CenterY;
		RightDeltaX = StopBorder.DeltaX;
		RightDeltaY = StopBorder.DeltaY;

		// average symbol width of start and stop borders
		AvgSymbolWidth = 0.5 * (StartBorder.AvgSymbolWidth + StopBorder.AvgSymbolWidth);
		MaxSymbolError = MAX_SYMBOL_ERROR * AvgSymbolWidth;
		return;
		}

	// left border x position as function of y
	internal int LeftXFuncY
			(
			int PosY
			)
		{
		// calculate x coordinate
		return LeftCenterX + (LeftDeltaX * (PosY - LeftCenterY)) / LeftDeltaY;
		}

	// right border x position as function of y
	internal int RightXFuncY
			(
			int PosY
			)
		{
		// calculate x coordinate
		return RightCenterX + (RightDeltaX * (PosY - RightCenterY)) / RightDeltaY;
		}
	}
}
