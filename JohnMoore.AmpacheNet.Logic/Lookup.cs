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
		private readonly AmpacheModel _model;
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
		
		public Lookup (TimeSpan cacheTimeToLive, AmpacheModel model)
		{
			_cacheTimeToLive = cacheTimeToLive;
			_model = model;
		}
		
		public ICollection<TEntity> LoadAll()
		{
			if(CachedEntites == null)
			{
				var selector = _model.Factory.GetInstanceSelectorFor<TEntity>();
				_cachedEntities = selector.SelectAll().OrderBy(e => e.Name).ToList();
			}
			return _cachedEntities;
		}
		
		public ICollection<TEntity> Search(string searchText)
		{
			ICollection<TEntity> res = null;
			if(CachedEntites == null)
			{
				var selector = _model.Factory.GetInstanceSelectorFor<TEntity>();
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
				var selector = _model.Factory.GetInstanceSelectorFor<AmpacheSong>();
				var res = selector.SelectBy(entity).ToList();
				_loadedSongs.Add(entity, res);
			}
			return _loadedSongs[entity];
		}
	}
}

