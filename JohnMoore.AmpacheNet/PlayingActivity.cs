//
// PlayingActivity.cs
//
// Author:
//       John Moore <jcwmoore@gmail.com>
//
// Copyright (c) 2012 John Moore
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

using Android.App;
using Android.Content;
using Android.OS;
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
		
		protected override void OnResume()
		{
			base.OnResume();
			if(_model == null)
			{
				BindService(new Intent(this.ApplicationContext, typeof(AmpacheService)), _connection, Bind.AutoCreate);
			}
			else
			{
				_model.PropertyChanged += Handle_modelPropertyChanged;
				OnModelLoaded();
				UpdateArt();
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
			if(!_uiActions.ContainsKey(AmpacheModel.USER_MESSAGE))
			{
				_uiActions.Add(AmpacheModel.USER_MESSAGE, DisplayMessage);
			}
			_model.PropertyChanged += Handle_modelPropertyChanged;
			_model.PropertyChanged += ModelDisposed;
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
		
		void DisplayMessage()
		{
			if(!string.IsNullOrEmpty(_model.UserMessage))
			{
				Toast.MakeText(this, _model.UserMessage, ToastLength.Long).Show();
				_model.UserMessage = null;
			}
		}
		
		void UpdateArt()
		{
			if(_currentAlbumArt != null)
			{
				_currentAlbumArt.Recycle();
				_currentAlbumArt.Dispose();
			}
			if(_model.AlbumArtStream != null)
			{
				Console.WriteLine ("Change Art");
				_currentAlbumArt = Android.Graphics.BitmapFactory.DecodeByteArray(_model.AlbumArtStream.ToArray(), 0, (int)_model.AlbumArtStream.Length);
				FindViewById<ImageView>(Resource.Id.imgPlayingAlbumArt).SetImageBitmap(_currentAlbumArt);
			}
		}
		
		void UpdateNextButton()
		{
			var view = FindViewById<ImageButton>(Resource.Id.imgPlayingNext);
			if(view != null){
				if(_model.NextRequested)
				{ 
					view.SetImageDrawable(Resources.GetDrawable(Resource.Drawable.ic_media_next_invert));
				}
				else
				{
					view.SetImageDrawable(Resources.GetDrawable(Resource.Drawable.ic_media_next));
				}
			}
		}
		
		void UpdatePreviousButton()
		{
			var view = FindViewById<ImageButton>(Resource.Id.imgPlayingPrevious);
			if(view != null){
				if(_model.PreviousRequested)
				{
					view.SetImageDrawable(Resources.GetDrawable(Resource.Drawable.ic_media_previous_invert));
				}
				else
				{
					view.SetImageDrawable(Resources.GetDrawable(Resource.Drawable.ic_media_previous));
				}
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
			var img = FindViewById<ImageButton>(Resource.Id.imgPlayingShuffle);
			if(img != null){
				if(_model.Shuffling)
				{
					img.SetImageDrawable(Resources.GetDrawable(Resource.Drawable.ic_menu_shuffle_invert));
				}
				else
				{
					img.SetImageDrawable(Resources.GetDrawable(Resource.Drawable.ic_menu_shuffle));
				}
			}
		}
		void HandleShuffleClick (object sender, EventArgs e)
		{
			Task.Factory.StartNew(() => _model.Shuffling = !_model.Shuffling);
		}

		void HandlePlayClick (object sender, EventArgs e)
		{
			Task.Factory.StartNew(() => _model.PlayPauseRequested = true);
		}

		void HandlePreviousClick (object sender, EventArgs e)
		{
			Task.Factory.StartNew(() => _model.PreviousRequested = true);
		}

		void HandleNextClick (object sender, EventArgs e)
		{
			Task.Factory.StartNew(() => _model.NextRequested = true);
		}
		
		void ModelDisposed(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == AmpacheModel.IS_DISPOSED && _model.IsDisposed)
			{
				Finish();
			}
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
				case Resource.Id.help:
					var intent = new Intent(Intent.ActionView);
					intent.SetData(Android.Net.Uri.Parse("https://gitorious.org/ampache-net/pages/Android"));
					StartActivity(intent);
					break;
				case Resource.Id.clearPlaylist:
					Task.Factory.StartNew(() => _model.Playlist = new List<AmpacheSong>());
					break;
				case Resource.Id.search:
					StartActivity(typeof(SongSearch));
					break;
				default:
					// unknown
					return false;	
			}
			return true;
		}
		
		public override bool OnSearchRequested ()
		{
			StartActivity(typeof(SongSearch));
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
			if (_currentAlbumArt != null)
			{
				_currentAlbumArt.Recycle ();
				_currentAlbumArt.Dispose ();
				_currentAlbumArt = null;
			}
			UnbindService(_connection);
			_connection.Dispose();
			_connection = null;
		}
	}
}