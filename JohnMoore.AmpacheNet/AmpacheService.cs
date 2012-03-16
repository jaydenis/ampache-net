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
using System.Threading.Tasks;
using System.ComponentModel;

using Android.App;
using Android.Content;
using Android.OS;
using Environment = Android.OS.Environment;

using JohnMoore.AmpacheNet.Entities;
using JohnMoore.AmpacheNet.DataAccess;
using JohnMoore.AmpacheNet.Logic;

namespace JohnMoore.AmpacheNet
{
	[Service]
	public class AmpacheService : Service
	{
		private static AmpacheModel _model;
		private static AlbumArtLoader _loader;
		private const string CONFIGURATION = "configuration";
		private const string URL_KEY = "url";
		private const string USER_NAME_KEY = "user";
		private const string PASSWORD_KEY = "password";
		private const string ALLOW_SEEKING_KEY = "allowSeeking";
		private const string CACHE_ART_KEY = "cacheArt";
		private const string PLAYLIST_CSV_KEY = "playlist";
		private AndroidPlayer _player;
		private AmpacheNotifications _notifications;
		private PendingIntent _stopIntent;
		private PendingIntent _pingIntent;
		#region implemented abstract members of Android.App.Service
		public override IBinder OnBind (Intent intent)
		{
			return new Binder(_model);
		}
		
		public override void OnLowMemory ()
		{
			GC.Collect();
		}
		#endregion
		
		public override StartCommandResult OnStartCommand (Intent intent, StartCommandFlags flags, int startId)
		{
			return StartCommandResult.NotSticky;
		}
		
		public override void OnCreate ()
		{
			base.OnCreate ();
			_model = new AmpacheModel();
			Task.Factory.StartNew(() => ServiceStartup());
			var telSvc = this.ApplicationContext.GetSystemService(Context.TelephonyService) as Android.Telephony.TelephonyManager;
			if(telSvc != null)
			{
				telSvc.Listen(new AmpachePhoneStateListener(_model), Android.Telephony.PhoneStateListenerFlags.CallState);
			}
			var settings = GetSharedPreferences(CONFIGURATION,FileCreationMode.Private);
			var config = new UserConfiguration();
			config.ServerUrl = settings.GetString(AmpacheService.URL_KEY, string.Empty);
			config.User = settings.GetString(AmpacheService.USER_NAME_KEY, string.Empty);
			config.Password = settings.GetString(AmpacheService.PASSWORD_KEY, string.Empty);
			config.AllowSeeking = settings.GetBoolean(AmpacheService.ALLOW_SEEKING_KEY, true);
			config.CacheArt = settings.GetBoolean(AmpacheService.CACHE_ART_KEY, true);
			_model.Configuration = config;			
			AmpacheSelectionFactory.ArtLocalDirectory =  CacheDir.AbsolutePath;
			var am = (AlarmManager)ApplicationContext.GetSystemService(Context.AlarmService);
			var stop = new Intent(StopServiceReceiver.INTENT);
			var ping = new Intent(PingReceiver.INTENT);
			_stopIntent = PendingIntent.GetBroadcast(ApplicationContext, 0, stop, PendingIntentFlags.UpdateCurrent);
			_pingIntent = PendingIntent.GetBroadcast(ApplicationContext, 0, ping, PendingIntentFlags.UpdateCurrent);
			am.Set(AlarmType.RtcWakeup, Java.Lang.JavaSystem.CurrentTimeMillis() + (long)TimeSpan.FromMinutes(30).TotalMilliseconds , _stopIntent);
			am.SetRepeating(AlarmType.RtcWakeup, Java.Lang.JavaSystem.CurrentTimeMillis() + (long)TimeSpan.FromMinutes(5).TotalMilliseconds, (long)TimeSpan.FromMinutes(5).TotalMilliseconds, _pingIntent);
		}
		
		private void ServiceStartup()
		{
			Console.SetOut(new AndroidLogTextWriter());
			Console.WriteLine ("service created");
			var stm = Resources.OpenRawResource(Resource.Drawable.icon);
			var stream = new System.IO.MemoryStream();
			Android.Graphics.BitmapFactory.DecodeResource(Resources, Resource.Drawable.icon_thumbnail).Compress(Android.Graphics.Bitmap.CompressFormat.Png, 100, stream);
			_loader = new AlbumArtLoader(_model, stream);
			try 
			{
				if (_model.Configuration.ServerUrl != string.Empty) 
				{
					var hs = new Authenticate(_model.Configuration.ServerUrl, _model.Configuration.User, _model.Configuration.Password);
					_model.Factory = new AmpacheSelectionFactory(hs);
					_model.UserMessage = GetString(Resource.String.connectedToAmpache);
					var sngLookup = _model.Factory.GetInstanceSelectorFor<AmpacheSong> ();
					var settings = GetSharedPreferences (CONFIGURATION, FileCreationMode.Private);
					_model.Playlist = settings.GetString (PLAYLIST_CSV_KEY, string.Empty).Split (new [] {','}, StringSplitOptions.RemoveEmptyEntries).Select (s => sngLookup.SelectBy (int.Parse (s))).ToList ();
				}
			}
			catch (Exception ex) 
			{
				_model.UserMessage = ex.Message;
				Console.WriteLine (ex.GetType().Name);
				Console.WriteLine (ex.Message);
			}
			_player = new AndroidPlayer(_model, ApplicationContext);
			_notifications = new AmpacheNotifications(this.ApplicationContext, _model);
			_model.PropertyChanged += Handle_modelPropertyChanged;
		}
		
