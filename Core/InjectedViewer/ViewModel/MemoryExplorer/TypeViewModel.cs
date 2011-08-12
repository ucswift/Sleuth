using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace Sleuth.InjectedViewer.ViewModel.MemoryExplorer
{
    public class TypeViewModel
        : MemoryExplorerTreeItemViewModel,
        IBreadcrumb,
        ICanRefresh,
        IHaveFilterSettings
    {
        #region Fields

        static readonly BitmapSource s_ClassOrStructIcon;
        static readonly BitmapSource s_EnumIcon;
        static readonly BitmapSource s_InterfaceIcon;

        readonly AssemblyViewModel _containingAssembly;
        readonly MemoryExplorerListFilterSettingsViewModel _filterSettings;
        readonly Type _type;

        ICommand _openInReflectorCommand;
        List<TypeMemberViewModel> _staticPropertiesAndFieldsList;
        AnimatedObservableCollection<TypeMemberViewModel> _staticPropertiesAndFieldsInternal;
        ReadOnlyObservableCollection<TypeMemberViewModel> _staticPropertiesAndFieldsReadOnly;
        string _unqualifiedName;

        #endregion // Fields

        #region Static Constructor

        static TypeViewModel()
        {
            using (Stream stream = typeof(TypeViewModel).Assembly.GetManifestResourceStream("Sleuth.InjectedViewer.View.MemoryExplorer.Resources.Images.ClassOrStruct.gif"))
                s_ClassOrStructIcon = GifBitmapDecoder.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.Default).Frames[0];

            using (Stream stream = typeof(TypeViewModel).Assembly.GetManifestResourceStream("Sleuth.InjectedViewer.View.MemoryExplorer.Resources.Images.Enum.gif"))
                s_EnumIcon = GifBitmapDecoder.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.Default).Frames[0];

            using (Stream stream = typeof(TypeViewModel).Assembly.GetManifestResourceStream("Sleuth.InjectedViewer.View.MemoryExplorer.Resources.Images.Interface.gif"))
                s_InterfaceIcon = GifBitmapDecoder.Create(stream, BitmapCreateOptions.None, BitmapCacheOption.Default).Frames[0];
        }

        #endregion // Static Constructor

        #region Constructor

        public TypeViewModel(Type type, AssemblyViewModel containingAssembly)
            : base(containingAssembly.AssemblyBrowser.MemoryExplorer)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            _type = type;
            _containingAssembly = containingAssembly;
            _filterSettings = new MemoryExplorerListFilterSettingsViewModel(containingAssembly.AssemblyBrowser.MemoryExplorer);

            base.DisplayName = _type.FullName;
        }

        #endregion // Constructor

        #region ContainingAssembly

        public AssemblyViewModel ContainingAssembly
        {
            get { return _containingAssembly; }
        }

        #endregion // ContainingAssembly

        #region GetTypeInternal

        internal Type GetTypeInternal()
        {
            return _type;
        }

        #endregion // GetTypeInternal

        #region Icon

        public override BitmapSource Icon
        {
            get
            {
                if (_type.IsEnum)
                    return s_EnumIcon;

                if (_type.IsInterface)
                    return s_InterfaceIcon;

                return s_ClassOrStructIcon;
            }
        }

        #endregion // Icon

        #region IsSelected [override]

        public override bool IsSelected
        {
            get { return base.IsSelected; }
            set
            {
                if (value)
                {
                    this.ContainingAssembly.IsExpanded = true;

                    // Loading and displaying the static members can take a while if the
                    // system is slow, or the type has many static members.  Set this flag
                    // now so that the UI can show a wait cursor while the work happens.
                    this.ContainingAssembly.AssemblyBrowser.MemoryExplorer.IsDoingWork = true;
                }

                base.IsSelected = value;
            }
        }

        #endregion // IsSelected [override]

        #region OpenInReflectorCommand

        public ICommand OpenInReflectorCommand
        {
            get
            {
                if (_openInReflectorCommand == null)
                    _openInReflectorCommand = new RelayCommand(() => Reflector.OpenType(_type));

                return _openInReflectorCommand;
            }
        }

        #endregion // OpenInReflectorCommand

        #region PropertiesAndFields

        public ReadOnlyObservableCollection<TypeMemberViewModel> PropertiesAndFields
        {
            get
            {
                if (_staticPropertiesAndFieldsReadOnly == null)
                {
                    _staticPropertiesAndFieldsList = this.LoadPropertiesAndFields();
                    _staticPropertiesAndFieldsInternal = new AnimatedObservableCollection<TypeMemberViewModel>();
                    _staticPropertiesAndFieldsReadOnly = new ReadOnlyObservableCollection<TypeMemberViewModel>(_staticPropertiesAndFieldsInternal);
                    _staticPropertiesAndFieldsInternal.AddRangeOverTime(_staticPropertiesAndFieldsList);
                }

                return _staticPropertiesAndFieldsReadOnly;
            }
        }

        List<TypeMemberViewModel> LoadPropertiesAndFields()
        {
            List<TypeMemberViewModel> list = new List<TypeMemberViewModel>();

            BindingFlags flags = BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static;

            Type currentType = _type;
            while (currentType != null)
            {
                foreach (FieldInfo staticField in currentType.GetFields(flags))
                    list.Add(new TypeMemberViewModel(this, staticField));

                foreach (PropertyInfo staticProperty in currentType.GetProperties(flags))
                {
                    // Ignore write-only properties.
                    MethodInfo getMethod = staticProperty.GetGetMethod(true);
                    if (getMethod != null)
                        list.Add(new TypeMemberViewModel(this, staticProperty));
                }

                currentType = currentType.BaseType;
            }

            list.Sort(MemoryExplorerListItemViewModel.CompareListItems);

            return list;
        }

        #endregion // PropertiesAndFields

        #region UnqualifiedName

        public string UnqualifiedName
        {
            get
            {
                if (_unqualifiedName == null)
                {
                    if (_type.IsNested)
                    {
                        string name = _type.Name;
                        Type current = _type;
                        while (current != null && current.IsNested)
                        {
                            name = current.DeclaringType.Name + "+" + name;
                            current = current.DeclaringType;
                        }

                        _unqualifiedName = name;
                    }
                    else
                    {
                        _unqualifiedName = _type.Name;
                    }
                }
                return _unqualifiedName;
            }
        }

        #endregion // UnqualifiedName

        #region IBreadcrumb Members

        public string BreadcrumbDisplayName
        {
            get { return this.UnqualifiedName; }
        }

        public string BreadcrumbToolTipText
        {
            get { return this.DisplayName; }
        }

        public bool CanNavigateBackToObject
        {
            get { return true; }
        }

        #endregion // IBreadcrumb Members

        #region ICanRefresh Members

        public void RefreshValues()
        {
            foreach (TypeMemberViewModel typeMember in this.PropertiesAndFields)
                typeMember.RefreshValue();
        }

        #endregion // ICanRefresh Members

        #region IHaveFilterSettings Members

        public MemoryExplorerListFilterSettingsViewModel FilterSettings
        {
            get { return _filterSettings; }
        }

        #endregion // IHaveFilterSettings Members
    }
}