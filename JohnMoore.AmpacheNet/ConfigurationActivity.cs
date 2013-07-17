//
// ConfigurationActivity.cs
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
using System.Threading.Tasks;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using JohnMoore.AmpacheNet.DataAccess;
using JohnMoore.AmpacheNet.Entities;
using JohnMoore.AmpacheNet.Logic;

namespace JohnMoore.AmpacheNet
{
	[Activity(Theme="@android:style/Theme.Dialog", Label = "@string/configure")]
	public class ConfigurationActivity : Configuration, AmpacheService.IClient
	{
		private AmpacheService.Connection _connection;
		private static UserConfiguration _config = null;
		
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			SetContentView(Resource.Layout.Configuration);
			FindViewById<Button>(Resource.Id.btnConfigTest).Click += HandleTestClick;
			FindViewById<Button>(Resource.Id.btnConfigCancel).Click += HandleCancelClick;
			FindViewById<Button>(Resource.Id.btnConfigOk).Click += HandleOkClick;
			FindViewById<TextView>(Resource.Id.txtConfigUrl).TextChanged += HandleTextChanged;
			FindViewById<TextView>(Resource.Id.txtConfigUser).TextChanged += HandleTextChanged;
			FindViewById<TextView>(Resource.Id.txtPasswordConfig).TextChanged += HandleTextChanged;
			_successMessage = GetString(Resource.String.connectedToAmpache);
			_connection = new AmpacheService.Connection(this);
			BindService(new Intent(this.ApplicationContext, typeof(AmpacheService)), _connection, Bind.AutoCreate);
		}

		void HandleTextChanged (object sender, Android.Text.TextChangedEventArgs e)
		{
			if(_config == null)
			{
				_config = new UserConfiguration();
			}
			_config.Password = FindViewById<EditText>(Resource.Id.txtPasswordConfig).Text;
			_config.ServerUrl = FindViewById<EditText>(Resource.Id.txtConfigUrl).Text;
			_config.User = FindViewById<EditText>(Resource.Id.txtConfigUser).Text;
		}

		public void Connected (Demeter.Container container)
        {
            _model = container.Resolve<AmpacheModel>();
			var tmp = _config ?? _model.Configuration;
			FindViewById<EditText>(Resource.Id.txtConfigUrl).Text = tmp.ServerUrl;
			FindViewById<EditText>(Resource.Id.txtConfigUser).Text = tmp.User;
			FindViewById<EditText>(Resource.Id.txtPasswordConfig).Text = tmp.Password;
			FindViewById<CompoundButton>(Resource.Id.chkSeeking).Checked = tmp.AllowSeeking;
			FindViewById<CompoundButton>(Resource.Id.chkArtCache).Checked = tmp.CacheArt;
		}

		void HandleOkClick (object sender, EventArgs e)
		{
			var dlg = ProgressDialog.Show(this, GetString(Resource.String.connecting), GetString(Resource.String.connectingToAmpache));
			bool success = false;
			var config = new UserConfiguration();
			config.AllowSeeking = FindViewById<CompoundButton>(Resource.Id.chkSeeking).Checked;
			config.CacheArt = FindViewById<CompoundButton>(Resource.Id.chkArtCache).Checked;
			config.Password =  FindViewById<EditText>(Resource.Id.txtPasswordConfig).Text;
			config.User =  FindViewById<EditText>(Resource.Id.txtConfigUser).Text;
			config.ServerUrl =  FindViewById<EditText>(Resource.Id.txtConfigUrl).Text;
			Task.Factory.StartNew(() => success = TrySaveConfiguration(config))
						.ContinueWith((t) => RunOnUiThread(()=> dlg.Dismiss()))
						.ContinueWith(delegate(Task obj) { if(success) { Finish(); _config = null; } });
		}

		void HandleCancelClick (object sender, EventArgs e)
		{
			_config = null;
			Finish();
		}

		void HandleTestClick (object sender, EventArgs e)
		{
			var dlg = ProgressDialog.Show(this, GetString(Resource.String.connecting), GetString(Resource.String.connectingToAmpache));
			var task = new Task(() => PerformTest(FindViewById<EditText>(Resource.Id.txtConfigUrl).Text,
												 FindViewById<EditText>(Resource.Id.txtConfigUser).Text,
												 FindViewById<EditText>(Resource.Id.txtPasswordConfig).Text));
			task.ContinueWith((o) => RunOnUiThread(() => dlg.Dismiss()));
			task.Start();
		}
		
		protected override void OnDestroy ()
		{
			base.OnDestroy ();
			FindViewById<Button>(Resource.Id.btnConfigTest).Click -= HandleTestClick;
			FindViewById<Button>(Resource.Id.btnConfigCancel).Click -= HandleCancelClick;
			FindViewById<Button>(Resource.Id.btnConfigOk).Click -= HandleOkClick;
			FindViewById<TextView>(Resource.Id.txtConfigUrl).TextChanged -= HandleTextChanged;
			FindViewById<TextView>(Resource.Id.txtConfigUser).TextChanged -= HandleTextChanged;
			FindViewById<TextView>(Resource.Id.txtPasswordConfig).TextChanged -= HandleTextChanged;
			UnbindService(_connection);
			_connection.Dispose();
			_connection = null;
		}
	}
}

