using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Data;
using System.Windows.Input;

namespace Sleuth.InjectedViewer.Filtering
{
    public class PropertyFilter : INotifyPropertyChanged
    {
        #region Data

        ICommand _applyCommand;
        ICommand _clearCommand;

        FilterCriterion _activeCriterion;
        string _appliedValue;
        FilterCriterion[] _availableCriteria;
        string _displayName;
        Type _displayType;
        string _value;

        bool? _cachedIsValueValid;

        #endregion // Data

        #region Constructor

        public PropertyFilter()
        {
        }

        public PropertyFilter(string propertyName, Type propertyType, IValueConverter propertyConverter)
        {
            this.PropertyName = propertyName;
            this.PropertyType = propertyType;
            this.PropertyConverter = propertyConverter;
        }

        #endregion // Constructor

        #region ApplyCommand

        public ICommand ApplyCommand
        {
            get
            {
                if (_applyCommand == null)
                {
                    _applyCommand = new RelayCommand(
                        () => this.Apply(),
                        () => this.CanApply);
                }
                return _applyCommand;
            }
        }

        void Apply()
        {
            _appliedValue = this.Value;
            this.Group.ApplyFilters();
        }

        bool CanApply
        {
            get
            {
                return
                    !String.IsNullOrEmpty(this.Value) &&
                    this.Value != _appliedValue &&
                    this.ActiveCriterion != null &&
                    this.IsValueValid;
            }
        }

        #endregion // ApplyCommand

        #region ClearCommand

        public ICommand ClearCommand
        {
            get
            {
                if (_clearCommand == null)
                {
                    _clearCommand = new RelayCommand(
                        () => this.Clear(),
                        () => this.CanClear);
                }
                return _clearCommand;
            }
        }

        void Clear()
        {
            _appliedValue = null;
            this.Value = null;
            this.Group.ApplyFilters();
        }

        bool CanClear
        {
            get { return _appliedValue != null; }
        }

        #endregion // ClearCommand

        #region Methods

        public bool IsFilteredIn(object dataItem)
        {
            if (_appliedValue == null)
                return true;

            if (dataItem == null || dataItem == DBNull.Value)
                return true;

            try
            {
                PropertyDescriptor desc = TypeDescriptor.CreateProperty(dataItem.GetType(), this.PropertyName, this.PropertyType);
                if (desc == null)
                    return false;

                object propertyValue = desc.GetValue(dataItem);

                if (this.PropertyConverter != null)
                {
                    propertyValue = this.PropertyConverter.Convert(
                        propertyValue,
                        this.DisplayType,
                        null, // TODO: Look into adding support for passing a parameter.
                        System.Threading.Thread.CurrentThread.CurrentCulture);
                }

                return this.ActiveCriterion.IsFilteredIn(propertyValue, this.Value);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while evaluating a property filter.", ex);
            }
        }

        public static bool IsNumericType(Type type)
        {
            return
                   type == typeof(Byte)
                || type == typeof(Int16)
                || type == typeof(Int32)
                || type == typeof(Int64)
                || type == typeof(UInt16)
                || type == typeof(UInt32)
                || type == typeof(UInt64)
                || type == typeof(Single)
                || type == typeof(Double)
                || type == typeof(Decimal);
        }

        #endregion // Methods

        #region Properties

        public FilterCriterion ActiveCriterion
        {
            get { return _activeCriterion; }
            set
            {
                _activeCriterion = value;

                CommandManager.InvalidateRequerySuggested();

                this.OnPropertyChanged("ActiveCriterion");
            }
        }

        public FilterCriterion[] AvailableCriteria
        {
            get { return _availableCriteria ?? (_availableCriteria = this.GetAvailableCriteria()); }
        }

        public string DisplayName
        {
            get { return _displayName ?? this.PropertyName; }
            set { _displayName = value; }
        }

        public Type DisplayType
        {
            get { return _displayType ?? this.PropertyType; }
            set { _displayType = value; }
        }

        public PropertyFilterGroup Group { get; internal set; }
        public IValueConverter PropertyConverter { get; set; }
        public string PropertyName { get; set; }
        public Type PropertyType { get; set; }

        public string Value
        {
            get
            {
                if (_value != null)
                    return _value;

                // Giving a default value of "false" prevents a binding 
                // error from being emitted to the Output window.
                if (this.PropertyType == typeof(bool))
                    return "false";

                return null;
            }
            set
            {
                _value = value;

                _cachedIsValueValid = null;

                this.OnPropertyChanged("Value");

                CommandManager.InvalidateRequerySuggested();
            }
        }

        #endregion // Properties

        #region Private Helpers

        #region GetAvailableCriteria

        FilterCriterion[] GetAvailableCriteria()
        {
            FilterCriterion[] criteria = null;

            if (this.DisplayType == typeof(string))
            {
                criteria = new FilterCriterion[] 
                        { 
                            FilterCriterion.Contains, 
                            FilterCriterion.EndsWith, 
                            FilterCriterion.IsEqualTo,
                            FilterCriterion.StartsWith 
                        };
            }
            else if (IsNumericType(this.DisplayType))
            {
                criteria = new FilterCriterion[] 
                        { 
                            FilterCriterion.IsEqualTo, 
                            FilterCriterion.IsGreaterThan, 
                            FilterCriterion.IsLessThan 
                        };
            }
            else if (this.DisplayType == typeof(bool))
            {
                criteria = new FilterCriterion[] 
                        { 
                            FilterCriterion.IsEqualTo
                        };
            }
            else
            {
                criteria = new FilterCriterion[0];
                // NOTE: Many common field types are not supported in this demo, such as DateTime.
                Debug.WriteLine("Unsupported field type: " + this.DisplayType);
            }

            return criteria;
        }

        #endregion // GetAvailableCriteria

        #region IsValueValid

        bool IsValueValid
        {
            get
            {
                if (_cachedIsValueValid == null)
                {
                    if (this.DisplayType == typeof(string))
                    {
                        _cachedIsValueValid = _value != null;
                    }
                    else
                    {
                        try
                        {
                            Convert.ChangeType(this.Value, this.DisplayType);
                            _cachedIsValueValid = true;
                        }
                        catch
                        {
                            _cachedIsValueValid = false;
                        }
                    }
                }
                return _cachedIsValueValid.Value;
            }
        }

        #endregion // IsValueValid

        #endregion // Private Helpers

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        void OnPropertyChanged(string prop)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(prop));
        }

        #endregion // INotifyPropertyChanged Implementation
    }
}