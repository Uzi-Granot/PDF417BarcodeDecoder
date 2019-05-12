/*
Camera_NET - Camera wrapper for directshow for .NET
Copyright (C) 2013
https://github.com/free5lot/Camera_Net

The code below is somewhat modified in relation to the
original code. The original code is available at:
https://www.codeproject.com/Articles/671407/Camera_Net-Library

This library is free software; you can redistribute it and/or
modify it under the terms of the GNU Lesser General Public
License as published by the Free Software Foundation; either
version 3.0 of the License, or (at your option) any later version.

This library is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
Lesser General Public License for more details.

You should have received a copy of the GNU LesserGeneral Public 
License along with this library. If not, see <http://www.gnu.org/licenses/>.
*/

// Use DirectShowLib (LGPL v2.1)
using DirectShowLib;

// Microsoft.Win32 is used for SystemEvents namespace
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Windows.Forms;

namespace Pdf417VideoDecoderDemo
{
/// <summary>
/// The Camera class is an main class that is a wrapper for video device.
/// </summary>
/// 
/// <author> free5lot (free5lot@yandex.ru) </author>
/// <version> 2013.12.16 </version>
public sealed class Camera : IDisposable
	{
	private Control _DisplayPanel;
	private readonly IMoniker _CameraMoniker;
	private readonly FrameSize _FrameSize;
	private IFilterGraph2 FilterGraph;
	private IBaseFilter VMRenderer;
	private IVMRWindowlessControl9 WindowlessCtrl;
	private IMediaControl MediaControl;
	private ISampleGrabber SampleGrabber;
	private IBaseFilter SampleGrabberFilter;
	private IBaseFilter SmartTee;
	private IBaseFilter CaptureFilter;
    private bool _bGraphIsBuilt = false;
    private bool _bHandlersAdded = false;
    private SampleGrabberHelper _pSampleGrabberHelper;

	/// <summary>
	/// Returns available frame sizes with RGB color system for device moniker
	/// </summary>
	/// <param name="moniker">Moniker (device identification) of camera.</param>
	/// <returns>List of frame sizes with RGB color system of device</returns>
	public static FrameSize[] GetFrameSizeList(IMoniker moniker)
		{
		int hr;

		FrameSize[] FrameSizeArray = null;

		// Get the graphbuilder object
		IFilterGraph2 filterGraph = new FilterGraph() as IFilterGraph2;
		IBaseFilter capFilter = null;

		try
			{
			// add the video input device
			hr = filterGraph.AddSourceFilterForMoniker(moniker, null, "Source Filter", out capFilter);
			DsError.ThrowExceptionForHR(hr);

			FrameSizeArray = GetFrameSizesAvailable(capFilter);
			}
		finally
			{
			SafeReleaseComObject(filterGraph);
			filterGraph = null;

			SafeReleaseComObject(capFilter);
			capFilter = null;
			}

		return FrameSizeArray;
		}

		/// <summary>
		/// Initializes camera and connects it to HostingControl and Moniker.
		/// </summary>
		/// <param name="DisplayPanel">Control that is used for hosting camera's output.</param>
		/// <param name="Moniker">Moniker (device identification) of camera.</param>
		/// <param name="FrameSize">Frame size</param>
		public Camera
			(
			Control DisplayPanel,
			IMoniker Moniker,
			FrameSize FrameSize
			)
		{
		if(DisplayPanel == null) throw new ApplicationException("Display panel should be set.");
		if(Moniker == null) throw new ApplicationException("Camera's moniker should be set.");
		if(FrameSize == null) throw new ApplicationException("Frame size should be set.");

		_DisplayPanel = DisplayPanel;
		_CameraMoniker = Moniker;
		_FrameSize = FrameSize;

		// Build and Run graph
		BuildGraph();
		RunGraph();
		return;
		}

