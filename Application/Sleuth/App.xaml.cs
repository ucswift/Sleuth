using System;
using System.Windows;

namespace Sleuth
{
	public partial class App : Application
	{
		public App()
		{
			AppDomain.CurrentDomain.UnhandledException += this.OnCurrentDomainUnhandledException;
		}

		void OnCurrentDomainUnhandledException(object sender, UnhandledExceptionEventArgs e)
		{
			string msg = e.ExceptionObject == null ? "(no error message is available)" : e.ExceptionObject.ToString();
			Clipboard.SetText(msg);

			MessageBox.Show(
					"Sleuth threw an unhandled exception.  The following exception details have been copied to the Windows clipboard.  Please report this issue here: http://www.github.com/sleuth \n\n" + msg,
					"Unexpected Error",
					MessageBoxButton.OK,
					MessageBoxImage.Error);

			System.Diagnostics.Process.GetCurrentProcess().Kill();
		}
	}
}