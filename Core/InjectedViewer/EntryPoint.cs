using System;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;
using System.Windows.Threading;
using Sleuth.InjectedViewer.View.Shell;
using Sleuth.InjectedViewer.ViewModel.Shell;

namespace Sleuth.InjectedViewer
{
	public class EntryPoint
	{
		/// <summary>
		/// This is the method called by the Injector class after
		/// it loads this DLL into another process's memory space.
		/// </summary>
		public static void InjectedMain()
		{

			Dispatcher dispatcher;
			if (System.Windows.Application.Current == null)
				dispatcher = Dispatcher.CurrentDispatcher;
			else
				dispatcher = System.Windows.Application.Current.Dispatcher;

			if (dispatcher.CheckAccess())
			{
				InjectedWindow injectedWindow = new InjectedWindow
				{
				  DataContext = new InjectedWindowViewModel(),
				  Title = "Sleuth Viewer"
				};

				if (IsWpfApplication)
					OpenInWpf(injectedWindow);
				else
					OpenInWindowsForms(injectedWindow);
			}
			else
			{
				dispatcher.Invoke((Action)InjectedMain);
			}


			//InjectedWindow injectedWindow = new InjectedWindow
			//{
			//  DataContext = new InjectedWindowViewModel(),
			//  Title = "Sleuth Viewer"
			//};

			//try
			//{
			//  Process process = Process.GetCurrentProcess();
			//  injectedWindow.Title += " :: " + process.ProcessName;
			//}
			//catch { }

			//try
			//{
			//  if (IsWpfApplication)
			//  {
			//    OpenInWpf(injectedWindow);
			//  }
			//  else if (IsWindowFormsApplication)
			//  {
			//    OpenInWindowsForms(injectedWindow);
			//  }
			//}
			//catch (Exception ex)
			//{
			//  Debug.Fail(ex.ToString());
			//}
		}

		#region WPF

		static bool IsWpfApplication
		{
			get { return System.Windows.Application.Current != null; }
		}

		static void OpenInWpf(InjectedWindow injectedWindow)
		{
			injectedWindow.Title += " (WPF)";

			System.Windows.Window mainWindow = System.Windows.Application.Current.MainWindow;
			if (mainWindow != null && mainWindow.IsLoaded)
				injectedWindow.Owner = System.Windows.Application.Current.MainWindow;

			injectedWindow.Show();
		}

		#endregion // WPF

		#region Windows Forms

		static bool IsWindowFormsApplication
		{
			get { return System.Windows.Forms.Application.MessageLoop; }
		}

		static void OpenInWindowsForms(InjectedWindow injectedWindow)
		{
			// If we're not running in a WPF app, we need to host the UI in a WinForms Form.
			// If we do not, and try instead to run a WPF Application object, the keyboard 
			// navigation system does not work properly in the WinForms app.

			System.Windows.Forms.Form form = new System.Windows.Forms.Form();

			form.Font = new System.Drawing.Font("Consolas", form.Font.Size);
			form.Height = (int)injectedWindow.Height;
			form.MinimumSize = new System.Drawing.Size((int)injectedWindow.MinWidth, (int)injectedWindow.MinHeight);
			form.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			form.Text = injectedWindow.Title + " (Windows Forms)";
			form.TopMost = injectedWindow.Topmost;
			form.Width = (int)injectedWindow.Width;

			using (Stream stream = typeof(InjectedWindow).Assembly.GetManifestResourceStream("Sleuth.InjectedViewer.WinForms.ico"))
				form.Icon = new System.Drawing.Icon(stream);

			InjectedWindowView child = new InjectedWindowView();
			child.DataContext = injectedWindow.DataContext;
			injectedWindow.DataContext = null;
			System.Windows.Forms.Integration.ElementHost host = new System.Windows.Forms.Integration.ElementHost();
			host.Child = child;
			form.Controls.Add(host);
			host.Dock = System.Windows.Forms.DockStyle.Fill;

			// This addresses a weird issue regarding resizing of ElementHost
			// that leaves a large empty black area on the Form.  Without this
			// workaround, decreasing the width of the rightmost column in the
			// ListView causes the Form to display strangely.
			form.ResizeEnd += delegate
			{
				host.Width = form.Width;
			};

			form.Show(System.Windows.Forms.Form.ActiveForm);
		}

		#endregion // Windows Forms
	}
}