using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using System;
using System.Xml.Linq;
using System.Collections.Generic;
using System.IO;
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
	public class AlbumArtRepositoryFixture : IWebRequestCreate
	{
		WebRequest _request;
		
		[SetUp]
		public void Setup()
		{
			if(!Directory.Exists("art"))
			{
				Directory.CreateDirectory("art");
			}
			_request = null;
			WebRequest.RegisterPrefix("art", this);
		}
		
		[TearDown]
		public void TearDown()
		{
			foreach(var file in Directory.GetFiles("art"))
			{
				File.Delete(file);
			}
		}
		
		[Test()]
		[ExpectedException(typeof(NotSupportedException))]
		public void AlbumArtRepositorySelectAllTest()
		{
			var target = new AlbumArtRepository("art");
			target.SelectAll();
			Assert.Fail();
		}
		[Test()]
		[ExpectedException(typeof(NotSupportedException))]
		public void AlbumArtRepositorySelectByIdTest()
		{
			var target = new AlbumArtRepository("art");
			target.SelectBy(1);
			Assert.Fail();
		}
		[Test()]
		[ExpectedException(typeof(NotSupportedException))]
		public void AlbumArtRepositorySelectByStringTest()
		{
			var target = new AlbumArtRepository("art");
			target.SelectBy(String.Empty);
			Assert.Fail();
		}
		[Test]
		public void AlbumArtRepositorySelectByIArtExistingTest()
		{
			var fs = File.Open(Path.Combine("art", 1.ToString()), FileMode.Create);
			fs.Close();
			var art = new Art{ ArtId = 1 };
			var target = new AlbumArtRepository("art");
			var actual = target.SelectBy(art).FirstOrDefault();
			Assert.That(actual, Is.Not.Null);
			Assert.That(actual.AlbumId, Is.EqualTo(1));
			Assert.That(actual.ArtStream.Length, Is.EqualTo(0)); // the empty file i just wrote
		}
		[Test]
		public void AlbumArtRepositorySelectByIArtNonExistingTest()
		{
			var art = new Art{ ArtId = 1, ArtUrl = "art://art" };
			_request = Substitute.For<WebRequest>();
			var mockRes = Substitute.For<WebResponse>();
			_request.GetResponse().Returns(mockRes);
			var stream = new MemoryStream();
			stream.WriteByte(0x34);
			stream.WriteByte(0x34);
			stream.Position = 0;
			mockRes.GetResponseStream().Returns(stream);
			var target = new AlbumArtRepository("art");
			var actual = target.SelectBy(art).FirstOrDefault();
			Assert.That(actual, Is.Not.Null);
			Assert.That(actual.AlbumId, Is.EqualTo(1));
			Assert.That(actual.ArtStream.Length, Is.EqualTo(2)); // the empty file i just wrote
		}
		[Test]
		public void AlbumArtRepositorySelectByIArtNonExistingErrorOnWebRequestTest()
		{
			var art = new Art{ ArtId = 1, ArtUrl = "art://art" };
			_request = Substitute.For<WebRequest>();
			var mockRes = Substitute.For<WebResponse>();
			_request.GetResponse().Returns(mockRes);
			int timesCalled = 0;
			mockRes.GetResponseStream().Returns( x => { ++timesCalled; throw new Exception(); });
			var target = new AlbumArtRepository("art");
			var actual = target.SelectBy(art);
			Assert.That(timesCalled, Is.EqualTo(1));
			Assert.That(actual, Is.Not.Null);
			Assert.That(actual.Count(), Is.EqualTo(0));
		}
		[Test]
		[ExpectedException(typeof(InvalidOperationException))]
		public void AlbumArtRepositorySelectByNotIArtTest()
		{
			var fs = File.Open(Path.Combine("art", 1.ToString()), FileMode.Create);
			fs.Close();
			var art = new NotArt();
			var target = new AlbumArtRepository("art");
			var actual = target.SelectBy(art);
			Assert.Fail();
		}
		
		[Test]
		public void AlbumArtRepositoryRemoveExistingTest()
		{
			var fs = File.Open(Path.Combine("art", 1.ToString()), FileMode.Create);
			fs.Close();
			var art = new Art{ ArtId = 1 };
			var target = new AlbumArtRepository("art");
			target.Remove(new AlbumArt { AlbumId = 1 });
			Assert.That(File.Exists(Path.Combine("art", "1")), Is.False);
		}
		
		[Test]
		public void AlbumArtRepositoryRemoveNonExistingTest()
		{
			Assert.That(File.Exists(Path.Combine("art", "1")), Is.False);
			var art = new Art{ ArtId = 1 };
			var target = new AlbumArtRepository("art");
			target.Remove(new AlbumArt { AlbumId = 1 });
			Assert.That(File.Exists(Path.Combine("art", "1")), Is.False);
		}
		
		[Test]
		public void AlbumArtRepositoryPersistExistingTest()
		{
			var fs = File.Open(Path.Combine("art", 1.ToString()), FileMode.Create);
			fs.Close();
			var stream = new MemoryStream();
			stream.WriteByte(0x38);
			var art = new AlbumArt{ AlbumId = 1, ArtStream = stream };
			var target = new AlbumArtRepository("art");
			target.Persist(art);
			Assert.That(File.Exists(Path.Combine("art", "1")));
			using(var nfs = File.Open(Path.Combine("art", 1.ToString()), FileMode.Open))
			{
				Assert.That(nfs.Length, Is.EqualTo(1));
			}
		}
		
		[Test]
		public void AlbumArtRepositoryPersistNewArtTest()
		{
			var stream = new MemoryStream();
			stream.WriteByte(0x38);
			var art = new AlbumArt{ AlbumId = 1, ArtStream = stream };
			var target = new AlbumArtRepository("art");
			target.Persist(art);
			Assert.That(File.Exists(Path.Combine("art", "1")));
			using(var nfs = File.Open(Path.Combine("art", 1.ToString()), FileMode.Open))
			{
				Assert.That(nfs.Length, Is.EqualTo(1));
			}
		}
	
		#region IWebRequestCreate implementation
		public System.Net.WebRequest Create (System.Uri uri)
		{
			return _request;
		}
		#endregion
		
		#region Helper Classes
		
		private class Art : IArt, IEntity
		{
			#region IArt implementation
			public string ArtUrl { get; set; }
	
			public int ArtId { get; set; }
			#endregion

			#region IEntity implementation
			public int Id {
				get {
					throw new System.NotImplementedException ();
				}
				set {
					throw new System.NotImplementedException ();
				}
			}
	
			public string Name {
				get {
					throw new System.NotImplementedException ();
				}
				set {
					throw new System.NotImplementedException ();
				}
			}
			#endregion
		}
		
		private class NotArt : IEntity
		{
			#region IEntity implementation
			public int Id {
				get {
					throw new NotImplementedException ();
				}
				set {
					throw new NotImplementedException ();
				}
			}

			public string Name {
				get {
					throw new NotImplementedException ();
				}
				set {
					throw new NotImplementedException ();
				}
			}
			#endregion
		}
		
		#endregion
	}
}

