using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;

namespace Sleuth.InjectedViewer.ViewModel.MemoryExplorer
{
    public class BreadcrumbTrailViewModel : ViewModelBase
    {
        #region Fields

        readonly ObservableCollection<IBreadcrumb> _breadcrumbsInternal;
        readonly ReadOnlyObservableCollection<IBreadcrumb> _breadcrumbsReadOnly;
        readonly MemoryExplorerViewModel _memoryExplorer;

        // This can reference an IBreadcrumb, or an AssemblyViewModel.
        ViewModelBase _previousSelectedObject;

        #endregion Fields

        #region Constructor

        public BreadcrumbTrailViewModel(MemoryExplorerViewModel memoryExplorer)
        {
            if (memoryExplorer == null)
                throw new ArgumentNullException("memoryExplorer");

            _memoryExplorer = memoryExplorer;
            _memoryExplorer.PropertyChanged += this.OnMemoryExplorerPropertyChanged;
            _previousSelectedObject = _memoryExplorer.SelectedObject;

            _breadcrumbsInternal = new ObservableCollection<IBreadcrumb>();
            _breadcrumbsInternal.Add(new CurrentBreadcrumbViewModel(_memoryExplorer));
            _breadcrumbsReadOnly = new ReadOnlyObservableCollection<IBreadcrumb>(_breadcrumbsInternal);
        }

        #endregion // Constructor

        #region Breadcrumbs

        public ReadOnlyObservableCollection<IBreadcrumb> Breadcrumbs
        {
            get { return _breadcrumbsReadOnly; }
        }

        #endregion // Breadcrumbs

		#region ClearTrail

		public void ClearTrail()
		{
			// Remove all but the "current" item at the end.
			for (int i = _breadcrumbsInternal.Count - 2; i > -1; --i)
				_breadcrumbsInternal.RemoveAt(i);
		}

		#endregion // ClearTrail

		#region MoveBackTo

		public void MoveBackTo(IBreadcrumb breadcrumb)
        {
            if (breadcrumb == null)
                throw new ArgumentNullException("breadcrumb");

            if (!_breadcrumbsInternal.Contains(breadcrumb))
                throw new ArgumentException("'breadcrumb' is not in the breadcrumb trail.");

            for (int i = _breadcrumbsInternal.Count - 2; i > -1; --i)
            {
                bool finished = _breadcrumbsInternal[i] == breadcrumb;
                _breadcrumbsInternal.RemoveAt(i);
                if (finished)
                    break;
            }

            _previousSelectedObject = breadcrumb as ViewModelBase;

            _memoryExplorer.PropertyChanged -= this.OnMemoryExplorerPropertyChanged;
            _memoryExplorer.SelectedObject = breadcrumb as ViewModelBase;
            _memoryExplorer.PropertyChanged += this.OnMemoryExplorerPropertyChanged;
        }

        #endregion // MoveBackTo      

        #region Private Helpers

        #region AddItemToTrail

        void AddItemToTrail(IBreadcrumb breadcrumb)
        {
            int idx = _breadcrumbsInternal.Count - 1;

            if (idx < 0)
            {
                Debug.Fail("Insertion index should never be less than zero.");
                idx = 0;
            }

            _breadcrumbsInternal.Insert(idx, breadcrumb);
        }

        #endregion // AddItemToTrail

        #region CanAddPreviouslySelectedItemToTrail

        bool CanAddPreviouslySelectedItemToTrail
        {
            get
            {
                ViewModelBase selectedObject = _memoryExplorer.SelectedObject;
                return
                    _previousSelectedObject is IBreadcrumb &&
                    !(_previousSelectedObject is ObjectViewModel && selectedObject is ObjectViewModel == false) &&
                    (_previousSelectedObject is TypeViewModel == false || selectedObject is ObjectViewModel);
            }
        }

        #endregion // CanAddPreviouslySelectedItemToTrail

        #region OnMemoryExplorerPropertyChanged

        void OnMemoryExplorerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            string SelectedObject = "SelectedObject";
            _memoryExplorer.VerifyPropertyName(SelectedObject);

            if (e.PropertyName == SelectedObject)
            {
                if (this.CanAddPreviouslySelectedItemToTrail)
                {
                    this.AddItemToTrail(_previousSelectedObject as IBreadcrumb);
                }
                else if (1 < this.Breadcrumbs.Count)
                {
                    this.ClearTrail();
                }

                _previousSelectedObject = _memoryExplorer.SelectedObject;
            }
        }

        #endregion // OnMemoryExplorerPropertyChanged

        #endregion // Private Helpers
    }
}