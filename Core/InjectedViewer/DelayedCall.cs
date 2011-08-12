using System.Windows;
using System.Windows.Threading;

namespace Sleuth.InjectedViewer
{
	public delegate void DelayedHandler();

	public class DelayedCall
	{
		#region Fields

		private DelayedHandler _handler;
		private DispatcherPriority _priority;
		private bool _queued;

		#endregion

		#region Constructors

		public DelayedCall(DelayedHandler handler, DispatcherPriority priority)
		{
			_handler = handler;
			_priority = priority;
		}

		#endregion

		#region Methods

		#region Public

		public void Enqueue()
		{
			if (!_queued)
			{
				_queued = true;

				Dispatcher dispatcher;
				if (System.Windows.Application.Current == null)
					dispatcher = Dispatcher.CurrentDispatcher;
				else
					dispatcher = System.Windows.Application.Current.Dispatcher;

				dispatcher.BeginInvoke(_priority, new DispatcherOperationCallback(Process), null);
			}
		}

		#endregion

		#region Private

		private object Process(object arg)
		{
			_queued = false;
			_handler();
			return null;
		}

		#endregion

		#endregion
	}
}
