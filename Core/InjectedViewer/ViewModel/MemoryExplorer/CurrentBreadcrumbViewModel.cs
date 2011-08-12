using System;
using System.ComponentModel;

namespace Sleuth.InjectedViewer.ViewModel.MemoryExplorer
{
    /// <summary>
    /// Represents the last item in the breadcrumb trail.  It displays 
    /// information for the currently selected object in Memory Explorer.
    /// </summary>
    public class CurrentBreadcrumbViewModel : ViewModelBase, IBreadcrumb
    {
        #region Fields

        readonly MemoryExplorerViewModel _memoryExplorer;

        #endregion // Fields

        #region Constructor

        public CurrentBreadcrumbViewModel(MemoryExplorerViewModel memoryExplorer)
        {
            if (memoryExplorer == null)
                throw new ArgumentNullException("memoryExplorer");

            _memoryExplorer = memoryExplorer;
            _memoryExplorer.PropertyChanged += this.OnMemoryExplorerPropertyChanged;
        }

        #endregion // Constructor

        #region OnMemoryExplorerPropertyChanged

        void OnMemoryExplorerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            string SelectedObject = "SelectedObject";
            _memoryExplorer.VerifyPropertyName(SelectedObject);

            if (e.PropertyName == SelectedObject)
            {
                this.OnPropertyChanged("BreadcrumbDisplayName");
                this.OnPropertyChanged("BreadcrumbToolTipText");
            }
        }

        #endregion // OnMemoryExplorerPropertyChanged

        #region IBreadcrumb Members

        public string BreadcrumbDisplayName
        {
            get
            {
                IBreadcrumb breadcrumb = _memoryExplorer.SelectedObject as IBreadcrumb;
                return breadcrumb == null ? String.Empty : breadcrumb.BreadcrumbDisplayName;
            }
        }

        public string BreadcrumbToolTipText
        {
            get
            {
                IBreadcrumb breadcrumb = _memoryExplorer.SelectedObject as IBreadcrumb;
                return breadcrumb == null ? String.Empty : breadcrumb.BreadcrumbToolTipText;
            }
        }

        public bool CanNavigateBackToObject
        {
            get { return false; }
        }

        #endregion // IBreadcrumb Members
    }
}