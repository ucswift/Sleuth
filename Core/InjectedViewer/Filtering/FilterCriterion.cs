using System;

namespace Sleuth.InjectedViewer.Filtering
{
    public class FilterCriterion
    {
        #region Static Fields

        #region Contains

        public static readonly FilterCriterion Contains = new FilterCriterion(
            "Contains",
            (dataItem, value) => dataItem != null && dataItem.ToString().IndexOf(value, StringComparison.InvariantCultureIgnoreCase) > -1);

        #endregion // Contains

        #region EndsWith

        public static readonly FilterCriterion EndsWith = new FilterCriterion(
            "Ends With",
            (dataItem, value) => dataItem != null && dataItem.ToString().EndsWith(value, StringComparison.InvariantCultureIgnoreCase));

        #endregion // EndsWith

        #region IsEqualTo

        public static readonly FilterCriterion IsEqualTo = new FilterCriterion(
            "Is Equal To",
            (dataItem, value) =>
            {
                if (dataItem == null)
                    return value == null;

                object convertedValue = value;
                try
                {
                    if (PropertyFilter.IsNumericType(dataItem.GetType()))
                    {
                        dataItem = Convert.ChangeType(dataItem, typeof(double));
                        convertedValue = Convert.ChangeType(value, typeof(double));
                    }
                    else if (dataItem is bool)
                    {
                        convertedValue = Convert.ChangeType(value, typeof(bool));
                    }
                    else if (dataItem is Enum)
                    {
                        dataItem = dataItem.ToString();
                    }

                    return Object.Equals(dataItem, convertedValue);
                }
                catch
                {
                    return false;
                }
            });

        #endregion // IsEqualTo

        #region IsGreaterThan

        public static readonly FilterCriterion IsGreaterThan = new FilterCriterion(
            "Is Greater Than",
            (dataItem, value) =>
            {
                if (dataItem == null)
                    return false;

                object convertedValue = value;
                try
                {
                    if (PropertyFilter.IsNumericType(dataItem.GetType()))
                    {
                        dataItem = Convert.ChangeType(dataItem, typeof(double));
                        convertedValue = Convert.ChangeType(value, typeof(double));
                    }

                    IComparable comp = dataItem as IComparable;
                    if (comp == null)
                        return false;

                    return comp.CompareTo(convertedValue) > 0;
                }
                catch
                {
                    return false;
                }
            });

        #endregion // IsGreaterThan

        #region IsLessThan

        public static readonly FilterCriterion IsLessThan = new FilterCriterion(
            "Is Less Than",
            (dataItem, value) =>
            {
                if (dataItem == null)
                    return true;

                object convertedValue = value;
                try
                {
                    if (PropertyFilter.IsNumericType(dataItem.GetType()))
                    {
                        dataItem = Convert.ChangeType(dataItem, typeof(double));
                        convertedValue = Convert.ChangeType(value, typeof(double));
                    }

                    IComparable comp = dataItem as IComparable;
                    if (comp == null)
                        return false;

                    return comp.CompareTo(convertedValue) < 0;
                }
                catch
                {
                    return false;
                }
            });

        #endregion // IsLessThan

        #region StartsWith

        public static readonly FilterCriterion StartsWith = new FilterCriterion(
            "Starts With",
            (dataItem, value) => dataItem != null && dataItem.ToString().StartsWith(value, StringComparison.InvariantCultureIgnoreCase));

        #endregion // StartsWith

        #endregion // Static Fields

        #region Instance Properties

        public string DisplayName { get; set; }

        public Func<object, string, bool> IsFilteredIn { get; private set; }

        #endregion // Instance Properties

        #region Private Constructor

        FilterCriterion(string displayName, Func<object, string, bool> isFilteredIn)
        {
            this.DisplayName = displayName;
            this.IsFilteredIn = isFilteredIn;
        }

        #endregion // Private Constructor
    }
}