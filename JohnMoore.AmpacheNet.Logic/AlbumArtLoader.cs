//
// AlbumArtLoader.cs
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
using System.Net;
using System.IO;
using System.Linq;
using JohnMoore.AmpacheNet.Entities;

namespace JohnMoore.AmpacheNet.Logic
{
	public class AlbumArtLoader : IDisposable
	{
		private readonly AmpacheModel _model;
		private readonly MemoryStream _defaultStream;
		private MemoryStream _currentStream;
		
		public AlbumArtLoader (AmpacheModel model, MemoryStream defaultStream)
		{
			_defaultStream = defaultStream;
			_model = model;
			_model.PropertyChanged += Handle_modelPropertyChanged;
			LoadAlbumImage();
		}

		void Handle_modelPropertyChanged (object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			if (e.PropertyName == AmpacheModel.PLAYING_SONG && _model.PlayingSong != null)
			{
				System.Threading.ThreadPool.QueueUserWorkItem((o) => LoadAlbumImage());
			}
		}
		
		void LoadAlbumImage()
		{
			if(string.IsNullOrEmpty((_model.PlayingSong ?? new AmpacheSong()).ArtUrl))
			{
				_model.AlbumArtStream = _defaultStream;
				return;
			}
			if(_currentStream != null)
			{
				_currentStream.Dispose();
			}
			var persit = _model.Factory.GetPersistorFor<AlbumArt>();
			if(!persit.IsPersisted(_model.PlayingSong))
			{
				_model.AlbumArtStream = _defaultStream;
			}
			var sel = _model.Factory.GetInstanceSelectorFor<AlbumArt>();
			var art = sel.SelectBy(_model.PlayingSong).FirstOrDefault();
			if(art != null)
			{
				art.ArtStream.Position = 0;
				_currentStream = art.ArtStream;
				_model.AlbumArtStream = _currentStream;
				if (_model.Configuration.CacheArt && !persit.IsPersisted (art)) 
				{
					persit.Persist (art);
				}
			}
			else
			{
				_model.AlbumArtStream = _defaultStream;
			}
		}
		
		#region IDisposable implementation
		public void Dispose ()
		{
			_defaultStream.Dispose();
		}
		#endregion
	}
}

