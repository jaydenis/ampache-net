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
using JohnMoore.AmpacheNet.Entities;

namespace JohnMoore.AmpacheNet.Logic
{
	public abstract class AmpachePlayer
	{
		protected readonly AmpacheModel _model;
		protected bool _isPaused = false;
		private int _playerPositionMilliSecond;
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
			System.Threading.ThreadPool.QueueUserWorkItem((o) => PropertyChanged(e));
		}
		
		void PropertyChanged(PropertyChangedEventArgs e)
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

		public void PlayPause ()
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
		}

		void Next ()
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
				_model.PlayingSong = _model.Playlist[nextIndex];
				Console.WriteLine ("Playing next Song: " + _model.PlayingSong.Name);
				_model.PercentPlayed = 0;
				_model.PercentDownloaded = 0;
				PlaySong (_model.PlayingSong);
				_isPaused = false;
				_model.IsPlaying = true;
			}
			_model.NextRequested = false;
		}

		void Previous ()
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
		}

		void Stop ()
		{
			Console.WriteLine ("Stop Requested");
			_model.PlayingSong = null;
			if (_model.IsPlaying || _isPaused)
			{
				StopPlay();
				_model.PercentPlayed = 0;
				_model.PercentDownloaded = 0;
				_isPaused = false;
				_model.IsPlaying = false;
			}
			_model.StopRequested = false;
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

