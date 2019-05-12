/////////////////////////////////////////////////////////////////////
//
//	PDF417 Barcode Decoder
//
//	BorderPattern class
//	Start and stop patterns geometry
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

namespace Pdf417DecoderLibrary
{
internal class BorderPattern
	{
	internal int CenterX;
	internal int CenterY;
	internal int DeltaX;
	internal int DeltaY;
	internal double BorderLength;
	internal double AvgSymbolWidth;

	internal BorderPattern
			(
			bool StopPattern,
			List<BorderSymbol> SymbolList
			)
		{
		int ArrLen = SymbolList.Count;
		int TotalWidth = 0;
		double DoubleDeltaX = 0;
		double DoubleDeltaY = 0;

		// start pattern
		if (!StopPattern)
			{ 
			foreach (BorderSymbol Sym in SymbolList)
				{
				CenterX += Sym.X2;
				CenterY += Sym.Y1;
				TotalWidth += Sym.X2 - Sym.X1;
				}
			CenterX /= ArrLen;
			CenterY /= ArrLen;

			// slope of x as func of y
			foreach (BorderSymbol Sym in SymbolList)
				{
				DoubleDeltaX += (Sym.X2 - CenterX) * (Sym.Y1 - CenterY);
				DoubleDeltaY += (Sym.Y1 - CenterY) * (Sym.Y1 - CenterY);
				}
			}

		// stop pattern
		else
			{
			foreach (BorderSymbol Sym in SymbolList)
				{
				CenterX += Sym.X1;
				CenterY += Sym.Y1;
				TotalWidth += Sym.X2 - Sym.X1;
				}
			CenterX /= ArrLen;
			CenterY /= ArrLen;
			foreach (BorderSymbol Sym in SymbolList)
				{
				DoubleDeltaX += (Sym.X1 - CenterX) * (Sym.Y1 - CenterY);
				DoubleDeltaY += (Sym.Y1 - CenterY) * (Sym.Y1 - CenterY);
				}
			}

		// border line length
		BorderLength = Math.Sqrt(DoubleDeltaX * DoubleDeltaX + DoubleDeltaY * DoubleDeltaY);

		// calculate barcode angle of rotation relative to the image
		double CosRot = DoubleDeltaY / BorderLength;
		double SinRot = DoubleDeltaX / BorderLength;

		// horizontal pattern width
		double HorWidth = (double) TotalWidth / ArrLen;

		// barcode average pattern width
		AvgSymbolWidth = CosRot * HorWidth;

		// the center position is either too high or too low
		// if the barcode is not parallel to the image coordinates
		double CenterAdj = 0.5 * SinRot * HorWidth;

		if(!StopPattern)
			{ 
			CenterX -= (int) Math.Round(CenterAdj * SinRot, 0, MidpointRounding.AwayFromZero);
			CenterY -= (int) Math.Round(CenterAdj * CosRot, 0, MidpointRounding.AwayFromZero);
			}

		else
			{ 
			CenterX += (int) Math.Round(CenterAdj * SinRot, 0, MidpointRounding.AwayFromZero);
			CenterY += (int) Math.Round(CenterAdj * CosRot, 0, MidpointRounding.AwayFromZero);
			}

		// left indicators edge slope
		DeltaY = 1000;
		DeltaX = (int)((DeltaY * DoubleDeltaX) / DoubleDeltaY);
		return;
		}
	}
}
