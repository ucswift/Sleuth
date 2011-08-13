using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Input;

namespace Sleuth.InjectedViewer.ViewModel.MemoryExplorer
{
	/// <summary>
	/// Base class for items that appear in a ListView in the 'Memory Explorer' workspace.
	/// </summary>
	public abstract class MemoryExplorerListItemViewModel : ViewModelBase
	{
		#region Fields

		protected FieldInfo _field;
		protected MemberInfo _member;
		protected PropertyInfo _property;

		readonly MemoryExplorerViewModel _memoryExplorer;

		string _accessModifier;
		ICommand _copyValueTextToClipboardCommand;
		bool _isSelected;
		ICommand _navigateToValueCommand;
		ICommand _openInReflectorCommand;
		string _toolTipText;

		#endregion // Fields

		#region Constructor

		public MemoryExplorerListItemViewModel(MemoryExplorerViewModel memoryExplorer)
		{
			if (memoryExplorer == null)
				throw new ArgumentNullException("memoryExplorer");

			_memoryExplorer = memoryExplorer;
		}

		#endregion // Constructor

		#region CompareListItems [static]

		public static int CompareListItems(MemoryExplorerListItemViewModel item1, MemoryExplorerListItemViewModel item2)
		{
			// We sort the items in the same way that the SelectedEntityListView wants to show them.
			// This ensures that objects with many members are loaded up and displayed smoothly
			// when the item population is animated by the PopulatePropertiesAndFields method.

			if (item1 == null)
				return item2 == null ? 0 : -1;

			if (item2 == null)
				return +1;

			if (item1.GroupByCategory != item2.GroupByCategory)
				return item1.GroupByCategory.CompareTo(item2.GroupByCategory);
			else
				return item1.DisplayName.CompareTo(item2.DisplayName);
		}

		#endregion // CompareListItems [static]

		#region Commands

		#region CopyValueTextToClipboardCommand

		public ICommand CopyValueTextToClipboardCommand
		{
			get
			{
				if (_copyValueTextToClipboardCommand == null)
				{
					_copyValueTextToClipboardCommand = new RelayCommand(
							() => Clipboard.SetText(this.Value.ToString()),
							() => !this.HasNullValue);
				}
				return _copyValueTextToClipboardCommand;
			}
		}

		#endregion // CopyValueTextToClipboardCommand

		#region NavigateToValueCommand

		public ICommand NavigateToValueCommand
		{
			get
			{
				if (_navigateToValueCommand == null)
					_navigateToValueCommand = new RelayCommand(() => this.NavigateToValue());

				return _navigateToValueCommand;
			}
		}

		void NavigateToValue()
		{
			_memoryExplorer.IsDoingWork = true;
			_memoryExplorer.SelectedObject = new ObjectViewModel(this.Value, _memoryExplorer, this);
		}

		#endregion // NavigateToValueCommand

		#region OpenInReflectorCommand

		public ICommand OpenInReflectorCommand
		{
			get
			{
				if (_openInReflectorCommand == null)
				{
					_openInReflectorCommand = new RelayCommand(
							() => Reflector.OpenMember(_member),
							() => _member != null);
				}
				return _openInReflectorCommand;
			}
		}

		#endregion // OpenInReflectorCommand

		#endregion // Commands

		#region Public Properties

		#region AccessModifier

		public virtual string AccessModifier
		{
			get
			{
				if (String.IsNullOrEmpty(_accessModifier))
					_accessModifier = this.GetAccessModifier();

				return _accessModifier;
			}
		}

		string GetAccessModifier()
		{
			string accessModifier = "n/a"; ;
			if (this.IsField)
			{
				if (_field.IsPublic)
					accessModifier = "public";
				else if (_field.IsPrivate)
					accessModifier = "private";
				else if (_field.IsAssembly)
					accessModifier = "internal";
				else if (_field.IsFamily)
					accessModifier = "protected";
				else if (_field.IsFamilyOrAssembly)
					accessModifier = "protected internal";
				else
					accessModifier = "protected AND internal";
			}
			else
			{
				if (_property == null)
				{
					Debug.Fail("If the member is not a field, it must be a property.");
				}
				else
				{
					MethodInfo getMethod = _property.GetGetMethod(true);
					if (getMethod == null)
					{
						Debug.Fail("Property does not have a getter.");
					}
					else
					{
						if (getMethod.IsPublic)
							accessModifier = "public";
						else if (getMethod.IsPrivate)
							accessModifier = "private";
						else if (getMethod.IsAssembly)
							accessModifier = "internal";
						else if (getMethod.IsFamily)
							accessModifier = "protected";
						else if (getMethod.IsFamilyOrAssembly)
							accessModifier = "protected internal";
						else
							accessModifier = "protected AND internal";
					}
				}
			}
			return accessModifier;
		}

