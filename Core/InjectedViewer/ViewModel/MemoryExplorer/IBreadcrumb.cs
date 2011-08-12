namespace Sleuth.InjectedViewer.ViewModel.MemoryExplorer
{
    public interface IBreadcrumb
    {
        string BreadcrumbDisplayName { get; }
        string BreadcrumbToolTipText { get; }
        bool CanNavigateBackToObject { get; }
    }
}