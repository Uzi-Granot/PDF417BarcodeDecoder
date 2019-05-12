/////////////////////////////////////////////////////////////////////
//
//	PDF417 Barcode Decoder
//
//	ErrorCorrection class
//	Error detection and correction
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
//	This source code is base on the work of ZXing.com.
//	The original source was modified to enable integeration.
//	The original copyright notice is:
//	Copyright 2012 ZXing authors
//	Licensed under the Apache License, Version 2.0 (the "License");
//
//	Version History
//	---------------
//
//	Version 1.0 2019/05/01
//		Original version
/////////////////////////////////////////////////////////////////////

using System;
using System.Text;

namespace Pdf417DecoderLibrary
{
internal static class ErrorCorrection
    {
	#if DEBUG
	internal static int DataRows;
	internal static int DataColumns;
	#endif

    // Decode the received codewords
	internal static int TestCodewords
			(
			int[] Codewords,
			int ErrorCorrectionLength
			)
		{
		// create codewords polynomial
		Polynomial PolyCodewords = new Polynomial(Codewords);

		// create syndrom coefficients array
		int[] Syndrome = new int[ErrorCorrectionLength];

		// assume no errors
		bool Error = false;

		// test for errors
		// if the syndrom array is all zeros, there is no error
		for(int Index = ErrorCorrectionLength; Index > 0; Index--)
			{
			if((Syndrome[ErrorCorrectionLength - Index] = PolyCodewords.EvaluateAt(Modulus.ExpTable[Index])) != 0) Error = true;
			}

		// no errors
		if(!Error) return 0;

		// convert syndrom array to polynomial
		Polynomial PolySyndrome = new Polynomial(Syndrome);

		// Greatest Common Divisor (return -1 if error cannot be corrected)
		if(!EuclideanAlgorithm(ErrorCorrectionLength, PolySyndrome,
			out Polynomial ErrorLocator, out Polynomial ErrorEvaluator)) return -1;

		// error locator (return -1 if error cannot be corrected)
		int[] ErrorLocations = FindErrorLocations(ErrorLocator);
		if(ErrorLocations == null) return -1;

		// formal derivatives
		Polynomial FormalDerivative = FindFormalDerivatives(ErrorLocator);
		
		// This is directly applying Forney's Formula
		int Errors = ErrorLocations.Length;
		for (int Index = 0; Index < Errors; Index++)
			{
			// error location
			int ErrLoc = ErrorLocations[Index];

			// error position  (return -1 if error cannot be corrected)
			int ErrPos = Codewords.Length - 1 - Modulus.LogTable[Modulus.Inverse(ErrLoc)];
			if (ErrPos < 0) return -1;

			// error magnitude
			int ErrorMagnitude = Modulus.Divide(Modulus.Negate(ErrorEvaluator.EvaluateAt(ErrLoc)), FormalDerivative.EvaluateAt(ErrLoc));

			// correct codeword
			Codewords[ErrPos] = Modulus.Subtract(Codewords[ErrPos], ErrorMagnitude);

			// save error position in error locations array
			ErrorLocations[Index] = ErrPos;
			}

		#if DEBUG && ERRCORRECT
		Array.Sort(ErrorLocations);
		StringBuilder Str1 = new StringBuilder();
		for(int Row = 0; Row < DataRows; Row++)
			{
			Str1.Clear();
			Str1.AppendFormat("{0}: ", Row);
			for(int Col = 1; Col <= DataColumns; Col++)
				{
				int Pos = DataColumns * Row + Col - 1;
				int Index = Array.BinarySearch(ErrorLocations, Pos);
				if(Index < 0)	
					Str1.AppendFormat("{0}, ", Codewords[Pos]);
				else
					Str1.AppendFormat("{0}*, ", Codewords[Pos]);
				}
			Pdf417Trace.Write(Str1.ToString());
			}
		#endif

		// message was successfuly repaired
		return Errors;
		}

