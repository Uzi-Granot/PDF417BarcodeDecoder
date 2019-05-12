/////////////////////////////////////////////////////////////////////
//
//	PDF417 Barcode Decoder
//
//	Polynomial class
//	Represent a polynomial for error detection and correction
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

namespace Pdf417DecoderLibrary
{
internal class Polynomial
    {
	// Polynomial coefficients.
	internal int[] Coefficients;

	// polynomial length (Coefficient.Length)
	internal int PolyLength;

	// polynomial degree (Coefficient.Length - 1) 
	internal int PolyDegree;

	// zero and one polynomial
	internal static Polynomial Zero = new Polynomial(new int[] {0});
	internal static Polynomial One = new Polynomial(new int[] {1});

	// Constructor
	internal Polynomial
			(
			int[] Coefficients
			)
        {
		// Coefficients lenght
		PolyLength = Coefficients.Length;

		// make sure first term is not zero (except for zero polynomial)
		if(PolyLength > 1 && Coefficients[0] == 0)
			{
			// count leading zeros
			int FirstNonZero;
			for(FirstNonZero = 1; FirstNonZero < PolyLength && Coefficients[FirstNonZero] == 0; FirstNonZero++);

			// all coefficients are zeros
			if(FirstNonZero == PolyLength)
				{
				this.Coefficients = new int[] {0};
				PolyLength = 1;
				}
			else
				{
				// new length
				PolyLength -= FirstNonZero;

				// create shorter coefficients array
				this.Coefficients = new int[PolyLength];

				// copy non zero part to new array
				Array.Copy(Coefficients, FirstNonZero, this.Coefficients, 0, PolyLength);
				}
			}

		// save coefficient array argument unchanged
		else
			{
			this.Coefficients = Coefficients;
			}

		// set polynomial degree
		PolyDegree = PolyLength - 1;
		return;
		}

	// polynomial constructor for monomial
	internal Polynomial
			(
			int Degree,
			int Coefficient
			)
		{
		// create polynomial coefficients array with one leading non zero value
		PolyDegree = Degree;
		PolyLength = Degree + 1;
		Coefficients = new int[PolyLength];
		Coefficients[0] = Coefficient;
		return;
		}

	// test for zero polynomial
    internal bool IsZero
		{
		get
			{
			return Coefficients[0] == 0;
			}
		}

	// coefficient value of degree term in this polynomial
	internal int GetCoefficient
			(
			int Degree
			)
		{
		return Coefficients[PolyDegree - Degree];
		}

	// coefficient value of zero degree term in this polynomial
	internal int LastCoefficient
		{
		get
			{
			return Coefficients[PolyDegree];
			}
		}

	// leading coefficient
	internal int LeadingCoefficient
		{
		get
			{
			return Coefficients[0];
			}
		}

	// evaluation of this polynomial at a given point
	internal int EvaluateAt
			(
			int XValue
			)
		{
		// return the x^0 coefficient
		if(XValue == 0) return Coefficients[0];

		// set result to zero
		int Result;

		// return the x^1 coefficient
		if(XValue == 1)
			{
			// return the sum of the coefficients
			Result = 0;
			foreach(int Coefficient in Coefficients) Result = Modulus.Add(Result, Coefficient);
			}

		// X value > 1
		else
			{
			Result = Coefficients[0];
			for (int Index = 1; Index < PolyLength; Index++)
				{
				Result = Modulus.Add(Modulus.Multiply(XValue, Result), Coefficients[Index]);
				}
			}
		return Result;
		}

	// Adds two polynomials
	internal Polynomial Add
			(
			Polynomial Other
			)
		{
		// this polynomial is zero
		if(IsZero) return Other;

		// other polynomial is zero
		if(Other.IsZero) return this;

		// assume this polynomial is smaller than the other one
		int[] Smaller = Coefficients;
		int[] Larger = Other.Coefficients;

		// assumption is wrong. exchange the two arrays
		if(Smaller.Length > Larger.Length)
			{
			int[] Temp = Smaller;
			Smaller = Larger;
			Larger = Temp;
			}

		// create new coefficient array
		int[] Result = new int[Larger.Length];
	
		// length difference
		int Delta = Larger.Length - Smaller.Length;

		// Copy high-order terms only found in higher-degree polynomial's coefficients
		Array.Copy(Larger, 0, Result, 0, Delta);

		// add the coefficients of the two polynomials
		for(int Index = Delta; Index < Larger.Length; Index++)
			{
			Result[Index] = Modulus.Add(Smaller[Index - Delta], Larger[Index]);
			}

		// return the result
		return new Polynomial(Result);
		}

	// Subtract two polynomials
	internal Polynomial Subtract
			(
			Polynomial other
			)
		{
		if(other.IsZero) return this;
		return Add(other.MakeNegative());
		}

	// Multiply two polynomials
	internal Polynomial Multiply
			(
			Polynomial Other
			)
		{
		// result is zero if either one is zero
		if(IsZero || Other.IsZero) return Zero;

		// shortcut of other
		int[] OtherCoefficients = Other.Coefficients;
		int OtherLength = Other.PolyLength;

		// result
		int[] Result = new int[PolyLength + OtherLength - 1];

		// multiply
		for(int i = 0; i < PolyLength; i++)
			{
			int Coeff = Coefficients[i];
			for (int j = 0; j < OtherLength; j++)
				{
				Result[i + j] = Modulus.Add(Result[i + j], Modulus.Multiply(Coeff, OtherCoefficients[j]));
				}
			}

		// return result
		return new Polynomial(Result);
		}

	// Returns a Negative version of this instance
	internal Polynomial MakeNegative()
		{
		int[] Result = new int[PolyLength];
		for (int i = 0; i < PolyLength; i++)
			{
			Result[i] = Modulus.Negate(Coefficients[i]);
			}
		return new Polynomial(Result);
		}

	// Multiply by a constant
	internal Polynomial Multiply
			(
			int Constant
			)
		{
		// result is zero
		if(Constant == 0) return Zero;

		// scalar is one
		if(Constant == 1) return this;

		// result array
		int[] Result = new int[PolyLength];

		// multiply
		for(int Index = 0; Index < PolyLength; Index++)
			{
			Result[Index] = Modulus.Multiply(Coefficients[Index], Constant);
			}

		// return result
		return new Polynomial(Result);
		}

	// Multiplies by a Monomial
	internal Polynomial MultiplyByMonomial
			(
			int Degree,
			int Constant
			)
		{
		// result is zero
		if(Constant == 0) return Zero;

		// create result array
		int[] Result = new int[PolyLength + Degree];

		// multiply
		for (int Index = 0; Index < PolyLength; Index++)
			{
			Result[Index] = Modulus.Multiply(Coefficients[Index], Constant);
			}

		// return result
		return new Polynomial(Result);
		}
    }
}