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
using System.Threading.Tasks;

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
	[MetaData("android.app.default_searchable", Value = ".SongSearch")]
	public class AmpacheNetActivity : PlayingActivity
	{		
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			SetContentView(Resource.Layout.Main);
			if(WindowManager.DefaultDisplay.Height > WindowManager.DefaultDisplay.Width)
			{
				FindViewById<LinearLayout>(Resource.Id.mainLayout).Orientation = Orientation.Vertical;
			}
			FindViewById<ListView>(Resource.Id.lstPlaylist).ItemClick += HandleItemSelected;
			FindViewById<ListView>(Resource.Id.lstPlaylist).ItemLongClick += HandleItemLongClick;
			FindViewById<ImageView>(Resource.Id.imgPlayingAlbumArt).Click += HandleImageClick;
			_uiActions.Add(AmpacheModel.PLAYING_SONG, ChangeSong);
			_uiActions.Add(AmpacheModel.PLAYLIST, PopulateSongs);
		}

		void HandleItemLongClick (object sender, AdapterView.ItemLongClickEventArgs e)
		{
			var lv = FindViewById<ListView>(Resource.Id.lstPlaylist);
			lv.SetSelection(e.Position);
			lv.SmoothScrollToPosition(e.Position);
			var adp = lv.Adapter as AmpacheArrayAdapter<AmpacheSong>;
			var sng = adp[e.Position];
			Task.Factory.StartNew(() => _model.StopRequested = true)
						.ContinueWith((t) => _model.PlayingSong = sng)
						.ContinueWith((t) => _model.PlayPauseRequested = true);
		}
		
		protected override void OnModelLoaded ()
		{
			base.OnModelLoaded ();
			RunOnUiThread(() => PopulateSongs());
			RunOnUiThread(() => ChangeSong());
		}

		void HandleImageClick (object sender, EventArgs e)
		{
			StartActivity(typeof(NowPlaying));
		}

		void HandleItemSelected (object sender, AdapterView.ItemClickEventArgs e)
		{
			var lv = FindViewById<ListView>(Resource.Id.lstPlaylist);
			lv.SetSelection(e.Position);
			lv.SmoothScrollToPosition(e.Position);
			if(!_model.IsPlaying)
			{
				var adp = lv.Adapter as AmpacheArrayAdapter<AmpacheSong>;
				var sng = adp[e.Position];
				Task.Factory.StartNew(() => _model.StopRequested = true)
							.ContinueWith((t) => _model.PlayingSong = sng)
							.ContinueWith((t) => _model.PlayPauseRequested = true);
			}
		}
		
		void PopulateSongs()
		{
			var adp = new AmpacheArrayAdapter<AmpacheSong>(HydrateSong, this.LayoutInflater, this.ApplicationContext, Android.Resource.Layout.SimpleListItem1, _model.Playlist);
			FindViewById<ListView>(Resource.Id.lstPlaylist).Adapter = adp;
		}
		
		void ChangeSong()
		{
			if(_model.PlayingSong == null)
			{
				return;
			}
			var lv = FindViewById<ListView>(Resource.Id.lstPlaylist);
			var adp = lv.Adapter as AmpacheArrayAdapter<AmpacheSong>;
			lv.SetSelection(adp.GetPosition(_model.PlayingSong));
			var lbl = FindViewById<TextView>(Resource.Id.lblNowPlayingTrack);
			if(lbl != null){
				lbl.Text = _model.PlayingSong.Name;
			}
			lbl = FindViewById<TextView>(Resource.Id.lblNowPlayingAlbum);
			if(lbl != null){
				lbl.Text = string.Format("{0} - {1}", _model.PlayingSong.AlbumName, _model.PlayingSong.ArtistName);
			}
		}
		
		protected override void OnDestroy ()
		{
			base.OnDestroy ();
			FindViewById<ListView>(Resource.Id.lstPlaylist).ItemClick -= HandleItemSelected;
			FindViewById<ListView>(Resource.Id.lstPlaylist).ItemLongClick -= HandleItemLongClick;
			FindViewById<ImageView>(Resource.Id.imgPlayingAlbumArt).Click -= HandleImageClick;
			
		}
		
		private View HydrateSong(AmpacheSong song, View v)
		{
			if(song != null){
				v.FindViewById<TextView>(Android.Resource.Id.Text1).Text = song.Name;
			}
			return v;
		}
	}
}