	// Runs the euclidean algorithm (Greatest Common Divisor) until r's degree is less than R/2
	private static bool EuclideanAlgorithm
			(
			int ErrorCorrectionLength,
			Polynomial PolyR,
			out Polynomial ErrorLocator,
			out Polynomial ErrorEvaluator
			)
		{
		// reset output
		ErrorLocator = null;
		ErrorEvaluator = null;

		// set last remainder polynomial to monomial 
		// this polynomial degree is always greater than PolyR input argument
		Polynomial PolyRLast = new Polynomial(ErrorCorrectionLength, 1);

		Polynomial PolyTLast = Polynomial.Zero;
		Polynomial PolyT = Polynomial.One;

		// Run Euclidean algorithm until r's degree is less than R/2
		while (PolyR.PolyDegree >= ErrorCorrectionLength / 2)
			{
			Polynomial PolyRLast2 = PolyRLast;
			Polynomial PolyTLast2 = PolyTLast;
			PolyRLast = PolyR;
			PolyTLast = PolyT;

			// Euclidean algorithm already terminated?
			if (PolyRLast.IsZero) return false;

			// Divide rLastLast by PolyRLast, with quotient in q and remainder in r
			PolyR = PolyRLast2;

			// initial quotient polynomial
			Polynomial Quotient = Polynomial.Zero;

			// inverse of leading coefficient
			int dltInverse = Modulus.Inverse(PolyRLast.LeadingCoefficient);

			// subtract 
			while (PolyR.PolyDegree >= PolyRLast.PolyDegree && !PolyR.IsZero)
				{
				// divide PolyR and PolyRLast leading coefficents
				int Scale = Modulus.Multiply(PolyR.LeadingCoefficient, dltInverse);

				// degree difference between PolyR and PolyRLest
				int DegreeDiff = PolyR.PolyDegree - PolyRLast.PolyDegree;

				// build quotient
				Quotient = Quotient.Add(new Polynomial(DegreeDiff, Scale));

				// update remainder
				PolyR = PolyR.Subtract(PolyRLast.MultiplyByMonomial(DegreeDiff, Scale));
				}

			PolyT = Quotient.Multiply(PolyTLast).Subtract(PolyTLast2).MakeNegative();
			}

		int SigmaTildeAtZero = PolyT.LastCoefficient;
		if (SigmaTildeAtZero == 0) return false;

		int Inverse = Modulus.Inverse(SigmaTildeAtZero);
		ErrorLocator = PolyT.Multiply(Inverse);
		ErrorEvaluator = PolyR.Multiply(Inverse);
		return true;
		}

	// Finds the error locations as a direct application of Chien's search
	// error locations are not error positions within codewords array
	private static int[] FindErrorLocations
			(
			Polynomial ErrorLocator
			)
		{
		// This is a direct application of Chien's search
		int LocatorDegree = ErrorLocator.PolyDegree;
		int[] ErrorLocations = new int[LocatorDegree];
		int ErrCount = 0;
		for (int Index = 1; Index < Modulus.MOD && ErrCount < LocatorDegree; Index++)
			{
			if(ErrorLocator.EvaluateAt(Index) == 0) ErrorLocations[ErrCount++] = Index;
			}
		return ErrCount == LocatorDegree ? ErrorLocations : null;
		}

	// Finds the error magnitudes by directly applying Forney's Formula
	private static Polynomial FindFormalDerivatives
			(
			Polynomial ErrorLocator
			)
		{
		int LocatorDegree = ErrorLocator.PolyDegree;
		int[] DerivativesCoefficients = new int[LocatorDegree];
		for (int Index = 1; Index <= LocatorDegree; Index++)
			{
			DerivativesCoefficients[LocatorDegree - Index] = Modulus.Multiply(Index, ErrorLocator.GetCoefficient(Index));
			}
		return new Polynomial(DerivativesCoefficients);
		}
    }
}