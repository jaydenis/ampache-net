//
// AmpacheModel.cs
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
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using JohnMoore.AmpacheNet.Entities;
using JohnMoore.AmpacheNet.DataAccess;

namespace JohnMoore.AmpacheNet.Logic
{
	public class AmpacheModel : INotifyPropertyChanged
	{
	  	public AmpacheModel() {}
		
		#region Factory

		public const string FACTORY = "Factory";
		private AmpacheSelectionFactory _factory = null;

		public AmpacheSelectionFactory Factory
		{
			get { return _factory; }
			set
			{
				if (_factory == value)
					return;

				_factory = value;
				if(PropertyChanged != null)
					PropertyChanged(this, new PropertyChangedEventArgs(FACTORY));
			}
		}

		#endregion
		
		#region PlayingSong

		public const string PLAYING_SONG = "PlayingSong";
		private AmpacheSong _playingSong = null;

		public AmpacheSong PlayingSong
		{
			get { return _playingSong; }
			set
			{
				if (_playingSong == value)
					return;

				_playingSong = value;
				if(PropertyChanged != null)
					PropertyChanged(this, new PropertyChangedEventArgs("PlayingSong"));
			}
		}

		#endregion
		
		#region PlayList

		public const string PLAYLIST = "PlayList";
		private IList<AmpacheSong> _playlist = new List<AmpacheSong>();

		public IList<AmpacheSong> Playlist
		{
			get { return _playlist; }
			set
			{
				if (_playlist == value)
					return;

				_playlist = value;
				if(PropertyChanged != null)
					PropertyChanged(this, new PropertyChangedEventArgs("PlayList"));
			}
		}

		#endregion
		
		#region Shuffling

		public const string SHUFFELING = "Shuffling";
		private bool _shuffling = false;

		public bool Shuffling
		{
			get { return _shuffling; }
			set
			{
				if (_shuffling == value)
					return;

				_shuffling = value;
				if(PropertyChanged != null)
					PropertyChanged(this, new PropertyChangedEventArgs("Shuffling"));
			}
		}

		#endregion
		
		#region Configuration

		public const string CONFIGURATION = "Configuration";
		private UserConfiguration _config = null;

		public UserConfiguration Configuration
		{
			get { return _config; }
			set
			{
				if (_config == value)
					return;

				_config = value;
				if(PropertyChanged != null)
					PropertyChanged(this, new PropertyChangedEventArgs("Configuration"));
			}
		}

		#endregion
		
		#region AlbumArtStream

		public const string ALBUM_ART_STREAM = "AlbumArtStream";
		private MemoryStream _artStream = null;

		public MemoryStream AlbumArtStream
		{
			get { return _artStream; }
			set
			{
				if (_artStream == value)
					return;

				_artStream = value;
				if(PropertyChanged != null)
					PropertyChanged(this, new PropertyChangedEventArgs("AlbumArtStream"));
			}
		}

		#endregion
		
		#region PlayPauseRequested

		public const string PLAY_PAUSE_REQUESTED = "PlayPauseRequested";
		private bool _playPauseRequested = false;

		public bool PlayPauseRequested
		{
			get { return _playPauseRequested; }
			set
			{
				if (_playPauseRequested == value)
					return;

				_playPauseRequested = value;
				if(PropertyChanged != null)
					PropertyChanged(this, new PropertyChangedEventArgs("PlayPauseRequested"));
			}
		}

		#endregion
		
		#region NextRequested

		public const string NEXT_REQUESTED = "NextRequested";
		private bool _nextRequested = false;

		public bool NextRequested
		{
			get { return _nextRequested; }
			set
			{
				if (_nextRequested == value)
					return;

				_nextRequested = value;
				if(PropertyChanged != null)
					PropertyChanged(this, new PropertyChangedEventArgs("NextRequested"));
			}
		}

		#endregion
		
		#region PreviousRequested

		public const string PREVIOUS_REQUESTED = "PreviousRequested";
		private bool _previousRequested = false;

		public bool PreviousRequested
		{
			get { return _previousRequested; }
			set
			{
				if (_previousRequested == value)
					return;

				_previousRequested = value;
				if(PropertyChanged != null)
					PropertyChanged(this, new PropertyChangedEventArgs("PreviousRequested"));
			}
		}

		#endregion
		
		#region IsPlaying

		public const string IS_PLIAYING = "IsPlaying";
		private bool _isPlaying = false;

		public bool IsPlaying
		{
			get { return _isPlaying; }
			set
			{
				if (_isPlaying == value)
					return;

				_isPlaying = value;
				if(PropertyChanged != null)
					PropertyChanged(this, new PropertyChangedEventArgs("IsPlaying"));
			}
		}

		#endregion
				
		#region INotifyPropertyChanged implementation
		public event PropertyChangedEventHandler PropertyChanged;
		#endregion
	}
}

