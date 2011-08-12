using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace Sleuth.InjectedViewer.ViewModel.ElementTreeExplorer
{
    public abstract class ResourceContainerItem : ElementTreeItem
    {
        #region Constructors

        public ResourceContainerItem(object target, ElementTreeItem parent)
            : base(target, parent)
        {
        }

        #endregion

        #region Properties

        protected abstract ResourceDictionary ResourceDictionary { get; }

        #endregion

        #region Methods

        #region Protected

        protected override void Reload(List<ElementTreeItem> toBeRemoved)
        {
            base.Reload(toBeRemoved);

            ResourceDictionary resources = ResourceDictionary;

            if (resources != null && resources.Count != 0)
            {
                bool foundItem = false;
                foreach (ElementTreeItem item in toBeRemoved)
                {
                    if (item.Target == resources)
                    {
                        toBeRemoved.Remove(item);
                        item.Reload();
                        foundItem = true;
                        break;
                    }
                }
                if (!foundItem)
                    Children.Add(ElementTreeItem.Construct(resources, this));
            }
        }

        #endregion

        #endregion
    }
}