	/// <summary>
	/// Close and dispose all camera and DirectX stuff.
	/// </summary>
	public void Dispose()
		{
		_bGraphIsBuilt = false;

		// stop rendering
		if(MediaControl != null)
			{
			try
				{
				MediaControl.StopWhenReady();
				MediaControl.Stop();
				}
			catch (Exception ex)
				{
				Debug.WriteLine(ex);
				}
			}

		if(_bHandlersAdded)
			{
			// remove handlers when they are no more needed
			_bHandlersAdded = false;
			_DisplayPanel.Paint -= new PaintEventHandler(HostingControl_Paint);
			_DisplayPanel.Resize -= new EventHandler(HostingControl_ResizeMove);
			_DisplayPanel.Move -= new EventHandler(HostingControl_ResizeMove);
			SystemEvents.DisplaySettingsChanged -= new EventHandler(SystemEvents_DisplaySettingsChanged);
			}

		// Dispose Managed Direct3D objects
		if(_pSampleGrabberHelper != null)
			{
			_pSampleGrabberHelper.Dispose();
			_pSampleGrabberHelper = null;
			}

		if(VMRenderer != null)
			{
			Marshal.ReleaseComObject(VMRenderer);
			VMRenderer = null;
			WindowlessCtrl = null;
			}

		if(FilterGraph != null)
			{
			Marshal.ReleaseComObject(FilterGraph);
			FilterGraph = null;
			MediaControl = null;
			}

		if(SmartTee != null)
			{
			Marshal.ReleaseComObject(SmartTee);
			SmartTee = null;
			}

		if(SampleGrabber != null)
			{
			Marshal.ReleaseComObject(SampleGrabber);
			SampleGrabber = null;
			SampleGrabberFilter = null;
			}

		if(CaptureFilter != null)
			{
			Marshal.ReleaseComObject(CaptureFilter);
			CaptureFilter = null;
			}
		return;
		}

	/// <summary>
	/// Builds DirectShow graph for rendering.
	/// </summary>
	public void BuildGraph()
		{
		try
			{
			// define filter graph
			FilterGraph = (IFilterGraph2) new FilterGraph();

			// define media control
			MediaControl = (IMediaControl) FilterGraph;

			// add filters to the filter graph
			AddFilter_Source();
			AddFilter_Renderer();
			AddFilter_TeeSplitter();
			AddFilter_SampleGrabber();

			// connect filters
			GraphBuilding_ConnectPins();

			// set the sample grabber
            _pSampleGrabberHelper.SaveMode();

			// graph was built successfuly
			_bGraphIsBuilt = true;
			}
		catch
			{
			Dispose();
			throw;
			}

		#if DEBUG
		// Double check to make sure we aren't releasing something
		// important.
		GC.Collect();
		GC.WaitForPendingFinalizers();
		#endif
		}

	/// <summary>
	/// Runs DirectShow graph for rendering.
	/// </summary>
	public void RunGraph()
		{
		if(MediaControl != null)
			{
			int hr = MediaControl.Run();
			DsError.ThrowExceptionForHR(hr);
			}
		return;
		}

	/// <summary>
	/// Runs DirectShow graph for rendering.
	/// </summary>
	public void PauseGraph()
		{
		if(MediaControl != null)
			{
			int hr = MediaControl.Pause();
			DsError.ThrowExceptionForHR(hr);
			}
		return;
		}

	/// <summary>
	/// Make snapshot of source image. Much faster than SnapshotOutputImage.
	/// </summary>
	/// <returns>Snapshot as a Bitmap</returns>
	public Bitmap SnapshotSourceImage()
		{
		return _pSampleGrabberHelper.SnapshotNextFrame();
		}

	/// <summary>
	/// Checks if AMMediaType's frame size is appropriate for desired frame size.
	/// </summary>
	/// <param name="media_type">Media type to analyze.</param>
	/// <param name="RefFrameSize">Desired frame size. Can be null or have 0 for height or width if it's not important.</param>
	private static bool IsFrameSizeAppropiate
			(
			AMMediaType media_type,
			FrameSize RefFrameSize
			)
		{
		// if we were asked to choose frame size
		if(RefFrameSize == null) return true;

		VideoInfoHeader videoInfoHeader = new VideoInfoHeader();
		Marshal.PtrToStructure(media_type.formatPtr, videoInfoHeader);

		if(RefFrameSize.Width > 0 && videoInfoHeader.BmiHeader.Width != RefFrameSize.Width) return false;
		if(RefFrameSize.Height > 0 && videoInfoHeader.BmiHeader.Height != RefFrameSize.Height) return false;
		return true;
		}

