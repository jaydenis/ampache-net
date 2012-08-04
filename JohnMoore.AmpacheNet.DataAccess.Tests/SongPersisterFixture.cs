using NUnit.Framework;
using System;
using System.Data;
using System.Data.Sqlite;
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
	public class SongPersisterFixture
	{
		[Test()]
		public void SongPersisterIsPersistedTest()
		{
			var conn = new SqliteConnection("Data Source=:memory:");
			using(var target = new SongPersister(conn))
			using(var cmd = conn.CreateCommand())
			{
				var song = new AmpacheSong();
				song.Id =  555;
				cmd.CommandText = string.Format("INSERT INTO SongCache (SongId) VALUES ({0});", song.Id);
				cmd.ExecuteNonQuery();
				Assert.That(target.IsPersisted(song), Is.True);
			}

		}
		[Test()]
		public void SongPersisterIsNotPersistedTest()
		{
			var conn = new SqliteConnection("Data Source=:memory:");
			using(var target = new SongPersister(conn))
			using(var cmd = conn.CreateCommand())
			{
				var song = new AmpacheSong();
				song.Id = 555;
				Assert.That(target.IsPersisted(song), Is.False);
			}

		}

		[Test]
		public void SongPersisterPersistTest()
		{
			
			var conn = new SqliteConnection("Data Source=:memory:");
			using(var target = new SongPersister(conn))
			using(var cmd = conn.CreateCommand())
			{
				var song = new AmpacheSong();
				song.Id = 555;
				song.AlbumId = 88;
				song.AlbumName = "album name";
				song.ArtistId =45;
				song.ArtistName = "artist";
				song.ArtUrl = "url";
				song.Name = "name";
				song.PerciseRating = 2456;
				song.Rating = 24;
				song.Tags = new List<Tag>();
				song.TrackLength = TimeSpan.FromSeconds(2442);
				song.TrackNumber = 5;
				song.Url = "sasasdf";
				Assert.That(target.IsPersisted(song), Is.False);
				target.Persist(song);
				cmd.CommandText = string.Format("select * from SongCache where SongId = {0};", song.Id);
				var reader = cmd.ExecuteReader();
				Assert.That(reader.Read(), Is.True);
				Assert.That(reader["SongId"], Is.EqualTo(song.Id));
				Assert.That(reader["ArtistId"], Is.EqualTo(song.ArtistId));
				Assert.That(reader["AlbumId"], Is.EqualTo(song.AlbumId));
				Assert.That(reader["SongName"], Is.EqualTo(song.Name));
				Assert.That(reader["AlbumName"], Is.EqualTo(song.AlbumName));
				Assert.That(reader["ArtistName"], Is.EqualTo(song.ArtistName));
				Assert.That(reader["TrackNumber"], Is.EqualTo(song.TrackNumber));
				Assert.That(reader["ArtUrl"], Is.EqualTo(song.ArtUrl));
				Assert.That(reader["TrackLengthSeconds"], Is.EqualTo(song.TrackLength.TotalSeconds));
				Assert.That(reader["SongUrl"], Is.EqualTo(song.Url));
				Assert.That(reader["Rating"], Is.EqualTo(song.Rating));
				Assert.That(reader["PerciseRating"], Is.EqualTo(song.PerciseRating));
				// NOTE: tags are not persisted!
			}
		}

		[Test]
		public void SongPersisterRemoveTest()
		{
			
			var conn = new SqliteConnection("Data Source=:memory:");
			using(var target = new SongPersister(conn))
			using(var cmd = conn.CreateCommand())
			{
				var song = new AmpacheSong();
				song.Id = 555;
				song.AlbumId = 88;
				song.AlbumName = "album name";
				song.ArtistId =45;
				song.ArtistName = "artist";
				song.ArtUrl = "url";
				song.Name = "name";
				song.PerciseRating = 2456;
				song.Rating = 24;
				song.Tags = new List<Tag>();
				song.TrackLength = TimeSpan.FromSeconds(2442);
				song.TrackNumber = 5;
				song.Url = "sasasdf";
				Assert.That(target.IsPersisted(song), Is.False);
				target.Persist(song);
				cmd.CommandText = string.Format("select COUNT(*) from SongCache where SongId = {0};", song.Id);
				Assert.That(cmd.ExecuteScalar(), Is.EqualTo(1));
				target.Remove(song);
				Assert.That(cmd.ExecuteScalar(), Is.EqualTo(0));

			}
		}

		[Test]
		public void SongPersisterSelectAllTest()
		{
			
			var conn = new SqliteConnection("Data Source=:memory:");
			using(var target = new SongPersister(conn))
			using(var cmd = conn.CreateCommand())
			{
				var song = new AmpacheSong();
				song.Id = 555;
				song.AlbumId = 88;
				song.AlbumName = "album name";
				song.ArtistId =45;
				song.ArtistName = "artist";
				song.ArtUrl = "url";
				song.Name = "name";
				song.PerciseRating = 2456;
				song.Rating = 24;
				song.Tags = new List<Tag>();
				song.TrackLength = TimeSpan.FromSeconds(2442);
				song.TrackNumber = 5;
				song.Url = "sasasdf";
				Assert.That(target.IsPersisted(song), Is.False);
				target.Persist(song);
				cmd.CommandText = string.Format("select COUNT(*) from SongCache where SongId = {0};", song.Id);
				Assert.That(cmd.ExecuteScalar(), Is.EqualTo(1));
				var actual = target.SelectAll().ToList();
				Assert.That(actual, Is.Not.Null);
				Assert.That(actual.Count, Is.EqualTo(1));
				var first = actual.First();
				Assert.That(first.Id, Is.EqualTo(song.Id));
				Assert.That(first.ArtistId, Is.EqualTo(song.ArtistId));
				Assert.That(first.AlbumId, Is.EqualTo(song.AlbumId));
				Assert.That(first.Name, Is.EqualTo(song.Name));
				Assert.That(first.AlbumName, Is.EqualTo(song.AlbumName));
				Assert.That(first.ArtistName, Is.EqualTo(song.ArtistName));
				Assert.That(first.TrackNumber, Is.EqualTo(song.TrackNumber));
				Assert.That(first.ArtUrl, Is.EqualTo(song.ArtUrl));
				Assert.That(first.TrackLength, Is.EqualTo(song.TrackLength));
				Assert.That(first.Url, Is.EqualTo(song.Url));
				Assert.That(first.Rating, Is.EqualTo(song.Rating));
				Assert.That(first.PerciseRating, Is.EqualTo(song.PerciseRating));
			}
		}

		[Test]
		[ExpectedException(typeof(NotSupportedException))]
		public void SongPersisterSelectByParameterTest ()
		{
			var conn = new SqliteConnection("Data Source=:memory:");
			using(var target = new SongPersister(conn))
			{
				target.SelectBy(new AmpacheSong());
				Assert.Fail();
			}
		}

		[Test]
		[ExpectedException(typeof(NotSupportedException))]
		public void SongPersisterSelectByIntegerTest ()
		{
			var conn = new SqliteConnection("Data Source=:memory:");
			using(var target = new SongPersister(conn))
			{
				target.SelectBy(1);
				Assert.Fail();
			}
		}

		[Test]
		[ExpectedException(typeof(NotSupportedException))]
		public void SongPersisterSelectByStringTest ()
		{
			var conn = new SqliteConnection("Data Source=:memory:");
			using(var target = new SongPersister(conn))
			{
				target.SelectBy("test");
				Assert.Fail();
			}
		}

		[Test]
		public void SongPersistorDisposeTest()
		{
			var mockCon = Substitute.For<IDbConnection>();
			bool opened = false, closed = false, disposed = false;
			mockCon.When(m=>m.Open()).Do(m => opened = true);
			mockCon.When(m=>m.Close()).Do(m => closed = true);
			mockCon.When(m=>m.Dispose()).Do(m => disposed = true);
			var target = new SongPersisterHandle(mockCon);
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

		private class SongPersisterHandle : SongPersister
		{
			public bool Initialized { get; private set; }

			public SongPersisterHandle (IDbConnection conn) : base(conn) {}

			protected override void InitializeDatabase (IDbConnection conn)
			{
				Initialized = true;
			}
		}
	}
}

