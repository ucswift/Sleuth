using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Media.Imaging;

namespace Sleuth.InjectedViewer.ViewModel.MemoryExplorer
{
    public class AssemblyViewModel : MemoryExplorerTreeItemViewModel
    {
        #region Fields

        static readonly BitmapSource s_assemblyIcon;

        readonly Assembly _assembly;
        readonly AssemblyBrowserViewModel _assemblyBrowser;

        List<TypeViewModel> _allTypes;
        string _filter;
        TypeViewModel _selectedType;
        ObservableCollection<TypeViewModel> _typesInternal;
        ReadOnlyObservableCollection<TypeViewModel> _typesReadOnly;

        #endregion // Fields

        #region Static Constructor

        static AssemblyViewModel()
        {
            using (Stream stream = typeof(TypeViewModel).Assembly.GetManifestResourceStream("Sleuth.InjectedViewer.View.MemoryExplorer.Resources.Images.Assembly.gif"))
                s_assemblyIcon = GifBitmapDecoder.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.Default).Frames[0];
        }

        #endregion // Static Constructor

        #region Constructor

        public AssemblyViewModel(Assembly assembly, AssemblyBrowserViewModel assemblyBrowser)
            :base(assemblyBrowser.MemoryExplorer)
        {
            if (assembly == null)
                throw new ArgumentNullException("assembly");

            _assembly = assembly;
            _assemblyBrowser = assemblyBrowser;            

            base.DisplayName = _assembly.GetName().Name;
        }

        #endregion // Constructor

        #region GetAssemblyInternal

        internal Assembly GetAssemblyInternal()
        {
            return _assembly;
        }

        #endregion // GetAssemblyInternal

        #region TreeView Members

        #region ApplyFilter

        /// <summary>
        /// Applies a filter to all types in this assembly.
        /// Types that do not pass the filter are removed from the Types collection.
        /// If none of the types pass the filter, this object's IsRelevant property returns false.
        /// </summary>
        /// <param name="filter">Some, or all, of a type name.</param>
        /// <param name="matchWholeWord">If true, only types whose name is identical to the 'filter' argument remain.</param>
        public void ApplyFilter(string filter, bool matchWholeWord)
        {
            bool hadFilter = !String.IsNullOrEmpty(_filter);

            _filter = filter;

            bool hasFilter = !String.IsNullOrEmpty(_filter);

            if (hadFilter && !hasFilter)
            {
                _typesInternal = null;
                _typesReadOnly = null;
                base.OnPropertyChanged("Types");
            }
            else if (hasFilter && _typesInternal != null)
            {
                _typesInternal.Clear();

                foreach (TypeViewModel typeVM in _allTypes)
                {
                    string typeName = typeVM.UnqualifiedName;
                    int idx = typeName.IndexOf(_filter, StringComparison.InvariantCultureIgnoreCase);

                    bool passesFilter = idx == 0;
                    if (passesFilter && matchWholeWord)
                        passesFilter = _filter.Length == typeName.Length;

                    if (passesFilter)
                        _typesInternal.Add(typeVM);
                }
            }

            this.IsExpanded = hasFilter;

            base.OnPropertyChanged("IsRelevant");
        }

        #endregion // ApplyFilter

        #region AssemblyBrowser

        public AssemblyBrowserViewModel AssemblyBrowser
        {
            get { return _assemblyBrowser; }
        }

        #endregion // AssemblyBrowser

        #region Icon

        public override BitmapSource Icon
        {
            get { return s_assemblyIcon; }
        }

        #endregion // Icon

        #region IsExpanded [override]

        public override bool IsExpanded
        {
            get { return base.IsExpanded; }
            set
            {
                if (!base.IsExpanded && value)
                    _assemblyBrowser.MemoryExplorer.IsDoingWork = true;

                base.IsExpanded = value;
            }
        }

        #endregion // IsExpanded [override]

        #region IsFiltered

        internal bool IsFiltered
        {
            get { return !String.IsNullOrEmpty(_filter); }
        }

        #endregion // IsFiltered

        #region IsRelevant [override]

        public override bool IsRelevant
        {
            // This assembly is irrelevant if none of its children pass through the filter.
            get { return !this.IsFiltered || this.Types.Count != 0; }
        }

        #endregion // IsRelevant [override]

        #region Types

        public ReadOnlyCollection<TypeViewModel> Types
        {
            get
            {
                if (_typesReadOnly == null)
                {
                    _allTypes = this.LoadTypesInAssembly();
                    _typesInternal = new ObservableCollection<TypeViewModel>(_allTypes);
                    _typesReadOnly = new ReadOnlyObservableCollection<TypeViewModel>(_typesInternal);
                }
                return _typesReadOnly;
            }
        }

        List<TypeViewModel> LoadTypesInAssembly()
        {
            List<TypeViewModel> list = new List<TypeViewModel>();
            foreach (Type type in _assembly.GetTypes())
            {
                TypeViewModel typeVM = new TypeViewModel(type, this);

                // Exclude the Private Implementation Details classes.
                if (!type.FullName.Contains("<") &&
                    !type.FullName.Contains("$"))
                {
                    typeVM.PropertyChanged += this.OnTypeViewModelPropertyChanged;
                    list.Add(typeVM);
                }
            }

            list.Sort((t1, t2) =>
                {
                    bool t1IsInternal = t1.UnqualifiedName.StartsWith("_");
                    bool t2IsInternal = t2.UnqualifiedName.StartsWith("_");

                    if (t1IsInternal && !t2IsInternal)
                        return +1;

                    if (!t1IsInternal && t2IsInternal)
                        return -1;

                    return (t1.UnqualifiedName.CompareTo(t2.UnqualifiedName));
                });

            return list;
        }

        void OnTypeViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            TypeViewModel typeVM = sender as TypeViewModel;
            if (typeVM.IsSelected)
                this.SelectedType = typeVM;
            else
                this.SelectedType = null;
        }

        #endregion // Types

        #region SelectedType

        public TypeViewModel SelectedType
        {
            get { return _selectedType; }
            set
            {
                if (_selectedType == value)
                    return;

                _selectedType = value;

                base.OnPropertyChanged("SelectedType");
            }
        }

        #endregion // SelectedType

        #endregion // TreeView Members

        #region Assembly Info Members

        public string CodeBase
        {
            get { return _assembly.CodeBase; }
        }

        public string EntryPointName
        {
            get { return _assembly.EntryPoint == null ? "n/a" : _assembly.EntryPoint.Name; }
        }

        public string FullName
        {
            get { return _assembly.FullName; }
        }

        public bool GlobalAssemblyCache
        {
            get { return _assembly.GlobalAssemblyCache; }
        }

        public int HashCode
        {
            get { return _assembly.GetHashCode(); }
        }

        public long HostContext
        {
            get { return _assembly.HostContext; }
        }

        public string ImageRuntimeVersion
        {
            get { return _assembly.ImageRuntimeVersion; }
        }

        public string Location
        {
            get { return _assembly.Location; }
        }

        public string ManifestModuleName
        {
            get { return _assembly.ManifestModule.FullyQualifiedName; }
        }

        public List<string> ReferencedAssemblyNames
        {
            get 
            {
                IEnumerable<string> names =
                    from assemblyName in _assembly.GetReferencedAssemblies()
                    orderby assemblyName.FullName
                    select assemblyName.FullName;

                return names.ToList(); 
            }
        }

        public bool ReflectionOnly
        {
            get { return _assembly.ReflectionOnly; }
        }

        #endregion // Assembly Info Members
    }
}