//
// ConfigurationActivity.cs
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

using JohnMoore.AmpacheNet.DataAccess;
using JohnMoore.AmpacheNet.Entities;

namespace JohnMoore.AmpacheNet
{
	[Activity(Theme="@android:style/Theme.Dialog")]			
	public class ConfigurationActivity : Activity
	{		
		private AmpacheService.Connection _connection;
		
		protected override void OnCreate (Bundle bundle)
		{
			base.OnCreate (bundle);
			SetContentView(Resource.Layout.Configuration);
			FindViewById<Button>(Resource.Id.btnConfigTest).Click += HandleTestClick;
			FindViewById<Button>(Resource.Id.btnConfigCancel).Click += HandleCancelClick;
			FindViewById<Button>(Resource.Id.btnConfigOk).Click += HandleOkClick;
			
			_connection = new AmpacheService.Connection();
			_connection.OnConnected += Handle_connectionOnConnected;
			BindService(new Intent(this.ApplicationContext, typeof(AmpacheService)), _connection, Bind.AutoCreate);
		}

		void Handle_connectionOnConnected (object sender, EventArgs e)
		{
			FindViewById<EditText>(Resource.Id.txtConfigUrl).Text = _connection.Model.Configuration.ServerUrl;
			FindViewById<EditText>(Resource.Id.txtConfigUser).Text = _connection.Model.Configuration.User;
			FindViewById<EditText>(Resource.Id.txtPasswordConfig).Text = _connection.Model.Configuration.Password;
			FindViewById<CheckBox>(Resource.Id.chkSeeking).Checked = _connection.Model.Configuration.AllowSeeking;
		}

		void HandleOkClick (object sender, EventArgs e)
		{
			var dlg = ProgressDialog.Show(this, GetString(Resource.String.connecting), GetString(Resource.String.connectingToAmpache));
			System.Threading.ThreadPool.QueueUserWorkItem( (o) => {
				try{
					_connection.Model.Configuration = new UserConfiguration{ ServerUrl = FindViewById<EditText>(Resource.Id.txtConfigUrl).Text, User = FindViewById<EditText>(Resource.Id.txtConfigUser).Text, Password = FindViewById<EditText>(Resource.Id.txtPasswordConfig).Text, AllowSeeking = FindViewById<CheckBox>(Resource.Id.chkSeeking).Checked};
					Finish();
				}
				catch(Exception ex)
				{
					RunOnUiThread(() => Toast.MakeText(this.ApplicationContext, ex.Message, ToastLength.Short).Show());
				} 
			RunOnUiThread(()=>dlg.Dismiss());});
		}

		void HandleCancelClick (object sender, EventArgs e)
		{
			Finish();
		}

		void HandleTestClick (object sender, EventArgs e)
		{
			var dlg = ProgressDialog.Show(this, GetString(Resource.String.connecting), GetString(Resource.String.connectingToAmpache));
			System.Threading.ThreadPool.QueueUserWorkItem( (o) => {
				try{
					Handshake tmp = new Authenticate(FindViewById<EditText>(Resource.Id.txtConfigUrl).Text,
												 FindViewById<EditText>(Resource.Id.txtConfigUser).Text,
												 FindViewById<EditText>(Resource.Id.txtPasswordConfig).Text);
					RunOnUiThread(() => Toast.MakeText(this.ApplicationContext, GetString(Resource.String.connectedToAmpache), ToastLength.Long).Show());
				}
				catch(Exception ex)
				{
					RunOnUiThread(() => Toast.MakeText(this.ApplicationContext, ex.Message, ToastLength.Short).Show());
				} 
			RunOnUiThread(()=>dlg.Dismiss());});
		}
		
		protected override void OnDestroy ()
		{
			base.OnDestroy ();
			FindViewById<Button>(Resource.Id.btnConfigTest).Click -= HandleTestClick;
			FindViewById<Button>(Resource.Id.btnConfigCancel).Click -= HandleCancelClick;
			FindViewById<Button>(Resource.Id.btnConfigOk).Click -= HandleOkClick;
			UnbindService(_connection);
			_connection.Dispose();
			_connection = null;
		}
	}
}