	/// <summary>
	/// Get resolution from if AMMediaType's frame size is appropriate for frame size_desired
	/// </summary>
	/// <param name="media_type">Media type to analyze.</param>
	private static FrameSize GetFrameSizeForMediaType
			(
			AMMediaType media_type
			)
		{
		VideoInfoHeader videoInfoHeader = new VideoInfoHeader();
		Marshal.PtrToStructure(media_type.formatPtr, videoInfoHeader);
		return new FrameSize(videoInfoHeader.BmiHeader.Width, videoInfoHeader.BmiHeader.Height);
		}

	/// <summary>
	/// Get bit count for mediatype
	/// </summary>
	/// <param name="media_type">Media type to analyze.</param>
	private static int GetBitCountForMediaType(AMMediaType media_type)
		{
		VideoInfoHeader videoInfoHeader = new VideoInfoHeader();
		Marshal.PtrToStructure(media_type.formatPtr, videoInfoHeader);
		return videoInfoHeader.BmiHeader.BitCount;
		}

	/// <summary>
	/// Check if bit count is appropriate for us
	/// </summary>
	private static bool IsColorBitCountOK
			(
			int bit_count
			)
		{
		return bit_count == 16 || bit_count == 24 || bit_count == 32;
		}

	/// <summary>
	/// Analyze AMMediaType during enumeration and decide if it's good choice for us.
	/// </summary>
	private static void AnalyzeMediaType
			(
			AMMediaType media_type,
			FrameSize FrameSizeDesired,
			out bool bit_count_ok,
			out bool sub_type_ok,
			out bool FrameSizeOK
			)
		{
		int bit_count = GetBitCountForMediaType(media_type);
		bit_count_ok = IsColorBitCountOK(bit_count);

		// We want (A)RGB32, RGB24 or RGB16 and YUY2.
		// These have priority
		// Change this if you're not agree.
		sub_type_ok = media_type.subType == MediaSubType.RGB32 ||
					media_type.subType == MediaSubType.ARGB32 ||
					media_type.subType == MediaSubType.RGB24 ||
					media_type.subType == MediaSubType.RGB16_D3D_DX9_RT ||
					media_type.subType == MediaSubType.RGB16_D3D_DX7_RT ||
					media_type.subType == MediaSubType.YUY2;

		// flag to show if media_type's frame size is appropriate for us
		FrameSizeOK = IsFrameSizeAppropiate(media_type, FrameSizeDesired);
		return;
		}

	/// <summary>
	/// Sets parameters for source capture pin.
	/// </summary>
	/// <param name="pinSourceCapture">Pin of source capture.</param>
	/// <param name="FrameSize">frame size to set if possible.</param>
	private static void SetSourceParams(IPin pinSourceCapture, FrameSize FrameSize)
		{
		AMMediaType media_type_most_appropriate = null;
		AMMediaType media_type = null;

		//NOTE: pSCC is not used. All we need is media_type
		IntPtr pSCC = IntPtr.Zero;

		bool appropriate_media_type_found = false;

		try
			{
			// We want the interface to expose all media types it supports and not only the last one set
			IAMStreamConfig videoStreamConfig = pinSourceCapture as IAMStreamConfig;
			int hr = videoStreamConfig.SetFormat(null);
			DsError.ThrowExceptionForHR(hr);

			hr = videoStreamConfig.GetNumberOfCapabilities(out int piCount, out int piSize);
			DsError.ThrowExceptionForHR(hr);

			for (int i = 0; i < piCount; i++)
				{
				pSCC = Marshal.AllocCoTaskMem(piSize);
				videoStreamConfig.GetStreamCaps(i, out media_type, pSCC);
				FreeSCCMemory(ref pSCC);

				AnalyzeMediaType(media_type, FrameSize, out bool bit_count_ok, out bool sub_type_ok, out bool FrameSizeOK);

				if(bit_count_ok && FrameSizeOK)
					{
					if(sub_type_ok)
						{
						hr = videoStreamConfig.SetFormat(media_type);
						DsError.ThrowExceptionForHR(hr);

						appropriate_media_type_found = true;
						break; // stop search, we've found appropriate media type
						}
					else
						{
						// save as appropriate if no other found
						if(media_type_most_appropriate == null)
							{
							media_type_most_appropriate = media_type;
							media_type = null; // we don't want for free it, now it's media_type_most_appropriate's problem
							}
						}
					}
                    
				FreeMediaType(ref media_type);
				}

			if(!appropriate_media_type_found)
				{
				// Found nothing exactly as we were asked 
				if(media_type_most_appropriate != null)
					{
					// set appropriate RGB format with different frame size
					hr = videoStreamConfig.SetFormat(media_type_most_appropriate);
					DsError.ThrowExceptionForHR(hr);
					}
				else
					{
					// throw. We didn't find exactly what we were asked to
					throw new Exception("Camera doesn't support media type with requested frame size and bits per pixel.");
					}
				}
			}
		catch
			{
			throw;
			}
		finally
			{
			// clean up
			FreeMediaType(ref media_type);
			FreeMediaType(ref media_type_most_appropriate);
			FreeSCCMemory(ref pSCC);
			}
		return;
		}

