//
// UserConfigurationPersister.cs
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
using System.Data;
using System.Linq;
using JohnMoore.AmpacheNet.Entities;

namespace JohnMoore.AmpacheNet.DataAccess
{
	internal class UserConfigurationPersister : SqlBase, IPersister<UserConfiguration>
	{
		private const string URL_KEY = "url";
		private const string USER_NAME_KEY = "user";
		private const string PASSWORD_KEY = "password";
		private const string ALLOW_SEEKING_KEY = "allowSeeking";
		private const string CACHE_ART_KEY = "cacheArt";
		private const string UPDATE_SQL = @"UPDATE [Properties] SET [Value] = '{1}' WHERE [Key] = '{0}';";
		private const string SELECT_SQL = "SELECT [Value] FROM [Properties] WHERE [Key] = '{0}';";

		private readonly IDbConnection _conn;

		public UserConfigurationPersister (IDbConnection conn) : base(conn)
		{
			_conn = conn;
		}

		#region IAmpacheSelector implementation
		public System.Collections.Generic.IEnumerable<UserConfiguration> SelectAll ()
		{
			var res = new UserConfiguration();
			using(var cmd = _conn.CreateCommand()){
				cmd.CommandText = string.Format(SELECT_SQL, ALLOW_SEEKING_KEY);
				res.AllowSeeking = bool.Parse(cmd.ExecuteScalar().ToString());
				cmd.CommandText = string.Format(SELECT_SQL, CACHE_ART_KEY);
				res.CacheArt = bool.Parse(cmd.ExecuteScalar().ToString());
				cmd.CommandText = string.Format(SELECT_SQL, USER_NAME_KEY);
				res.User = cmd.ExecuteScalar().ToString().Replace("&quot;", @"'");
				cmd.CommandText = string.Format(SELECT_SQL, PASSWORD_KEY);
				res.Password = cmd.ExecuteScalar().ToString().Replace("&quot;", @"'");
				cmd.CommandText = string.Format(SELECT_SQL, URL_KEY);
				res.ServerUrl = cmd.ExecuteScalar().ToString();
			}
			return new [] {res};
		}

		public System.Collections.Generic.IEnumerable<UserConfiguration> SelectBy<TParameter> (TParameter parameter) where TParameter : IEntity
		{
			return SelectAll();
		}

		public System.Collections.Generic.IEnumerable<UserConfiguration> SelectBy (string searchText)
		{
			return SelectAll();
		}

		public UserConfiguration SelectBy (int ampacheId)
		{
			return SelectAll().First();
		}
		#endregion

		#region IDisposable implementation
		public void Dispose ()
		{
			_conn.Close();
			_conn.Dispose();
		}
		#endregion

		#region IPersister implementation
		public bool IsPersisted<TParameter> (TParameter parameter) where TParameter : IEntity
		{
			return true;
		}

		public void Persist (UserConfiguration entity)
		{
			using(var cmd = _conn.CreateCommand()){
				cmd.CommandText = string.Format(UPDATE_SQL, ALLOW_SEEKING_KEY, entity.AllowSeeking);
				cmd.ExecuteNonQuery();
				cmd.CommandText = string.Format(UPDATE_SQL, CACHE_ART_KEY, entity.CacheArt);
				cmd.ExecuteNonQuery();
				cmd.CommandText = string.Format(UPDATE_SQL, USER_NAME_KEY, entity.User.Replace(@"'", @"&quot;"));
				cmd.ExecuteNonQuery();
				cmd.CommandText = string.Format(UPDATE_SQL, PASSWORD_KEY, entity.Password.Replace(@"'", @"&quot;"));
				cmd.ExecuteNonQuery();
				cmd.CommandText = string.Format(UPDATE_SQL, URL_KEY, entity.ServerUrl);
				cmd.ExecuteNonQuery();
			}
		}

		public void Remove (UserConfiguration entity)
		{
			using(var cmd = _conn.CreateCommand()){
				cmd.CommandText = string.Format(UPDATE_SQL, ALLOW_SEEKING_KEY, true);
				cmd.ExecuteNonQuery();
				cmd.CommandText = string.Format(UPDATE_SQL, CACHE_ART_KEY, true);
				cmd.ExecuteNonQuery();
				cmd.CommandText = string.Format(UPDATE_SQL, USER_NAME_KEY, string.Empty);
				cmd.ExecuteNonQuery();
				cmd.CommandText = string.Format(UPDATE_SQL, PASSWORD_KEY, string.Empty);
				cmd.ExecuteNonQuery();
				cmd.CommandText = string.Format(UPDATE_SQL, URL_KEY, string.Empty);
				cmd.ExecuteNonQuery();
			}
		}
		#endregion

	}
}

