using System.Diagnostics;
using System.Runtime.InteropServices;

namespace SongProcessor.UI;

public static class TaskbarProgress
{
	private static readonly Lazy<ITaskbarList3> _TaskbarInstance
		= new(() => (ITaskbarList3)new TaskbarInstance());
	private static TaskbarStates _TaskbarState;

	/// <summary>
	/// Updates the Window's taskbar icon with the percentage complete.
	/// </summary>
	/// <param name="percentage">Ranges from 0.00 to 1.00</param>
	public static void UpdateTaskbarProgress(double? percentage)
	{
		if (!OperatingSystem.IsWindowsVersionAtLeast(6, 1))
		{
			return;
		}

		if (percentage is null)
		{
			var hwnd = Process.GetCurrentProcess().MainWindowHandle;
			SetState(hwnd, TaskbarStates.NoProgress);
			return;
		}

		const ulong MAX = 100U;

		var cast = (ulong)(percentage.Value * MAX);
		if (cast > MAX)
		{
			throw new ArgumentOutOfRangeException(nameof(percentage), "Must be between 0.00 and 1.00 or null.");
		}

		var handle = Process.GetCurrentProcess().MainWindowHandle;
		SetState(handle, TaskbarStates.Normal);
		SetValue(handle, cast, MAX);
	}

	private static void SetState(IntPtr hwnd, TaskbarStates taskbarState)
	{
		if (_TaskbarState != taskbarState)
		{
			_TaskbarState = taskbarState;
			_TaskbarInstance.Value.SetProgressState(hwnd, taskbarState);
		}
	}

	private static void SetValue(IntPtr hwnd, ulong progressValue, ulong progressMax)
		=> _TaskbarInstance.Value.SetProgressValue(hwnd, progressValue, progressMax);

	private enum TaskbarStates
	{
		NoProgress = 0,
		Indeterminate = 0x1,
		Normal = 0x2,
		Error = 0x4,
		Paused = 0x8
	}

	[ComImport]
	[Guid("ea1afb91-9e28-4b86-90e9-9e9f8a5eefaf")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	private interface ITaskbarList3
	{
		// ITaskbarList
		[PreserveSig]
		void HrInit();

		[PreserveSig]
		void AddTab(IntPtr hwnd);

		[PreserveSig]
		void DeleteTab(IntPtr hwnd);

		[PreserveSig]
		void ActivateTab(IntPtr hwnd);

		[PreserveSig]
		void SetActiveAlt(IntPtr hwnd);

		// ITaskbarList2
		[PreserveSig]
		void MarkFullscreenWindow(IntPtr hwnd, [MarshalAs(UnmanagedType.Bool)] bool fFullscreen);

		// ITaskbarList3
		[PreserveSig]
		void SetProgressValue(IntPtr hwnd, ulong ullCompleted, ulong ullTotal);

		[PreserveSig]
		void SetProgressState(IntPtr hwnd, TaskbarStates state);
	}

	[ComImport]
	[Guid("56fdf344-fd6d-11d0-958a-006097c9a090")]
	[ClassInterface(ClassInterfaceType.None)]
	private class TaskbarInstance;
}