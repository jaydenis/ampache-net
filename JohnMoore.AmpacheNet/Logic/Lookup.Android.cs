//
// Lookup.Android.cs
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
using System.Linq;
using JohnMoore.AmpacheNet.Entities;
using JohnMoore.AmpacheNet.Logic;
using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Android.Content;

namespace JohnMoore.AmpacheNet.Logic
{
    public partial class Lookup<TEntity> : ListActivity, AmpacheService.IClient where TEntity : IEntity
	{
		protected ProgressDialog _prgDlg;
		protected AmpacheService.Connection _connection;
		private AmpacheArrayAdapter<TEntity> _adapter;
		
		public Lookup () : this(TimeSpan.FromMinutes(30))
		{}
		
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			SetContentView(Resource.Layout.Filter);
            _connection = new AmpacheService.Connection(this);
			BindService(new Intent(this.ApplicationContext, typeof(AmpacheService)), _connection, Bind.AutoCreate);	
			this.ListView.Enabled = true;
			this.ListView.FastScrollEnabled = true;
			this.ListView.ItemLongClick += HandleListViewhandleItemLongClick;
		}
		
		public void Connected (Demeter.Container container)
		{
            _model = container.Resolve<AmpacheModel>();
			AfterConnection();
		}	
		
		protected virtual void AfterConnection()
		{}
		
		protected void UpdateUi(IList<TEntity> entities)
		{
			if(_prgDlg != null && _prgDlg.IsShowing)
			{
				_prgDlg.Dismiss();
			}
			_adapter = new AmpacheArrayAdapter<TEntity>(HydrateEntity, this.LayoutInflater, this.ApplicationContext, Android.Resource.Layout.SimpleListItem1, entities ?? new List<TEntity>().AsReadOnly());
			this.ListAdapter = _adapter;
		}
				
		private View HydrateEntity(TEntity ent, View v)
		{
			v.FindViewById<TextView>(Android.Resource.Id.Text1).Text = ent.Name;
			return v;
		}
		
		void HandleListViewhandleItemLongClick (object sender, AdapterView.ItemLongClickEventArgs e)
		{
			var ent = _adapter[e.Position];
			System.Threading.ThreadPool.QueueUserWorkItem((o) => AddSongsToPlaylistFor(ent));
			Toast.MakeText(this.ApplicationContext, GetString(Resource.String.addToPlaylist), ToastLength.Short).Show();
		}
		void AddSongsToPlaylistFor(TEntity ent)
		{
			var sel = _container.Resolve<DataAccess.IAmpacheSelector<AmpacheSong>>();
			var res = sel.SelectBy(ent);
			_model.Playlist = new List<AmpacheSong>(_model.Playlist.Concat(res));
		}
		
		protected override void OnDestroy ()
		{
			base.OnDestroy ();
			this.ListView.ItemLongClick -= HandleListViewhandleItemLongClick;
			UnbindService(_connection);
            _prgDlg = null;
		}
	}
}

