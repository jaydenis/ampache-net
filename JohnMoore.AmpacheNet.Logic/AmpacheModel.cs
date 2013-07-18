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
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel;
using JohnMoore.AmpacheNet.Entities;
using JohnMoore.AmpacheNet.DataAccess;
using Athena;

namespace JohnMoore.AmpacheNet.Logic
{
	public class AmpacheModel : INotifyPropertyChanged, IDisposable
	{
        public AmpacheModel()
        {
            IsDisposed = false;
        }

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

		/// <summary>
		/// Gets or sets the configuration.  Changing the value of this member should be synchronous
		/// </summary>
		/// <value>
		/// The configuration.
		/// </value>
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

		public const string IS_PLAYING = "IsPlaying";
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
				
		#region StopRequested

		public const string STOP_REQUESTED = "StopRequested";
		private bool _stopRequested = false;

		public bool StopRequested
		{
			get { return _stopRequested; }
			set
			{
				if (_stopRequested == value)
					return;

				_stopRequested = value;
				if(PropertyChanged != null)
					PropertyChanged(this, new PropertyChangedEventArgs("StopRequested"));
			}
		}

		#endregion
		
		#region PercentDownloaded

		public const string PERCENT_DOWNLOADED = "PercentDownloaded";
		private double _percentDownloaded = 0.0;

		public double PercentDownloaded
		{
			get { return _percentDownloaded; }
			set
			{
				if (_percentDownloaded == value)
					return;

				_percentDownloaded = value;
				if(PropertyChanged != null)
					PropertyChanged(this, new PropertyChangedEventArgs("PercentDownloaded"));
			}
		}

		#endregion
		
		#region PercentPlayed

		public const string PERCENT_PLAYED = "PercentPlayed";
		private double _percentPlayed = 0.0;

		public double PercentPlayed
		{
			get { return _percentPlayed; }
			set
			{
				if (_percentPlayed == value)
					return;

				_percentPlayed = value;
				if(PropertyChanged != null)
					PropertyChanged(this, new PropertyChangedEventArgs("PercentPlayed"));
			}
		}

		#endregion
		
		#region RequestedSeekToPercentage

		public const string REQUESTED_SEEK_TO_PERCENTAGE = "RequestedSeekToPercentage";
		private double? _requestedSeekToPercentage = null;

		public double? RequestedSeekToPercentage
		{
			get { return _requestedSeekToPercentage; }
			set
			{
				if (_requestedSeekToPercentage == value)
					return;

				_requestedSeekToPercentage = value;
				if(PropertyChanged != null)
					PropertyChanged(this, new PropertyChangedEventArgs("RequestedSeekToPercentage"));
			}
		}

		#endregion
		
		#region UserMessage

		public const string USER_MESSAGE = "UserMessage";
		private string _userMessage = null;

		public string UserMessage
		{
			get { return _userMessage; }
			set
			{
				if (_userMessage == value)
					return;

				_userMessage = value;
				if(PropertyChanged != null)
					PropertyChanged(this, new PropertyChangedEventArgs("UserMessage"));
			}
		}
		
		#endregion
		
		#region IsDisposed

		public const string IS_DISPOSED = "IsDisposed";
		private bool _isDisposed = false;

		public bool IsDisposed
		{
			get { return _isDisposed; }
			private set
			{
				if (_isDisposed == value)
					return;

				_isDisposed = value;
				if(PropertyChanged != null)
					PropertyChanged(this, new PropertyChangedEventArgs("IsDisposed"));
			}
		}

		#endregion
		
		
		#region INotifyPropertyChanged implementation
		public event PropertyChangedEventHandler PropertyChanged;
		#endregion
		
		#region IDisposable implementation
		public void Dispose ()
		{
			IsDisposed = true;
			if(PropertyChanged != null)
			{
				foreach(var del in PropertyChanged.GetInvocationList().OfType<PropertyChangedEventHandler>())
				{
					PropertyChanged -= del;
				}
			}
		}
		#endregion
	}
}

