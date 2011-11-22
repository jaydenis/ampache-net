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
		
		public AmpachePlayer (AmpacheModel model, Context context)
		{
			_context = context;
			_model = model;			
			_model.PropertyChanged += Handle_modelPropertyChanged;
		}

		void Handle_modelPropertyChanged (object sender, PropertyChangedEventArgs e)
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
				default:
					break;
			}
		}
		
		public void PlayPause()
		{
			_model.PlayPauseRequested = false;
			if(_isPaused)
			{
				_player.Start();
				_isPaused = false;
				_model.IsPlaying = true;
				return;
			}
			if(_player.IsPlaying)
			{
				_player.Pause();
				_isPaused = true;
				_model.IsPlaying = false;
				return;
			}
			if (_model.Playlist == null || _model.Playlist.Count == 0) 
			{
				return;
			}
			Console.WriteLine ("Begining Playback");
			_model.PlayingSong = _model.Playlist.First();
			_player.Release();
			_player.Completion -= Handle_playerCompletion;
			_player.Dispose();
			_player = MediaPlayer.Create(_context, Android.Net.Uri.Parse(_model.PlayingSong.Url));
			_player.Completion += Handle_playerCompletion;
			_player.Start();
			_isPaused = false;
			_model.IsPlaying = true;
		}

		void Handle_playerCompletion (object sender, EventArgs e)
		{
			Next();
		}
		
		public void Previous()
		{
			_model.PreviousRequested = false;
			if(_player.CurrentPosition < 2000)
			{
				_model.PlayingSong = _model.Playlist[(_model.Playlist.IndexOf(_model.PlayingSong) + _model.Playlist.Count - 1) % _model.Playlist.Count];
				Console.WriteLine ("Playing Previous Song");
			}
			if (_model.Playlist == null || _model.Playlist.Count == 0) 
			{
				if (_player.IsPlaying)
				{
					_player.Stop();
					_model.PlayingSong = null;
				}
				return;
			}
			_player.Stop();
			_player.Release();
			_player.Completion -= Handle_playerCompletion;
			_player.Dispose();
			_player = MediaPlayer.Create(_context, Android.Net.Uri.Parse(_model.PlayingSong.Url));
			_player.Completion += Handle_playerCompletion;
			_player.Start();
			_isPaused = false;
		}
		
		public void Next()
		{
			_model.NextRequested = false;
			if (_model.Playlist == null || _model.Playlist.Count == 0) 
			{
				if (_player.IsPlaying)
				{
					_player.Stop();
				}
				_model.PlayingSong = null;
				return;
			}
			var nextIndex = _model.Playlist.IndexOf(_model.PlayingSong) + 1;
			if(_model.Shuffling)
			{
				nextIndex = new Random().Next(0, _model.Playlist.Count - 1);
			}
			if (nextIndex == _model.Playlist.Count) {
				return;
			}
			_model.PlayingSong = _model.Playlist[nextIndex];
			Console.WriteLine ("Playing next Song: " + _model.PlayingSong.Name);
			_player.Stop();
			_player.Release();
			_player.Completion -= Handle_playerCompletion;
			_player.Dispose();
			_player = MediaPlayer.Create(_context, Android.Net.Uri.Parse(_model.PlayingSong.Url));
			_player.Completion += Handle_playerCompletion;
			_player.Start();
			_isPaused = false;
			GC.Collect(0);
		}
		
		public void Stop()
		{
			if (_player.IsPlaying)
			{
				_player.Stop();
			}
			_model.PlayingSong = null;
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
				_model.PlayingSong = null;
			}
		}
		#endregion
	}
}

