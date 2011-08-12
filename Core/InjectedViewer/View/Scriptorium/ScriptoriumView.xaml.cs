using System.Windows.Controls;
using System.Windows.Input;
using Sleuth.InjectedViewer.ViewModel.Scriptorium;

namespace Sleuth.InjectedViewer.View.Scriptorium
{
    public partial class ScriptoriumView : UserControl
    {
        public ScriptoriumView()
        {
            InitializeComponent();

            this.scriptEditor.PreviewKeyDown += this.OnScriptEditorPreviewKeyDown;
            this.executionResultsEditor.TextChanged += this.OnExecutionResultsEditorTextChanged;
        }

        void OnExecutionResultsEditorTextChanged(object sender, TextChangedEventArgs e)
        {
            if(!this.executionResultsEditor.IsKeyboardFocused)
                this.executionResultsEditor.ScrollToEnd(); 
        }

        void OnScriptEditorPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F5)
            {
                ScriptoriumViewModel viewModel = base.DataContext as ScriptoriumViewModel;
                if (viewModel == null)
                    return;

                if ((e.KeyboardDevice.Modifiers & ModifierKeys.Control) != 0)
                {
                    if (viewModel.ClearExecutionResultsCommand.CanExecute(null))
                        viewModel.ClearExecutionResultsCommand.Execute(null);
                }
                                
                if (viewModel.ExecuteScriptCommand.CanExecute(null))
                    viewModel.ExecuteScriptCommand.Execute(null);
            }
        }
    }
}