		public override void OnDestroy ()
		{
			base.OnDestroy ();
			_model.PropertyChanged -= Handle_modelPropertyChanged;
			_player.Dispose();
			_loader.Dispose();
			_notifications.Dispose();
			Console.WriteLine ("So long and Thanks for all the fish!");
			Java.Lang.JavaSystem.RunFinalizersOnExit(true);
			Java.Lang.JavaSystem.Exit(0);
		}
			
		
		void Handle_modelPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			if(e.PropertyName == AmpacheModel.CONFIGURATION)
			{
				var settings = GetSharedPreferences(CONFIGURATION, FileCreationMode.Private);
				var editor = settings.Edit();
				editor.PutString(URL_KEY, _model.Configuration.ServerUrl);
				editor.PutString(USER_NAME_KEY, _model.Configuration.User);
				editor.PutString(PASSWORD_KEY, _model.Configuration.Password);
				editor.PutBoolean(ALLOW_SEEKING_KEY, _model.Configuration.AllowSeeking);				
				editor.Commit();
				if(!_model.Configuration.CacheArt)
				{
					foreach(var file in System.IO.Directory.GetFiles(CacheDir.AbsolutePath))
					{
						System.IO.File.Delete(file);
					}
				}
			}
			if(e.PropertyName == AmpacheModel.IS_PLAYING)
			{
				if(_model.IsPlaying)
				{
					StartForeground(AmpacheNotifications.NOTIFICATION_ID, _notifications.AmpacheNotification);
					var am = (AlarmManager)ApplicationContext.GetSystemService(Context.AlarmService);
					am.Cancel(_stopIntent);
				}
				else
				{
					StopForeground(false);
					var am = (AlarmManager)ApplicationContext.GetSystemService(Context.AlarmService);
					am.Set(AlarmType.ElapsedRealtimeWakeup, Java.Lang.JavaSystem.CurrentTimeMillis() + (long)TimeSpan.FromMinutes(30).Milliseconds, _stopIntent);
				}
			}
			if(e.PropertyName == AmpacheModel.PLAYLIST)
			{
				var settings = GetSharedPreferences(CONFIGURATION, FileCreationMode.Private);
				var editor = settings.Edit();
				editor.PutString(PLAYLIST_CSV_KEY, string.Join(",", _model.Playlist.Select(s=>s.Id.ToString()).ToArray()));
				editor.Commit();
			}
			if(e.PropertyName == AmpacheModel.IS_DISPOSED && _model.IsDisposed)
			{
				StopForeground(true);
				var am = (AlarmManager)ApplicationContext.GetSystemService(Context.AlarmService);
				am.Cancel(_pingIntent);
				am.Cancel(_stopIntent);
				StopSelf();
			}
		}		
		
		#region Binding Classes
		public class Binder : Android.OS.Binder
		{
			public readonly AmpacheModel Model;
			
			public Binder (AmpacheModel model)
			{
				Model = model;
			}
		}
		
		public class Connection : Java.Lang.Object, IServiceConnection
		{
			public event EventHandler OnConnected;
			public AmpacheModel Model { get; private set; }
			
			public Connection ()
			{}

			#region IServiceConnection implementation
			public void OnServiceConnected (ComponentName name, IBinder service)
			{
				var bind = service as Binder;
				if(bind != null)
				{
					Model = bind.Model;
					if (OnConnected != null)
					{
						OnConnected(this, EventArgs.Empty);
					}
				}
			}

			public void OnServiceDisconnected (ComponentName name)
			{}
			#endregion

		}
		
		#endregion
		
		#region Receiver Classes
		
		[BroadcastReceiver(Enabled = true)]
		[IntentFilter(new string[] {StopServiceReceiver.INTENT})]
		public class StopServiceReceiver : BroadcastReceiver
		{
			public const string INTENT = "JohnMoore.AmpacheNET.STOP";
			
			public override void OnReceive (Context context, Intent intent)
			{
				Console.WriteLine ("Shutdown Broadcast Received");
				_model.Dispose();
			}
		}
		[BroadcastReceiver(Enabled = true)]
		[IntentFilter(new string[] {PingReceiver.INTENT})]
		public class PingReceiver : BroadcastReceiver
		{
			public const string INTENT = "JohnMoore.AmpacheNET.PING";
			
			public override void OnReceive (Context context, Intent intent)
			{
				Console.WriteLine ("Ping Broadcast Received");
				if(_model.Factory != null)
				{
					_model.Factory.Ping();
				}
			}
		}
		#endregion
	}
}

