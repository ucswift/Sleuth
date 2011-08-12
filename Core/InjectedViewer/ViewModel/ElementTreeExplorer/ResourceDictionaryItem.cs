using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Sleuth.InjectedViewer.ViewModel.ElementTreeExplorer
{
    public class ResourceDictionaryItem : ElementTreeItem
    {
        #region Fields

        private ResourceDictionary _dictionary;

        #endregion

        #region  Constructors

        public ResourceDictionaryItem(ResourceDictionary dictionary, ElementTreeItem parent)
            : base(dictionary, parent)
        {
            _dictionary = dictionary;
        }

        #endregion

        #region Methods

        #region Public

        public override string ToString()
        {
            return _dictionary.Count + " Resources";
        }

        #endregion

        #region Protected

        protected override void Reload(List<ElementTreeItem> toBeRemoved)
        {
            base.Reload(toBeRemoved);

            foreach (object key in _dictionary.Keys)
            {

                object target = _dictionary[key];

                bool foundItem = false;
                foreach (ElementTreeItem item in toBeRemoved)
                {
                    if (item.Target == target)
                    {
                        toBeRemoved.Remove(item);
                        item.Reload();
                        foundItem = true;
                        break;
                    }
                }

                if (!foundItem)
                    Children.Add(new ResourceItem(target, key, this));
            }
        }

        #endregion

        #endregion
    }
}
