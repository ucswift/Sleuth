using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Media;

namespace Sleuth.InjectedViewer.ViewModel.ElementTreeExplorer
{
    public class ElementTreeItem : ViewModelBase
    {
        #region Fields

        private ObservableCollection<ElementTreeItem> _children = new ObservableCollection<ElementTreeItem>();
        private bool _isSelected = false;
        private bool _isExpanded = false;
        private string _name;
        private string _nameLower = string.Empty;
        private ElementTreeItem _parent;
        private object _target;
        private string _typeNameLower = string.Empty;
        private int _visualChildrenCount;

        #endregion

        #region Constructors

        protected ElementTreeItem(object target, ElementTreeItem parent)
        {
            _target = target;
            _parent = parent;
        }

        #endregion

        #region Properties

        public ObservableCollection<ElementTreeItem> Children
        {
            get { return _children; }
        }

        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                if (_isExpanded != value)
                {
                    _isExpanded = value;
                    OnPropertyChanged("IsExpanded");
                }
            }
        }

        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;

                    if (_isSelected && _parent != null)
                        _parent.Expand();

                    OnPropertyChanged("IsSelected");
                    OnSelectionChanged();
                }
            }
        }

        public virtual Visual MainVisual
        {
            get { return null; }
        }

        public object Target
        {
            get { return _target; }
        }

        public virtual Brush TreeBackgroundBrush
        {
            get { return new SolidColorBrush(Color.FromArgb(255, 240, 240, 240)); }
        }

        public virtual Brush VisualBrush
        {
            get { return null; }
        }

        #endregion

        #region Methods

        #region Public

        public static ElementTreeItem Construct(object target, ElementTreeItem parent)
        {
            ElementTreeItem item;
            if (target is Visual)
            {
                item = new VisualItem((Visual)target, parent);
            }
            else if (target is ResourceDictionary)
            {
                item = new ResourceDictionaryItem((ResourceDictionary)target, parent);
            }
            else if (target is Application)
            {
                item = new ApplicationTreeItem((Application)target, parent);
            }
            else
            {
                item = new ElementTreeItem(target, parent);
            }
            item.Reload();
            return item;
        }

        public bool Filter(string value)
        {
            if (_typeNameLower.Contains(value))
            {
                return true;
            }
            if (_nameLower.Contains(value))
            {
                return true;
            }
            return false;
        }

        public ElementTreeItem FindNode(object target)
        {
            if (Target == target) return this;

            foreach (ElementTreeItem child in Children)
            {
                ElementTreeItem node = child.FindNode(target);
                if (node != null) return node;
            }
            return null;
        }

        public virtual bool HasBindingError
        {
            get { return false; }
        }

        public override string ToString()
        {
            if (_visualChildrenCount != 0)
            {
                return _name + " (" + Target.GetType().Name + ") " + _visualChildrenCount;
            }
            return _name + " (" + Target.GetType().Name + ")";
        }

        public int UpdateVisualChildrenCount()
        {
            _visualChildrenCount = 0;
            foreach (ElementTreeItem child in Children)
            {
                if (child is VisualItem)
                {
                    _visualChildrenCount += child.UpdateVisualChildrenCount();
                }
            }
            if (this is VisualItem)
            {
                return _visualChildrenCount + 1;
            }
            return _visualChildrenCount;
        }

        public void Reload()
        {
            _name = (_target is FrameworkElement) ? ((FrameworkElement)_target).Name : string.Empty;
            _nameLower = _name.ToLower();
            _typeNameLower = Target.GetType().Name.ToLower();

            List<ElementTreeItem> toBeRemoved = new List<ElementTreeItem>(Children);
            Reload(toBeRemoved);

            foreach (ElementTreeItem item in toBeRemoved)
            {
                RemoveChild(item);
            }
        }

        #endregion

        #region Protected

        protected virtual void OnSelectionChanged()
        {
        }

        protected virtual void Reload(List<ElementTreeItem> toBeRemoved)
        {
        }

        protected void RemoveChild(ElementTreeItem item)
        {
            item.IsSelected = false;
            Children.Remove(item);
        }

        #endregion

        #region Private

        private void Expand()
        {
            if (_parent != null)
                _parent.Expand();
            IsExpanded = true;
        }

        #endregion
        
        #endregion
    }
}