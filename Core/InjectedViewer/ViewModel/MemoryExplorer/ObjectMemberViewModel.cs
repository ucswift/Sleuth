using System;
using System.Diagnostics;
using System.Reflection;

namespace Sleuth.InjectedViewer.ViewModel.MemoryExplorer
{
    public class ObjectMemberViewModel : MemoryExplorerListItemViewModel
    {
        #region Fields

        readonly int _collectionIndex;
        readonly ObjectViewModel _instance;
        readonly bool _isCollectionItem;
        object _value;

        #endregion // Fields

        #region Constructors

        public ObjectMemberViewModel(ObjectViewModel instance, FieldInfo field)
            : this(instance)
        {
            if (field == null)
                throw new ArgumentNullException("field");

            _member = _field = field;
            _property = null;
            _isCollectionItem = false;

            base.DisplayName = _field.Name;
        }

        public ObjectMemberViewModel(ObjectViewModel instance, PropertyInfo property)
            : this(instance)
        {
            if (property == null)
                throw new ArgumentNullException("property");

            _member = _property = property;
            _field = null;
            _isCollectionItem = false;

            base.DisplayName = _property.Name;
        }

        public ObjectMemberViewModel(ObjectViewModel instance, object collectionItem, int collectionIndex)
            : this(instance)
        {
            _isCollectionItem = true;
            _collectionIndex = collectionIndex;
            _value = collectionItem;

            string indexString = collectionIndex.ToString();
            if (collectionIndex < 10)
            {
                indexString = "00" + indexString;
            }
            else if (collectionIndex < 100)
            {
                indexString = "0" + indexString;
            }
            else if (collectionIndex == Int32.MaxValue)
            {
                indexString = "END";
            }

            base.DisplayName = String.Format("[{0}]", indexString);
        }

        private ObjectMemberViewModel(ObjectViewModel instance)
            : base(instance.MemoryExplorer)
        {
            if (instance == null)
                throw new ArgumentNullException("instance");

            _instance = instance;
        }

        #endregion // Constructors

        #region Collection Item Overrides

        public override string AccessModifier
        {
            get
            {
                if (!_isCollectionItem)
                    return base.AccessModifier;

                // An item in a collection has no access modifier
                // so return them all, ensuring that whatever valid 
                // filter is applied will pass this object through.
                return "public private protected internal";
            }
        }

        public override string DeclaringTypeName
        {
            get
            {
                if(!_isCollectionItem)
                    return base.DeclaringTypeName;

                return _instance.GetType().Name;
            }
        }

        public override string ToolTipText
        {
            get
            {
                if (!_isCollectionItem)
                    return base.ToolTipText;

                return base.DisplayName;
            }
        }

        public override string FullyQualifiedTypeName
        {
            get
            {
                if (!_isCollectionItem)
                    return base.FullyQualifiedTypeName;

                return this.Value.GetType().FullName;
            }
        }

        public override string GroupByCategory
        {
            get
            {
                if (!_isCollectionItem)
                    return base.GroupByCategory;

                return "3_collectionItem";
            }
        }

        public override string TypeName
        {
            get
            {
                if (!_isCollectionItem)
                    return base.TypeName;

                return this.Value == null ? "(null)" : this.Value.GetType().Name;
            }
        }

        #endregion // Collection Item Overrides

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
                            _value = _field.GetValue(_instance.GetInstanceInternal());
                        else
                            _value = _property.GetValue(_instance.GetInstanceInternal(), null);
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine("[ObjectMemberViewModel] Exception thrown while retrieving value: " + ex);
                    }
                }
                return _value;
            }
        }

        #endregion // Value [override]
    }
}