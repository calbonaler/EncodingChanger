using System;
using System.Windows.Input;

namespace EncodingChanger
{
	public sealed class DelegateCommand : ICommand
	{
		public DelegateCommand(Action action) => m_Action = action;

		readonly Action m_Action;

		public void Execute() => m_Action();

		bool ICommand.CanExecute(object parameter) => true;
		void ICommand.Execute(object parameter) => Execute();

		event EventHandler ICommand.CanExecuteChanged
		{
			add { }
			remove { }
		}
	}
}