	/// <summary>
	/// Connects pins of graph
	/// </summary>
	private void GraphBuilding_ConnectPins()
		{
		// Pins used in graph
		IPin pinSourceCapture = null;
		IPin pinTeeInput = null;
		IPin pinTeePreview = null;
		IPin pinTeeCapture = null;
		IPin pinSampleGrabberInput = null;
		IPin pinRendererInput = null;

		try
			{
			// Collect pins
			pinSourceCapture = DsFindPin.ByDirection(CaptureFilter, PinDirection.Output, 0);
			pinTeeInput = DsFindPin.ByDirection(SmartTee, PinDirection.Input, 0);
			pinTeePreview = DsFindPin.ByName(SmartTee, "Preview");
			pinTeeCapture = DsFindPin.ByName(SmartTee, "Capture");

			pinSampleGrabberInput = DsFindPin.ByDirection(SampleGrabberFilter, PinDirection.Input, 0);
			pinRendererInput = DsFindPin.ByDirection(VMRenderer, PinDirection.Input, 0);

			// Connect source to tee splitter
			int hr = FilterGraph.Connect(pinSourceCapture, pinTeeInput);
			DsError.ThrowExceptionForHR(hr);

			// Connect samplegrabber on preview-pin of tee splitter
			hr = FilterGraph.Connect(pinTeePreview, pinSampleGrabberInput);
			DsError.ThrowExceptionForHR(hr);

			// Connect the capture-pin of tee splitter to the renderer
			hr = FilterGraph.Connect(pinTeeCapture, pinRendererInput);
			DsError.ThrowExceptionForHR(hr);
			}
		catch
			{
			throw;
			}
		finally
			{
			SafeReleaseComObject(pinSourceCapture);
			pinSourceCapture = null;

			SafeReleaseComObject(pinTeeInput);
			pinTeeInput = null;

			SafeReleaseComObject(pinTeePreview);
			pinTeePreview = null;

			SafeReleaseComObject(pinTeeCapture);
			pinTeeCapture = null;

			SafeReleaseComObject(pinSampleGrabberInput);
			pinSampleGrabberInput = null;

			SafeReleaseComObject(pinRendererInput);
			pinRendererInput = null;
			}
		return;
		}
        
	/// <summary>
	/// Adds video source filter to the filter graph.
	/// </summary>
	private void AddFilter_Source()
		{
		CaptureFilter = null;
		int hr = FilterGraph.AddSourceFilterForMoniker(_CameraMoniker, null, "Source Filter", out CaptureFilter);
		DsError.ThrowExceptionForHR(hr);

		// Pins used in graph
		IPin pinSourceCapture = null;
		try
			{
			pinSourceCapture = DsFindPin.ByDirection(CaptureFilter, PinDirection.Output, 0);
			SetSourceParams(pinSourceCapture, _FrameSize);
			}
		catch
			{
			throw;
			}
		finally
			{
			SafeReleaseComObject(pinSourceCapture);
			}
		return;
		}

