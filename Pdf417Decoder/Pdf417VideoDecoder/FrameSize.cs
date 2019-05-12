/////////////////////////////////////////////////////////////////////
//
//	PDF417 Barcode Decoder
//
//	FrameSize class
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

namespace Pdf417VideoDecoderDemo
{
/// <summary>
/// Frame Size
/// </summary>
public class FrameSize
    {
    /// <summary>
    /// Width of frame of video output.
    /// </summary>
    public int Width { set; get; }

    /// <summary>
    /// Height of frame of video output.
    /// </summary>
    public int Height { set; get; }

    /// <summary>
    /// Constructor for <see cref="FrameSize"/> class.
    /// </summary>
    /// <param name="width">Width of frame of video output.</param>
    /// <param name="height">Height of frame of video output.</param>
    public FrameSize
			(
			int width,
			int height
			)
		{
		Width = width;
		Height = height;
		return;
		}
    }
}
