using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace Sleuth.InjectedViewer.ViewModel.MemoryExplorer
{
	public class ObjectViewModel
		: ViewModelBase,
		IBreadcrumb,
		ICanRefresh,
		IHaveFilterSettings
	{
		#region Fields

		readonly MemoryExplorerListFilterSettingsViewModel _filterSettings;
		readonly object _instance;
		readonly MemoryExplorerViewModel _memoryExplorer;
		readonly MemoryExplorerListItemViewModel _referencedBy;

		List<ObjectMemberViewModel> _propertiesAndFieldsList;
		AnimatedObservableCollection<ObjectMemberViewModel> _propertiesAndFieldsInternal;
		ReadOnlyObservableCollection<ObjectMemberViewModel> _propertiesAndFieldsReadOnly;

		#endregion // Fields

		#region Constructors

		public ObjectViewModel(object instance, MemoryExplorerViewModel memoryExplorer, MemoryExplorerListItemViewModel referencedBy)
		{
			if (instance == null)
				throw new ArgumentNullException("instance");

			if (memoryExplorer == null)
				throw new ArgumentNullException("memoryExplorer");

			_instance = instance;
			_memoryExplorer = memoryExplorer;
			_referencedBy = referencedBy;
			_filterSettings = new MemoryExplorerListFilterSettingsViewModel(_memoryExplorer);

			base.DisplayName = _instance.GetType().FullName;
		}

		#endregion // Constructors

		#region GetInstanceInternal

		internal object GetInstanceInternal()
		{
			return _instance;
		}

		#endregion // GetInstanceInternal

		#region MemoryExplorer

		public MemoryExplorerViewModel MemoryExplorer
		{
			get { return _memoryExplorer; }
		}

		#endregion // MemoryExplorer

		#region PropertiesAndFields

		public ReadOnlyObservableCollection<ObjectMemberViewModel> PropertiesAndFields
		{
			get
			{
				if (_propertiesAndFieldsReadOnly == null)
				{
					_propertiesAndFieldsList = this.LoadPropertiesAndFields();
					_propertiesAndFieldsInternal = new AnimatedObservableCollection<ObjectMemberViewModel>();
					_propertiesAndFieldsReadOnly = new ReadOnlyObservableCollection<ObjectMemberViewModel>(_propertiesAndFieldsInternal);
					_propertiesAndFieldsInternal.AddRangeOverTime(_propertiesAndFieldsList);
				}

				return _propertiesAndFieldsReadOnly;
			}
		}

		List<ObjectMemberViewModel> LoadPropertiesAndFields()
		{
			List<ObjectMemberViewModel> list = new List<ObjectMemberViewModel>();

			BindingFlags flags = BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance;

			Type currentType = _instance.GetType();
			while (currentType != null)
			{
				foreach (FieldInfo field in currentType.GetFields(flags))
					list.Add(new ObjectMemberViewModel(this, field));

				foreach (PropertyInfo property in currentType.GetProperties(flags))
				{
					if (IsPropertyDisplayable(property, currentType))
					{
						ObjectMemberViewModel viewModel = new ObjectMemberViewModel(this, property);
						if (!list.Any(item => item.DisplayName == viewModel.DisplayName))
							list.Add(viewModel);
					}
				}

				currentType = currentType.BaseType;
			}

			if (_instance is IEnumerable)
			{
				int index = 0;
				foreach (object value in (_instance as IEnumerable))
				{
					list.Add(new ObjectMemberViewModel(this, value, index));

					// Stop at 1000, just so it doesn't take forever to load.
					if (++index == 1000)
					{
						list.Add(new ObjectMemberViewModel(this, "--Crack.NET displays up to 1000 collection items--", Int32.MaxValue));
						break;
					}
				}
			}

			list.Sort(MemoryExplorerListItemViewModel.CompareListItems);

			return list;
		}

		static bool IsPropertyDisplayable(PropertyInfo property, Type currentType)
		{
			// Ignore write-only properties.
			MethodInfo getMethod = property.GetGetMethod(true);
			if (getMethod == null)
				return false;

			// Ignore indexers.
			ParameterInfo[] parameters = getMethod.GetParameters();
			if (parameters != null && 0 < parameters.Length)
				return false;

			// Accessing the DeclaringMethod property of a non-generic type throws an exception.
			if (property.Name == "DeclaringMethod" && currentType == typeof(Type) && !currentType.IsGenericType)
				return false;

			return true;
		}

		#endregion // PropertiesAndFields

		#region IBreadcrumb Members

		public string BreadcrumbDisplayName
		{
			get { return _referencedBy == null ? String.Empty : _referencedBy.DisplayName; }
		}

		public string BreadcrumbToolTipText
		{
			get { return _referencedBy == null ? String.Empty : _referencedBy.FullyQualifiedTypeName; }
		}

		public bool CanNavigateBackToObject
		{
			get { return true; }
		}

		#endregion // IBreadcrumb Members

		#region ICanRefresh Members

		public void RefreshValues()
		{
			// Throw away the old list, because if this object
			// is a collection, we might have lost or gained items.
			_propertiesAndFieldsReadOnly = null;
			base.OnPropertyChanged("PropertiesAndFields");
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