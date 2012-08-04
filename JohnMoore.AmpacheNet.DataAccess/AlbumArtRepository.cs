//
// AlbumArtRepository.cs
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
using System.IO;
using JohnMoore.AmpacheNet.Entities;
using System.Collections.Generic;

namespace JohnMoore.AmpacheNet.DataAccess
{
	public class AlbumArtRepository : IPersister<AlbumArt>
	{
		private readonly string _directory;
		
		public AlbumArtRepository (string directory)
		{
			if(!Directory.Exists(directory))
			{
				Directory.CreateDirectory(directory);
			}
			_directory = directory;
		}

		#region IAmpacheSelector[JohnMoore.AmpacheNet.Entities.AlbumArt] implementation
		public System.Collections.Generic.IEnumerable<JohnMoore.AmpacheNet.Entities.AlbumArt> SelectAll ()
		{
			throw new NotSupportedException();
		}
	
		public System.Collections.Generic.IEnumerable<JohnMoore.AmpacheNet.Entities.AlbumArt> SelectBy<TParameter> (TParameter parameter) where TParameter : JohnMoore.AmpacheNet.Entities.IEntity
		{
			var art = parameter as IArt;
			if(art == null)
			{
				throw new InvalidOperationException(string.Format("Can not select art with a {0} parameter", typeof(TParameter).Name));
			}
			var res = new List<AlbumArt>();
			try 
			{
				if (File.Exists (Path.Combine (_directory, art.ArtId.ToString ())))
				{
					using (var file = File.OpenRead (Path.Combine (_directory, art.ArtId.ToString ())))
					{
						var stream = new MemoryStream();
						file.CopyTo(stream);
						stream.Position = 0;
						res.Add (new AlbumArt { AlbumId = art.ArtId, ArtStream =  stream });
					}
				} 
				else 
				{
					var con = System.Net.WebRequest.Create (art.ArtUrl);
					var stream = new MemoryStream();
					con.GetResponse ().GetResponseStream ().CopyTo(stream);
					stream.Position = 0;
					res.Add (new AlbumArt { AlbumId = art.ArtId, ArtStream = stream });
				}
			}
			catch (Exception ex)
			{
				Console.WriteLine(string.Format("Exception of type {0} was thrown while getting album art.", ex.GetType().Name));
				Console.WriteLine(ex.Message);
			}
			return res;
		}
	
		public System.Collections.Generic.IEnumerable<JohnMoore.AmpacheNet.Entities.AlbumArt> SelectBy (string searchText)
		{
			throw new NotSupportedException();
		}
	
		public JohnMoore.AmpacheNet.Entities.AlbumArt SelectBy (int ampacheId)
		{
			throw new NotSupportedException();
		}
		#endregion

		#region IPersistor[JohnMoore.AmpacheNet.Entities.AlbumArt] implementation
		public bool IsPersisted<TParamter> (TParamter paramter) where TParamter : IEntity
		{
			if(paramter is IArt){
				return IsPersisted(paramter as IArt);
			}
			if(paramter is AlbumArt){
				return File.Exists(Path.Combine(_directory, (paramter as AlbumArt).AlbumId.ToString()));
			}
			throw new NotImplementedException();
		}
		
		public bool IsPersisted (JohnMoore.AmpacheNet.Entities.IArt entity)
		{
			return File.Exists(Path.Combine(_directory, entity.ArtId.ToString()));
		}
	
		public void Persist (JohnMoore.AmpacheNet.Entities.AlbumArt entity)
		{
			if(IsPersisted(entity))
			{
				Remove(entity);
			}
			var stream = File.Create(Path.Combine(_directory, entity.AlbumId.ToString()));
			entity.ArtStream.Position = 0;
			entity.ArtStream.CopyTo(stream);
			entity.ArtStream.Position = 0;
			stream.Close();
		}
	
		public void Remove (JohnMoore.AmpacheNet.Entities.AlbumArt entity)
		{
			if(IsPersisted(entity))
			{
				File.Delete(Path.Combine(_directory, entity.AlbumId.ToString()));
			}
		}
		#endregion

		#region IDisposable implementation
		public void Dispose ()
		{
		}
		#endregion


	}
}