		#endregion // AccessModifier

		#region DeclaringTypeName

		public virtual string DeclaringTypeName
		{
			get { return this.IsField ? _field.DeclaringType.Name : _property.DeclaringType.Name; }
		}

		#endregion // DeclaringTypeName

		#region FullyQualifiedTypeName

		public virtual string FullyQualifiedTypeName
		{
			get { return this.IsField ? _field.FieldType.FullName : _property.PropertyType.FullName; }
		}

		#endregion // FullyQualifiedTypeName

		#region GroupByCategory

		public virtual string GroupByCategory
		{
			get { return this.IsField ? "2_field" : "1_property"; }
		}

		#endregion // GroupByCategory

		#region HasNullValue

		public bool HasNullValue
		{
			get { return this.Value == null; }
		}

		#endregion // HasNullValue

		#region IsField

		public bool IsField
		{
			get { return _field != null && _property == null; }
		}

		#endregion // IsField

		#region IsRelevant

		public bool IsRelevant
		{
			get
			{
				object value = this.Value;
				if (value == null)
					return false;

				Type type = value.GetType();
				return
						!type.IsEnum &&
						!type.IsInterface &&
						!Utilities.IsPrimitiveType(type);
			}
		}

		#endregion // IsRelevant

		#region IsSelected

		/// <summary>
		/// Gets/sets whether this object is in the 'selected' state in the UI.
		/// </summary>
		public virtual bool IsSelected
		{
			get { return _isSelected; }
			set
			{
				if (!_memoryExplorer.IsViewLoaded)
					return;

				if (value == _isSelected)
					return;

				_isSelected = value;

				base.OnPropertyChanged("IsSelected");
			}
		}

		#endregion // IsSelected

		#region ToolTipText

		public virtual string ToolTipText
		{
			get
			{
				if (_toolTipText == null)
				{
					if (this.IsField)
					{
						_toolTipText = String.Format(
								"{0}.{1}{2}Access modifier: {3}",
								_field.DeclaringType.FullName,
								_field.Name,
								Environment.NewLine,
								this.AccessModifier);
					}
					else if (_property.Name.Contains("."))
					{
						// If the property is an explicitly implemented interface member, 
						// its name is already fully-qualified.
						_toolTipText = String.Format(
								"{0}{1}Access modifier: {2}{1}Implemented by: {3}",
								_property.Name,
								Environment.NewLine,
								this.AccessModifier,
								_property.DeclaringType.FullName);
					}
					else
					{
						_toolTipText = String.Format(
								"{0}.{1}{2}Access modifier: {3}",
								_property.DeclaringType.FullName,
								_property.Name,
								Environment.NewLine,
								this.AccessModifier);
					}
				}
				return _toolTipText;
			}
		}

		#endregion // ToolTipText

		#region TypeName

		public virtual string TypeName
		{
			get { return this.IsField ? _field.FieldType.Name : _property.PropertyType.Name; }
		}

		#endregion // TypeName

		#region ValueString

		public string ValueString
		{
			get
			{
				if (this.Value == null)
					return "(null)";

				// For some reason, calling ToString() on a Hash freezes the application indefinitely.
				System.Security.Policy.Hash hash = this.Value as System.Security.Policy.Hash;
				if (hash != null)
					return "System.Security.Policy.Hash";

				// For some reason, calling ToString() on a PropertyChangedEventManager freezes the application indefinitely.
				System.ComponentModel.PropertyChangedEventManager mgr = this.Value as System.ComponentModel.PropertyChangedEventManager;
				if (mgr != null)
					return "System.ComponentModel.PropertyChangedEventManager";

				return this.Value.ToString();
			}
		}

		#endregion // ValueString

		#endregion // Public Properties

		#region Protected Properties

		#region Value

		/// <summary>
		/// Returns the value associated with this list item in Memory Explorer.
		/// </summary>
		protected abstract object Value { get; }

		#endregion // Value

		#endregion // Protected Properties

		#region OnPropertyChanged [override]

		protected override void OnPropertyChanged(string propertyName)
		{
			base.OnPropertyChanged(propertyName);

			if (propertyName == "Value")
				base.OnPropertyChanged("ValueString");
		}

		#endregion // OnPropertyChanged [override]
	}
}