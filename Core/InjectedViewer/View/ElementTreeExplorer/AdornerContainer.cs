using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;

namespace Sleuth.InjectedViewer.ViewModel.ElementTreeExplorer
{
    public class AdornerContainer : Adorner
    {
        #region Fields

        private UIElement _child;

        #endregion

        #region Constructors

        public AdornerContainer(UIElement adornedElement)
            : base(adornedElement)
        {
        }

        #endregion 

        #region Methods

        #region Public

        public UIElement Child
        {
            get { return this._child; }
            set
            {
                this.AddVisualChild(value);
                this._child = value;
            }
        }

        #endregion

        #region Protected

        protected override Size ArrangeOverride(Size finalSize)
        {
            if (this._child != null)
                this._child.Arrange(new Rect(finalSize));
            return finalSize;
        }

        protected override Visual GetVisualChild(int index)
        {
            if (index == 0 && this._child != null)
                return this._child;
            return base.GetVisualChild(index);
        }

        protected override int VisualChildrenCount
        {
            get { return this._child == null ? 0 : 1; }
        }

        #endregion

        #endregion
    }
}