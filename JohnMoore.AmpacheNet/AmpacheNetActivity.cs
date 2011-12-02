//
// AmpacheNetActivity.cs
//
// Author:
//       John Moore <jcwmoore@gmail.com>
//
// Copyright (c) 2011 John Moore
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
using System.Linq;
using Android.App;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Android.OS;
using JohnMoore.AmpacheNet.Entities;
using JohnMoore.AmpacheNet.DataAccess;
using JohnMoore.AmpacheNet.Logic;

namespace JohnMoore.AmpacheNet
{
	[Activity (Label = "Ampache.NET", MainLauncher = true)]
	public class AmpacheNetActivity : Activity
	{
		private AmpacheModel _model;
		private AmpacheService.Connection _connection;
		private Android.Graphics.Bitmap _currentAlbumArt;
		
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			SetContentView(Resource.Layout.Main);
 			var ori = WindowManager.DefaultDisplay.Rotation;
			if(ori == SurfaceOrientation.Rotation0 || ori == SurfaceOrientation.Rotation180)
			{
				FindViewById<LinearLayout>(Resource.Id.mainLayout).Orientation = Orientation.Vertical;
			}
			StartService(new Intent(ApplicationContext, typeof(AmpacheService)));
			_connection = new AmpacheService.Connection();
			_connection.OnConnected += Handle_connectionOnConnected;
			FindViewById<ListView>(Resource.Id.lstPlaylist).ItemClick += HandleItemSelected;
			FindViewById<ImageButton>(Resource.Id.imgPlayingNext).Click += HandleNextClick;
			FindViewById<ImageButton>(Resource.Id.imgPlayingPrev).Click += HandlePreviousClick;
			FindViewById<ImageButton>(Resource.Id.imgPlayingPlayPause).Click += HandlePlayClick;
			FindViewById<ImageButton>(Resource.Id.imgPlayingShuffle).Click += HandleShuffleClick;
			FindViewById<ImageView>(Resource.Id.imgPlayingAlbumArt).Click += HandleImageClick;
		}
		
