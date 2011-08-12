using System.Windows;
using System.Windows.Controls;

namespace Sleuth.InjectedViewer.View.MemoryExplorer
{
    public partial class AssemblyBrowserView : UserControl
    {
        public AssemblyBrowserView()
        {
            InitializeComponent();
        }

        void OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            // This hack prevents the TreeView from entering into an infinite layout loop
            // after the tree is filtered and the user selects an item far down in the list.
            // The bug this works around only exists when the TreeView uses UI virtualization.
            (sender as TreeView).UpdateLayout();
        }
    }
}