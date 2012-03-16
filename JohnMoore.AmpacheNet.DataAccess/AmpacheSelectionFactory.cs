//
// AmpacheSelectionFactory.cs
//
// Author:
//       John Moore <jcwmoore@gmail.com>
//
// Copyright (c) 2010 John Moore
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
using System.Threading;
using JohnMoore.AmpacheNet.Entities;
using System.IO;

namespace JohnMoore.AmpacheNet.DataAccess
{
    public class AmpacheSelectionFactory
    {
        private Authenticate _handshake;
		public static string ArtLocalDirectory = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), ".AmpacheNet");
		
		public AmpacheSelectionFactory ()
		{}
		
        public AmpacheSelectionFactory (Authenticate hs)
        {
            _handshake = hs;
        }

        public virtual IAmpacheSelector<TEntity> GetInstanceSelectorFor<TEntity>() where TEntity : IEntity
        {
            if (typeof(TEntity) == typeof(AmpacheArtist)) {
                return new ArtistSelector(_handshake, new ArtistFactory()) as IAmpacheSelector<TEntity>;
            }
            if (typeof(TEntity) == typeof(AmpacheAlbum)) {
                return new AlbumSelector(_handshake, new AlbumFactory()) as IAmpacheSelector<TEntity>;
            }
            if (typeof(TEntity) == typeof(AmpacheSong)) {
                return new SongSelector(_handshake, new SongFactory()) as IAmpacheSelector<TEntity>;
            }
            if (typeof(TEntity) == typeof(AmpachePlaylist)){
                return new PlaylistSelector(_handshake, new PlaylistFactory(), new SongFactory()) as IAmpacheSelector<TEntity>;
            }
			if (typeof(TEntity) == typeof(AlbumArt))
			{
				return new AlbumArtRepository(ArtLocalDirectory) as IAmpacheSelector<TEntity>;
			}
            throw new InvalidOperationException(string.Format("{0} is not yet supported for selection from ampache", typeof(TEntity).Name));
        }
		
		public virtual IPersistor<TEntity> GetPersistorFor<TEntity>() where TEntity : IEntity
		{
			if (typeof(TEntity) == typeof(AlbumArt))
			{
				return new AlbumArtRepository(ArtLocalDirectory) as IPersistor<TEntity>;
			}
            throw new InvalidOperationException(string.Format("{0} is not yet supported for persisting", typeof(TEntity).Name));
		}
		
		public virtual Authenticate AuthenticateToServer(string server, string user, string password)
		{
			var tmp = new Authenticate(server, user, password);
			_handshake = tmp;
			return tmp;
		}
		
		public virtual Authenticate AuthenticationTest(string server, string user, string password)
		{
			var tmp = new Authenticate(server, user, password);
			return tmp;
		}
		
		public virtual void Ping()
		{
			if(_handshake != null)
			{
				_handshake.Ping();
			}
		}
    }
}
