using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Sleuth.InjectedViewer.ViewModel.MemoryExplorer;
using Sleuth.InjectedViewer.ViewModel.Shell;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;
using Microsoft.Win32;

namespace Sleuth.InjectedViewer.ViewModel.Scriptorium
{
	public class ScriptoriumViewModel : WorkspaceViewModel
	{
		#region Fields

		ICommand _clearExecutionResultsCommand;
		ICommand _executeScriptCommand;
		ICommand _insertStarterScriptCommand;
		ICommand _saveExecutionResultsCommand;

		ViewModelBase _inputVariable;
		object _unwrappedInputVariable;

		string _executionResults;
		string _script;

		#endregion // Fields

		#region Initialization

		public ScriptoriumViewModel()
		{
			base.DisplayName = "Scriptorium";
		}

		public override void Initialize(InjectedWindowViewModel owner)
		{
			base.Initialize(owner);

			_inputVariable = owner.FindInputVariableForNewScriptorium();
			_unwrappedInputVariable = this.UnwrapInputVariableFromViewModel();
		}

		object UnwrapInputVariableFromViewModel()
		{
			if (_inputVariable == null)
				return null;

			AssemblyViewModel assemblyVM = _inputVariable as AssemblyViewModel;
			if (assemblyVM != null)
				return assemblyVM.GetAssemblyInternal();

			TypeViewModel typeVM = _inputVariable as TypeViewModel;
			if (typeVM != null)
				return typeVM.GetTypeInternal();

			ObjectViewModel objectVM = _inputVariable as ObjectViewModel;
			if (objectVM != null)
				return objectVM.GetInstanceInternal();

			Debug.Fail("Unexpected input variable type: " + _inputVariable.GetType());
			return null;
		}

		#endregion // Initialization

		#region AllowMultipleInstances

		public override bool AllowMultipleInstances
		{
			get { return true; }
		}

		#endregion // AllowMultipleInstances

		#region ClearExecutionResultsCommand

		public ICommand ClearExecutionResultsCommand
		{
			get
			{
				if (_clearExecutionResultsCommand == null)
				{
					_clearExecutionResultsCommand = new RelayCommand(
							() => this.ClearExecutionResults(),
							() => this.CanClearExecutionResults);
				}
				return _clearExecutionResultsCommand;
			}
		}

		void ClearExecutionResults()
		{
			this.ExecutionResults = null;
		}

		bool CanClearExecutionResults
		{
			get { return !String.IsNullOrEmpty(this.ExecutionResults); }
		}

		#endregion // ClearExecutionResultsCommand

		#region ExecuteScriptCommand

		public ICommand ExecuteScriptCommand
		{
			get
			{
				if (_executeScriptCommand == null)
				{
					_executeScriptCommand = new RelayCommand(
							() => this.ExecuteScript(),
							() => this.CanExecuteScript);
				}
				return _executeScriptCommand;
			}
		}

		void ExecuteScript()
		{
			if (this.ExecutionResults == null)
				this.ExecutionResults = String.Empty;
			else
				this.ExecutionResults += Environment.NewLine + Environment.NewLine;

			this.ExecutionResults += "* * * * * * * * * * * * * * * * * * * * * * * * * *" + Environment.NewLine;
			this.ExecutionResults += "* executed @ " + DateTime.Now.ToString() + Environment.NewLine;
			this.ExecutionResults += "* * * * * * * * * * * * * * * * * * * * * * * * * *" + Environment.NewLine;

			try
			{
				//ScriptEngine engine = new Python.CreateEngine();
				var engine = Python.CreateEngine();
				MemoryStream outputStream = new MemoryStream();
				//engine.SetStandardError(outputStream);
				//engine.SetStandardOutput(outputStream);

				engine.ImportModule("sys");
				engine.ImportModule("Site");
				//engine.Globals["INPUT"] = _unwrappedInputVariable;
				engine.Execute(this.Script);

				outputStream.Position = 0;
				if (outputStream.Length != 0)
				{
					string results = UTF8Encoding.Default.GetString(outputStream.ToArray());
					this.ExecutionResults += results;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Script Error", MessageBoxButton.OK, MessageBoxImage.Error);
			}
		}

		bool CanExecuteScript
		{
			get { return !String.IsNullOrEmpty(this.Script) && this.Script.Trim().Length != 0; }
		}

		#endregion // ExecuteScriptCommand

		#region InsertStarterScriptCommand

		public ICommand InsertStarterScriptCommand
		{
			get
			{
				if (_insertStarterScriptCommand == null)
					_insertStarterScriptCommand = new RelayCommand(() => this.InsertStarterScript());

				return _insertStarterScriptCommand;
			}
		}

		void InsertStarterScript()
		{
			// The Python script file is stored as an embedded resource, instead of a resource,
			// because this might be running in a Windows Forms application.  Embedded resources
			// are accessible by both WPF and WinForms applications.
			using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("Sleuth.InjectedViewer.ViewModel.Scriptorium.StarterScript.py"))
			using (StreamReader reader = new StreamReader(stream))
			{
				string starterScript = reader.ReadToEnd();
				string script = this.Script ?? String.Empty;
				this.Script = starterScript + script;
			}
		}

		#endregion // InsertStarterScriptCommand

		#region SaveExecutionResultsCommand

		public ICommand SaveExecutionResultsCommand
		{
			get
			{
				if (_saveExecutionResultsCommand == null)
				{
					_saveExecutionResultsCommand = new RelayCommand(
							() => this.SaveExecutionResults(),
							() => this.CanSaveExecutionResults);
				}
				return _saveExecutionResultsCommand;
			}
		}

		void SaveExecutionResults()
		{
			SaveFileDialog dlg = new SaveFileDialog();
			dlg.Filter = "Text files (*.txt)|*.txt|XML files (*.xml)|*.xml|All files (*.*)|*.*";
			if (dlg.ShowDialog() ?? false)
			{
				string header = ">> This file was created by Crack.NET (http://Sleuthproject.com)" + Environment.NewLine + Environment.NewLine;
				File.WriteAllText(dlg.FileName, header + this.ExecutionResults);
			}
		}

		bool CanSaveExecutionResults
		{
			get { return !String.IsNullOrEmpty(this.ExecutionResults); }
		}

		#endregion // SaveExecutionResultsCommand

		#region ExecutionResults

		public string ExecutionResults
		{
			get { return _executionResults; }
			set
			{
				if (value == _executionResults)
					return;

				_executionResults = value;

				base.OnPropertyChanged("ExecutionResults");
			}
		}

		#endregion // ExecutionResults

		#region InputVariableDisplayText

		public string InputVariableDisplayText
		{
			get
			{
				if (_inputVariable == null)
					return "(null)";

				string text = _inputVariable.DisplayName;

				if (_inputVariable is AssemblyViewModel)
				{
					text += " (assembly)";
				}
				else if (_inputVariable is TypeViewModel)
				{
					text += " (type)";
				}
				else if (_inputVariable is ObjectViewModel)
				{
					text += " (object)";
				}
				else
				{
					Debug.Fail("Unexpected input variable type: " + _inputVariable.GetType().FullName);
				}

				return text;
			}
		}

		#endregion // InputVariableDisplayText

		#region Script

		public string Script
		{
			get { return _script; }
			set
			{
				if (value == _script)
					return;

				_script = value;

				base.OnPropertyChanged("Script");
			}
		}

		#endregion // Script
	}
}