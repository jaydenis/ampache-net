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
	public partial class Lookup<TEntity> : ListActivity where TEntity : IEntity
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
			_connection = new AmpacheService.Connection();
			_connection.OnConnected += Handle_connectionOnConnected;
			BindService(new Intent(this.ApplicationContext, typeof(AmpacheService)), _connection, Bind.AutoCreate);	
			this.ListView.Enabled = true;
			this.ListView.FastScrollEnabled = true;
			this.ListView.ItemLongClick += HandleListViewhandleItemLongClick;
		}
		
		void Handle_connectionOnConnected (object sender, EventArgs e)
		{
			_model = _connection.Model;
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
			var ent = _adapter.GetItem(e.Position);
			System.Threading.ThreadPool.QueueUserWorkItem((o) => AddSongsToPlaylistFor(ent));
			Toast.MakeText(this.ApplicationContext, GetString(Resource.String.addToPlaylist), ToastLength.Short).Show();
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
			UnbindService(_connection);
		}
	}
}

