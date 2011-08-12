using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Markup;

namespace Sleuth.InjectedViewer.Filtering
{
    [ContentProperty("Filters")]
    public class PropertyFilterGroup
    {
        ICollectionView _dataSourceView;
        ObservableCollection<PropertyFilter> _filters;

        public PropertyFilterGroup()
        {
        }

        public ICollectionView DataSourceView
        {
            get { return _dataSourceView; }
            set 
            {
                _dataSourceView = value;

                if (_dataSourceView == null)
                    return;

                _dataSourceView.Filter = dataItem =>
                {
                    foreach (PropertyFilter filter in _filters)
                        if (!filter.IsFilteredIn(dataItem))
                            return false;

                    return true;
                };                
            }
        }

        public ObservableCollection<PropertyFilter> Filters
        {
            get { return _filters ?? (this.Filters = new ObservableCollection<PropertyFilter>()); }
            set
            {
                if (value == _filters)
                    return;

                if (_filters != null)
                    _filters.CollectionChanged -= this.OnFiltersCollectionChanged;

                _filters = value;

                if (_filters != null)
                {
                    _filters.CollectionChanged += this.OnFiltersCollectionChanged;

                    foreach (PropertyFilter filter in _filters)
                        filter.Group = this;
                }
            }
        }

        void OnFiltersCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null && e.NewItems.Count != 0)
            {
                foreach (PropertyFilter filter in e.NewItems)
                    filter.Group = this;
            }

            this.ApplyFilters();
        }

        internal void ApplyFilters()
        {
            if (_dataSourceView == null)
                return;

            _dataSourceView.Refresh();

            this.PrintItemsToConsole();
        }

        [Conditional("DEBUG")]
        void PrintItemsToConsole()
        {
            Console.WriteLine();
            Console.WriteLine("ITEMS IN COLLECTION VIEW:");
            foreach (object obj in _dataSourceView)
                Console.WriteLine("Item: " + obj);
        }
    }
}