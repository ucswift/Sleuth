using System.Windows;
using System.Windows.Controls;
using Sleuth.InjectedViewer.ViewModel.MemoryExplorer;

namespace Sleuth.InjectedViewer.View.MemoryExplorer
{
    public partial class SelectedEntityListView : UserControl
    {
        #region Constructor

        public SelectedEntityListView()
        {
            InitializeComponent();
        }

        #endregion // Constructor

        #region GroupStyle

        public GroupStyle GroupStyle
        {
            get { return (GroupStyle)GetValue(GroupStyleProperty); }
            set { SetValue(GroupStyleProperty, value); }
        }
        
        public static readonly DependencyProperty GroupStyleProperty =
            DependencyProperty.Register(
            "GroupStyle", 
            typeof(GroupStyle), 
            typeof(SelectedEntityListView), 
            new UIPropertyMetadata(null, OnGroupStyleChanged));

        static void OnGroupStyleChanged(DependencyObject depObj, DependencyPropertyChangedEventArgs e)
        {
            SelectedEntityListView view = depObj as SelectedEntityListView;
            view._listView.GroupStyle.Clear();
            view._listView.GroupStyle.Add(e.NewValue as GroupStyle);
        }

        #endregion // GroupStyle

        #region OnRefreshButtonClick

        void OnRefreshButtonClick(object sender, RoutedEventArgs e)
        {
            ICanRefresh refreshable = base.DataContext as ICanRefresh;
            if (refreshable != null)
                refreshable.RefreshValues();
        }

        #endregion // OnRefreshButtonClick
    }
}