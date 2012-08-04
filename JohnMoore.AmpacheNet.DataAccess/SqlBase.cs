using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace JohnMoore.AmpacheNet.DataAccess
{
	internal class SqlBase
	{
		private static readonly Dictionary<int, string> SQL_SCHEMA_VERSIONS = new Dictionary<int, string>();

		#region SQL Schema Strings
		private const string VERSION_SELECT = @"SELECT [Value] FROM [Properties] WHERE [Key] = 'Version' LIMIT 1;";
		private const string VERSION_000 = @"CREATE TABLE IF NOT EXISTS Properties (Key TEXT UNIQUE ON CONFLICT FAIL, Value TEXT); INSERT OR IGNORE INTO [Properties] VALUES('Version', '0');";
		private const string VERSION_001 = @"CREATE TABLE IF NOT EXISTS SongCache (SongId INTEGER UNIQUE ON CONFLICT FAIL, ArtistId INTEGER, AlbumId INTEGER, SongName TEXT, AlbumName TEXT, ArtistName TEXT, TrackNumber INTEGER, ArtUrl TEXT, TrackLengthSeconds FLOAT, SongURL TEXT, Rating INTEGER, PerciseRating INTEGER); UPDATE [Properties] SET [Value] = 1 WHERE [Key] = 'Version';";
		#endregion

		static SqlBase()
		{
			SQL_SCHEMA_VERSIONS.Add(0, VERSION_000);
			SQL_SCHEMA_VERSIONS.Add(1, VERSION_001);
		}

		protected SqlBase (IDbConnection conn)
		{
			if(conn.State != ConnectionState.Open){
				conn.Open();
			}
			InitializeDatabase(conn);
		}

		protected virtual void InitializeDatabase(IDbConnection conn)
		{
			using(var cmd = conn.CreateCommand()){
				cmd.CommandText = VERSION_000;
				cmd.ExecuteNonQuery();
				cmd.CommandText = VERSION_SELECT;
				var version = Convert.ToInt32(cmd.ExecuteScalar());
				while(version < SQL_SCHEMA_VERSIONS.Keys.Max()){
					++version;
					cmd.CommandText = SQL_SCHEMA_VERSIONS[version];
					cmd.ExecuteNonQuery();
				}
			}
		}
	}
}

