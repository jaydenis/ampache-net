using System;
using System.Collections.Generic;
using System.Data;
using JohnMoore.AmpacheNet.Entities;

namespace JohnMoore.AmpacheNet.DataAccess
{
	internal class SongPersister : SqlBase, IPersister<AmpacheSong>
	{
		private readonly IDbConnection _conn;
		private const string EXISTS_SQL = "SELECT COUNT(*) FROM [SongCache] WHERE [SongId] = {0};";
		private const string DELETE_SQL = "DELETE FROM [SongCache] WHERE [SongId] = {0};";
		private const string INSERT_SQL = @"INSERT OR IGNORE INTO [SongCache] VALUES ({0}, {1}, {2}, '{3}', '{4}', '{5}', {6}, '{7}', {8}, '{9}', {10}, {11})";
		private const string SELECT_ALL_SQL = "SELECT * FROM [SongCache];";

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
			using (var cmd = _conn.CreateCommand()) {
				cmd.CommandText = string.Format(INSERT_SQL, entity.Id, entity.ArtistId, entity.AlbumId, entity.Name.Replace(@"'", "&QUOT;"), entity.AlbumName.Replace(@"'", "&QUOT;"), entity.ArtistName.Replace(@"'", "&QUOT;"), entity.TrackNumber, entity.ArtUrl, entity.TrackLength.TotalSeconds, entity.Url, entity.Rating, entity.PerciseRating);
				cmd.ExecuteNonQuery();
			}
		}

		public void Remove (AmpacheSong entity)
		{
			using(var cmd = _conn.CreateCommand()){
				cmd.CommandText = string.Format(DELETE_SQL, entity.Id);
				cmd.ExecuteNonQuery();
			}
		}
		#endregion		

		#region IAmpacheSelector implementation
		public System.Collections.Generic.IEnumerable<AmpacheSong> SelectAll ()
		{
			using(var cmd = _conn.CreateCommand()){
				cmd.CommandText = SELECT_ALL_SQL;
				var reader = cmd.ExecuteReader();
				var res = new List<AmpacheSong>();
				while(reader.Read()){
					res.Add(MapSong(reader));
				}
				return res;
			}
		}

		public System.Collections.Generic.IEnumerable<AmpacheSong> SelectBy<TParameter> (TParameter parameter) where TParameter : IEntity
		{
			throw new System.NotSupportedException ();
		}

		public System.Collections.Generic.IEnumerable<AmpacheSong> SelectBy (string searchText)
		{
			throw new System.NotSupportedException ();
		}

		public AmpacheSong SelectBy (int ampacheId)
		{
			throw new System.NotSupportedException ();
		}
		#endregion

		#region IDisposable implementation
		public void Dispose ()
		{
			_conn.Close();
			_conn.Dispose();
		}
		#endregion

		private static AmpacheSong MapSong(IDataReader reader)
		{
			var res =  new AmpacheSong();
			res.Id = (int)reader["SongId"];
			res.ArtistId = (int)reader["ArtistId"];
			res.AlbumId = (int)reader["AlbumId"];
			res.Name = ((string)reader["SongName"]).Replace("&QUOT;", @"'");
			res.AlbumName = ((string)reader["AlbumName"]).Replace("&QUOT;", @"'");
			res.ArtistName = ((string)reader["ArtistName"]).Replace("&QUOT;", @"'");
			res.TrackNumber = (int)reader["TrackNumber"];
			res.ArtUrl = (string)reader["ArtUrl"];
			res.TrackLength = TimeSpan.FromSeconds((double)reader["TrackLengthSeconds"]);
			res.Url = (string)reader["SongUrl"];
			res.Rating = (int)reader["Rating"];
			res.PerciseRating = (int)reader["PerciseRating"];
			return res;
		}
	}
}

