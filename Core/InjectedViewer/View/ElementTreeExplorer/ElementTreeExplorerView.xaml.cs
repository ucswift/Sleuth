using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Sleuth.InjectedViewer.ViewModel.ElementTreeExplorer;

namespace Sleuth.InjectedViewer.View.ElementTreeExplorer
{
    public partial class ElementTreeExplorerView : UserControl
    {
        #region Fields

        public static readonly RoutedCommand RefreshCommand = new RoutedCommand("Refresh", typeof(ElementTreeExplorerView));
        private Visual _crackRootVisual = null;

        #endregion

        #region Constructors

        static ElementTreeExplorerView()
        {
            ElementTreeExplorerView.RefreshCommand.InputGestures.Add(new KeyGesture(Key.F5));
        }

        public ElementTreeExplorerView()
        {
            CommandBindings.Add(new CommandBinding(ElementTreeExplorerView.RefreshCommand, OnRefresh));
            Loaded += OnWorkspaceLoaded;
            Unloaded += OnWorkspaceUnloaded;
            InitializeComponent();
        }

        #endregion

        #region Properties

        public ElementTreeExplorerViewModel ViewModel { get; private set; }

        #endregion

        #region Methods

        #region Private

        private void OnPreProcessInput(object sender, PreProcessInputEventArgs e)
        {
            KeyboardDevice keyboard = InputManager.Current.PrimaryKeyboardDevice;
            ModifierKeys currentModifiers = keyboard.Modifiers;
            if (!((currentModifiers & ModifierKeys.Control) != 0
                && (currentModifiers & ModifierKeys.Shift) != 0))
            {
                return;
            }

            Visual directlyOver = Mouse.PrimaryDevice.DirectlyOver as Visual;
            if ((directlyOver == null) || directlyOver.IsDescendantOf(_crackRootVisual))
                return;

            ViewModel.SelectItem(directlyOver);
        }

        private void OnRefresh(object sender, ExecutedRoutedEventArgs e)
        {
            ViewModel.RefreshCommand.Execute(null);
        }

        private void OnTreeSelectionChanged(object sender, EventArgs e)
        {
            ElementTreeItem item = Tree.SelectedItem as ElementTreeItem;
            if (item != null)
                ViewModel.SelectedItem = item;
        }

        private void OnWorkspaceUnloaded(object sender, RoutedEventArgs e)
        {
            // stop monitoring input
            InputManager.Current.PreProcessInput -= OnPreProcessInput;
            _crackRootVisual = null;

            // stop monitoring selection changes in the TreeView
            Tree.SelectedItemChanged -= OnTreeSelectionChanged;

            // release the viewmodel
            ViewModel = null;
        }


        private void OnWorkspaceLoaded(object sender, RoutedEventArgs e)
        {
            // store a reference to the ViewModel for easy access
            ViewModel = DataContext as ElementTreeExplorerViewModel;

            // monitor selection changes in the TreeView
            Tree.SelectedItemChanged += OnTreeSelectionChanged;

            // monitor input to catch the Shift+Ctrl modifiers
            _crackRootVisual = PresentationSource.FromVisual(this).RootVisual;
            InputManager.Current.PreProcessInput += OnPreProcessInput;

            // if the root has not been set, locate and inspect the application's root
            if (ViewModel.Root == null)
            {
                object root = null;
                if (Application.Current != null && Application.Current.MainWindow != null)
                {
                    root = Application.Current;
                }
                else
                {
                    foreach (PresentationSource presentationSource in PresentationSource.CurrentSources)
                    {
                        if (presentationSource.RootVisual != null)
                        {
                            root = presentationSource.RootVisual;
                            break;
                        }
                    }
                }

                if (root != null)
                {
                    ViewModel.Inspect(root);
                }
            }
        }

        #endregion

        #endregion
    }
}