using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;

namespace Sleuth.InjectedViewer.ViewModel.ElementTreeExplorer
{
    public class ApplicationTreeItem : ResourceContainerItem
    {
        #region Fields

        private Application _application;

        #endregion

        #region Constructors

        public ApplicationTreeItem(Application application, ElementTreeItem parent)
            : base(application, parent)
        {
            _application = application;
        }

        #endregion

        #region Properties

        public override Visual MainVisual
        {
            get { return _application.MainWindow; }
        }

        #endregion

        #region Methods

        #region Protected

        protected override void Reload(List<ElementTreeItem> toBeRemoved)
        {
            base.Reload(toBeRemoved);

            if (_application.MainWindow != null)
            {
                bool foundMainWindow = false;
                foreach (ElementTreeItem item in toBeRemoved)
                {
                    if (item.Target == _application.MainWindow)
                    {
                        toBeRemoved.Remove(item);
                        item.Reload();
                        foundMainWindow = true;
                        break;
                    }
                }

                if (!foundMainWindow)
                    Children.Add(ElementTreeItem.Construct(_application.MainWindow, this));
            }
        }

        protected override ResourceDictionary ResourceDictionary
        {
            get { return _application.Resources; }
        }

        #endregion

        #endregion
    }
}
