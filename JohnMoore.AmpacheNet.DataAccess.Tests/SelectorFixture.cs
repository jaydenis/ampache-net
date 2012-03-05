using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using System;
using System.Xml.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using JohnMoore.AmpacheNet.DataAccess;
using JohnMoore.AmpacheNet.Entities;
using NSubstitute;

namespace JohnMoore.AmpacheNet.DataAccess.Tests
{
	[TestFixture()]
	public class SelectorFixture : Handshake, IWebRequestCreate
	{
		const string SELECT_ALL = "selectall";
		const string SELECT_SINGLE = "selectsingle";
		const string NODE_NAME = "mock";
		private static Dictionary<Type, string> MethodMap;
		private WebRequest _mockRequest;
		private string _uri;
		
		[SetUp]
		public void TestSetup()
		{
			MethodMap = new Dictionary<Type, string>();
			this.Passphrase = "Passphrase";
			this.User = "a_user";
			this.Server = "test://server";
			_mockRequest = null;
			WebRequest.RegisterPrefix("test", this);
			_uri = null;
		}
		[Test()]
		public void SelectorSelectAllTest ()
		{
			var xml = new XElement("root",
			              new XElement(NODE_NAME),
			              new XElement(NODE_NAME),
			              new XElement(NODE_NAME),
			              new XElement(NODE_NAME));
			var stream = new System.IO.MemoryStream();
			xml.Save(stream);
			stream.Position = 0;
			var mockResponse = Substitute.For<WebResponse>();
			mockResponse.GetResponseStream().Returns(stream);
			_mockRequest = Substitute.For<WebRequest>();
			_mockRequest.GetResponse().Returns(mockResponse);
			
			var mockFactory = Substitute.For<IEntityFactory<MockEntity>>();
			var result = new List<MockEntity>{new MockEntity()};
			mockFactory.Construct(Arg.Is<ICollection<XElement>>(x => x.All(e=>e.Name.LocalName == NODE_NAME))).Returns(result);
			
			var target = new MockSelector(this, mockFactory);
			var res = target.SelectAll().ToList();
			Assert.That(res.Count, Is.EqualTo(result.Count));
			Assert.That(res.All(e => result.Contains(e)));
			Assert.That(_uri, Is.Not.Null);
			var expUri = string.Format("{0}/server/xml.server.php?action={1}&auth={2}&limit={3}", Server, SELECT_ALL,Passphrase, 500);
			Assert.That(_uri, Is.EqualTo(expUri));
		}
		
		/// <summary>
		/// When Selecting my an unknown entity type, we will default to the select all behavior
		/// </summary>
		[Test()]
		public void SelectorSelectByUnknownEntityTest ()
		{
			var xml = new XElement("root",
			              new XElement(NODE_NAME),
			              new XElement(NODE_NAME),
			              new XElement(NODE_NAME),
			              new XElement(NODE_NAME));
			var stream = new System.IO.MemoryStream();
			xml.Save(stream);
			stream.Position = 0;
			var mockResponse = Substitute.For<WebResponse>();
			mockResponse.GetResponseStream().Returns(stream);
			_mockRequest = Substitute.For<WebRequest>();
			_mockRequest.GetResponse().Returns(mockResponse);
			
			var mockFactory = Substitute.For<IEntityFactory<MockEntity>>();
			var result = new List<MockEntity>{new MockEntity()};
			mockFactory.Construct(Arg.Is<ICollection<XElement>>(x => x.All(e=>e.Name.LocalName == NODE_NAME))).Returns(result);
			
			var target = new MockSelector(this, mockFactory);
			var res = target.SelectBy(new AmpachePlaylist()).ToList();
			Assert.That(res.Count, Is.EqualTo(result.Count));
			Assert.That(res.All(e => result.Contains(e)));
			Assert.That(_uri, Is.Not.Null);
			var expUri = string.Format("{0}/server/xml.server.php?action={1}&auth={2}&limit={3}", Server, SELECT_ALL,Passphrase, 500);
			Assert.That(_uri, Is.EqualTo(expUri));
		}
		
		[Test()]
		public void SelectorSelectBySameEntityTest ()
		{
			var xml = new XElement("root",
			              new XElement(NODE_NAME),
			              new XElement(NODE_NAME),
			              new XElement(NODE_NAME),
			              new XElement(NODE_NAME));
			var stream = new System.IO.MemoryStream();
			xml.Save(stream);
			stream.Position = 0;
			var mockResponse = Substitute.For<WebResponse>();
			mockResponse.GetResponseStream().Returns(stream);
			_mockRequest = Substitute.For<WebRequest>();
			_mockRequest.GetResponse().Returns(mockResponse);
			
			var mockFactory = Substitute.For<IEntityFactory<MockEntity>>();
			
			var target = new MockSelector(this, mockFactory);
			var ent = new MockEntity();
			var res = target.SelectBy(ent).ToList();
			Assert.That(res.Count, Is.EqualTo(1));
			Assert.That(Is.ReferenceEquals(ent, res.Single()));
			Assert.That(_uri, Is.Null);
		}
				
