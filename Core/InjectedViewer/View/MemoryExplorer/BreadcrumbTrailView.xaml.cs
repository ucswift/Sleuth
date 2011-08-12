using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Sleuth.InjectedViewer.ViewModel.MemoryExplorer;

namespace Sleuth.InjectedViewer.View.MemoryExplorer
{
    public partial class BreadcrumbTrailView : UserControl
    {
        public BreadcrumbTrailView()
        {
            InitializeComponent();
        }

        void OnHyperlinkClick(object sender, RoutedEventArgs e)
        {
            Hyperlink link = sender as Hyperlink;
            if (link == null)
                return;

            IBreadcrumb breadcrumb = link.DataContext as IBreadcrumb;
            if (breadcrumb == null)
                return;

            BreadcrumbTrailViewModel breadcrumbTrail = base.DataContext as BreadcrumbTrailViewModel;
            if (breadcrumbTrail == null)
                return;

            breadcrumbTrail.MoveBackTo(breadcrumb);
        }
    }
}