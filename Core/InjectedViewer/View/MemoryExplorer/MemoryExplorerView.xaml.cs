using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Sleuth.InjectedViewer.ViewModel.MemoryExplorer;

namespace Sleuth.InjectedViewer.View.MemoryExplorer
{
	public partial class MemoryExplorerView : UserControl
	{
		public MemoryExplorerView()
		{
			InitializeComponent();

			base.AddHandler(Hyperlink.ClickEvent, (RoutedEventHandler)OnHyperlinkClick);

			this.Loaded += delegate { this.ActivateViewModel(); };
			this.Unloaded += delegate { this.DeactivateViewModel(); };
		}

		void ActivateViewModel()
		{
			MemoryExplorerViewModel vm = base.DataContext as MemoryExplorerViewModel;
			if (vm != null)
				vm.Activate();
		}

		void DeactivateViewModel()
		{
			MemoryExplorerViewModel vm = base.DataContext as MemoryExplorerViewModel;
			if (vm != null)
				vm.Deactivate();
		}

		void OnHyperlinkClick(object sender, RoutedEventArgs e)
		{
			Hyperlink link = e.OriginalSource as Hyperlink;
			if (link == null || link == this.exploreElementTreeLink)
				return;

			Duration duration = new Duration(TimeSpan.FromMilliseconds(400));

			DoubleAnimation animScaleX = new DoubleAnimation
			{
				DecelerationRatio = 0.95,
				Duration = duration,
				From = 0.3,
				To = 1
			};

			DoubleAnimation animScaleY = new DoubleAnimation
			{
				DecelerationRatio = 1,
				Duration = duration,
				From = 0.3,
				To = 1
			};

			DoubleAnimation animOpacity = new DoubleAnimation
			{
				AccelerationRatio = 1,
				Duration = duration,
				From = 0.3,
				To = 1
			};

			ScaleTransform scaleTransform = this.contentControl.RenderTransform as ScaleTransform;
			Point pos = link.IsMouseOver ? Mouse.GetPosition(this.contentControl) : new Point(0, 0);
			scaleTransform.CenterX = pos.X;
			scaleTransform.CenterY = pos.Y;

			this.contentControl.BeginAnimation(ContentControl.OpacityProperty, animOpacity);
			scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, animScaleX);
			scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, animScaleY);
		}

		protected override void OnVisualParentChanged(DependencyObject oldParent)
		{
			// If this control is being removed from the element tree, notify 
			// the ViewModel now, before we lose a reference to the DataContext.
			this.NotifyViewModelOfNewVisualParent();

			base.OnVisualParentChanged(oldParent);

			// If this control is being added to the element tree, notify 
			// the ViewModel now, after we get a refernce to the DataContext.
			this.NotifyViewModelOfNewVisualParent();
		}

		void NotifyViewModelOfNewVisualParent()
		{
			MemoryExplorerViewModel viewModel = base.DataContext as MemoryExplorerViewModel;
			if (viewModel != null)
				viewModel.IsViewLoaded = (VisualTreeHelper.GetParent(this) != null);
		}

		void Grid_PreviewKeyDown(object sender, KeyEventArgs e)
		{
			MemoryExplorerViewModel viewModel = base.DataContext as MemoryExplorerViewModel;
			if (viewModel != null)
				e.Handled = viewModel.IsExploringElementTree;
		}
	}
}