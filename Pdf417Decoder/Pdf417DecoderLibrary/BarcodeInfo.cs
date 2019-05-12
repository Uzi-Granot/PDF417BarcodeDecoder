/////////////////////////////////////////////////////////////////////
//
//	PDF417 Barcode Decoder
//
//	BarcodeInfo class
//	This class contains the result of decoding.
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
/// <summary>
/// Barcode results extra information
/// </summary>
public class BarcodeInfo
	{
	/// <summary>
	/// Barcode binary (byte array) data
	/// </summary>
	public byte[] BarcodeData { get; internal set; }

	/// <summary>
	/// Global Label Identifier character set (ISO-8859-n)
	/// The n represent part number 1 to 9, 13 and 15
	/// </summary>
	public string CharacterSet { get; internal set; }

	/// <summary>
	/// Global Label Identifier character set number
	/// This number is two more than the part number
	/// </summary>
	public int GliCharacterSetNo { get; internal set; }

	/// <summary>
	/// Global label identifier general purpose number
	/// code word 926 value 900 to 810899
	/// </summary>
	public int GliGeneralPurpose { get; internal set; }

	/// <summary>
	/// Global label identifier user defined number
	/// code word 925 value 810,900 to 811,799
	/// </summary>
	public int GliUserDefined { get; internal set; }

	/// <summary>
	/// Data columns
	/// </summary>
	public int DataColumns { get; internal set; }

	/// <summary>
	/// data rows
	/// </summary>
	public int DataRows { get; internal set; }

	/// <summary>
	/// Error correction length
	/// </summary>
	public int ErrorCorrectionLength { get; internal set; }

	/// <summary>
	/// Error correction count
	/// </summary>
	public int ErrorCorrectionCount { get; internal set;}
	}
}
