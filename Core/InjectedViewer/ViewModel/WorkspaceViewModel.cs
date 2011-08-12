using System;
using System.Windows.Input;
using Sleuth.InjectedViewer.ViewModel.Shell;

namespace Sleuth.InjectedViewer.ViewModel
{
    /// <summary>
    /// This ViewModelBase subclass requests to be removed 
    /// from the UI when its CloseCommand executes.
    /// This class is abstract.
    /// </summary>
    public abstract class WorkspaceViewModel : ViewModelBase
    {
        #region Fields

        ICommand _closeCommand;
        InjectedWindowViewModel _owner;

        #endregion // Fields

        #region Constructor

        protected WorkspaceViewModel()
        {
        }

        #endregion // Constructor

        #region AllowMultipleInstances

        /// <summary>
        /// Returns true if more than one instance of this workspace
        /// type can be in the UI at a time.  Child classes can override
        /// this property.  The default value is false.
        /// </summary>
        public virtual bool AllowMultipleInstances
        {
            get { return false; }
        }

        #endregion // AllowMultipleInstances

        #region CloseCommand

        /// <summary>
        /// Returns the command that, when invoked, attempts
        /// to remove this workspace from the user interface.
        /// </summary>
        public ICommand CloseCommand
        {
            get
            {
                if (_closeCommand == null)
                    _closeCommand = new RelayCommand(() => this.OnRequestClose());

                return _closeCommand;
            }
        }

        #endregion // CloseCommand

        #region Initialize

        /// <summary>
        /// Initializes this workspace with its owner.  Derived
        /// classes can override this method to perform initialization work.
        /// Be sure to call the base implementation when overriding.
        /// </summary>
        /// <param name="owner">The containing ViewModel</param>
        public virtual void Initialize(InjectedWindowViewModel owner)
        {
            if (owner == null)
                throw new ArgumentNullException("owner");

            _owner = owner;
        }

        #endregion // Initialize

        #region Owner

        /// <summary>
        /// Returns the containing ViewModel object.
        /// </summary>
        protected InjectedWindowViewModel Owner
        {
            get { return _owner; }
        }

        #endregion // Owner

        #region RequestClose [event]

        /// <summary>
        /// Raised when this workspace should be removed from the UI.
        /// </summary>
        public event EventHandler RequestClose;

        protected virtual void OnRequestClose()
        {
            EventHandler handler = this.RequestClose;
            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        #endregion // RequestClose [event]
    }
}