using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Input;
using AistTrader.Commands;
using AistTrader.Models;

namespace AistTrader.ViewModels
{
    class AgentConfigViewModel : ViewModelBase, IDataErrorInfo
    {


		  private readonly AgentConfigModel currentAgentConfigModel;
		  private Dictionary<string, bool> validProperties;
		  private bool allPropertiesValid = false;

		  private DelegateCommand exitCommand;
		  private DelegateCommand saveCommand;



		  public AgentConfigViewModel(AgentConfigModel newAgentConfig)
		  {
				this.currentAgentConfigModel = newAgentConfig;
				this.validProperties = new Dictionary<string, bool>();
				this.validProperties.Add("AlgorithmName", false);
		  }



		  public string AlgorithmName
		  {
				get { return currentAgentConfigModel.AlgorithmName; }
				set
				{
					 if (currentAgentConfigModel.AlgorithmName != value)
					 {
                           
						  currentAgentConfigModel.AlgorithmName = value;
						  base.OnPropertyChanged("AlgorithmName");
					 }
				}
		  }

	

		  public bool AllPropertiesValid
		  {
				get { return allPropertiesValid; }
				set
				{
					 if (allPropertiesValid != value)
					 {
						  allPropertiesValid = value;
						  base.OnPropertyChanged("AllPropertiesValid");
					 }
				}
		  }



		  public string Error
		  {
				get { return (currentAgentConfigModel as IDataErrorInfo).Error; }
		  }

		  public string this[string propertyName]
		  {
				get 
				{
					 string error = (currentAgentConfigModel as IDataErrorInfo)[propertyName];
					 validProperties[propertyName] = String.IsNullOrEmpty(error) ? true : false;
					 ValidateProperties();
					 CommandManager.InvalidateRequerySuggested();
					 return error;
				}
		  }



		  public ICommand ExitCommand
		  {
              
				get
				{
					 if (exitCommand == null)
					 {
						  exitCommand = new DelegateCommand(Exit);
					 }
					 return exitCommand;
				}
		  }

		  public ICommand SaveCommand
		  {
				get
				{
					 if (saveCommand == null)
					 {
						  saveCommand = new DelegateCommand(Save);
					 }
					 return saveCommand;
				}
		  }



		  private void ValidateProperties()
		  {
				foreach(bool isValid in validProperties.Values)
				{
					 if (!isValid)
					 {
						  this.AllPropertiesValid = false;
						  return;
					 }
				}
				this.AllPropertiesValid = true;
		  }

		  private void Exit()
		  {


				Application.Current.Shutdown();
		  }

		  private void Save()
		  {
              currentAgentConfigModel.Save();
		  }
		  
	 }

}