	/// <summary>
	/// Adds VMR9 (renderer) filter to the filter graph.
	/// </summary>
	private void AddFilter_Renderer()
		{
		VMRenderer = (IBaseFilter) new VideoMixingRenderer9();

		IVMRFilterConfig9 filterConfig = (IVMRFilterConfig9) VMRenderer;

		// Not really needed for vmr but don't forget calling it with VMR7
		int hr = filterConfig.SetNumberOfStreams(1);
		DsError.ThrowExceptionForHR(hr);

		// Change vmr mode to Windowless
		hr = filterConfig.SetRenderingMode(VMR9Mode.Windowless);
		DsError.ThrowExceptionForHR(hr);

		// video renderer
		WindowlessCtrl = (IVMRWindowlessControl9) VMRenderer;

		// set clipping window
		hr = WindowlessCtrl.SetVideoClippingWindow(_DisplayPanel.Handle);
		DsError.ThrowExceptionForHR(hr);

		// Set Aspect-Ratio
		hr = WindowlessCtrl.SetAspectRatioMode(VMR9AspectRatioMode.LetterBox);
		DsError.ThrowExceptionForHR(hr);

		// Add delegates for Windowless operations
		_DisplayPanel.Paint += new PaintEventHandler(HostingControl_Paint); // for WM_PAINT
		_DisplayPanel.Resize += new EventHandler(HostingControl_ResizeMove); // for WM_SIZE
		_DisplayPanel.Move += new EventHandler(HostingControl_ResizeMove); // for WM_MOVE
		SystemEvents.DisplaySettingsChanged += new EventHandler(SystemEvents_DisplaySettingsChanged); // for WM_DISPLAYCHANGE
		_bHandlersAdded = true;

		// Call the resize handler to configure the output size
		HostingControl_ResizeMove(null, null);

		hr = FilterGraph.AddFilter(VMRenderer, "Video Mixing Renderer 9");
		DsError.ThrowExceptionForHR(hr);
		}

	/// <summary>
	/// Adds tee splitter filter to split for grabber and for capture.
	/// </summary>
	private void AddFilter_TeeSplitter()
		{
		// Add a splitter
		SmartTee = (IBaseFilter) new SmartTee();

		int hr = FilterGraph.AddFilter(SmartTee, "SmartTee");
		DsError.ThrowExceptionForHR(hr);
		return;
		}
        
	/// <summary>
	/// Adds SampleGrabber for screenshot making.
	/// </summary>
	private void AddFilter_SampleGrabber()
		{
		// Get the SampleGrabber interface
		SampleGrabber = new SampleGrabber() as ISampleGrabber;
            
		// Configure the sample grabber
		SampleGrabberFilter = SampleGrabber as IBaseFilter;
		_pSampleGrabberHelper = new SampleGrabberHelper(SampleGrabber, false);
		_pSampleGrabberHelper.ConfigureMode();

		// Add the sample grabber to the graph
		int hr = FilterGraph.AddFilter(SampleGrabberFilter, "Sample Grabber");
		DsError.ThrowExceptionForHR(hr);
		return;
		}

	/// <summary>
	/// Handler of Paint event of HostingControl.
	/// </summary>
	private void HostingControl_Paint
			(
			object sender,
			PaintEventArgs e
			)
		{
		if(_bGraphIsBuilt)
			{
			IntPtr hdc = e.Graphics.GetHdc();
			try
				{
				int hr = WindowlessCtrl.RepaintVideo(_DisplayPanel.Handle, hdc);
				}
			catch (System.Runtime.InteropServices.COMException ex)
				{
				// Catch com-expection VFW_E_BUFFER_NOTSET (0x8004020c) in RepaintVideo() and ignore it
				// it can be in the moment of moving window out of first monitor to second one.
				// NOTE: This could be probably fixed with checking if graph is running or not
				if(ex.ErrorCode != DsResults.E_BufferNotSet) throw; // re-throw exception up
				}
			finally
				{
				e.Graphics.ReleaseHdc(hdc);
				}
			}
		return;
		}

