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
			if (e.PropertyName ==AmpacheModel.PLAYING_SONG)
			{
				LoadAlbumImage();
			}
		}
		
		void LoadAlbumImage()
		{
			if(_model.PlayingSong == null)
			{
				_model.AlbumArtStream = _defaultStream;
				return;
			}
			try 
			{
				if(_currentStream != null)
				{
					_currentStream.Dispose();
				}
				var con = System.Net.WebRequest.Create(_model.PlayingSong.ArtUrl);
				_currentStream = new System.IO.MemoryStream();
				con.GetResponse().GetResponseStream().CopyTo(_currentStream);
				_model.AlbumArtStream = _currentStream;
			} 
			catch(Exception ex) 
			{
				_model.AlbumArtStream = _defaultStream;
				Console.WriteLine(string.Format("Exception of type {0} was thrown while downloading album art.", ex.GetType().Name));
				Console.WriteLine(ex.Message);
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

