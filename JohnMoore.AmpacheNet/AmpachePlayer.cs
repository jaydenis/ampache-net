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
	internal class AmpachePlayer : IDisposable
	{
		private readonly AmpacheModel _model;
		private bool _isPaused = false;
		private MediaPlayer _player = new MediaPlayer();
		private readonly Context _context;
		private readonly Timer _timer;
		
		public AmpachePlayer (AmpacheModel model, Context context)
		{
			_context = context;
			_model = model;
			_timer = new Timer((o) => UpdatePlayProgress(), new object(), TimeSpan.FromMilliseconds(500), TimeSpan.FromMilliseconds(500));
			_model.PropertyChanged += Handle_modelPropertyChanged;
		}
		
		void Handle_modelPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			//System.Threading.ThreadPool.QueueUserWorkItem((o) => PropertyChanged(e));
			PropertyChanged(e);
		}
		
		void PropertyChanged (PropertyChangedEventArgs e)
		{
			lock(this)
			{
				switch (e.PropertyName) 
				{
					case AmpacheModel.PLAY_PAUSE_REQUESTED:
						if(_model.PlayPauseRequested) PlayPause();
						break;
					case AmpacheModel.NEXT_REQUESTED:
						if(_model.NextRequested) Next();
						break;
					case AmpacheModel.PREVIOUS_REQUESTED:
						if(_model.PreviousRequested) Previous();
						break;
					case AmpacheModel.STOP_REQUESTED:
						if(_model.StopRequested) Stop();
						break;
					case AmpacheModel.REQUESTED_SEEK_TO_PERCENTAGE:
						Seek();
						break;
					default:
						break;
				}
			}
		}
		
		public void PlayPause()
		{
			if(_isPaused)
			{
				_player.Start();
				_isPaused = false;
				_model.IsPlaying = true;
			}
			else if(_player.IsPlaying)
			{
				_player.Pause();
				_isPaused = true;
				_model.IsPlaying = false;
			}
			else if (_model.Playlist != null || _model.Playlist.Count != 0) 
			{
				Console.WriteLine ("Begining Playback");
				if(_model.PlayingSong == null)
				{
					Console.WriteLine ("Playback First Song");
					_model.PlayingSong = _model.Playlist.First();
				}
				PrepareMediaPlayerSynchronous();
				_isPaused = false;
				_model.IsPlaying = true;
			}
			_model.PlayPauseRequested = false;
		}

		void Handle_playerCompletion (object sender, EventArgs e)
		{
			System.Threading.ThreadPool.QueueUserWorkItem((o) => Next());
		}
		
		public void Previous()
		{
			if (_model.Playlist == null || _model.Playlist.Count == 0) 
			{
				if (_player.IsPlaying)
				{
					_player.Stop();
					_model.PlayingSong = null;
				}
				_model.PreviousRequested = false;
				return;
			}
			if(_player.CurrentPosition < 2000)
			{
				_model.PlayingSong = _model.Playlist[(_model.Playlist.IndexOf(_model.PlayingSong) + _model.Playlist.Count - 1) % _model.Playlist.Count];
				Console.WriteLine ("Playing Previous Song");
			}
			PrepareMediaPlayerSynchronous();
			_isPaused = false;
			_model.PreviousRequested = false;
		}
		
		public void Next()
		{
			if (_model.Playlist == null || _model.Playlist.Count == 0) 
			{
				if (_player.IsPlaying)
				{
					_player.Stop();
				}
				_model.PlayingSong = null;
			}
			else
			{
				var nextIndex = (_model.Playlist.IndexOf(_model.PlayingSong) + 1) % _model.Playlist.Count;
				if(_model.Shuffling)
				{
					nextIndex = new Random().Next(0, _model.Playlist.Count - 1);
				}
				_model.PlayingSong = _model.Playlist[nextIndex];
				Console.WriteLine ("Playing next Song: " + _model.PlayingSong.Name);
				PrepareMediaPlayerSynchronous();
				_isPaused = false;
			}
			_model.NextRequested = false;
			GC.Collect(0);
		}
		
		void PrepareMediaPlayerSynchronous()
		{
			if(_player == null || _model.PlayingSong == null)
			{
				return;
			}
			if(_player.IsPlaying) 
			{
				_player.Stop();
			}
			_isPaused = true;
			_model.PercentPlayed = 0;
			_model.PercentDownloaded = 0;
			_player.Release();
			_player.Completion -= Handle_playerCompletion;
			_player.BufferingUpdate -= Handle_playerBufferingUpdate;
			_player.Dispose();
			_player = new MediaPlayer();
			_player.SetAudioStreamType(Stream.Music);
			_player.SetDataSource(_context, Android.Net.Uri.Parse(_model.PlayingSong.Url));
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
			_isPaused = false;
			_player.Completion += Handle_playerCompletion;
			_player.BufferingUpdate += Handle_playerBufferingUpdate;
		}

		void Handle_playerBufferingUpdate (object sender, MediaPlayer.BufferingUpdateEventArgs e)
		{
			_model.PercentDownloaded = e.Percent;
		}
		
		void UpdatePlayProgress()
		{
			if(_player.IsPlaying)
			{
				_model.PercentPlayed = (_player.CurrentPosition / _model.PlayingSong.TrackLength.TotalMilliseconds) * 100;
			}
		}
		
		public void Stop()
		{
			_model.PlayingSong = null;
			if (_player.IsPlaying || _isPaused)
			{
				_player.Stop();
				_isPaused = false;
				_model.IsPlaying = false;
			}
			_model.StopRequested = false;
		}
		
		void Seek()
		{
			if(_model.Configuration.AllowSeeking && _model.RequestedSeekToPercentage != null && !_isPaused && _player.IsPlaying)
			{
				var seekPositionMilli = (int)(_model.RequestedSeekToPercentage * _model.PlayingSong.TrackLength.TotalMilliseconds);
				Console.WriteLine (seekPositionMilli);
				_player.SeekTo(seekPositionMilli);
				_model.RequestedSeekToPercentage = null;
			}
		}

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

