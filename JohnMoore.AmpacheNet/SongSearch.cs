using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using JohnMoore.AmpacheNet.Entities;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using JohnMoore.AmpacheNet.Logic;

namespace JohnMoore.AmpacheNet
{
	[Activity (Label = "SongSearch",Theme="@android:style/Theme.Dialog")]			
	public class SongSearch : Lookup<AmpacheSong>
	{
		private string _searchString;
		private Timer _searchTimer;
		
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			FindViewById<EditText>(Resource.Id.txtFilter).TextChanged += HandleTextChanged;
			FindViewById<EditText>(Resource.Id.txtFilter).Hint = GetString(Resource.String.search);
			_searchTimer = new Timer((o) => ExecuteSearch(), new object(), Timeout.Infinite, Timeout.Infinite);
		}

		void HandleTextChanged (object sender, Android.Text.TextChangedEventArgs e)
		{
			_searchString = new string(e.Text.ToArray());
			_searchTimer.Change(TimeSpan.FromSeconds(3), TimeSpan.FromMilliseconds(Timeout.Infinite));
		}

		void ExecuteSearch ()
		{
			RunOnUiThread(() => _prgDlg = ProgressDialog.Show(this, GetString(Resource.String.loading), GetString(Resource.String.loading)));
			RunOnUiThread(() => FindViewById<EditText>(Resource.Id.txtFilter).ClearFocus());
			try 
			{
				var res = Search (_searchString).ToList ();
				RunOnUiThread(() => UpdateUi(res));
			}
			catch (Exception ex)
			{
				Console.WriteLine (ex.Message);
				RunOnUiThread(() => _prgDlg.Dismiss());
				_model.UserMessage = ex.Message;
			}
		}
	}
}