		[Test()]
		public void SelectorSelectByKnownEntityTest ()
		{
			var xml = new XElement("root",
			              new XElement(NODE_NAME),
			              new XElement(NODE_NAME),
			              new XElement(NODE_NAME),
			              new XElement(NODE_NAME));
			var stream = new System.IO.MemoryStream();
			xml.Save(stream);
			stream.Position = 0;
			var mockResponse = Substitute.For<WebResponse>();
			mockResponse.GetResponseStream().Returns(stream);
			_mockRequest = Substitute.For<WebRequest>();
			_mockRequest.GetResponse().Returns(mockResponse);
			
			var mockFactory = Substitute.For<IEntityFactory<MockEntity>>();
			var result = new List<MockEntity>{new MockEntity()};
			mockFactory.Construct(Arg.Is<ICollection<XElement>>(x => x.All(e=>e.Name.LocalName == NODE_NAME))).Returns(result);
			
			int id = 123;
			string method = "playlist_test";
			MethodMap.Add(typeof(AmpachePlaylist), method);
			var target = new MockSelector(this, mockFactory);
			var res = target.SelectBy(new AmpachePlaylist{ Id = id }).ToList();
			Assert.That(res.Count, Is.EqualTo(result.Count));
			Assert.That(res.All(e => result.Contains(e)));
			Assert.That(_uri, Is.Not.Null);
			var expUri = string.Format("{0}/server/xml.server.php?action={1}&auth={2}&filter={3}", Server, method ,Passphrase, id);
			Assert.That(_uri, Is.EqualTo(expUri));
		}
		
		[Test()]
		public void SelectorSearchTest ()
		{
			var xml = new XElement("root",
			              new XElement(NODE_NAME),
			              new XElement(NODE_NAME),
			              new XElement(NODE_NAME),
			              new XElement(NODE_NAME));
			var stream = new System.IO.MemoryStream();
			xml.Save(stream);
			stream.Position = 0;
			var mockResponse = Substitute.For<WebResponse>();
			mockResponse.GetResponseStream().Returns(stream);
			_mockRequest = Substitute.For<WebRequest>();
			_mockRequest.GetResponse().Returns(mockResponse);
			
			var mockFactory = Substitute.For<IEntityFactory<MockEntity>>();
			var result = new List<MockEntity>{new MockEntity()};
			mockFactory.Construct(Arg.Is<ICollection<XElement>>(x => x.All(e=>e.Name.LocalName == NODE_NAME))).Returns(result);
			
			var target = new MockSelector(this, mockFactory);
			string search = "searching";
			var res = target.SelectBy(search).ToList();
			Assert.That(res.Count, Is.EqualTo(result.Count));
			Assert.That(res.All(e => result.Contains(e)));
			Assert.That(_uri, Is.Not.Null);
			var expUri = string.Format("{0}/server/xml.server.php?action={1}&auth={2}&filter={3}&limit={4}", Server, SELECT_ALL, Passphrase, search, 500);
			Assert.That(_uri, Is.EqualTo(expUri));
		}
		
		[Test()]
		public void SelectorSelectbyIdTest ()
		{
			var xml = new XElement("root",
			              new XElement(NODE_NAME),
			              new XElement(NODE_NAME),
			              new XElement(NODE_NAME),
			              new XElement(NODE_NAME));
			var stream = new System.IO.MemoryStream();
			xml.Save(stream);
			stream.Position = 0;
			var mockResponse = Substitute.For<WebResponse>();
			mockResponse.GetResponseStream().Returns(stream);
			_mockRequest = Substitute.For<WebRequest>();
			_mockRequest.GetResponse().Returns(mockResponse);
			
			var mockFactory = Substitute.For<IEntityFactory<MockEntity>>();
			var result = new List<MockEntity>{new MockEntity(), new MockEntity()};
			mockFactory.Construct(Arg.Is<ICollection<XElement>>(x => x.All(e=>e.Name.LocalName == NODE_NAME))).Returns(result);
			
			var target = new MockSelector(this, mockFactory);
			int id = 1234;
			var res = target.SelectBy(id);
			Assert.That(res, Is.Not.Null);
			Assert.That(result.Contains(res));
			Assert.That(_uri, Is.Not.Null);
			var expUri = string.Format("{0}/server/xml.server.php?action={1}&auth={2}&filter={3}", Server, SELECT_SINGLE, Passphrase, id);
			Assert.That(_uri, Is.EqualTo(expUri));
		}
		
		#region Helper classes
		
		public class MockEntity : IEntity
		{
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
		
		private class MockSelector : AmpacheSelectorBase<MockEntity>
		{
			public MockSelector (Handshake handshake, IEntityFactory<MockEntity> factory) : base(handshake, factory)
	        {}
			
			#region implemented abstract members of JohnMoore.AmpacheNet.DataAccess.AmpacheSelectorBase[JohnMoore.AmpacheNet.DataAccess.Tests.SelectorFixture.MockEntity]
			protected override string SelectAllMethod { get { return SELECT_ALL; } }

			protected override string XmlNodeName { get { return NODE_NAME; } }

			protected override string SelectSingleMethod { get  { return SELECT_SINGLE; } }

			protected override System.Collections.Generic.Dictionary<Type, string> SelectMethodMap { get { return MethodMap; } }
			#endregion
		
		}
		
		#region IWebRequestCreate implementation
		public System.Net.WebRequest Create (System.Uri uri)
		{
			_uri = uri.AbsoluteUri;
			return _mockRequest;
		}
		#endregion
		
		#endregion
	}
}