		protected override void OnStop ()
		{
			base.OnStop ();
			_model.PropertyChanged -= Handle_modelPropertyChanged;
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
				SetupUiFromModel();
			}
		}
		
		void Handle_connectionOnConnected (object sender, EventArgs e)
		{
			_model = _connection.Model;
			SetupUiFromModel();
		}
		
		void SetupUiFromModel()
		{
			_model.PropertyChanged += Handle_modelPropertyChanged;			
			PopulateSongs();
			UpdateArt();
		}

		void HandleImageClick (object sender, EventArgs e)
		{
			StartActivity(typeof(NowPlaying));
		}

		void HandleShuffleClick (object sender, EventArgs e)
		{
			System.Threading.ThreadPool.QueueUserWorkItem((o) => _model.Shuffling = !_model.Shuffling);
		}

		void HandlePlayClick (object sender, EventArgs e)
		{
			System.Threading.ThreadPool.QueueUserWorkItem((o) => _model.PlayPauseRequested = true);
		}

		void HandlePreviousClick (object sender, EventArgs e)
		{
			FindViewById<ImageButton>(Resource.Id.imgPlayingNext).SetImageDrawable(Resources.GetDrawable(Resource.Drawable.ic_media_previous_invert));
			System.Threading.ThreadPool.QueueUserWorkItem((o) => _model.PreviousRequested = true);
		}

		void HandleNextClick (object sender, EventArgs e)
		{
			FindViewById<ImageButton>(Resource.Id.imgPlayingNext).SetImageDrawable(Resources.GetDrawable(Resource.Drawable.ic_media_next_invert));
			System.Threading.ThreadPool.QueueUserWorkItem((o) => _model.NextRequested = true);
		}

		void HandleItemSelected (object sender, ItemEventArgs e)
		{
			var lv = FindViewById<ListView>(Resource.Id.lstPlaylist);
			lv.SetSelection(e.Position);
			lv.SmoothScrollToPosition(e.Position);
			if(!_model.IsPlaying)
			{
				var adp = lv.Adapter as AmpacheArrayAdapter<AmpacheSong>;
				var sng = adp.GetItem(e.Position);
				_model.Playlist = new List<AmpacheSong>(_model.Playlist.SkipWhile(s=> s != sng).Concat(_model.Playlist.TakeWhile(s => s != sng)));
				_model.PlayPauseRequested = true;
				FindViewById<ImageButton>(Resource.Id.imgPlayingPlayPause).SetImageDrawable(Resources.GetDrawable(Resource.Drawable.ic_media_pause));
			}
		}
		
		void Handle_modelPropertyChanged (object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			RunOnUiThread(() => PropertyChanged(e.PropertyName));
		}
		
		void PropertyChanged(string PropertyName)
		{
			switch(PropertyName)
			{
				case AmpacheModel.FACTORY:
					Toast.MakeText(this.ApplicationContext, "Connected to Ampache", ToastLength.Short).Show();
					break;
				case AmpacheModel.PLAYING_SONG:
					ChangeSong();
					break;
				case AmpacheModel.PLAYLIST:
					PopulateSongs();
					break;
				case AmpacheModel.ALBUM_ART_STREAM:
					UpdateArt();
					break;
				case AmpacheModel.NEXT_REQUESTED:
					UpdateNextButton();
					break;
				case AmpacheModel.PREVIOUS_REQUESTED:
					UpdatePreviousButton();
					break;
				case AmpacheModel.SHUFFELING:
					UpdateShuffleButton();
					break;
			}
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
		
		void PopulateSongs()
		{
			var adp = new AmpacheArrayAdapter<AmpacheSong>(HydrateSong, this.LayoutInflater, this, Android.Resource.Layout.SimpleListItem1, _model.Playlist);
			FindViewById<ListView>(Resource.Id.lstPlaylist).Adapter = adp;
		}
		
		void ChangeSong()
		{
			if(_model.PlayingSong == null)
			{
				FindViewById<ImageButton>(Resource.Id.imgPlayingPlayPause).SetImageDrawable(Resources.GetDrawable(Resource.Drawable.ic_media_pause));
				return;
			}
			var lv = FindViewById<ListView>(Resource.Id.lstPlaylist);
			var adp = lv.Adapter as AmpacheArrayAdapter<AmpacheSong>;
			lv.SetSelection(adp.GetPosition(_model.PlayingSong));
		}
		
		protected override void OnDestroy ()
		{
			base.OnDestroy ();
			_connection.OnConnected -= Handle_connectionOnConnected;
			FindViewById<ListView>(Resource.Id.lstPlaylist).ItemClick -= HandleItemSelected;
			FindViewById<ImageButton>(Resource.Id.imgPlayingNext).Click -= HandleNextClick;
			FindViewById<ImageButton>(Resource.Id.imgPlayingPrev).Click -= HandlePreviousClick;
			FindViewById<ImageButton>(Resource.Id.imgPlayingPlayPause).Click -= HandlePlayClick;
			FindViewById<ImageView>(Resource.Id.imgPlayingAlbumArt).Click -= HandleImageClick;
			if (!_model.IsPlaying) 
			{ 
				StopService(new Intent(ApplicationContext, typeof(AmpacheService)));
			}
			UnbindService(_connection);
			_connection.Dispose();
			_connection = null;
		}
		
		private View HydrateSong(AmpacheSong song, View v)
		{
			v.FindViewById<TextView>(Android.Resource.Id.Text1).Text = song.Name;
			return v;
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
					var intab = new Intent(this.ApplicationContext, typeof(LookupActivity));
					intab.PutExtra(LookupActivity.TYPE, Resource.Id.albums);
					StartActivity(intab);
					break;
				case Resource.Id.artists:
					var intar = new Intent(this.ApplicationContext, typeof(LookupActivity));
					intar.PutExtra(LookupActivity.TYPE, Resource.Id.artists);
					StartActivity(intar);
					break;
				case Resource.Id.playlists:
					var intp = new Intent(this.ApplicationContext, typeof(LookupActivity));
					intp.PutExtra(LookupActivity.TYPE, Resource.Id.playlists);
					StartActivity(intp);
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
				FindViewById<ImageButton>(Resource.Id.imgPlayingNext).SetImageDrawable(Resources.GetDrawable(Resource.Drawable.ic_media_previous_invert));
			}
			else
			{
				FindViewById<ImageButton>(Resource.Id.imgPlayingNext).SetImageDrawable(Resources.GetDrawable(Resource.Drawable.ic_media_previous));
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
	}
}
