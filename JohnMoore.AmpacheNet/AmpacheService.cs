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
	public class AmpacheService : Background
	{
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
		private int _intentId = 1;
		
		#region implemented abstract members of Android.App.Service
		public override IBinder OnBind (Intent intent)
		{
			return new Binder(_model);
		}
		
		public override void OnLowMemory ()
		{
			GC.Collect();
		}
		
		public override StartCommandResult OnStartCommand (Intent intent, StartCommandFlags flags, int startId)
		{
			return StartCommandResult.NotSticky;
		}
		
		public override void OnCreate ()
		{
			base.OnCreate ();
			Console.SetOut(new AndroidLogTextWriter());
			var telSvc = this.ApplicationContext.GetSystemService(Context.TelephonyService) as Android.Telephony.TelephonyManager;
			if(telSvc != null)
			{
				telSvc.Listen(new AmpachePhoneStateListener(_model), Android.Telephony.PhoneStateListenerFlags.CallState);
			}
			_artCachePath = CacheDir.AbsolutePath;
			var am = (AlarmManager)ApplicationContext.GetSystemService(Context.AlarmService);
			var ping = new Intent(PingReceiver.INTENT);
			_pingIntent = PendingIntent.GetBroadcast(ApplicationContext, 0, ping, PendingIntentFlags.UpdateCurrent);
			am.SetRepeating(AlarmType.RtcWakeup, Java.Lang.JavaSystem.CurrentTimeMillis() + (long)TimeSpan.FromMinutes(5).TotalMilliseconds, (long)TimeSpan.FromMinutes(5).TotalMilliseconds, _pingIntent);
			var stream = new System.IO.MemoryStream();
			Android.Graphics.BitmapFactory.DecodeResource(Resources, Resource.Drawable.icon_thumbnail).Compress(Android.Graphics.Bitmap.CompressFormat.Png, 100, stream);
			Start(stream);
			_player = new AndroidPlayer(_model, ApplicationContext);
			_notifications = new AmpacheNotifications(this.ApplicationContext, _model);
		}
		
		public override void OnDestroy ()
		{
			base.OnDestroy ();
			_player.Dispose();
			_loader.Dispose();
			_notifications.Dispose();
			Console.WriteLine ("So long and Thanks for all the fish!");
			Java.Lang.JavaSystem.RunFinalizersOnExit(true);
			Java.Lang.JavaSystem.Exit(0);
		}
		#endregion
		
		#region implemented abstract members of JohnMoore.AmpacheNet.Logic.Background
		public override UserConfiguration LoadPersistedConfiguration ()
		{
			var settings = GetSharedPreferences(CONFIGURATION,FileCreationMode.Private);
			var config = new UserConfiguration();
			config.ServerUrl = settings.GetString(AmpacheService.URL_KEY, string.Empty);
			config.User = settings.GetString(AmpacheService.USER_NAME_KEY, string.Empty);
			config.Password = settings.GetString(AmpacheService.PASSWORD_KEY, string.Empty);
			config.AllowSeeking = settings.GetBoolean(AmpacheService.ALLOW_SEEKING_KEY, true);
			config.CacheArt = settings.GetBoolean(AmpacheService.CACHE_ART_KEY, true);
			return config;
		}

		public override List<AmpacheSong> LoadPersistedSongs ()
		{
			var settings = GetSharedPreferences(CONFIGURATION, FileCreationMode.Private);
			var sngLookup = _model.Factory.GetInstanceSelectorFor<AmpacheSong>();
			return settings.GetString (PLAYLIST_CSV_KEY, string.Empty).Split (new [] {','}, StringSplitOptions.RemoveEmptyEntries).Select (s => sngLookup.SelectBy (int.Parse (s))).ToList ();
		}

		public override void PersistUserConfig (UserConfiguration config)
		{
			var settings = GetSharedPreferences(CONFIGURATION, FileCreationMode.Private);
			var editor = settings.Edit();
			editor.PutString(URL_KEY, config.ServerUrl);
			editor.PutString(USER_NAME_KEY, config.User);
			editor.PutString(PASSWORD_KEY, config.Password);
			editor.PutBoolean(ALLOW_SEEKING_KEY, config.AllowSeeking);				
			editor.Commit();
			if(!config.CacheArt)
			{
				foreach(var file in System.IO.Directory.GetFiles(CacheDir.AbsolutePath))
				{
					System.IO.File.Delete(file);
				}
			}
		}

		public override void PersistSongs (IList<AmpacheSong> songs)
		{
			var settings = GetSharedPreferences(CONFIGURATION, FileCreationMode.Private);
			var editor = settings.Edit();
			editor.PutString(PLAYLIST_CSV_KEY, string.Join(",", songs.Select(s=>s.Id.ToString()).ToArray()));
			editor.Commit();
		}

		public override void PlatformFinalize ()
		{
			StopForeground(true);
			var am = (AlarmManager)ApplicationContext.GetSystemService(Context.AlarmService);
			am.Cancel(_pingIntent);
			am.Cancel(_stopIntent);
			StopSelf();
		}

		public override void StartAutoShutOff ()
		{
			StopForeground(false);
			var am = (AlarmManager)ApplicationContext.GetSystemService(Context.AlarmService);
			am.Set(AlarmType.ElapsedRealtimeWakeup, Java.Lang.JavaSystem.CurrentTimeMillis() + (long)TimeSpan.FromMinutes(30).Milliseconds, _stopIntent);
			var stop = new Intent(StopServiceReceiver.INTENT);
			_stopIntent = PendingIntent.GetBroadcast(ApplicationContext, ++_intentId, stop, PendingIntentFlags.UpdateCurrent);
			am.Set(AlarmType.RtcWakeup, Java.Lang.JavaSystem.CurrentTimeMillis() + (long)TimeSpan.FromMinutes(30).TotalMilliseconds , _stopIntent);
		}

		public override void StopAutoShutOff ()
		{
			StartForeground(AmpacheNotifications.NOTIFICATION_ID, _notifications.AmpacheNotification);
			var am = (AlarmManager)ApplicationContext.GetSystemService(Context.AlarmService);
			am.Cancel(_stopIntent);
			_stopIntent.Dispose();
			_stopIntent = null;
		}
		#endregion
		
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
			
			#region IServiceConnection implementation
			public void OnServiceConnected (ComponentName name, IBinder service)
			{
				Model = ((Binder)service).Model;
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

