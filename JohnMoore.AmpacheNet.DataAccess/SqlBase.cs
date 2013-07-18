//
// SqlBase.cs
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
		private const string VERSION_002 = @"
INSERT INTO [Properties] VALUES ('url', '');
INSERT INTO [Properties] VALUES ('user', '');
INSERT INTO [Properties] VALUES ('password', '');
INSERT INTO [Properties] VALUES ('cacheArt', 'true');
INSERT INTO [Properties] VALUES ('allowSeeking', 'true');
UPDATE [Properties] SET [Value] = 2 WHERE [Key] = 'Version';";
		#endregion

		static SqlBase()
		{
			SQL_SCHEMA_VERSIONS.Add(0, VERSION_000);
			SQL_SCHEMA_VERSIONS.Add(1, VERSION_001);
			SQL_SCHEMA_VERSIONS.Add(2, VERSION_002);
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

