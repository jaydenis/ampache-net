//
// AmpachePlayer.cs
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
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using JohnMoore.AmpacheNet.Entities;

namespace JohnMoore.AmpacheNet.Logic
{
	public abstract class AmpachePlayer
	{
		protected readonly AmpacheModel _model;
		protected bool _isPaused = false;
		private int _playerPositionMilliSecond;
		private static object _syncLock = new object();
		
		protected int PlayerPositionMilliSecond
		{
			get { return _playerPositionMilliSecond; }
			set
			{
				_playerPositionMilliSecond = value;
				_model.PercentPlayed = (_playerPositionMilliSecond / _model.PlayingSong.TrackLength.TotalMilliseconds) * 100;
			}
		}
		
		protected AmpachePlayer (AmpacheModel model)
		{
			_model = model;
			_model.PropertyChanged += Handle_modelPropertyChanged;
		}
		
		void Handle_modelPropertyChanged (object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName) 
			{
				case AmpacheModel.PLAY_PAUSE_REQUESTED:
					if(_model.PlayPauseRequested) Task.Factory.StartNew(() => PlayPause());
					break;
				case AmpacheModel.NEXT_REQUESTED:
					if(_model.NextRequested) Task.Factory.StartNew(() => Next());
					break;
				case AmpacheModel.PREVIOUS_REQUESTED:
					if(_model.PreviousRequested) Task.Factory.StartNew(() => Previous());
					break;
				case AmpacheModel.STOP_REQUESTED:
					if(_model.StopRequested)
					{
						_model.PlayingSong = null;
						Stop();
					}
					break;
				case AmpacheModel.REQUESTED_SEEK_TO_PERCENTAGE:
					Task.Factory.StartNew(() => Seek());
					break;
				default:
					break;
			}
		}

		public void PlayPause ()
		{
			lock(_syncLock)
			{
				Console.WriteLine ("Play Pause Requested");
				if(_isPaused)
				{
					Unpause();
					_isPaused = false;
					_model.IsPlaying = true;
				}
				else if(_model.IsPlaying)
				{
					Pause();
					_isPaused = true;
					_model.IsPlaying = false;
				}
				else if((_model.Playlist ?? new List<AmpacheSong>()).Any())
				{
					Console.WriteLine ("Begining Playback");
					if(_model.PlayingSong == null)
					{
						Console.WriteLine ("Playback First Song");
						_model.PlayingSong = _model.Playlist.First();
					}
					try
					{
						_model.PercentPlayed = 0;
						_model.PercentDownloaded = 0;
						PlaySong (_model.PlayingSong);
						_isPaused = false;
						_model.IsPlaying = true;
					} 
					catch (Exception ex) 
					{
						_model.UserMessage = ex.Message;
						Console.WriteLine (ex.Message);
					}
				}
				_model.PlayPauseRequested = false;
				Console.WriteLine ("PlayPause done");
			}
		}

		void Next ()
		{
			lock(_syncLock)
			{
				Console.WriteLine ("Next Requested");
				if (_model.Playlist == null || _model.Playlist.Count == 0) 
				{
					if (_model.IsPlaying)
					{
						StopPlay();
						_model.PercentPlayed = 0;
						_model.PercentDownloaded = 0;
					}
					_model.PlayingSong = null;
				}
				else
				{
					int nextIndex = 0;
					if(_model.Shuffling)
					{
						nextIndex = new Random().Next(0, _model.Playlist.Count - 1);
					}
					else if(_model.PlayingSong != null)
					{
						nextIndex = (_model.Playlist.IndexOf(_model.PlayingSong) + 1) % _model.Playlist.Count;
					}
					Console.WriteLine ("Playing next Song: " + _model.PlayingSong.Name);
					_model.PercentPlayed = 0;
					_model.PercentDownloaded = 0;
                    Task.Factory.StartNew(() => _model.PlayingSong = _model.Playlist[nextIndex]);
                    PlaySong(_model.Playlist[nextIndex]);
					_isPaused = false;
					_model.IsPlaying = true;
				}
				_model.NextRequested = false;
				Console.WriteLine ("Next done");
			}
		}

		void Previous ()
		{
			lock(_syncLock)
			{
				Console.WriteLine ("Previous Requested");
				if (_model.Playlist == null || _model.Playlist.Count == 0) 
				{
					if (_model.IsPlaying)
					{
						StopPlay();
						_model.PercentPlayed = 0;
						_model.PercentDownloaded = 0;
						_model.PlayingSong = null;
					}
					_model.PreviousRequested = false;
					return;
				}
				if(PlayerPositionMilliSecond < 2000)
				{
					_model.PlayingSong = _model.Playlist[(_model.Playlist.IndexOf(_model.PlayingSong) + _model.Playlist.Count - 1) % _model.Playlist.Count];
					Console.WriteLine ("Playing Previous Song");
				}
				_model.PercentPlayed = 0;
				_model.PercentDownloaded = 0;
				PlaySong (_model.PlayingSong);
				_isPaused = false;
				_model.PreviousRequested = false;
				Console.WriteLine ("Previous done");
			}
		}

		void Stop ()
		{
			lock (_syncLock)
			{
				Console.WriteLine ("Stop Requested");
				if (_model.IsPlaying || _isPaused) {
					StopPlay ();
					_model.PercentPlayed = 0;
					_model.PercentDownloaded = 0;
					_isPaused = false;
					_model.IsPlaying = false;
				}
				_model.StopRequested = false;
				Console.WriteLine ("Stop done");
			}
		}

		void Seek ()
		{
			if(_model.Configuration.AllowSeeking && _model.RequestedSeekToPercentage != null && !_isPaused && _model.IsPlaying)
			{
				var seekPositionMilli = (int)(_model.RequestedSeekToPercentage * _model.PlayingSong.TrackLength.TotalMilliseconds);
				Console.WriteLine (seekPositionMilli);
				SeekTo(seekPositionMilli);
				_model.RequestedSeekToPercentage = null;
			}
		}
		
		protected abstract void PlaySong(AmpacheSong song);
		
		protected abstract void StopPlay();
		
		protected abstract void Pause();
		
		protected abstract void Unpause();
		
		protected abstract void SeekTo(int millis);
	}
}

