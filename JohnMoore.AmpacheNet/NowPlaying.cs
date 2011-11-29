//
// NowPlaying.cs
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
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using JohnMoore.AmpacheNet.Logic;

namespace JohnMoore.AmpacheNet
{
	[Activity (Label = "Now Playing")]			
	public class NowPlaying : Activity
	{
		private AmpacheModel _model;
		private AmpacheService.Connection _connection;
		private Android.Graphics.Bitmap _currentAlbumArt;
		
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			SetContentView(Resource.Layout.NowPlaying);
			var ori = WindowManager.DefaultDisplay.Rotation;
			if(ori == SurfaceOrientation.Rotation0 || ori == SurfaceOrientation.Rotation180)
			{
				FindViewById<LinearLayout>(Resource.Id.mainLayout).Orientation = Orientation.Vertical;
			}
			_connection = new AmpacheService.Connection();
			_connection.OnConnected += Handle_connectionOnConnected;
			BindService(new Intent(this, typeof(AmpacheService)), _connection, Bind.AutoCreate);
		}

		void Handle_connectionOnConnected (object sender, EventArgs e)
		{
			_model = _connection.Model;
			_model.PropertyChanged += Handle_connectionServicePropertyChanged;
			FindViewById<ImageButton>(Resource.Id.imgPlayingNext).Click += HandleNextClick;
			FindViewById<ImageButton>(Resource.Id.imgPlayingPrev).Click += HandlePreviousClick;
			FindViewById<ImageButton>(Resource.Id.imgPlayingPlayPause).Click += HandlePlayClick;
			FindViewById<ImageButton>(Resource.Id.imgPlayingShuffle).Click += HandleShuffleClick;
			RunOnUiThread(() => FindViewById<ImageView>(Resource.Id.imgPlayingAlbumArt).SetImageResource(Resource.Drawable.icon_thumbnail));			
			if(_model.IsPlaying)
			{
				RunOnUiThread(() => FindViewById<ImageButton>(Resource.Id.imgPlayingPlayPause).SetImageDrawable(Resources.GetDrawable(Resource.Drawable.ic_media_pause)));
				RunOnUiThread(() => UpdateUi());
				RunOnUiThread(() => UpdateArt());
			}
		}
		
		protected override void OnDestroy ()
		{
			base.OnDestroy ();
			FindViewById<ImageButton>(Resource.Id.imgPlayingNext).Click -= HandleNextClick;
			FindViewById<ImageButton>(Resource.Id.imgPlayingPrev).Click -= HandlePreviousClick;
			FindViewById<ImageButton>(Resource.Id.imgPlayingPlayPause).Click -= HandlePlayClick;
			FindViewById<ImageButton>(Resource.Id.imgPlayingShuffle).Click -= HandleShuffleClick;
			UnbindService(_connection);
			_connection.Dispose();
		}
		
		void Handle_connectionServicePropertyChanged (object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == AmpacheModel.PLAYING_SONG)
			{
				RunOnUiThread(() => UpdateUi());
			}
			if(e.PropertyName == AmpacheModel.ALBUM_ART_STREAM)
			{
				RunOnUiThread(() => UpdateArt());
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
		
		void UpdateUi()
		{
			if(_model.PlayingSong == null)
			{
				FindViewById<TextView>(Resource.Id.lblNowPlayingTrack).Text = string.Empty;
				FindViewById<TextView>(Resource.Id.lblNowPlayingAlbum).Text = string.Empty;
				FindViewById<TextView>(Resource.Id.lblNowPlayingArtist).Text = string.Empty;
			}
			else
			{
				FindViewById<TextView>(Resource.Id.lblNowPlayingTrack).Text = _model.PlayingSong.Name;
				FindViewById<TextView>(Resource.Id.lblNowPlayingAlbum).Text = _model.PlayingSong.AlbumName;
				FindViewById<TextView>(Resource.Id.lblNowPlayingArtist).Text = _model.PlayingSong.ArtistName;
			}
		}
		
		void HandleShuffleClick (object sender, EventArgs e)
		{
			System.Threading.ThreadPool.QueueUserWorkItem((o) => _model.Shuffling = !_model.Shuffling);
		}

		void HandlePlayClick (object sender, EventArgs e)
		{
			if(_model.PlayingSong == null)
			{
				return;
			}
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
	}
}

