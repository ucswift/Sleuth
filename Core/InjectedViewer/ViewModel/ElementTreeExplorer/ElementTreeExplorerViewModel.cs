using System.Collections.ObjectModel;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Sleuth.InjectedViewer.ViewModel.ElementTreeExplorer
{
    public class ElementTreeExplorerViewModel : WorkspaceViewModel
    {
        #region Fields

        ICommand _refreshCommand;

        private ElementTreeItem _root;
        private object _rootObject;

        private string _filter = string.Empty;
        private DelayedCall _filterCall;
        private ObservableCollection<ElementTreeItem> _filtered = new ObservableCollection<ElementTreeItem>();

        private ElementTreeItem _selectedItem = null;

        #endregion

        #region Constructors

        public ElementTreeExplorerViewModel()
        {
            base.DisplayName = "Element Tree Explorer";
            _filterCall = new DelayedCall(ProcessFilter, DispatcherPriority.Background);
        }

        #endregion

        #region Commands

        public ICommand RefreshCommand
        {
            get
            {
                if (_refreshCommand == null)
                {
                    _refreshCommand = new RelayCommand(() => Refresh());
                }
                return _refreshCommand;
            }
        }

        #endregion

        #region Properties

        public string Filter
        {
            get { return _filter; }
            set
            {
                _filter = value;
                _filterCall.Enqueue();
                OnPropertyChanged("Filter");
            }
        }

        public ObservableCollection<ElementTreeItem> Filtered
        {
            get { return _filtered; }
        }

        public ElementTreeItem Root
        {
            get { return _root; }
        }

        public ElementTreeItem SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (_selectedItem != value)
                {
                    if (_selectedItem != null)
                        _selectedItem.IsSelected = false;

                    _selectedItem = value;

                    if (_selectedItem != null)
                        _selectedItem.IsSelected = true;

                    OnPropertyChanged("SelectedItem");
                }
            }
        }

        #endregion

        #region Methods

        #region Public

        public void Inspect(object target)
        {
            _rootObject = target;
            Load(target);
            SelectedItem = _root;
            OnPropertyChanged("Root");
        }

        #endregion

        #region Protected

        protected override void OnRequestClose()
        {
            // cleanup
            _root = null;
            _rootObject = null;
            _filtered.Clear();
            _selectedItem = null;
            base.OnRequestClose();
        }

        #endregion

        #region Internal

        internal void SelectItem(object target)
        {
            ElementTreeItem node = _root.FindNode(target);
            Visual rootVisual = _root.MainVisual;
            if (node == null)
            {
                Visual visual = target as Visual;
                if (visual != null && rootVisual != null)
                {
                    // ensure that the visual is contained within the subtree of the _root element
                    if (!visual.IsDescendantOf(rootVisual))
                    {
                        _root = new VisualItem(PresentationSource.FromVisual(visual).RootVisual, null);
                    }
                }

                _root.Reload();
                _root.UpdateVisualChildrenCount();
                node = _root.FindNode(target);

                Filter = _filter;
            }
            if (node != null)
            {
                SelectedItem = node;
            }
        }

        #endregion

        #region Private

        private void FilterBindings(ElementTreeItem node)
        {
            foreach (ElementTreeItem child in node.Children)
            {
                if (child.HasBindingError)
                {
                    _filtered.Add(child);
                }
                else
                {
                    FilterBindings(child);
                }
            }
        }

        private void FilterTree(ElementTreeItem node, string filter)
        {
            foreach (ElementTreeItem child in node.Children)
            {
                if (child.Filter(filter))
                {
                    _filtered.Add(child);
                }
                else
                {
                    FilterTree(child, filter);
                }
            }
        }

        private void Load(object rootTarget)
        {
            _filtered.Clear();

            _root = ElementTreeItem.Construct(rootTarget, null);
            _root.Reload();
            _root.UpdateVisualChildrenCount();
            
            Filter = _filter;
        }

        private void ProcessFilter()
        {
            _filtered.Clear();
            if (_filter == "Clear Filter")
            {
                Application.Current.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, (ThreadStart)delegate()
                {
                    Filter = string.Empty;
                });
                return;
            }
            else if (_filter == "Visuals with binding errors")
            {
                FilterBindings(_root);
            }
            else if (_filter.Length == 0)
            {
                _filtered.Add(_root);
            }
            else
            {
                FilterTree(_root, _filter.ToLower());
            }
        }

        private void Refresh()
        {
            object currentTarget = (SelectedItem != null) ? SelectedItem.Target : null;

            _filtered.Clear();

            _root = ElementTreeItem.Construct(_rootObject, null);
            _root.Reload();
            _root.UpdateVisualChildrenCount();

            if (currentTarget != null)
            {
                SelectItem(currentTarget);
            }

            Filter = _filter;
        }

        #endregion

        #endregion
    }
}
