//
// SongSearch.cs
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

