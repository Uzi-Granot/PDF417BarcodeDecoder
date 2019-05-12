/////////////////////////////////////////////////////////////////////
//
//	PDF417 Barcode Decoder
//
//	BarcodeSymbol class
//	Border start or stop symbol coordinates
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
internal class BorderSymbol
	{
	internal int X1;
	internal int Y1;
	internal int X2;

	internal BorderSymbol
			(
			int X1,
			int Y1,
			int X2
			)
		{
		this.X1 = X1;
		this.Y1 = Y1;
		this.X2 = X2;
		return;
		}
	}
}
