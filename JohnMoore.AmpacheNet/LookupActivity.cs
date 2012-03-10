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
using System.Threading.Tasks;

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
	public class LookupActivity<TEntity> : Lookup<TEntity> where TEntity : IEntity
	{		
		private IList<TEntity> _filteredEntities;
		
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			_prgDlg = ProgressDialog.Show(this, GetString(Resource.String.loading), GetString(Resource.String.loading));
			FindViewById<EditText>(Resource.Id.txtFilter).TextChanged += HandleTextChanged;
		}
		
		protected override void AfterConnection ()
		{
			base.AfterConnection ();			
			var task = new Task(delegate { 
				_filteredEntities = LoadAll().ToList();
				RunOnUiThread(() => UpdateUi(_filteredEntities)); });
			task.Start();
		}
		
		void HandleTextChanged (object sender, Android.Text.TextChangedEventArgs e)
		{
			var filter = new string(e.Text.ToArray());
			if(string.IsNullOrEmpty(filter))
			{
				_filteredEntities = CachedEntites.ToList();
			}
			else
			{
				_filteredEntities = _cachedEntities.Where(t => t.Name.ToLower().Contains(filter.ToLower())).ToList();
			}
			RunOnUiThread(() => UpdateUi(_filteredEntities));
		}
			
		protected override void OnDestroy ()
		{
			base.OnDestroy ();
			FindViewById<EditText>(Resource.Id.txtFilter).TextChanged -= HandleTextChanged;
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