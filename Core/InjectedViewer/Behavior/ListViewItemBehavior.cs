using System.Windows;
using System.Windows.Controls;

namespace Sleuth.InjectedViewer.Behavior
{
    /// <summary>
    /// Exposes attached behaviors that can be
    /// applied to ListViewItem objects.
    /// </summary>
    public static class ListViewItemBehavior
    {
        #region BringIntoViewUponLoadIfSelected

        public static bool GetBringIntoViewUponLoadIfSelected(ListViewItem listViewItem)
        {
            return (bool)listViewItem.GetValue(BringIntoViewUponLoadIfSelectedProperty);
        }

        public static void SetBringIntoViewUponLoadIfSelected(
          ListViewItem listViewItem, bool value)
        {
            listViewItem.SetValue(BringIntoViewUponLoadIfSelectedProperty, value);
        }

        public static readonly DependencyProperty BringIntoViewUponLoadIfSelectedProperty =
            DependencyProperty.RegisterAttached(
            "BringIntoViewUponLoadIfSelected",
            typeof(bool),
            typeof(ListViewItemBehavior),
            new UIPropertyMetadata(false, OnBringIntoViewUponLoadIfSelectedChanged));

        static void OnBringIntoViewUponLoadIfSelectedChanged(
          DependencyObject depObj, DependencyPropertyChangedEventArgs e)
        {
            ListViewItem item = depObj as ListViewItem;
            if (item == null)
                return;

            if (e.NewValue is bool == false)
                return;

            bool bringIntoView = (bool)e.NewValue && item.IsSelected;

            if (bringIntoView)
                item.BringIntoView();
        }

        #endregion // IsBroughtIntoViewWhenSelected
    }
}