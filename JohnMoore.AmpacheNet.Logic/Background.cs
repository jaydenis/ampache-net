using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using JohnMoore.AmpacheNet.Entities;
using JohnMoore.AmpacheNet.DataAccess;

namespace JohnMoore.AmpacheNet.Logic
{
	public abstract partial class Background
	{
		protected static AmpacheModel _model;
		protected static AlbumArtLoader _loader;
		protected string _artCachePath;
		protected string _successConnectionMessage;
		
		public virtual void Start(MemoryStream defaultArtStream)
		{
			if(defaultArtStream == null)
			{
				throw new ArgumentNullException("defaultArtStream");
			}
			_model = new AmpacheModel();
			var tmpConfig = LoadPersistedConfiguration();
			if(tmpConfig == null)
			{
				tmpConfig = new UserConfiguration();
				tmpConfig.AllowSeeking = true;
				tmpConfig.CacheArt = true;
				tmpConfig.Password = string.Empty;
				tmpConfig.User = string.Empty;
				tmpConfig.ServerUrl = string.Empty;
			}
			_model.Configuration = tmpConfig;
			AmpacheSelectionFactory.ArtLocalDirectory = _artCachePath ?? "Art";
			_loader = new AlbumArtLoader(_model, defaultArtStream);
			
			_model.Factory = CreateFactory();
			if (_model.Configuration.ServerUrl != string.Empty) 
			{
				var task = new Task(() => _model.Factory.AuthenticateToServer(_model.Configuration));
				task.ContinueWith((t) => _model.UserMessage = t.Exception.InnerExceptions.First().Message, TaskContinuationOptions.OnlyOnFaulted)
					.ContinueWith((t) => _model.PropertyChanged += Handle_modelPropertyChanged, TaskContinuationOptions.NotOnCanceled);
				task.ContinueWith((t) => _model.UserMessage = _successConnectionMessage, TaskContinuationOptions.NotOnFaulted)
					.ContinueWith((t) => _model.Playlist = LoadPersistedSongs(), TaskContinuationOptions.NotOnCanceled)
					.ContinueWith((t) => _model.PropertyChanged += Handle_modelPropertyChanged, TaskContinuationOptions.NotOnCanceled);
				task.Start();
			}
			else
			{
				_model.PropertyChanged += Handle_modelPropertyChanged;
			}
			StartAutoShutOff();
		}

		void Handle_modelPropertyChanged (object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			switch(e.PropertyName)
			{
				case AmpacheModel.IS_DISPOSED:
					if(_model.IsDisposed) Stop ();
					break;
				case AmpacheModel.CONFIGURATION:
					if(_model.Configuration != null) PersistUserConfig (_model.Configuration);
					break;
				case AmpacheModel.PLAYLIST:
					if(_model.Playlist != null) PersistSongs (_model.Playlist);
					break;
				case AmpacheModel.IS_PLAYING:
					if(_model.IsPlaying) StopAutoShutOff ();
					else StartAutoShutOff ();
					break;
			}
		}
		
		public virtual void Stop()
		{
			_model.PropertyChanged -= Handle_modelPropertyChanged;
			PlatformFinalize();
		}
		
		// TODO: use a file for user configuration so this operation is not platform dependent
		public abstract UserConfiguration LoadPersistedConfiguration();
		// TODO: use a file for loading saved playlist so this operation is not platform dependent
		public abstract List<AmpacheSong> LoadPersistedSongs();
		// TODO: use a file for user configuration so this operation is not platform dependent
		public abstract void PersistUserConfig(UserConfiguration config);
		// TODO: use a file for loading saved playlist so this operation is not platform dependent
		public abstract void PersistSongs(IList<AmpacheSong> songs);
		public abstract void PlatformFinalize();
		public abstract void StartAutoShutOff();
		public abstract void StopAutoShutOff();
		
		public virtual AmpacheSelectionFactory CreateFactory()
		{
			return new AmpacheSelectionFactory();
		}
	}
}

