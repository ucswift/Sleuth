using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using Sleuth.InjectedViewer.ViewModel.DebugOutput;
using Sleuth.InjectedViewer.ViewModel.MemoryExplorer;
using Sleuth.InjectedViewer.ViewModel.Scriptorium;
using Sleuth.InjectedViewer.ViewModel.ElementTreeExplorer;

namespace Sleuth.InjectedViewer.ViewModel.Shell
{
    public class InjectedWindowViewModel : ViewModelBase
    {
        #region Fields

        ReadOnlyCollection<CommandViewModel> _commands;
        ObservableCollection<WorkspaceViewModel> _workspacesInternal;
        ReadOnlyObservableCollection<WorkspaceViewModel> _workspacesReadOnly;

        #endregion // Fields

        #region Constructor

        public InjectedWindowViewModel()
        {
            base.DisplayName = "Crack.NET [Injected Viewer]";
        }

        #endregion // Constructor

        #region Commands

        public ReadOnlyCollection<CommandViewModel> Commands
        {
            get
            {
                if (_commands == null)
                {
                    List<CommandViewModel> commands = this.CreateCommands();
                    _commands = new ReadOnlyCollection<CommandViewModel>(commands);
                }
                return _commands;
            }
        }

        List<CommandViewModel> CreateCommands()
        {
            return new List<CommandViewModel>
            {
                new CommandViewModel(
                    "Element Tree Explorer",
                    new RelayCommand(() => this.SetActiveWorkspace<ElementTreeExplorerViewModel>())),

                new CommandViewModel(
                    "Memory Explorer",
                    new RelayCommand(() => this.SetActiveWorkspace<MemoryExplorerViewModel>())),

                new CommandViewModel(
                    "Debug Output",
                    new RelayCommand(() => this.SetActiveWorkspace<DebugOutputViewModel>())),

                new CommandViewModel(
                    "Scriptorium",
                    new RelayCommand(() => this.SetActiveWorkspace<ScriptoriumViewModel>())),
            };
        }

        #endregion // Commands

        #region FindInputVariableForNewScriptorium

        internal ViewModelBase FindInputVariableForNewScriptorium()
        {
            MemoryExplorerViewModel memoryExplorer =
                this.Workspaces.FirstOrDefault(ws => ws is MemoryExplorerViewModel)
                as MemoryExplorerViewModel;

            if (memoryExplorer == null)
                return null;

            return memoryExplorer.SelectedObject;
        }

        #endregion // FindInputVariableForNewScriptorium

        #region Workspaces

        public ReadOnlyObservableCollection<WorkspaceViewModel> Workspaces
        {
            get
            {
                if (_workspacesReadOnly == null)
                {
                    _workspacesInternal = new ObservableCollection<WorkspaceViewModel>();
                    _workspacesReadOnly = new ReadOnlyObservableCollection<WorkspaceViewModel>(_workspacesInternal);

                    this.SetActiveWorkspace<MemoryExplorerViewModel>();
                    //this.SetActiveWorkspace<ElementTreeExplorerViewModel>();
                }
                return _workspacesReadOnly;
            }
        }

        #endregion // Workspaces

        #region Private Helpers

        void SetActiveWorkspace<TWorkspace>() where TWorkspace : WorkspaceViewModel, new()
        {
            TWorkspace workspace = this.FindOrCreateWorkspace<TWorkspace>();

            if (!_workspacesInternal.Contains(workspace))
                _workspacesInternal.Add(workspace);

            ICollectionView collView = CollectionViewSource.GetDefaultView(this.Workspaces);
            if (collView != null)
                collView.MoveCurrentTo(workspace);
        }

        TWorkspace FindOrCreateWorkspace<TWorkspace>() where TWorkspace : WorkspaceViewModel, new()
        {
            TWorkspace workspace = this.Workspaces.FirstOrDefault(ws => ws is TWorkspace) as TWorkspace;

            if (workspace == null || workspace.AllowMultipleInstances)
            {
                workspace = new TWorkspace();
                workspace.Initialize(this);
                workspace.RequestClose += this.OnWorkspaceRequestClose;
            }

            return workspace;
        }

        void OnWorkspaceRequestClose(object sender, EventArgs e)
        {
            WorkspaceViewModel workspace = sender as WorkspaceViewModel;
            workspace.RequestClose -= this.OnWorkspaceRequestClose;
            _workspacesInternal.Remove(workspace);
        }

        #endregion // Private Helpers
    }
}