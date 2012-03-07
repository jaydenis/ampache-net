//
// AmpachePlayer.cs
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
using System.ComponentModel;
using System.Threading;

using Android.Media;
using Android.Content;
using JohnMoore.AmpacheNet.Logic;

namespace JohnMoore.AmpacheNet
{
	internal class AndroidPlayer : AmpachePlayer, IDisposable
	{
		private MediaPlayer _player = new MediaPlayer();
		private readonly Context _context;
		private readonly Timer _timer;
		
		public AndroidPlayer (AmpacheModel model, Context context) : base(model)
		{
			_context = context;
			_timer = new Timer((o) => UpdatePlayProgress(), new object(), TimeSpan.FromMilliseconds(500), TimeSpan.FromMilliseconds(500));
		}
		
		void Handle_playerCompletion (object sender, EventArgs e)
		{
			System.Threading.ThreadPool.QueueUserWorkItem((o) => _model.NextRequested = true);
		}
		
		void Handle_playerBufferingUpdate (object sender, MediaPlayer.BufferingUpdateEventArgs e)
		{
			_model.PercentDownloaded = e.Percent;
		}
		
		void UpdatePlayProgress()
		{
			if(_player.IsPlaying)
			{
				PlayerPositionMilliSecond = _player.CurrentPosition;
			}
		}

		#region implemented abstract members of JohnMoore.AmpacheNet.Logic.AmpachePlayer
		protected override void PlaySong (JohnMoore.AmpacheNet.Entities.AmpacheSong song)
		{
			if(_player == null || song == null)
			{
				return;
			}
			StopPlay();
			_player.Release();
			_player.Completion -= Handle_playerCompletion;
			_player.BufferingUpdate -= Handle_playerBufferingUpdate;
			_player.Dispose();
			_player = new MediaPlayer();
			_player.SetAudioStreamType(Stream.Music);
			_player.SetDataSource(_context, Android.Net.Uri.Parse(song.Url));
			try
			{
				_player.Prepare();
				_player.Start();
			}
			catch(Exception e)
			{
				_player.Release();
				_player.Dispose();
				_player = new MediaPlayer();
				Console.WriteLine (e.GetType().FullName);
				Console.WriteLine (e.Message);
			}
			_player.Completion += Handle_playerCompletion;
			_player.BufferingUpdate += Handle_playerBufferingUpdate;
		}

		protected override void StopPlay ()
		{
			if(_player.IsPlaying) 
			{
				_player.Stop();
			}
		}

		protected override void Pause ()
		{
			_player.Pause();
		}

		protected override void Unpause ()
		{
			_player.Start();
		}

		protected override void SeekTo (int millis)
		{
			_player.SeekTo(millis);
		}
		#endregion
		
		#region IDisposable implementation
		public void Dispose ()
		{
			if (_player != null) 
			{
				if(_player.IsPlaying)
				{
					_player.Stop();
				}
				_player.Release();
				_player.Dispose();
				_timer.Dispose();
				_model.PlayingSong = null;
			}
		}
		#endregion
	}
}