	/// <summary>
	/// Handler of Resize and Move events of HostingControl.
	/// </summary>
	private void HostingControl_ResizeMove
			(
			object sender,
			EventArgs e
			)
		{
		if(_bGraphIsBuilt) WindowlessCtrl.SetVideoPosition(null, DsRect.FromRectangle(_DisplayPanel.ClientRectangle));
		return;
		}

	/// <summary>
	/// Handler of SystemEvents.DisplaySettingsChanged.
	/// </summary>
	private void SystemEvents_DisplaySettingsChanged
			(
			object sender,
			EventArgs e
			)
		{
		if(_bGraphIsBuilt) WindowlessCtrl.DisplayModeChanged();
		return;
		}

	/// <summary>
	/// Gets available frame sizes (which are appropriate for us) for capture filter.
	/// </summary>
	/// <param name="captureFilter">Capture filter for asking for frame size list.</param>
	private static FrameSize[] GetFrameSizesAvailable(IBaseFilter captureFilter)
		{
		FrameSize[] FrameSizeArray = null;

		IPin pRaw = null;
		try
			{
			pRaw = DsFindPin.ByDirection(captureFilter, PinDirection.Output, 0);
			FrameSizeArray = GetSupportedFrameSizes(pRaw);
			}
		catch
			{
			throw;
			}
		finally
			{
			SafeReleaseComObject(pRaw);
			pRaw = null;
			}

		return FrameSizeArray;
		}

	/// <summary>
	/// Gets available supported frame sizes with 16 or 24 or 32 bits per color.
	/// </summary>
	private static FrameSize[] GetSupportedFrameSizes
			(
			IPin pinOutput
			)
		{
		List<FrameSize> FrameSizeList = new List<FrameSize>();

		// Media type (shoudl be cleaned)
		AMMediaType media_type = null;

		//NOTE: pSCC is not used. All we need is media_type
		IntPtr pSCC = IntPtr.Zero;

		try
			{
			IAMStreamConfig VideoStreamConfig = (IAMStreamConfig) pinOutput;

			// We want the interface to expose all media types it supports and not only the last one set
			int hr = VideoStreamConfig.SetFormat(null);
			DsError.ThrowExceptionForHR(hr);

			hr = VideoStreamConfig.GetNumberOfCapabilities(out int piCount, out int piSize);
			DsError.ThrowExceptionForHR(hr);

			for (int Index = 0; Index < piCount; Index++)
				{
				pSCC = Marshal.AllocCoTaskMem(piSize);
				VideoStreamConfig.GetStreamCaps(Index, out media_type, pSCC);

				if(IsColorBitCountOK(GetBitCountForMediaType(media_type)))
					{
					FrameSize FrameSize = GetFrameSizeForMediaType(media_type);
					if(!FrameSizeList.Contains(FrameSize)) FrameSizeList.Add(FrameSize);
					}

				FreeSCCMemory(ref pSCC);
				FreeMediaType(ref media_type);
				}
			}
		catch
			{
			throw;
			}
		finally
			{
			// clean up
			FreeSCCMemory(ref pSCC);
			FreeMediaType(ref media_type);
			}

		// return array
		return FrameSizeList.ToArray();
		}

	/// <summary>
	/// Free media type if needed.
	/// </summary>
	/// <param name="media_type">Media type to free.</param>
	private static void FreeMediaType
			(
			ref AMMediaType media_type
			)
		{
		if(media_type != null)
			{
			DsUtils.FreeAMMediaType(media_type);
			media_type = null;
			}
		return;
		}

	/// <summary>
	/// Free SCC (it's not used but required for GetStreamCaps()).
	/// </summary>
	/// <param name="pSCC">SCC to free.</param>
	private static void FreeSCCMemory(ref IntPtr pSCC)
		{
		if(pSCC != IntPtr.Zero)
			{
			Marshal.FreeCoTaskMem(pSCC);
			pSCC = IntPtr.Zero;
			}
		return;
		}

	/// <summary>
	/// Releases COM object
	/// </summary>
	/// <param name="obj">COM object to release.</param>
	private static void SafeReleaseComObject(object obj)
		{
		if(obj != null) Marshal.ReleaseComObject(obj);
		return;
		}
    }
}
