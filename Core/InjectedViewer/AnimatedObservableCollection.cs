using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Threading;

namespace Sleuth.InjectedViewer
{
	/// <summary>
	/// An observable collection that provides the ability to add a list of
	/// items over time, so that a large set of objects can be added to a 
	/// list control without degrading application responsiveness.
	/// </summary>
	/// <typeparam name="T">The type of object in the collection.</typeparam>
	public class AnimatedObservableCollection<T> : ObservableCollection<T>
	{
		const int MAX_ITEMS_FOR_IMMEDIATE_PROCESSING = 30;
		const int INITIAL_DELAY = 300;
		const int WORKING_INTERVAL = 5;
		const int ITEMS_PER_TICK = 20;

		public AnimatedObservableCollection()
			: base()
		{
		}

		public void AddRangeOverTime(IList<T> list)
		{
			if (Dispatcher.CurrentDispatcher == null)
				throw new InvalidOperationException("Cannot call AddRangeOverTime on a thread that does not have a Dispatcher.  This method must be invoked on the UI thread.");

			if (list == null)
				throw new ArgumentNullException("list");

			if (list.Count == 0)
				return;

			// This method works against a copy of the input list 
			// so that modifcations made to it will not interfere.
			IList<T> workingList = new List<T>(list);

			int index = 0;
			DispatcherTimer timer = null;
			Action<int> addItems = (int itemsToAdd) =>
			{
				// The timer initially has a long delay so that the UI has a chance to
				// update.  After that initial delay, make it tick faster as we add items.
				bool isFirstTick = timer != null && timer.Interval.TotalMilliseconds == INITIAL_DELAY;
				if (isFirstTick)
					timer.Interval = TimeSpan.FromMilliseconds(WORKING_INTERVAL);

				for (int i = 0; i < itemsToAdd; ++i)
				{
					if (workingList.Count <= index)
					{
						if (timer != null)
							timer.Stop();
						break;
					}

					base.Add(workingList[index]);
					++index;
				}
			};

			int itemCount = Math.Min(workingList.Count, MAX_ITEMS_FOR_IMMEDIATE_PROCESSING);
			addItems(itemCount);

			if (base.Count < workingList.Count)
			{
				timer = new DispatcherTimer(DispatcherPriority.Background);
				timer.Interval = TimeSpan.FromMilliseconds(INITIAL_DELAY);
				timer.Tick += delegate { addItems(ITEMS_PER_TICK); };
				timer.Start();
			}
		}
	}
}
