using System;
using System.Collections.ObjectModel;
using Sleuth.InjectedViewer.Filtering;

namespace Sleuth.InjectedViewer.ViewModel.MemoryExplorer
{
	public class MemoryExplorerListFilterSettingsViewModel : ViewModelBase
	{
		#region Fields

		PropertyFilterGroup _filterGroup;
		bool _isFilterAreaExpanded;
		readonly MemoryExplorerViewModel _memoryExplorer;

		#endregion // Fields

		#region Constructor

		public MemoryExplorerListFilterSettingsViewModel(MemoryExplorerViewModel memoryExplorer)
		{
			if (memoryExplorer == null)
				throw new ArgumentNullException("memoryExplorer");

			_memoryExplorer = memoryExplorer;
		}

		#endregion // Constructor

		#region FilterGroup

		public PropertyFilterGroup FilterGroup
		{
			get
			{
				if (_filterGroup == null)
				{
					_filterGroup = new PropertyFilterGroup();
					_filterGroup.Filters = new ObservableCollection<PropertyFilter>
										{
												new PropertyFilter
												{
															ActiveCriterion=FilterCriterion.StartsWith,
															DisplayName="Member name", 
															PropertyName="DisplayName", 
															PropertyType=typeof(string) 
												},
												new PropertyFilter
												{
															ActiveCriterion=FilterCriterion.Contains,
															DisplayName="Access modifier", 
															PropertyName="AccessModifier", 
															PropertyType=typeof(string) 
												},                      
												new PropertyFilter
												{
															ActiveCriterion=FilterCriterion.StartsWith,
															DisplayName="Value", 
															PropertyName="ValueString", 
															PropertyType=typeof(string) 
												},
												new PropertyFilter
												{
															ActiveCriterion=FilterCriterion.IsEqualTo,
															DisplayName="Has null value", 
															PropertyName="HasNullValue", 
															PropertyType=typeof(Boolean) 
												},
												new PropertyFilter
												{
															ActiveCriterion=FilterCriterion.StartsWith,
															DisplayName="Type", 
															PropertyName="TypeName", 
															PropertyType=typeof(string) 
												},
												new PropertyFilter
												{
															ActiveCriterion=FilterCriterion.StartsWith,
															DisplayName="Declaring type", 
															PropertyName="DeclaringTypeName", 
															PropertyType=typeof(string) 
												} 
										};
				}
				return _filterGroup;
			}
		}

		#endregion // FilterGroup

		#region IsFilterAreaExpanded

		public bool IsFilterAreaExpanded
		{
			get { return _isFilterAreaExpanded; }
			set
			{
				if (!_memoryExplorer.IsViewLoaded)
					return;

				if (value == _isFilterAreaExpanded)
					return;

				_isFilterAreaExpanded = value;

				base.OnPropertyChanged("IsFilterAreaExpanded");
			}
		}

		#endregion // IsFilterAreaExpanded
	}
}