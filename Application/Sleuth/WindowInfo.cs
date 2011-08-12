// (c) Copyright Cory Plotts.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Input;
using Sleuth.InjectedViewer;

namespace Sleuth
{
	public class WindowInfo
	{
		private IntPtr hwnd;
		private MainWindow mainWindow;

		private static Dictionary<int, bool> processIDToValidityMap = new Dictionary<int, bool>();

		public WindowInfo(IntPtr hwnd, MainWindow mainWindow)
		{
			this.hwnd = hwnd;
			this.mainWindow = mainWindow;
		}

		public IEnumerable<NativeMethods.MODULEENTRY32> Modules
		{
			get
			{
				if (_modules == null)
					_modules = GetModules().ToArray();
				return _modules;
			}
		}
		/// <summary>
		/// Similar to System.Diagnostics.WinProcessManager.GetModuleInfos,
		/// except that we include 32 bit modules when Snoop runs in 64 bit mode.
		/// See http://blogs.msdn.com/b/jasonz/archive/2007/05/11/code-sample-is-your-process-using-the-silverlight-clr.aspx
		/// </summary>
		private IEnumerable<NativeMethods.MODULEENTRY32> GetModules()
		{
			int processId;
			NativeMethods.GetWindowThreadProcessId(hwnd, out processId);

			var me32 = new NativeMethods.MODULEENTRY32();
			var hModuleSnap = NativeMethods.CreateToolhelp32Snapshot(NativeMethods.SnapshotFlags.Module | NativeMethods.SnapshotFlags.Module32, processId);
			if (!hModuleSnap.IsInvalid)
			{
				using (hModuleSnap)
				{
					me32.dwSize = (uint)System.Runtime.InteropServices.Marshal.SizeOf(me32);
					if (NativeMethods.Module32First(hModuleSnap, ref me32))
					{
						do
						{
							yield return me32;
						} while (NativeMethods.Module32Next(hModuleSnap, ref me32));
					}
				}
			}
		}
		private IEnumerable<NativeMethods.MODULEENTRY32> _modules;

		public bool IsValidProcess
		{
			get
			{
				bool isValid = false;
				try
				{
					if (this.hwnd == IntPtr.Zero)
						return false;

					Process process = this.OwningProcess;
					if (process == null)
						return false;

					// see if we have cached the process validity previously, if so, return it.
					if (WindowInfo.processIDToValidityMap.TryGetValue(process.Id, out isValid))
						return isValid;

					// else determine the process validity and cache it.
					if (process.Id == Process.GetCurrentProcess().Id)
					{
						isValid = false;

						// the above line stops the user from snooping on snoop, since we assume that ... that isn't their goal.
						// to get around this, the user can bring up two snoops and use the second snoop ... to snoop the first snoop.
						// well, that let's you snoop the app chooser. in order to snoop the main snoop ui, you have to bring up three snoops.
						// in this case, bring up two snoops, as before, and then bring up the third snoop, using it to snoop the first snoop.
						// since the second snoop inserted itself into the first snoop's process, you can now spy the main snoop ui from the
						// second snoop (bring up another main snoop ui to do so). pretty tricky, huh! and useful!
					}
					else
					{
						// a process is valid to snoop if it contains a dependency on PresentationFramework, PresentationCore, or milcore (wpfgfx).
						// this includes the files:
						// PresentationFramework.dll, PresentationFramework.ni.dll
						// PresentationCore.dll, PresentationCore.ni.dll
						// wpfgfx_v0300.dll (WPF 3.0/3.5)
						// wpfgrx_v0400.dll (WPF 4.0)

						// note: sometimes PresentationFramework.dll doesn't show up in the list of modules.
						// so, it makes sense to also check for the unmanaged milcore component (wpfgfx_vxxxx.dll).
						// see for more info: http://snoopwpf.codeplex.com/Thread/View.aspx?ThreadId=236335

						// sometimes the module names aren't always the same case. compare case insensitive.
						// see for more info: http://snoopwpf.codeplex.com/workitem/6090

						foreach (var module in Modules)
						{
							if (module.szModule.Contains("mscorlib"))
							{
								isValid = true;
								break;
							}
							//if
							//(
							//  module.szModule.StartsWith("PresentationFramework", StringComparison.OrdinalIgnoreCase) ||
							//  module.szModule.StartsWith("PresentationCore", StringComparison.OrdinalIgnoreCase) ||
							//  module.szModule.StartsWith("wpfgfx", StringComparison.OrdinalIgnoreCase)
							//)
							//{
							//  isValid = true;
							//  break;
							//}
						}
					}

					WindowInfo.processIDToValidityMap[process.Id] = isValid;
				}
				catch (Exception) { }
				return isValid;
			}
		}
		public Process OwningProcess
		{
			get { return NativeMethods.GetWindowThreadProcess(this.hwnd); }
		}
		public IntPtr HWnd
		{
			get { return this.hwnd; }
		}
		public string Description
		{
			get
			{
				Process process = this.OwningProcess;
				return process.MainWindowTitle + " - " + process.ProcessName + " [" + process.Id.ToString() + "]";
			}
		}
		public override string ToString()
		{
			return this.Description;
		}


		public static void ClearCachedProcessInfo()
		{
			WindowInfo.processIDToValidityMap.Clear();
		}
		public void Snoop()
		{
			Mouse.OverrideCursor = Cursors.Wait;
			try
			{
				Injector.Launch(this.HWnd, typeof(EntryPoint).Assembly, typeof(EntryPoint).FullName, "InjectedMain");
			}
			catch (Exception)
			{
				if (this.mainWindow != null)
					this.mainWindow.Refresh();
			}
			Mouse.OverrideCursor = null;
		}
	}
}