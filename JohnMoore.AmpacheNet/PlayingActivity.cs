using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using JohnMoore.AmpacheNet.Entities;
using JohnMoore.AmpacheNet.Logic;

namespace JohnMoore.AmpacheNet
{
	public abstract class PlayingActivity : Activity
	{	
		protected AmpacheModel _model;
		protected readonly Dictionary<string, Action> _uiActions = new Dictionary<string, Action>();
		private AmpacheService.Connection _connection;
		private Android.Graphics.Bitmap _currentAlbumArt;
		
		protected override void OnCreate (Bundle savedInstanceState)
		{
			base.OnCreate (savedInstanceState);
			_connection = new AmpacheService.Connection();
			_connection.OnConnected += OnServiceConnected;
			StartService(new Intent(this.ApplicationContext, typeof(AmpacheService)));
		}
		
		protected override void OnStart ()
		{
			base.OnStart ();
			if(_model == null)
			{
				BindService(new Intent(this.ApplicationContext, typeof(AmpacheService)), _connection, Bind.AutoCreate);
			}
			else
			{
				_model.PropertyChanged += Handle_modelPropertyChanged;
				OnModelLoaded();
			}
		}
		
		private void OnServiceConnected (object sender, EventArgs e)
		{
			_model = _connection.Model;
			if(!_uiActions.ContainsKey(AmpacheModel.ALBUM_ART_STREAM))
			{
				_uiActions.Add(AmpacheModel.ALBUM_ART_STREAM, UpdateArt);
				RunOnUiThread(() => UpdateArt());
			}
			if(!_uiActions.ContainsKey(AmpacheModel.NEXT_REQUESTED))
			{
				_uiActions.Add(AmpacheModel.NEXT_REQUESTED, UpdateNextButton);
				RunOnUiThread(() => UpdateNextButton());
			}
			if(!_uiActions.ContainsKey(AmpacheModel.PREVIOUS_REQUESTED))
			{
				_uiActions.Add(AmpacheModel.PREVIOUS_REQUESTED, UpdatePreviousButton);
				RunOnUiThread(() => UpdatePreviousButton());
			}
			if(!_uiActions.ContainsKey(AmpacheModel.SHUFFELING))
			{
				_uiActions.Add(AmpacheModel.SHUFFELING, UpdateShuffleButton);
				RunOnUiThread(() => UpdateShuffleButton());
			}
			if(!_uiActions.ContainsKey(AmpacheModel.PLAY_PAUSE_REQUESTED))
			{
				_uiActions.Add(AmpacheModel.PLAY_PAUSE_REQUESTED, UpdatePlayPauseButton);
				RunOnUiThread(() => UpdatePlayPauseButton());
			}
			_model.PropertyChanged += Handle_modelPropertyChanged;
			ImageButton btn = FindViewById<ImageButton>(Resource.Id.imgPlayingNext);
			if(btn != null)
			{
				btn.Click += HandleNextClick;
			}
			btn = FindViewById<ImageButton>(Resource.Id.imgPlayingPrevious);
			if(btn != null)
			{
				btn.Click += HandlePreviousClick;
			}
			btn = FindViewById<ImageButton>(Resource.Id.imgPlayingPlayPause);
			if(btn != null)
			{
				btn.Click += HandlePlayClick;
			}
			btn = FindViewById<ImageButton>(Resource.Id.imgPlayingShuffle);
			if(btn != null)
			{
				btn.Click += HandleShuffleClick;
			}
			OnModelLoaded();
		}

		void Handle_modelPropertyChanged (object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(_uiActions.ContainsKey(e.PropertyName))
			{
				RunOnUiThread(() => _uiActions[e.PropertyName].Invoke());
			}
		}
		
		protected virtual void OnModelLoaded()
		{			
		}
		
		void UpdateArt()
		{
			if(_currentAlbumArt != null)
			{
				_currentAlbumArt.Dispose();
			}
			Console.WriteLine ("Change Art");
			_currentAlbumArt = Android.Graphics.BitmapFactory.DecodeByteArray(_model.AlbumArtStream.ToArray(), 0, (int)_model.AlbumArtStream.Length);
			 FindViewById<ImageView>(Resource.Id.imgPlayingAlbumArt).SetImageBitmap(_currentAlbumArt);
		}
		
		void UpdateNextButton()
		{
			if(_model.NextRequested)
			{ 
				FindViewById<ImageButton>(Resource.Id.imgPlayingNext).SetImageDrawable(Resources.GetDrawable(Resource.Drawable.ic_media_next_invert));
			}
			else
			{
				FindViewById<ImageButton>(Resource.Id.imgPlayingNext).SetImageDrawable(Resources.GetDrawable(Resource.Drawable.ic_media_next));
			}
		}
		
