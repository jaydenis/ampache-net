//
// ConfigurationActivity.cs
//
// Author:
//       John Moore <jcwmoore@gmail.com>
//
// Copyright (c) 2013 John Moore
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
        public Notification AmpacheNotification { get { return _builder.Notification; } }
		public const int NOTIFICATION_ID = 0;
        private Notification.Builder _builder;
		
		public AmpacheNotifications (Context context, AmpacheModel model)
		{
			_model = model;
			_context = context;
			_model.PropertyChanged += Handle_modelPropertyChanged;
            _builder = new Notification.Builder(context)
                .SetSmallIcon(Resource.Drawable.ic_stat_notify_musicplayer)
                .SetContentTitle("Amapche.NET")
                .SetContentIntent(PendingIntent.GetActivity(_context, 0, new Intent(_context, typeof(NowPlaying)), PendingIntentFlags.UpdateCurrent));
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
					_builder.SetTicker(new Java.Lang.String(string.Format(_context.GetString(Resource.String.nowPlayingFormat), _model.PlayingSong.Name, _model.PlayingSong.ArtistName)));
					_builder.SetWhen(Java.Lang.JavaSystem.CurrentTimeMillis());
                    _builder.SetContentText(string.Format("Playing {0}", _model.PlayingSong.Name));
                    
                    var old = _builder.Notification.LargeIcon;
                    var map = Android.Graphics.BitmapFactory.DecodeByteArray(_model.AlbumArtStream.ToArray(), 0, (int)_model.AlbumArtStream.Length);

                    var size = _context.Resources.GetDimensionPixelSize(Android.Resource.Dimension.NotificationLargeIconWidth);
                    _builder.SetLargeIcon(Android.Graphics.Bitmap.CreateScaledBitmap(map, size, size, false));
                    map.Recycle();
                    map.Dispose();
                    if (old != null)
                    {
                        old.Recycle();
                        old.Dispose();
                    }

					((NotificationManager)_context.GetSystemService(Context.NotificationService)).Notify(NOTIFICATION_ID, _builder.Notification);
				}
			}
			if(e.PropertyName == AmpacheModel.IS_PLAYING)
			{
				if(_model.IsPlaying)
				{
					_builder.SetWhen(Java.Lang.JavaSystem.CurrentTimeMillis());
					((NotificationManager)_context.GetSystemService(Context.NotificationService)).Notify(NOTIFICATION_ID, AmpacheNotification);
				}
				else
				{
					((NotificationManager)_context.GetSystemService(Context.NotificationService)).Cancel(NOTIFICATION_ID);	
				}
			}
            if (e.PropertyName == AmpacheModel.ALBUM_ART_STREAM)
            {
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