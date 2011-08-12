using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Sleuth.InjectedViewer.ViewModel.ElementTreeExplorer
{
    public class ResourceItem : ElementTreeItem
    {
        #region Fields

        private object _key;

        #endregion

        #region Constructors

        public ResourceItem(object target, object key, ElementTreeItem parent)
            : base(target, parent)
        {
            _key = key;
        }

        #endregion

        #region Methods

        #region Public

        public override string ToString()
        {
            return _key.ToString() + " (" + Target.GetType().Name + ")";
        }

        #endregion

        #endregion
    }
}
