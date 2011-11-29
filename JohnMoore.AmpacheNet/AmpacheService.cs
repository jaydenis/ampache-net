//
// AmpacheService.cs
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
using System.ComponentModel;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Media;
using Android.Widget;

using JohnMoore.AmpacheNet.Entities;
using JohnMoore.AmpacheNet.DataAccess;
using JohnMoore.AmpacheNet.Logic;

namespace JohnMoore.AmpacheNet
{
	[Service]
	public class AmpacheService : Service
	{
		private static readonly AmpacheModel _model = AmpacheModel.Instance;
		private static AlbumArtLoader _loader;
		private const string CONFIGURATION = "configuration";
		private const string URL_KEY = "url";
		private const string USER_NAME_KEY = "user";
		private const string PASSWORD_KEY = "password";
		private Authenticate _handshake;
		private AmpachePlayer _player;
		private Timer _ping;
		private AmpacheNotifications _notifications;
				
		#region implemented abstract members of Android.App.Service
		public override IBinder OnBind (Intent intent)
		{
			return new Binder();
		}
		#endregion
		
		
		public override void OnCreate ()
		{
			base.OnCreate ();
			Console.SetOut(new AndroidLogTextWriter());
			var stm = Resources.OpenRawResource(Resource.Drawable.icon);
			var stream = new System.IO.MemoryStream();
			Android.Graphics.BitmapFactory.DecodeResource(Resources, Resource.Drawable.icon_thumbnail).Compress(Android.Graphics.Bitmap.CompressFormat.Png, 100, stream);
			
			_loader = new AlbumArtLoader(_model, stream);
			var settings = GetSharedPreferences(CONFIGURATION,FileCreationMode.Private);
			var config = new UserConfiguration();
			config.ServerUrl = settings.GetString(AmpacheService.URL_KEY, string.Empty);
			config.User = settings.GetString(AmpacheService.USER_NAME_KEY, string.Empty);
			config.Password = settings.GetString(AmpacheService.PASSWORD_KEY, string.Empty);			
			try 
			{
				if (config.ServerUrl != string.Empty) 
				{
					_handshake = new Authenticate(config.ServerUrl, config.User, config.Password);
					_model.Factory = new AmpacheSelectionFactory(_handshake);
					Toast.MakeText(this.ApplicationContext, "Connected to Ampache", ToastLength.Long).Show();
				}
				var telSvc = this.ApplicationContext.GetSystemService(Context.TelephonyService) as Android.Telephony.TelephonyManager;
				if(telSvc != null)
				{
					telSvc.Listen(new AmpachePhoneStateListener(_model), Android.Telephony.PhoneStateListenerFlags.CallState);
				}
			}
			catch (Exception ex) 
			{
				Toast.MakeText(this.ApplicationContext, ex.Message, ToastLength.Long).Show();
			}
			_ping = new Timer((o) => _handshake.Ping(), null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
			_player = new AmpachePlayer(_model, ApplicationContext);
			_model.Configuration = config;
			_notifications = new AmpacheNotifications(this.ApplicationContext, _model);
			_model.PropertyChanged += Handle_modelPropertyChanged;
			
		}

		public override void OnDestroy ()
		{
			base.OnDestroy ();
			_model.PropertyChanged -= Handle_modelPropertyChanged;
			_ping.Dispose();
			_player.Dispose();
			_loader.Dispose();
			_notifications.Dispose();
		}
			
		
		void Handle_modelPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == AmpacheModel.CONFIGURATION)
			{
				_handshake = new Authenticate(_model.Configuration.ServerUrl, _model.Configuration.User, _model.Configuration.Password);
				_model.Factory = new AmpacheSelectionFactory(_handshake);
				var settings = GetSharedPreferences(CONFIGURATION, FileCreationMode.Private);
				var editor = settings.Edit();
				editor.PutString(URL_KEY, _model.Configuration.ServerUrl);
				editor.PutString(USER_NAME_KEY, _model.Configuration.User);
				editor.PutString(PASSWORD_KEY, _model.Configuration.Password);
				editor.Commit();
			}
		}
		
		#region Binding Classes
		public class Binder : Android.OS.Binder
		{
			public Binder ()
			{}
			
			public Binder (IntPtr doNotUse) : base(doNotUse)
			{}
		}
		
		public class Connection : Java.Lang.Object, IServiceConnection
		{
			public event EventHandler OnConnected;
			public AmpacheModel Model { get; private set; }
			
			public Connection ()
			{}
			
			public Connection (IntPtr doNotUse) : base (doNotUse)
			{}

			#region IServiceConnection implementation
			public void OnServiceConnected (ComponentName name, IBinder service)
			{
				Model = _model;
				if (OnConnected != null)
				{
					OnConnected(this, EventArgs.Empty);
				}
			}

			public void OnServiceDisconnected (ComponentName name)
			{}
			#endregion


		}
		#endregion
	}
}

