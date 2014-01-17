using AOManager;
using Microsoft.WindowsAzure.Storage;
using NicolasDorier;
using NicolasDorier.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace WMIViewer
{
	public enum CommandStates
	{
		Running,
		Failure,
		Success,
	}
	public class WrapperCommandViewModel : NotifyPropertyChangedBase
	{
		public WrapperCommandViewModel(CommandViewModel command)
		{
			_Command = command;
			StartTime = DateTime.Now;
			Subscribe();
		}

		private readonly CommandViewModel _Command;
		public CommandViewModel Command
		{
			get
			{
				return _Command;
			}
		}

		public DateTime StartTime
		{
			get;
			set;
		}


		private CommandStates _State;
		public CommandStates State
		{
			get
			{
				return _State;
			}
			set
			{
				if(value != _State)
				{
					_State = value;
					OnPropertyChanged(() => this.State);
				}
			}
		}

		private string _Message;
		public string Message
		{
			get
			{
				return _Message;
			}
			set
			{
				if(value != _Message)
				{
					_Message = value;
					OnPropertyChanged(() => this.Message);
				}
			}
		}

		private string _Script;
		public string Script
		{
			get
			{
				return _Script;
			}
			set
			{
				if(value != _Script)
				{
					_Script = value;
					OnPropertyChanged(() => this.Script);
				}
			}
		}

		
		private void UnSubscribe()
		{
			Command.PropertyChanged -= Command_PropertyChanged;
			Command.Faulted -= Command_Faulted;
			Command.Executed -= Command_Executed;
			Command.Executing -= Command_Executing;

		}

	

		private void Subscribe()
		{
			Command.PropertyChanged += Command_PropertyChanged;
			Command.Faulted += Command_Faulted;
			Command.Executed += Command_Executed;
			Command.Executing += Command_Executing;
		}

		private void UpdateState()
		{
			if(Command.IsExecuting)
			{
				State = CommandStates.Running;
				Message = Command.RunningMessage;
			}
			else
			{
				if((Command.Execution != null && Command.Execution.IsFaulted) || !string.IsNullOrEmpty(Command.ErrorMessage))
				{
					State = CommandStates.Failure;
					Message = (String.IsNullOrEmpty(Command.ErrorMessage) || Command.Execution.IsFaulted) ? GetFaultString(Command.Execution) : Command.ErrorMessage;
					Command.ErrorMessage = Message;
				}
				else
				{
					State = CommandStates.Success;
					Message = Command.RunningMessage;
				}
				UnSubscribe();
			}
		}

		private string GetFaultString(Task task)
		{
			if(task == null || task.Exception == null || task.Exception.InnerExceptions.Count == 0)
				return null;
			var ex = UnWrap(task.Exception);
			return ex.Message;
		}

		private Exception UnWrap(AggregateException aggregateException)
		{
			while(aggregateException.InnerExceptions[0] is AggregateException)
			{
				return UnWrap((AggregateException)aggregateException.InnerExceptions[0]);
			}
			return aggregateException.InnerExceptions[0];
		}




		void Command_Executing()
		{
			UpdateState();
		}


		void Command_Executed(CommandViewModel obj)
		{
			UpdateState();
		}

		void Command_Faulted(CommandViewModel arg1, Exception arg2, bool arg3)
		{
			UpdateState();
		}

		void Command_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			UpdateState();
		}

	}
}
