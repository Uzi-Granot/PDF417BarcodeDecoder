/////////////////////////////////////////////////////////////////////
//
//	PDF417 Barcode Decoder
//
//	Modulus class
//	Mod 929 aritmetic
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
internal static class Modulus
    {
	internal const int MOD = 929;

    internal static readonly int[] ExpTable = new int[MOD];
    internal static readonly int[] LogTable = new int[MOD];

	// static initializer
	// create exponent and log tables 
	static Modulus()
		{
		int Value = 1;
		for (int Index = 0; Index < MOD; Index++)
			{
			ExpTable[Index] = Value;
			LogTable[Value] = Index;
			Value = (3 * Value) % MOD;
			}
		return;
		}

	// add two values
	internal static int Add
			(
			int ArgA,
			int ArgB
			)
		{
		return (ArgA + ArgB) % MOD;
		}

	// subtract two values
	internal static int Subtract
			(
			int ArgA,
			int ArgB
			)
		{
		return (MOD + ArgA - ArgB) % MOD;
		}

	// negate a value
	internal static int Negate
			(
			int Arg
			)
		{
		return (MOD - Arg) % MOD;
		}

	// invert a number
	internal static int Inverse
			(
			int Arg
			)
		{
		return ExpTable[MOD - LogTable[Arg] - 1];
		}

	// multiply two numbers
	internal static int Multiply
			(
			int ArgA,
			int ArgB
			)
		{
		// result is zero
		if (ArgA == 0 || ArgB == 0) return 0;

		// multiply
		return ExpTable[(LogTable[ArgA] + LogTable[ArgB]) % (MOD - 1)];
		}

	// divide two numbers
	internal static int Divide
			(
			int ArgA,
			int ArgB
			)
		{
		// invert ArgB
		return Multiply(ArgA, Inverse(ArgB));
		}
    }
}