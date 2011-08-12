using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Sleuth.InjectedViewer.Filtering
{
    public partial class PropertyFilterGroupView : UserControl
    {
        #region Constructor

        public PropertyFilterGroupView()
        {
            InitializeComponent();
        }

        #endregion // Constructor

        #region Dependency Properties

        public ICollectionView CollectionView
        {
            get { return (ICollectionView)GetValue(CollectionViewProperty); }
            set { SetValue(CollectionViewProperty, value); }
        }

        public static readonly DependencyProperty CollectionViewProperty =
            DependencyProperty.Register(
            "CollectionView",
            typeof(ICollectionView),
            typeof(PropertyFilterGroupView),
            new UIPropertyMetadata(null, ApplyCollectionViewToGroup));

        public PropertyFilterGroup FilterGroup
        {
            get { return (PropertyFilterGroup)GetValue(FilterGroupProperty); }
            set { SetValue(FilterGroupProperty, value); }
        }

        public static readonly DependencyProperty FilterGroupProperty =
            DependencyProperty.Register(
            "FilterGroup",
            typeof(PropertyFilterGroup),
            typeof(PropertyFilterGroupView),
            new UIPropertyMetadata(null, ApplyCollectionViewToGroup));

        static void ApplyCollectionViewToGroup(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
        {
            PropertyFilterGroupView view = depObj as PropertyFilterGroupView;
            if (view == null || view.FilterGroup == null || view.CollectionView == null)
                return;

            view.FilterGroup.DataSourceView = view.CollectionView;
        }

        #endregion // Dependency Properties

        #region textBox_PreviewKeyDown

        void textBox_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                e.Handled = true;

                TextBox txt = sender as TextBox;

                PropertyFilter filter = txt.DataContext as PropertyFilter;
                if (filter == null)
                    return;

                if (String.IsNullOrEmpty(filter.Value))
                {
                    if (filter.ClearCommand.CanExecute(null))
                        filter.ClearCommand.Execute(null);
                }
                else
                {
                    if (filter.ApplyCommand.CanExecute(null))
                        filter.ApplyCommand.Execute(null);
                }
            }
        }

        #endregion // textBox_PreviewKeyDown
    }
}