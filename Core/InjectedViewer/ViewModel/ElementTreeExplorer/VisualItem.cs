using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;

namespace Sleuth.InjectedViewer.ViewModel.ElementTreeExplorer
{
    public class VisualItem : ResourceContainerItem
    {
        #region Fields

        private Visual _visual;
        private AdornerContainer _adorner;

        #endregion

        #region Constructors

        public VisualItem(Visual visual, ElementTreeItem parent)
            : base(visual, parent)
        {
            _visual = visual;
        }

        #endregion

        #region Properties

        public override bool HasBindingError
        {
            get
            {
                PropertyDescriptorCollection propertyDescriptors = TypeDescriptor.GetProperties(Visual, new Attribute[] { new PropertyFilterAttribute(PropertyFilterOptions.All) });
                foreach (PropertyDescriptor property in propertyDescriptors)
                {
                    DependencyPropertyDescriptor dpd = DependencyPropertyDescriptor.FromProperty(property);
                    if (dpd != null)
                    {
                        BindingExpressionBase expression = BindingOperations.GetBindingExpressionBase(Visual, dpd.DependencyProperty);
                        if (expression != null
                            && (expression.HasError || expression.Status != BindingStatus.Active))
                        {
                            return true;
                        }
                    }
                }
                return false;
            }
        }

        public override Visual MainVisual
        {
            get { return _visual; }
        }

        protected override ResourceDictionary ResourceDictionary
        {
            get
            {
                FrameworkElement element = Visual as FrameworkElement;
                return (element != null) ? element.Resources : null;
            }
        }

        public override Brush TreeBackgroundBrush
        {
            get { return Brushes.Transparent; }
        }

        public Visual Visual
        {
            get { return _visual; }
        }

        public override Brush VisualBrush
        {
            get
            {
                VisualBrush brush = new VisualBrush(Visual);
                brush.Stretch = Stretch.Uniform;
                return brush;
            }
        }
        
        #endregion

        #region Methods

        #region Protected

        protected override void OnSelectionChanged()
        {
            // Add adorners for the visual this is representing.
            AdornerLayer adorners = AdornerLayer.GetAdornerLayer(_visual);
            UIElement visualElement = _visual as UIElement;
            if (adorners != null && visualElement != null)
            {
                if (IsSelected && _adorner == null)
                {
                    Border border = new Border();
                    border.BorderThickness = new Thickness(4);

                    Color borderColor = new Color();
                    borderColor.ScA = .3f;
                    borderColor.ScR = 1;
                    border.BorderBrush = new SolidColorBrush(borderColor);
                    border.IsHitTestVisible = false;
                    
                    _adorner = new AdornerContainer(visualElement);
                    _adorner.Child = border;
                    adorners.Add(_adorner);
                }
                else if (_adorner != null)
                {
                    adorners.Remove(_adorner);
                    _adorner.Child = null;
                    _adorner = null;
                }
            }
        }

        protected override void Reload(List<ElementTreeItem> toBeRemoved)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(Visual); i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(Visual, i);
                if (child != null)
                {
                    bool foundItem = false;
                    foreach (ElementTreeItem item in toBeRemoved)
                    {
                        if (item.Target == child)
                        {
                            toBeRemoved.Remove(item);
                            item.Reload();
                            foundItem = true;
                            break;
                        }
                    }
                    if (!foundItem)
                    {
                        Children.Add(ElementTreeItem.Construct(child, this));
                    }
                }
            }

            Grid grid = Visual as Grid;
            if (grid != null)
            {
                foreach (RowDefinition row in grid.RowDefinitions)
                {
                    Children.Add(ElementTreeItem.Construct(row, this));
                }
                foreach (ColumnDefinition column in grid.ColumnDefinitions)
                {
                    Children.Add(ElementTreeItem.Construct(column, this));
                }
            }

            base.Reload(toBeRemoved);
        }

        #endregion

        #endregion
    }
}
