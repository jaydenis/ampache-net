using System;
using System.Data;
using JohnMoore.AmpacheNet.Entities;

namespace JohnMoore.AmpacheNet.DataAccess
{
	internal class SongPersister : SqlBase, IPersister<AmpacheSong>
	{
		private readonly IDbConnection _conn;
		private const string EXISTS_SQL = "SELECT COUNT(*) FROM [SongCache] WHERE [SongId] = {0}";
		private const string DELETE_SQL = "DELETE FROM [SongCache] WHERE [SongId] = {0}";
		private const string INSERT_SQL = "INSERT OR IGNORE INTO [SongCache] VALUES ({0}, {1}, {2}, {3}, {4}, {5}, {6}, {7}, {8}, {9}, {10}, {11})";

		public SongPersister (IDbConnection conn) : base(conn)
		{
			_conn = conn;
		}

		#region IPersistor implementation
		
		public bool IsPersisted<TParameter> (TParameter parameter) where TParameter : IEntity
		{
			if(parameter is AmpacheSong){
				return IsPersisted(parameter as AmpacheSong);
			}
			throw new InvalidOperationException();
		}

		private bool IsPersisted (AmpacheSong entity)
		{
			using(var cmd = _conn.CreateCommand()){
				cmd.CommandText = string.Format(EXISTS_SQL, entity.Id);
				int res;
				int.TryParse(cmd.ExecuteScalar().ToString(), out res);
				return res == 1;
			}
		}

		public void Persist (AmpacheSong entity)
		{
			throw new System.NotImplementedException ();
		}

		public void Remove (AmpacheSong entity)
		{
			throw new System.NotImplementedException ();
		}
		#endregion		

		#region IAmpacheSelector implementation
		public System.Collections.Generic.IEnumerable<AmpacheSong> SelectAll ()
		{
			throw new System.NotImplementedException ();
		}

		public System.Collections.Generic.IEnumerable<AmpacheSong> SelectBy<TParameter> (TParameter parameter) where TParameter : IEntity
		{
			throw new System.NotImplementedException ();
		}

		public System.Collections.Generic.IEnumerable<AmpacheSong> SelectBy (string searchText)
		{
			throw new System.NotImplementedException ();
		}

		public AmpacheSong SelectBy (int ampacheId)
		{
			throw new System.NotImplementedException ();
		}
		#endregion

		#region IDisposable implementation
		public void Dispose ()
		{
			_conn.Close();
			_conn.Dispose();
		}
		#endregion


	}
}

