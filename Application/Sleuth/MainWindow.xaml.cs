using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using System.Runtime.InteropServices;
using System.Windows.Interop;

namespace Sleuth
{
	// *******************************************************************************************
	// NOTE: A lot of the code in this file came from Pete Blois's excellent utility called Snoop.
	// You can download Snoop, the runtime WPF application viewer, here: http://blois.us/Snoop/
	// I am using Mr. Blois's code with his permission.
	// - Josh Smith 10/2008
	// *******************************************************************************************

	public partial class MainWindow : Window
	{
		static MainWindow()
		{
			MainWindow.RefreshCommand.InputGestures.Add(new KeyGesture(Key.F5));
		}

		public MainWindow()
		{
			this.windowsView = CollectionViewSource.GetDefaultView(this.windows);

			this.InitializeComponent();

			this.CommandBindings.Add(new CommandBinding(MainWindow.RefreshCommand, this.HandleRefreshCommand));
			this.CommandBindings.Add(new CommandBinding(MainWindow.InspectCommand, this.HandleInspectCommand, this.HandleCanInspectOrMagnifyCommand));
			this.CommandBindings.Add(new CommandBinding(MainWindow.MinimizeCommand, this.HandleMinimizeCommand));
			this.CommandBindings.Add(new CommandBinding(ApplicationCommands.Close, this.HandleCloseCommand));

			AutoRefresh = false;
			DispatcherTimer timer =
				new DispatcherTimer
				(
					TimeSpan.FromSeconds(20),
					DispatcherPriority.Background,
					this.HandleRefreshTimer,
					Dispatcher.CurrentDispatcher
				);
			this.Refresh();
		}


		public static readonly RoutedCommand InspectCommand = new RoutedCommand();
		public static readonly RoutedCommand RefreshCommand = new RoutedCommand();
		public static readonly RoutedCommand MagnifyCommand = new RoutedCommand();
		public static readonly RoutedCommand MinimizeCommand = new RoutedCommand();


		public ICollectionView Windows
		{
			get { return this.windowsView; }
		}
		private ICollectionView windowsView;
		private ObservableCollection<WindowInfo> windows = new ObservableCollection<WindowInfo>();

		public bool AutoRefresh { get; set; }

		public void Refresh()
		{
			this.windows.Clear();

			Dispatcher.BeginInvoke
			(
				System.Windows.Threading.DispatcherPriority.Loaded,
				(DispatcherOperationCallback)delegate
				{
					try
					{
						Mouse.OverrideCursor = Cursors.Wait;

						foreach (IntPtr windowHandle in NativeMethods.ToplevelWindows)
						{
							WindowInfo window = new WindowInfo(windowHandle, this);
							if (window.IsValidProcess && !this.HasProcess(window.OwningProcess))	// Access Viotation Here
							{
								try
								{
									this.windows.Add(window);
								}
								catch (Exception) { }
							}
						}

						if (this.windows.Count > 0)
						{
							try
							{
								this.windowsView.MoveCurrentTo(this.windows[0]);
							}
							catch (Exception) { }
						}
					}
					finally
					{
						Mouse.OverrideCursor = null;
					}
					return null;
				},
				null
			);
		}


		protected override void OnSourceInitialized(EventArgs e)
		{
			base.OnSourceInitialized(e);

			try
			{
				// load the window placement details from the user settings.
				WINDOWPLACEMENT wp = (WINDOWPLACEMENT)Properties.Settings.Default.MainWindowPlacement;
				wp.length = Marshal.SizeOf(typeof(WINDOWPLACEMENT));
				wp.flags = 0;
				wp.showCmd = (wp.showCmd == Win32.SW_SHOWMINIMIZED ? Win32.SW_SHOWNORMAL : wp.showCmd);
				IntPtr hwnd = new WindowInteropHelper(this).Handle;
				Win32.SetWindowPlacement(hwnd, ref wp);
			}
			catch
			{
			}
		}
		protected override void OnClosing(CancelEventArgs e)
		{
			base.OnClosing(e);

			// persist the window placement details to the user settings.
			WINDOWPLACEMENT wp = new WINDOWPLACEMENT();
			IntPtr hwnd = new WindowInteropHelper(this).Handle;
			Win32.GetWindowPlacement(hwnd, out wp);
			Properties.Settings.Default.MainWindowPlacement = wp;
			Properties.Settings.Default.Save();
		}


		private bool HasProcess(Process process)
		{
			try
			{
				foreach (WindowInfo window in this.windows)
					if (window.OwningProcess.Id == process.Id)
						return true;
			}
			catch (Exception){ }
	
			return false;
		}

		private void HandleCanInspectOrMagnifyCommand(object sender, CanExecuteRoutedEventArgs e)
		{
			if (this.windowsView.CurrentItem != null)
				e.CanExecute = true;
			e.Handled = true;
		}
		private void HandleInspectCommand(object sender, ExecutedRoutedEventArgs e)
		{
			WindowInfo window = (WindowInfo)this.windowsView.CurrentItem;
			if (window != null)
				window.Snoop();
		}
		private void HandleRefreshCommand(object sender, ExecutedRoutedEventArgs e)
		{
			// clear out cached process info to make the force refresh do the process check over again.
			WindowInfo.ClearCachedProcessInfo();
			this.Refresh();
		}
		private void HandleMinimizeCommand(object sender, ExecutedRoutedEventArgs e)
		{
			this.WindowState = System.Windows.WindowState.Minimized;
		}
		private void HandleCloseCommand(object sender, ExecutedRoutedEventArgs e)
		{
			this.Close();
		}

		private void HandleRefreshTimer(object sender, EventArgs e)
		{
			if (AutoRefresh)
			{
				this.Refresh();
			}
		}
		private void HandleMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			this.DragMove();
		}
	}
}