using System;
using NUnit.Framework;
using JohnMoore.AmpacheNet.Entities;
using JohnMoore.AmpacheNet.Logic;

namespace JohnMoore.AmpacheNet.Logic.Tests
{
	[TestFixture()]
	public class SongHandshakeRemapperFixture
	{
		private const string VALID_URL = @"http://server.com/play/index.php?ssid=SESSION&oid=SONGID&uid=2&name=/Dama%20-%20Eliot.mp3";
		private const string URL_MISSING_NAME = @"http://server.com/play/index.php?ssid=SESSION&oid=SONGID&uid=2";
		private const string URL_MISSING_USER = @"http://server.com/play/index.php?ssid=SESSION&oid=SONGID&name=/Dama%20-%20Eliot.mp3";
		private const string ART_URL = @"http://server.com/image.php?id=ID&auth=AUTH&name=art.jpg";

		[Test()]
		[ExpectedException(typeof(ArgumentNullException))]
		public void SongHandshakeRemapperNullHandshakeTest()
		{
			var sng = new AmpacheSong();
			sng.MapToHandshake(null);
			Assert.Fail();
		}
		
		[Test()]
		public void SongHandshakeRemapperFullUrlTest()
		{
			var sng = new AmpacheSong();
			sng.Url = VALID_URL;
			sng.Id = 630;
			sng.AlbumId = 50;
			sng.ArtUrl = ART_URL;
			var handshake = new MockHandshake();
			handshake.SetPassphrase("test");
			handshake.SetServer("http://server.com");
			sng.MapToHandshake(handshake);
			Assert.That(sng.Url, Is.EqualTo(VALID_URL.Replace("SESSION", handshake.Passphrase).Replace("SONGID", sng.Id.ToString())));
			Assert.That(sng.ArtUrl, Is.EqualTo(ART_URL.Replace("ID", sng.ArtId.ToString()).Replace("AUTH", handshake.Passphrase)));
		}
		
		[Test()]
		public void SongHandshakeRemapperUrlMissingUserTest()
		{
			var sng = new AmpacheSong();
			sng.Url = URL_MISSING_USER;
			sng.Id = 630;
			sng.AlbumId = 50;
			sng.ArtUrl = ART_URL;
			var handshake = new MockHandshake();
			handshake.SetPassphrase("test");
			handshake.SetServer("http://server.com");
			sng.MapToHandshake(handshake);
			Assert.That(sng.Url, Is.EqualTo(URL_MISSING_USER.Replace("SESSION", handshake.Passphrase).Replace("SONGID", sng.Id.ToString())));
			Assert.That(sng.ArtUrl, Is.EqualTo(ART_URL.Replace("ID", sng.ArtId.ToString()).Replace("AUTH", handshake.Passphrase)));
		}
		
		[Test()]
		public void SongHandshakeRemapperUrlMissingNameTest()
		{
			var sng = new AmpacheSong();
			sng.Url = URL_MISSING_NAME;
			sng.Id = 630;
			sng.AlbumId = 50;
			sng.ArtUrl = ART_URL;
			var handshake = new MockHandshake();
			handshake.SetPassphrase("test");
			handshake.SetServer("http://server.com");
			sng.MapToHandshake(handshake);
			Assert.That(sng.Url, Is.EqualTo(URL_MISSING_NAME.Replace("SESSION", handshake.Passphrase).Replace("SONGID", sng.Id.ToString())));
			Assert.That(sng.ArtUrl, Is.EqualTo(ART_URL.Replace("ID", sng.ArtId.ToString()).Replace("AUTH", handshake.Passphrase)));
		}

		private class MockHandshake : Handshake
		{
			public void SetServer(string server)
			{
				Server = server;
			}

			public void SetPassphrase(string phrase)
			{
				Passphrase = phrase;
			}
		}
	}
}

