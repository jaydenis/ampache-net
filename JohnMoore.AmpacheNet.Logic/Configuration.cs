using System;
using JohnMoore.AmpacheNet.Entities;
using JohnMoore.AmpacheNet.DataAccess;

namespace JohnMoore.AmpacheNet.Logic
{
	public partial class Configuration
	{
		protected AmpacheModel _model;
		protected string _successMessage;
		
		public Configuration (AmpacheModel model)
		{
			_model = model;
		}
		
		public void PerformTest(string server, string user, string password)
		{
			if(_model != null)
			{
				try 
				{
					var tmp = _model.Factory.AuthenticationTest(server, user, password);
					_model.UserMessage = _successMessage ?? "Success";
				}
				catch (Exception ex) 
				{
					_model.UserMessage = ex.Message;
				}
			}
		}
		
		public bool TrySaveConfiguration(UserConfiguration config)
		{
			if(_model != null && config != null)
			{
				try 
				{
					var tmp = _model.Factory.AuthenticateToServer(config.ServerUrl, config.User, config.Password);
					_model.UserMessage = _successMessage ?? "Success";
					_model.Configuration = config;
					return true;
				}
				catch (Exception ex)
				{
					_model.UserMessage = ex.Message;
				}
			}
			return false;
		}
	}
}