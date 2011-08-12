using System;
using System.ComponentModel;
using System.Windows.Threading;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows;
using Sleuth.InjectedViewer.ViewModel.ElementTreeExplorer;

namespace Sleuth.InjectedViewer.ViewModel.MemoryExplorer
{
	public class MemoryExplorerViewModel : WorkspaceViewModel
	{
		#region Fields

		readonly AssemblyBrowserViewModel _assemblyBrowser;
		readonly BreadcrumbTrailViewModel _breadcrumbTrail;
		bool _isDoingWork;
		bool _isFilterAreaExpanded = true;
		bool _isViewLoaded;
		Visual _rootCrackVisual;
		ViewModelBase _selectedObject;

		#endregion // Fields

		#region Constructor

		public MemoryExplorerViewModel()
		{
			base.DisplayName = "Memory Explorer";

			_assemblyBrowser = new AssemblyBrowserViewModel(this);
			_assemblyBrowser.PropertyChanged += this.OnAssemblyBrowserPropertyChanged;

			_breadcrumbTrail = new BreadcrumbTrailViewModel(this);
		}

		#endregion // Constructor

		#region AssemblyBrowser

		public AssemblyBrowserViewModel AssemblyBrowser
		{
			get { return _assemblyBrowser; }
		}

		#endregion // AssemblyBrowser

		#region BreadcrumbTrail

		public BreadcrumbTrailViewModel BreadcrumbTrail
		{
			get { return _breadcrumbTrail; }
		}

		#endregion // BreadcrumbTrail

		#region IsDoingWork

		public bool IsDoingWork
		{
			get { return _isDoingWork; }
			set
			{
				if (value == _isDoingWork)
					return;

				_isDoingWork = value;

				base.OnPropertyChanged("IsDoingWork");

				if (value)
				{
					Dispatcher.CurrentDispatcher.BeginInvoke(
						DispatcherPriority.ApplicationIdle,
						(Action)delegate
						{
							this.IsDoingWork = false;
						});
				}
			}
		}

		#endregion // IsDoingWork

		#region IsViewLoaded

		/// <summary>
		/// Gets/sets whether the View that displays the 'Memory Explorer' workspace is in the visual tree.
		/// This property is necessary because when the 'Memory Explorer' tab is unselected (but not closed)
		/// the WPF data binding system tries to unselect all of the items in the ViewModel hierarchy.
		/// </summary>
		internal bool IsViewLoaded
		{
			get { return _isViewLoaded; }
			set { _isViewLoaded = value; }
		}

		#endregion // IsViewLoaded

		#region SelectedObject

		/// <summary>
		/// Gets/sets the object displayed in the main content area to the right of the TreeView.
		/// </summary>
		public ViewModelBase SelectedObject
		{
			get { return _selectedObject; }
			set
			{
				if (value == _selectedObject)
					return;

				IHaveFilterSettings previousFilterableObject = _selectedObject as IHaveFilterSettings;
				if (previousFilterableObject != null)
					_isFilterAreaExpanded = previousFilterableObject.FilterSettings.IsFilterAreaExpanded;

				_selectedObject = value;

				IHaveFilterSettings currentFilterableObject = _selectedObject as IHaveFilterSettings;
				if (currentFilterableObject != null)
					currentFilterableObject.FilterSettings.IsFilterAreaExpanded = _isFilterAreaExpanded;

				base.OnPropertyChanged("SelectedObject");
			}
		}

		#endregion // SelectedObject

		#region Private Helpers

		void OnAssemblyBrowserPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			string SelectedAssemblyOrType = "SelectedAssemblyOrType";
			_assemblyBrowser.VerifyPropertyName(SelectedAssemblyOrType);

			if (e.PropertyName == SelectedAssemblyOrType && _assemblyBrowser.SelectedAssemblyOrType != null)
				this.SelectedObject = _assemblyBrowser.SelectedAssemblyOrType;
		}

		#endregion // Private Helpers

		ICommand _exploreElementTreeCommand;
		public ICommand ExploreElementTreeCommand
		{
			get
			{
				if (_exploreElementTreeCommand == null)
					_exploreElementTreeCommand = new RelayCommand(() => this.IsExploringElementTree = true);

				return _exploreElementTreeCommand;
			}
		}

		bool _isExploringElementTree;
		public bool IsExploringElementTree
		{
			get { return _isExploringElementTree; }
			set
			{
				if (value.Equals(_isExploringElementTree))
					return;

				_isExploringElementTree = value;

				base.OnPropertyChanged("IsExploringElementTree");
			}
		}

		internal void Activate()
		{
			if (Application.Current != null)
			{
				foreach (Window wnd in Application.Current.Windows)
				{
					if (wnd is Sleuth.InjectedViewer.View.Shell.InjectedWindow)
					{
						_rootCrackVisual = wnd;
						break;
					}
				}
			}
		}

		internal void Deactivate()
		{
			_rootCrackVisual = null;
		}

		ElementTreeExplorerViewModel _elementTreeExplorerWorkspace;
		public ElementTreeExplorerViewModel ElementTreeExplorerWorkspace
		{
			get
			{
				if (_elementTreeExplorerWorkspace == null)
				{
					_elementTreeExplorerWorkspace = new ElementTreeExplorerViewModel();
					_elementTreeExplorerWorkspace.RequestClose += delegate { this.IsExploringElementTree = false; };
				}
				return _elementTreeExplorerWorkspace;
			}
		}
	}
}