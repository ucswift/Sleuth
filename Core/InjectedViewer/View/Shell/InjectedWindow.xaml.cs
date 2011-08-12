using System.Windows;

namespace Sleuth.InjectedViewer.View.Shell
{
    public partial class InjectedWindow : Window
    {
        public InjectedWindow()
        {
            // This propery setting allows the window to not inherit typed
            // Styles and DataTemplates contained in the Application.Resources
            // collection of a WPF application into which it is injected.
            base.InheritanceBehavior = InheritanceBehavior.SkipToThemeNext;

            InitializeComponent();

            // After setting InheritanceBehavior to 'SkipToThemeNext' we must
            // explicitly load the control to show in the Window, otherwise
            // the window remains empty.
            this.Loaded += delegate { base.Content = new InjectedWindowView(); };            
        }
    }
}