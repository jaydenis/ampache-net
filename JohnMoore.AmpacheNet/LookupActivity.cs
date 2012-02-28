//
// LookupActivity.cs
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
using System.Threading;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;

using JohnMoore.AmpacheNet.Entities;
using JohnMoore.AmpacheNet.DataAccess;
using JohnMoore.AmpacheNet.Logic;

namespace JohnMoore.AmpacheNet
{
	public abstract class LookupActivity<TEntity> : ListActivity where TEntity : IEntity
	{
		private static List<TEntity> _cachedEntities;
		private static Timer _cacheReset = new Timer((o) => _cachedEntities = null, null, Timeout.Infinite, Timeout.Infinite);
		private AmpacheModel _model;
		private ProgressDialog _prgDlg;
		private List<TEntity> _filterdEntities;
		private AmpacheService.Connection _connection;
		private AmpacheArrayAdapter<TEntity> _adapter;
		
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			SetContentView(Resource.Layout.Filter);
			_prgDlg = ProgressDialog.Show(this, GetString(Resource.String.loading), GetString(Resource.String.loading));
			_connection = new AmpacheService.Connection();
			_connection.OnConnected += Handle_connectionOnConnected;
			BindService(new Intent(this.ApplicationContext, typeof(AmpacheService)), _connection, Bind.AutoCreate);		
			this.ListView.ItemLongClick += HandleListViewhandleItemLongClick;
			this.ListView.FastScrollEnabled = true;
			FindViewById<EditText>(Resource.Id.txtFilter).TextChanged += HandleTextChanged;
			_cacheReset.Change(Timeout.Infinite, Timeout.Infinite);
		}

		void HandleTextChanged (object sender, Android.Text.TextChangedEventArgs e)
		{
			var filter = new string(e.Text.ToArray());
			if(string.IsNullOrEmpty(filter))
			{
				_filterdEntities = _cachedEntities;
			}
			else
			{
				_filterdEntities = _cachedEntities.Where(t => t.Name.ToLower().Contains(filter.ToLower())).ToList();
			}
			RunOnUiThread(() => UpdateUi());
		}

		void HandleListViewhandleItemLongClick (object sender, AdapterView.ItemLongClickEventArgs e)
		{
			var ent = _adapter.GetItem(e.Position);
			System.Threading.ThreadPool.QueueUserWorkItem((o) => AddSongsToPlaylistFor(ent));
			Toast.MakeText(this.ApplicationContext, GetString(Resource.String.addToPlaylist), ToastLength.Short).Show();
		}


		void Handle_connectionOnConnected (object sender, EventArgs e)
		{
			_model = _connection.Model;
			System.Threading.ThreadPool.QueueUserWorkItem((o) => LoadAll());
		}
		
		private void UpdateUi()
		{
			if(_prgDlg.IsShowing)
			{
				_prgDlg.Dismiss();
			}
			if(_cachedEntities == null || _filterdEntities == null)
			{
				return;
			}
			_adapter = new AmpacheArrayAdapter<TEntity>(HydrateEntity, this.LayoutInflater, this.ApplicationContext, Android.Resource.Layout.SimpleListItem1, _filterdEntities);
			this.ListAdapter = _adapter;
		}
		
		
		private void LoadAll()
		{
			try
			{
				if(_model.Factory == null)
				{
					RunOnUiThread(() => Toast.MakeText(ApplicationContext, GetString(Resource.String.configureRequest), ToastLength.Short).Show());
					Finish();
				}
				else if(_cachedEntities == null)
				{
					var selecter = _model.Factory.GetInstanceSelectorFor<TEntity>();
					_cachedEntities = selecter.SelectAll().OrderBy(e => e.Name).ToList();
				}
			}
			catch(Exception ex)
			{
				Console.WriteLine(ex.Message);
				Console.WriteLine(ex.StackTrace);
				RunOnUiThread(() => Toast.MakeText(this.ApplicationContext, ex.Message, ToastLength.Short).Show());
				_cachedEntities = null;
			}
			_filterdEntities = (_cachedEntities ?? Enumerable.Empty<TEntity>()).ToList();
			RunOnUiThread(() => UpdateUi());
		}
				
		private View HydrateEntity(TEntity ent, View v)
		{
			v.FindViewById<TextView>(Android.Resource.Id.Text1).Text = ent.Name;
			return v;
		}

		void AddSongsToPlaylistFor(TEntity ent)
		{
			var sel = _model.Factory.GetInstanceSelectorFor<AmpacheSong>();
			var res = sel.SelectBy(ent);
			_model.Playlist = new List<AmpacheSong>(_model.Playlist.Concat(res));
		}
		
		protected override void OnDestroy ()
		{
			base.OnDestroy ();
			this.ListView.ItemLongClick -= HandleListViewhandleItemLongClick;
			FindViewById<EditText>(Resource.Id.txtFilter).TextChanged -= HandleTextChanged;
			UnbindService(_connection);
			_cacheReset.Change(TimeSpan.FromMinutes(60), TimeSpan.FromMilliseconds(Timeout.Infinite));
		}
	}
	#region Activity Classes
	
	[Activity (Label = "@string/lookupLabel",Theme="@android:style/Theme.Dialog")]		
	public class PlaylistLookupActivity : LookupActivity<AmpachePlaylist> {}
	
	[Activity (Label = "@string/lookupLabel",Theme="@android:style/Theme.Dialog")]		
	public class AlbumLookupActivity : LookupActivity<AmpacheAlbum> {}
	
	[Activity (Label = "@string/lookupLabel",Theme="@android:style/Theme.Dialog")]		
	public class ArtistLookupActivity : LookupActivity<AmpacheArtist> {}
	
	#endregion
}

