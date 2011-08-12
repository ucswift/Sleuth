using System;
using System.Diagnostics;
using System.Reflection;

namespace Sleuth.InjectedViewer.ViewModel.MemoryExplorer
{
    /// <summary>
    /// Represents a static property or field of a type
    /// that can be shown in a list control.
    /// </summary>
    public class TypeMemberViewModel : MemoryExplorerListItemViewModel
    {
        #region Fields

        readonly TypeViewModel _type;
        object _value;

        #endregion // Fields

        #region Constructors

        public TypeMemberViewModel(TypeViewModel type, FieldInfo field)
            : this(type)
        {
            if (field == null)
                throw new ArgumentNullException("field");

            _member = _field = field;
            _property = null;

            base.DisplayName = _field.Name;
        }

        public TypeMemberViewModel(TypeViewModel type, PropertyInfo property)
            : this(type)
        {
            if (property == null)
                throw new ArgumentNullException("property");

            _member = _property = property;
            _field = null;

            base.DisplayName = _property.Name;
        }

        private TypeMemberViewModel(TypeViewModel type)
            : base(type.ContainingAssembly.AssemblyBrowser.MemoryExplorer)
        {
            if (type == null)
                throw new ArgumentNullException("type");

            _type = type;
        }

        #endregion // Constructors

        #region RefreshValue

        public void RefreshValue()
        {
            _value = null;
            base.OnPropertyChanged("Value");
        }

        #endregion // RefreshValue

        #region Value [override]

        protected override object Value
        {
            get
            {
                if (_value == null)
                {
                    try
                    {
                        if (base.IsField)
                            _value = _field.GetValue(null);
                        else
                            _value = _property.GetValue(null, null);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("[TypeMemberViewModel] Exception thrown while retrieving value: " + ex);
                    }
                }
                return _value;
            }
        }

        #endregion // Value [override]
    }
}