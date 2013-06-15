using NUnit.Framework;
using System;
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
	public class AuthenticateFixture : IWebRequestCreate
	{
		private WebRequest _mockRequest;
		private string _uri;
		
		[SetUp]
		public void TestSetup()
		{
			_mockRequest = null;
			WebRequest.RegisterPrefix("authenticate", this);
			_uri = null;
		}
		
		[Test()]
		[ExpectedException(typeof(ArgumentException))]
		public void AuthenticateNoUserNameTest ()
		{
			var test = new Authenticate("test", null, "test");
			Assert.Fail();
		}
		
		[Test()]
		[ExpectedException(typeof(ArgumentException))]
		public void AuthenticateNoServerNameTest ()
		{
			var test = new Authenticate(null, "test", "test");
			Assert.Fail();
		}
		
		[Test()]
		[ExpectedException(typeof(ArgumentException))]
		public void AuthenticateNoPasswordTest ()
		{
			var test = new Authenticate("test", "test", null);
			Assert.Fail();
		}
		
		[Test()]
		public void AuthenticateSuccessTest ()
		{
			var xml = new XElement("root",
			              new XElement("auth", "token"),
			              new XElement("version", 1),
			              new XElement("update", "1900-01-01"),
			              new XElement("add", "1900-01-02"),
			              new XElement("clean", "1900-01-03"),
			              new XElement("songs", "1234"),
			              new XElement("albums", "123"),
			              new XElement("artists", "12"),
			              new XElement("tags", "10"),
			              new XElement("videos", "1"));
			var stream = new System.IO.MemoryStream();
			xml.Save(stream);
			stream.Position = 0;
			var mockResponse = Substitute.For<WebResponse>();
			mockResponse.GetResponseStream().Returns(stream);
			_mockRequest = Substitute.For<WebRequest>();
			_mockRequest.GetResponse().Returns(mockResponse);
			var password = "password";
			var user = "test";
			var server = "authenticate://test";
			
			byte[] passBytes = Encoding.UTF8.GetBytes(password);
            var hasher = new SHA256Managed();
            var tmpBytes = hasher.ComputeHash(passBytes);
			var now = DateTime.Now.UnixEpoch();
			var hashword = HexString(tmpBytes);
			tmpBytes = Encoding.UTF8.GetBytes(now + hashword);
            tmpBytes = hasher.ComputeHash(tmpBytes);			
			var passphrase = HexString(tmpBytes);
			
			var res = new Authenticate(server, user, password);
			Assert.That(_uri, Is.Not.Null);
			Assert.That(_uri, Is.EqualTo(string.Format("{0}/server/xml.server.php?action=handshake&auth={1}&timestamp={2}&version=350001&user={3}", server, passphrase, now, user)));
			Assert.That(res.AlbumCount, Is.EqualTo(123));
			Assert.That(res.SongCount, Is.EqualTo(1234));
			Assert.That(res.ArtistCount, Is.EqualTo(12));
			Assert.That(res.User, Is.EqualTo(user));
			Assert.That(res.Server, Is.EqualTo(server));
			Assert.That(res.Passphrase, Is.EqualTo("token")); // from auth xml node above
			
		}
		
		[Test()]
		public void AuthenticatePingTest ()
		{
			var xml = new XElement("root",
			              new XElement("auth", "token"),
			              new XElement("version", 1),
			              new XElement("update", "1900-01-01"),
			              new XElement("add", "1900-01-02"),
			              new XElement("clean", "1900-01-03"),
			              new XElement("songs", "1234"),
			              new XElement("albums", "123"),
			              new XElement("artists", "12"),
			              new XElement("tags", "10"),
			              new XElement("videos", "1"));
			var stream = new System.IO.MemoryStream();
			xml.Save(stream);
			stream.Position = 0;
			var mockResponse = Substitute.For<WebResponse>();
			mockResponse.GetResponseStream().Returns(stream);
			_mockRequest = Substitute.For<WebRequest>();
			_mockRequest.GetResponse().Returns(mockResponse);
			var password = "password";
			var user = "test";
			var server = "authenticate://test";
			
			var res = new Authenticate(server, user, password);
			_uri = null;
			res.Ping();
			Assert.That(_uri, Is.Not.Null);
			Assert.That(_uri, Is.EqualTo(string.Format("{0}/server/xml.server.php?action=ping&auth={1}", server, "token")));
		}

        private string HexString(byte[] bytes)
        {
            StringBuilder hex = new StringBuilder(bytes.Length * 2);
            foreach (byte b in bytes) {
                hex.AppendFormat("{0:x2}", b);
            }
            return hex.ToString();
        }
		
		#region IWebRequestCreate implementation
		public System.Net.WebRequest Create (System.Uri uri)
		{
			_uri = uri.AbsoluteUri;
			return _mockRequest;
		}
		#endregion
	}
}

