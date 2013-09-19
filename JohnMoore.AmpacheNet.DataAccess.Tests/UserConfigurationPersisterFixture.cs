//
// UserConfigurationPersisterFixture.cs
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
using NUnit.Framework;
using System;
using System.Data;
using System.Data.SQLite;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Security.Cryptography;
using JohnMoore.AmpacheNet.DataAccess;
using JohnMoore.AmpacheNet.Entities;
using NSubstitute;

namespace JohnMoore.AmpacheNet.DataAccess.Tests
{
	[TestFixture()]
	public class UserConfigurationPersisterFixture
	{
		[Test]
		public void UserConfigurationPersisterIsPersistedTest()
		{
			var conn = new SQLiteConnection("Data Source=:memory:");
			using(var target = new UserConfigurationPersister(conn)){
				Assert.That(target.IsPersisted(new AmpacheSong()));
			}
		}

		[Test]
		public void UserConfigurationPersisterPersistedTest()
		{
			var config = new UserConfiguration();
			config.AllowSeeking = false;
			config.CacheArt = false;
			config.CacheSongs = true;
			config.Password = @"password's";
			config.User = "user's";
			config.ServerUrl = "testserver";
			var conn = new SQLiteConnection("Data Source=:memory:");
			using(var cmd = conn.CreateCommand())
			using(var target = new UserConfigurationPersister(conn)){
				target.Persist(config);
				string format = @"select value from properties where key = '{0}';";
				cmd.CommandText = string.Format(format, "url");
				Assert.That(cmd.ExecuteScalar(), Is.EqualTo(config.ServerUrl));
				cmd.CommandText = string.Format(format, "user");
				Assert.That(cmd.ExecuteScalar(), Is.EqualTo(config.User.Replace(@"'", @"&quot;")));
				cmd.CommandText = string.Format(format, "password");
				Assert.That(cmd.ExecuteScalar(), Is.EqualTo(config.Password.Replace(@"'", @"&quot;")));
				cmd.CommandText = string.Format(format, "allowSeeking");
				Assert.That(bool.Parse(cmd.ExecuteScalar().ToString()), Is.EqualTo(config.AllowSeeking));
				cmd.CommandText = string.Format(format, "cacheArt");
				Assert.That(bool.Parse(cmd.ExecuteScalar().ToString()), Is.EqualTo(config.CacheArt));
			}
		}

		[Test]
		public void UserConfigurationPersisterRemoveTest()
		{
			var config = new UserConfiguration();
			config.AllowSeeking = false;
			config.CacheArt = false;
			config.CacheSongs = true;
			config.Password = @"password's";
			config.User = "user's";
			config.ServerUrl = "testserver";
			var conn = new SQLiteConnection("Data Source=:memory:");
			using(var cmd = conn.CreateCommand())
			using(var target = new UserConfigurationPersister(conn)){
				target.Remove(config);
				string format = @"select value from properties where key = '{0}';";
				cmd.CommandText = string.Format(format, "url");
				Assert.That(cmd.ExecuteScalar(), Is.EqualTo(string.Empty));
				cmd.CommandText = string.Format(format, "user");
				Assert.That(cmd.ExecuteScalar(), Is.EqualTo(String.Empty));
				cmd.CommandText = string.Format(format, "password");
				Assert.That(cmd.ExecuteScalar(), Is.EqualTo(string.Empty));
				cmd.CommandText = string.Format(format, "allowSeeking");
				Assert.That(bool.Parse(cmd.ExecuteScalar().ToString()), Is.True);
				cmd.CommandText = string.Format(format, "cacheArt");
				Assert.That(bool.Parse(cmd.ExecuteScalar().ToString()), Is.True);
			}
		}

		[Test]
		public void UserConfigurationPersisterSelectAllTest()
		{
			var config = new UserConfiguration();
			config.AllowSeeking = false;
			config.CacheArt = false;
			config.CacheSongs = true;
			config.Password = @"password's";
			config.User = "user's";
			config.ServerUrl = "testserver";
			var conn = new SQLiteConnection("Data Source=:memory:");
			using(var cmd = conn.CreateCommand())
			using(var target = new UserConfigurationPersister(conn)){
				target.Persist(config);
				var actual = target.SelectAll().ToList();
				Assert.That(actual.Count, Is.EqualTo(1));
				var res = actual.First();
				Assert.That(res.AllowSeeking, Is.EqualTo(config.AllowSeeking));
				Assert.That(res.CacheArt, Is.EqualTo(config.CacheArt));
				Assert.That(res.ServerUrl, Is.EqualTo(config.ServerUrl));
				Assert.That(res.User, Is.EqualTo(config.User));
				Assert.That(res.Password, Is.EqualTo(config.Password));
			}
		}

		[Test]
		public void UserConfigurationPersisterSelectByParameterTest()
		{
			var conn = new SQLiteConnection("Data Source=:memory:");
			using(var target = new UserConfigurationPersister(conn)){
				var actual = target.SelectBy(new AmpacheSong()).ToList();
				Assert.That(actual.Count, Is.EqualTo(1));
			}
		}

		[Test]
		public void UserConfigurationPersisterSelectByIdTest()
		{
			var conn = new SQLiteConnection("Data Source=:memory:");
			using(var target = new UserConfigurationPersister(conn)){
				var actual = target.SelectBy(123456);
				Assert.That(actual, Is.Not.Null);
			}
		}

		[Test]
		public void UserConfigurationPersisterSelectByStringTest()
		{
			var conn = new SQLiteConnection("Data Source=:memory:");
			using(var target = new UserConfigurationPersister(conn)){
				var actual = target.SelectBy("test").ToList();
				Assert.That(actual.Count, Is.EqualTo(1));
			}
		}

		[Test]
		public void UserConfigurationPersisterDisposeTest()
		{
			var mockCon = Substitute.For<IDbConnection>();
			bool opened = false, closed = false, disposed = false;
			mockCon.When(m=>m.Open()).Do(m => opened = true);
			mockCon.When(m=>m.Close()).Do(m => closed = true);
			mockCon.When(m=>m.Dispose()).Do(m => disposed = true);
			var target = new UserConfigurationPersisterHandle(mockCon);
			Assert.That(opened, Is.True);
			Assert.That(target.Initialized, Is.True);
			Assert.That(closed, Is.False);
			Assert.That(disposed, Is.False);
			opened = false;
			target.Dispose();
			Assert.That(opened, Is.False);
			Assert.That(closed, Is.True);
			Assert.That(disposed, Is.True);
		}		

		private class UserConfigurationPersisterHandle : SongPersister
		{
			public bool Initialized { get; private set; }

			public UserConfigurationPersisterHandle (IDbConnection conn) : base(conn) {}

			protected override void InitializeDatabase (IDbConnection conn)
			{
				Initialized = true;
			}
		}
	}
}