		void UpdatePreviousButton()
		{
			if(_model.PreviousRequested)
			{
				FindViewById<ImageButton>(Resource.Id.imgPlayingPrevious).SetImageDrawable(Resources.GetDrawable(Resource.Drawable.ic_media_previous_invert));
			}
			else
			{
				FindViewById<ImageButton>(Resource.Id.imgPlayingPrevious).SetImageDrawable(Resources.GetDrawable(Resource.Drawable.ic_media_previous));
			}
		}
		
		void UpdatePlayPauseButton()
		{
			if(_model.IsPlaying)
			{
				FindViewById<ImageButton>(Resource.Id.imgPlayingPlayPause).SetImageDrawable(Resources.GetDrawable(Resource.Drawable.ic_media_pause));
			}
			else
			{
				FindViewById<ImageButton>(Resource.Id.imgPlayingPlayPause).SetImageDrawable(Resources.GetDrawable(Resource.Drawable.ic_media_play));
			}
		}
		
		void UpdateShuffleButton()
		{
			if(_model.Shuffling)
			{
				FindViewById<ImageButton>(Resource.Id.imgPlayingShuffle).SetImageDrawable(Resources.GetDrawable(Resource.Drawable.ic_menu_shuffle_invert));
			}
			else
			{
				FindViewById<ImageButton>(Resource.Id.imgPlayingShuffle).SetImageDrawable(Resources.GetDrawable(Resource.Drawable.ic_menu_shuffle));
			}
		}
		void HandleShuffleClick (object sender, EventArgs e)
		{
			System.Threading.ThreadPool.QueueUserWorkItem((o) => _model.Shuffling = !_model.Shuffling);
		}

		void HandlePlayClick (object sender, EventArgs e)
		{
			if(_model.IsPlaying)
			{
				FindViewById<ImageButton>(Resource.Id.imgPlayingPlayPause).SetImageDrawable(Resources.GetDrawable(Resource.Drawable.ic_media_play));
			}
			else
			{
				FindViewById<ImageButton>(Resource.Id.imgPlayingPlayPause).SetImageDrawable(Resources.GetDrawable(Resource.Drawable.ic_media_pause));
			}
			System.Threading.ThreadPool.QueueUserWorkItem((o) => _model.PlayPauseRequested = true);
		}

		void HandlePreviousClick (object sender, EventArgs e)
		{
			System.Threading.ThreadPool.QueueUserWorkItem((o) => _model.PreviousRequested = true);
		}

		void HandleNextClick (object sender, EventArgs e)
		{
			System.Threading.ThreadPool.QueueUserWorkItem((o) => _model.NextRequested = true);
		}
		
		public override bool OnCreateOptionsMenu (IMenu menu)
		{
			this.MenuInflater.Inflate(Resource.Menu.MainMenu, menu);
			return true;
		}
		
		public override bool OnOptionsItemSelected (IMenuItem item)
		{
			switch (item.ItemId)
			{
				case Resource.Id.configure:
					StartActivity(typeof(ConfigurationActivity));
					break;
				case Resource.Id.albums:
					StartActivity(typeof(AlbumLookupActivity));
					break;
				case Resource.Id.artists:
					StartActivity(typeof(ArtistLookupActivity));
					break;
				case Resource.Id.playlists:
					StartActivity(typeof(PlaylistLookupActivity));
					break;
				case Resource.Id.exit:
					if(_model.PlayingSong == null)
					{
						StopService(new Intent(this.ApplicationContext, typeof(AmpacheService)));
					}
					Finish();
					break;
				case Resource.Id.clearPlaylist:
					System.Threading.ThreadPool.QueueUserWorkItem((o) => _model.Playlist = new List<AmpacheSong>());
					break;
				default:
					// unknown
					return false;						
			}
			return true;
		}
			
		protected override void OnStop ()
		{
			base.OnStop ();
			if(_model != null)
			{
				_model.PropertyChanged -= Handle_modelPropertyChanged;
			}
		}
		
		protected override void OnDestroy ()
		{
			base.OnDestroy ();
			Console.WriteLine ("Activity Destroyed");
			ImageButton btn = FindViewById<ImageButton>(Resource.Id.imgPlayingNext);
			if(btn != null)
			{
				btn.Click -= HandleNextClick;
			}
			btn = FindViewById<ImageButton>(Resource.Id.imgPlayingPrevious);
			if(btn != null)
			{
				btn.Click -= HandlePreviousClick;
			}
			btn = FindViewById<ImageButton>(Resource.Id.imgPlayingPlayPause);
			if(btn != null)
			{
				btn.Click -= HandlePlayClick;
			}
			btn = FindViewById<ImageButton>(Resource.Id.imgPlayingShuffle);
			if(btn != null)
			{
				btn.Click -= HandleShuffleClick;
			}
			UnbindService(_connection);
			_connection.Dispose();
		}
	}
}