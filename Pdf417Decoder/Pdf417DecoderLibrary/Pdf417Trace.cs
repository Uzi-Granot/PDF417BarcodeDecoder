/////////////////////////////////////////////////////////////////////
//
//	PDF417 Barcode Decoder
//
//	Pdf417Trace class
//	FOR DEBUGGING ONLY
//	Trace key steps, errors and internal values
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
using System.IO;

namespace Pdf417DecoderLibrary
{
#if DEBUG
/// <summary>
/// Trace file
/// </summary>
static public class Pdf417Trace
	{
	private static string TraceFileName;		// trace file name
	private static readonly int MaxAllowedFileSize = 1024*1024;

	/// <summary>
	/// Open trace file
	/// </summary>
	/// <param name="FileName">File name</param>
	public static void Open
			(
			string	FileName
			)
		{
		// save full file name
		TraceFileName = Path.GetFullPath(FileName);
		Write("----");
		return;
		}

	/// <summary>
	/// Write formatted information to trace file
	/// </summary>
	/// <param name="Message">Format string</param>
	/// <param name="ArgArray">Arguments</param>
	public static void Format
			(
			string			Message,
			params Object[] ArgArray
			)
		{
		if(ArgArray.Length == 0) Write(Message);
		else Write(string.Format(Message, ArgArray));
		return;
		}

	/// <summary>
	/// write message to trace file
	/// </summary>
	/// <param name="Message">Message string</param>
	public static void Write
			(
			string Message
			)
		{
		// test file length
		TestSize();

		// open existing or create new trace file
		StreamWriter TraceFile = new StreamWriter(TraceFileName, true);

		// write date and time
		TraceFile.Write(string.Format("{0:yyyy}/{0:MM}/{0:dd} {0:HH}:{0:mm}:{0:ss} ", DateTime.Now));

		// write message
		TraceFile.WriteLine(Message);

		// close the file
		TraceFile.Close();

		// exit
		return;
		}

	internal static void TraceImage
			(
			Pdf417Decoder Pdf417Decoder,
			int CenterX,
			int CenterY,
			int Left,
			int Top,
			int Right,
			int Bottom
			)
		{
		int Width = Left + Right + 1;
		int Height = Top + Bottom + 1;

		int OrgX = CenterX - Left;
		int OrgY = CenterY - Top;

		char[] Line = new char[Width + 1];

		for(int Y = 0; Y < Height; Y++)
			{
			Line[0] = Y == Top ? '>' : ' ';
			int R = OrgY + Y;
			for(int X = 0; X < Width; X++)
				{
				int C = OrgX + X;
				if(R < 0 || R >= Pdf417Decoder.ImageHeight || C < 0 || C >= Pdf417Decoder.ImageWidth) Line[X + 1] = ' ';
				else Line[X + 1] = Pdf417Decoder.ImageMatrix[R, C] ? 'X' : '.';
				}
			Pdf417Trace.Write(new string(Line));
			}
		return;
		}

	/////////////////////////////////////////////////////////////////////
	// Test file size
	// If file is too big, remove first quarter of the file
	/////////////////////////////////////////////////////////////////////

	private static void TestSize()
		{
		// get trace file info
		FileInfo TraceFileInfo = new FileInfo(TraceFileName);

		// if file does not exist or file length less than max allowed file size do nothing
		if(TraceFileInfo.Exists == false || TraceFileInfo.Length <= MaxAllowedFileSize) return;

		// create file info class
		FileStream TraceFile = new FileStream(TraceFileName, FileMode.Open, FileAccess.ReadWrite, FileShare.None);

		// seek to 25% length
		TraceFile.Seek(TraceFile.Length / 4, SeekOrigin.Begin);

		// new file length
		int NewFileLength = (int) (TraceFile.Length - TraceFile.Position);

		// new file buffer
		byte[] Buffer = new byte[NewFileLength];

		// read file to the end
		TraceFile.Read(Buffer, 0, NewFileLength);

		// search for first end of line
		int StartPtr = 0;
		while(StartPtr < 1024 && Buffer[StartPtr++] != '\n');
		if(StartPtr == 1024) StartPtr = 0;

		// seek to start of file
		TraceFile.Seek(0, SeekOrigin.Begin);

		// write 75% top part of file over the start of the file
		TraceFile.Write(Buffer, StartPtr, NewFileLength - StartPtr);

		// truncate the file
		TraceFile.SetLength(TraceFile.Position);

		// close the file
		TraceFile.Close();

		// exit
		return;
		}
	}
#endif
}