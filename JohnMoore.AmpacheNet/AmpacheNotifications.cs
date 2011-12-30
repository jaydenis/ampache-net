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

using JohnMoore.AmpacheNet.Logic;

namespace JohnMoore.AmpacheNet
{
	class AmpacheNotifications : IDisposable
	{
		private readonly AmpacheModel _model;
		private readonly Context _context;
		public Notification AmpacheNotification { get; private set; }
		public const int NOTIFICATION_ID = 0;
		
		public AmpacheNotifications (Context context, AmpacheModel model)
		{
			_model = model;
			_context = context;
			_model.PropertyChanged += Handle_modelPropertyChanged;
			AmpacheNotification = new Notification(Resource.Drawable.stat_notify_musicplayer, "Ampache.NET");
			((NotificationManager)_context.GetSystemService(Context.NotificationService)).CancelAll();
		}

		void Handle_modelPropertyChanged (object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if(e.PropertyName == AmpacheModel.PLAYING_SONG)
			{
				if(_model.PlayingSong == null)
				{
					((NotificationManager)_context.GetSystemService(Context.NotificationService)).Cancel(NOTIFICATION_ID);	
				}
				else
				{
					AmpacheNotification.TickerText = new Java.Lang.String(string.Format(_context.GetString(Resource.String.nowPlayingFormat), _model.PlayingSong.Name, _model.PlayingSong.ArtistName));
					AmpacheNotification.When = Java.Lang.JavaSystem.CurrentTimeMillis();
					AmpacheNotification.SetLatestEventInfo(_context, "Ampache.NET", new string(AmpacheNotification.TickerText.ToArray()), PendingIntent.GetActivity(_context, 0, new Intent(_context, typeof(AmpacheNetActivity)), 0));
					((NotificationManager)_context.GetSystemService(Context.NotificationService)).Notify(NOTIFICATION_ID, AmpacheNotification);
				}
			}
			if(e.PropertyName == AmpacheModel.IS_PLAYING)
			{
				if(_model.IsPlaying)
				{
					AmpacheNotification.TickerText = new Java.Lang.String(string.Format(_context.GetString(Resource.String.nowPlayingFormat), _model.PlayingSong.Name, _model.PlayingSong.ArtistName));
					AmpacheNotification.When = Java.Lang.JavaSystem.CurrentTimeMillis();
					AmpacheNotification.SetLatestEventInfo(_context, "Ampache.NET", new string(AmpacheNotification.TickerText.ToArray()), PendingIntent.GetActivity(_context, 0, new Intent(_context, typeof(AmpacheNetActivity)), 0));
					((NotificationManager)_context.GetSystemService(Context.NotificationService)).Notify(NOTIFICATION_ID, AmpacheNotification);
				}
				else
				{
					((NotificationManager)_context.GetSystemService(Context.NotificationService)).Cancel(NOTIFICATION_ID);	
				}
			}
		}

		#region IDisposable implementation
		public void Dispose ()
		{
			_model.PropertyChanged -= Handle_modelPropertyChanged;
			((NotificationManager)_context.GetSystemService(Context.NotificationService)).Cancel(0);
			AmpacheNotification.Dispose();
		}
		#endregion
	}
}