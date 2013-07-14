//
// Lookup.cs
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
using System.Linq;
using JohnMoore.AmpacheNet.Entities;
using JohnMoore.AmpacheNet.Logic;

namespace JohnMoore.AmpacheNet.Logic
{
	/// <summary>
	/// This class contains a set of logic for use with searching
	/// or listing views, i.e. a view to list artists, to assist with
	/// the caching of the result set to avoid excessive calls to the 
	/// server, this class is parital so that your view class may inherit
	/// the proper view base class on different platforms as necessary
	/// </summary>
	public partial class Lookup<TEntity> where TEntity : IEntity
	{
		private readonly TimeSpan _cacheTimeToLive;
		protected DateTime _cacheLoadTime = DateTime.MinValue;
		protected static ICollection<TEntity> _cachedEntities;
		protected AmpacheModel _model;
		private readonly Dictionary<TEntity, ICollection<AmpacheSong>> _loadedSongs = new Dictionary<TEntity, ICollection<AmpacheSong>>();
		
		public ICollection<TEntity> CachedEntites
		{
			get
			{
				if(DateTime.Now - _cacheTimeToLive < _cacheLoadTime)
				{
					return _cachedEntities;
				}
				return null;
			}
				
		}
		
		public Lookup (TimeSpan cacheTimeToLive)
		{
			_cacheTimeToLive = cacheTimeToLive;
		}
		
		public Lookup (TimeSpan cacheTimeToLive, AmpacheModel model)
		{
			_cacheTimeToLive = cacheTimeToLive;
			_model = model;
		}
		
		public ICollection<TEntity> LoadAll()
		{
			if(CachedEntites == null)
			{
				var selector = _model.Container.Resolve<DataAccess.IAmpacheSelector<TEntity>>();
				try 
				{
					_cachedEntities = selector.SelectAll ().OrderBy (e => e.Name).ToList ();
					_cacheLoadTime = DateTime.Now;
				}
				catch (Exception ex)
				{
					_model.UserMessage = ex.Message;
					Console.WriteLine (ex.Message);
				}
			}
			return _cachedEntities;
		}
		
		public ICollection<TEntity> Search(string searchText)
		{
			ICollection<TEntity> res = null;
			if(CachedEntites == null)
			{
                var selector = _model.Container.Resolve<DataAccess.IAmpacheSelector<TEntity>>();
				res = selector.SelectBy(searchText).ToList();
			}
			else
			{
				res = _cachedEntities.Where(e => e.Name.ToLower().Contains(searchText)).ToList();
			}
			return res;
		}
		
		public ICollection<AmpacheSong> LoadSongsForEntity(TEntity entity)
		{
			if(!_loadedSongs.ContainsKey(entity))
			{
				var selector = _model.Container.Resolve<DataAccess.IAmpacheSelector<AmpacheSong>>();
				var res = selector.SelectBy(entity).ToList();
				_loadedSongs.Add(entity, res);
			}
			return _loadedSongs[entity];
		}
	}
}

