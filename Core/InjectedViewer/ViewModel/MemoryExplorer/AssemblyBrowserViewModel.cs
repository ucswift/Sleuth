using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Windows.Input;

namespace Sleuth.InjectedViewer.ViewModel.MemoryExplorer
{
    public class AssemblyBrowserViewModel : ViewModelBase
    {       
        #region Fields

        readonly MemoryExplorerViewModel _memoryExplorer;

        ICommand _applyFilterCommand;
        ReadOnlyCollection<AssemblyViewModel> _assemblies;
        ICommand _clearFilterCommand;
        string _filterText;
        bool _matchWholeWord;
        ViewModelBase _selectedAssemblyOrType;

        #endregion // Fields

        #region Constructor

        public AssemblyBrowserViewModel(MemoryExplorerViewModel memoryExplorer)
        {
            if (memoryExplorer == null)
                throw new ArgumentNullException("memoryExplorer");

            _memoryExplorer = memoryExplorer;

            base.DisplayName = "Assembly Browser"; 
        }

        #endregion // Constructor

        #region ApplyFilterCommand

        public ICommand ApplyFilterCommand
        {
            get
            {
                if (_applyFilterCommand == null)
                    _applyFilterCommand = new RelayCommand(
                        () => this.ApplyFilter(),
                        () => this.CanApplyFilter);

                return _applyFilterCommand;
            }
        }

        void ApplyFilter()
        {
            _memoryExplorer.IsDoingWork = true;

            foreach (AssemblyViewModel assemblyVM in this.Assemblies)
                assemblyVM.ApplyFilter(_filterText, _matchWholeWord);
        }

        bool CanApplyFilter
        {
            get { return !String.IsNullOrEmpty(this.FilterText); }
        }

        #endregion // ApplyFilterCommand

        #region Assemblies

        public ReadOnlyCollection<AssemblyViewModel> Assemblies
        {
            get
            {
                if (_assemblies == null)
                {
                    List<AssemblyViewModel> list = this.LoadAssemblies();
                    _assemblies = new ReadOnlyCollection<AssemblyViewModel>(list);
                }
                return _assemblies;
            }
        }

        List<AssemblyViewModel> LoadAssemblies()
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            List<AssemblyViewModel> list = new List<AssemblyViewModel>();
            foreach (Assembly assembly in assemblies)
            {
                AssemblyViewModel assemblyVM = new AssemblyViewModel(assembly, this);

                if (!assemblyVM.DisplayName.StartsWith("Crack.NET.InjectedViewer") &&
                    !assemblyVM.DisplayName.StartsWith("ManagedInjector"))
                {
                    assemblyVM.PropertyChanged += this.OnAssemblyViewModelPropertyChanged;
                    list.Add(assemblyVM);
                }
            }

            list.Sort((a1, a2) => a1.DisplayName.CompareTo(a2.DisplayName));

            if (list.Count != 0)
                list[0].IsSelected = true;

            return list;
        }

        void OnAssemblyViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            AssemblyViewModel assemblyVM = sender as AssemblyViewModel;

            string IsSelected = "IsSelected";
            string SelectedType = "SelectedType";
            assemblyVM.VerifyPropertyName(IsSelected);
            assemblyVM.VerifyPropertyName(SelectedType);

            if (e.PropertyName == IsSelected)
            {
                if (assemblyVM.IsSelected)
                    this.SelectedAssemblyOrType = assemblyVM;
            }
            else if (e.PropertyName == SelectedType)
            {
                if (assemblyVM.SelectedType != null)
                    this.SelectedAssemblyOrType = assemblyVM.SelectedType;
            }
        }

        #endregion // Assemblies

        #region ClearFilterCommand

        public ICommand ClearFilterCommand
        {
            get
            {
                if (_clearFilterCommand == null)
                    _clearFilterCommand = new RelayCommand(
                        () => this.ClearFilter(),
                        () => this.CanClearFilter);

                return _clearFilterCommand;
            }
        }

        void ClearFilter()
        {
            this.FilterText = null;

            this.SelectedAssemblyOrType = null;

            _assemblies = null;

            base.OnPropertyChanged("Assemblies");
        }

        bool CanClearFilter
        {
            get { return 0 < this.Assemblies.Count && this.Assemblies[0].IsFiltered; }
        }

        #endregion // ClearFilterCommand

        #region ExactMatch

        /// <summary>
        /// Gets/sets whether a type name must exactly match the filter text 
        /// for it to be filtered in.  This value does not affect the filter's
        /// case sensitivity.  The default value is false.
        /// </summary>
        public bool ExactMatch
        {
            get { return _matchWholeWord; }
            set
            {
                if (value == _matchWholeWord)
                    return;

                _matchWholeWord = value;

                base.OnPropertyChanged("ExactMatch");

                if (this.CanApplyFilter)
                    this.ApplyFilter();
            }
        }

        #endregion // ExactMatch

        #region FilterText

        public string FilterText
        {
            get { return _filterText; }
            set
            {
                if (value == _filterText)
                    return;

                _filterText = value;

                base.OnPropertyChanged("FilterText");
            }
        }

        #endregion // FilterText

        #region MemoryExplorer

        public MemoryExplorerViewModel MemoryExplorer
        {
            get { return _memoryExplorer; }
        }

        #endregion // MemoryExplorer

        #region SelectedAssemblyOrType

        public ViewModelBase SelectedAssemblyOrType
        {
            get { return _selectedAssemblyOrType; }
            set
            {
                if (value != null)
                    Debug.Assert(value is AssemblyViewModel || value is TypeViewModel, "Invalid assignment.");

                if (value == _selectedAssemblyOrType)
                    return;

                _selectedAssemblyOrType = value;

                base.OnPropertyChanged("SelectedAssemblyOrType");
            }
        }

        #endregion // SelectedAssemblyOrType
    }
}