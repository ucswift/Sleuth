using System;
using System.Windows.Media.Imaging;

namespace Sleuth.InjectedViewer.ViewModel.MemoryExplorer
{
    /// <summary>
    /// Base class for items that appear in the TreeView in the 'Memory Explorer' workspace.
    /// </summary>
    public abstract class MemoryExplorerTreeItemViewModel : ViewModelBase
    {
        #region Fields

        bool _isExpanded;
        bool _isSelected;

        readonly MemoryExplorerViewModel _memoryExplorer;

        #endregion // Fields

        #region Constructor

        public MemoryExplorerTreeItemViewModel(MemoryExplorerViewModel memoryExplorer)
        {
            if (memoryExplorer == null)
                throw new ArgumentNullException("memoryExplorer");

            _memoryExplorer = memoryExplorer;
        }

        #endregion // Constructor

        #region Icon

        /// <summary>
        /// Returns an image to display in the TreeViewItem.
        /// </summary>
        public abstract BitmapSource Icon { get; }

        #endregion // Icon

        #region IsExpanded

        /// <summary>
        /// Gets/sets whether this object is in the 'expanded' state in the UI.
        /// </summary>
        public virtual bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                if (!_memoryExplorer.IsViewLoaded)
                    return;

                if (value == _isExpanded)
                    return;

                _isExpanded = value;

                base.OnPropertyChanged("IsExpanded");
            }
        }

        #endregion // IsExpanded
        
        #region IsRelevant

        /// <summary>
        /// Returns whether this object is considered 'relevant' in the UI.
        /// Child classes can override this property to return false.
        /// The default value is true.
        /// </summary>
        public virtual bool IsRelevant
        {
            get { return true; }
        }

        #endregion // IsRelevant

        #region IsSelected

        /// <summary>
        /// Gets/sets whether this object is in the 'selected' state in the UI.
        /// </summary>
        public virtual bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (!_memoryExplorer.IsViewLoaded)
                    return;

                if (value == _isSelected)
                    return;

                _isSelected = value;

                base.OnPropertyChanged("IsSelected");
            }
        }

        #endregion // IsSelected
    }
}