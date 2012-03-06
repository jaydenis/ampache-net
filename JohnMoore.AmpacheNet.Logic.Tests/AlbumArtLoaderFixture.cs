using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using System;
using System.Linq;
using System.Collections.Generic;
using JohnMoore.AmpacheNet.Logic;
using JohnMoore.AmpacheNet.Entities;
using JohnMoore.AmpacheNet.DataAccess;
using System.IO;
using NSubstitute;

namespace JohnMoore.AmpacheNet.Logic.Tests
{
	[TestFixture()]
	public class AlbumArtLoaderFixture
	{
		
		[Test()]
		public void AlbumArtLoaderUsesDefaultWhenInitializedTest ()
		{
			var model = new AmpacheModel();
			var defaultStream = new MemoryStream();
			
			Assert.That(model.AlbumArtStream, Is.Not.SameAs(defaultStream));
			var target = new AlbumArtLoader(model, defaultStream);
			Assert.That(model.AlbumArtStream, Is.SameAs(defaultStream));
		}
		
		[Test()]
		public void AlbumArtLoaderUsesDefaultArtWhenNoArtAvailableTest ()
		{
			var model = new AmpacheModel();
			var defaultStream = new MemoryStream();
			
			var target = new AlbumArtLoader(model, defaultStream);
			var sng = new AmpacheSong();
			sng.ArtUrl = string.Empty;
			model.PlayingSong = sng;
			
			System.Threading.Thread.Sleep(100);
			Assert.That(model.AlbumArtStream, Is.SameAs(defaultStream));
		}
		
		[Test()]
		public void AlbumArtLoaderUsesDefaultArtWhenLoadingArtTest ()
		{
			var model = new AmpacheModel();
			var defaultStream = new MemoryStream();
			
			var factory = Substitute.For<AmpacheSelectionFactory>((Handshake)null);
			var persistor = Substitute.For<IPersistor<AlbumArt>>();
			persistor.IsPersisted(Arg.Any<IArt>()).Returns(false);
			var selector = Substitute.For<IAmpacheSelector<AlbumArt>>();
			int timesCalled = 0;
			selector.SelectBy<AmpacheSong>(Arg.Any<AmpacheSong>()).Returns(x => { ++timesCalled; Assert.That(model.AlbumArtStream, Is.SameAs(defaultStream)); return Enumerable.Empty<AlbumArt>();});
			factory.GetPersistorFor<AlbumArt>().Returns(persistor);
			factory.GetInstanceSelectorFor<AlbumArt>().Returns(selector);
			model.Factory = factory;
			
			var target = new AlbumArtLoader(model, defaultStream);
			var sng = new AmpacheSong();
			sng.ArtUrl = "test";
			model.PlayingSong = sng;
			
			System.Threading.Thread.Sleep(100);
			Assert.That(timesCalled, Is.EqualTo(1));
		}
		
		[Test()]
		public void AlbumArtLoaderUsesDefaultArtWhenErrorLoadingArtTest ()
		{
			var model = new AmpacheModel();
			var defaultStream = new MemoryStream();
			
			var factory = Substitute.For<AmpacheSelectionFactory>((Handshake)null);
			var persistor = Substitute.For<IPersistor<AlbumArt>>();
			persistor.IsPersisted(Arg.Any<IArt>()).Returns(false);
			var selector = Substitute.For<IAmpacheSelector<AlbumArt>>();
			int timesCalled = 0;
			selector.SelectBy<AmpacheSong>(Arg.Any<AmpacheSong>()).Returns(x => { ++timesCalled; return Enumerable.Empty<AlbumArt>();});
			factory.GetPersistorFor<AlbumArt>().Returns(persistor);
			factory.GetInstanceSelectorFor<AlbumArt>().Returns(selector);
			model.Factory = factory;
			
			var target = new AlbumArtLoader(model, defaultStream);
			var sng = new AmpacheSong();
			sng.ArtUrl = "test";
			model.PlayingSong = sng;
			
			System.Threading.Thread.Sleep(100);
			Assert.That(timesCalled, Is.EqualTo(1));
			Assert.That(model.AlbumArtStream, Is.SameAs(defaultStream)); 
		}
		
		[Test()]
		public void AlbumArtLoaderUsesLoadedArtAfterLoadingTest ()
		{
			var model = new AmpacheModel();
			var defaultStream = new MemoryStream();
			
			var factory = Substitute.For<AmpacheSelectionFactory>((Handshake)null);
			var persistor = Substitute.For<IPersistor<AlbumArt>>();
			persistor.IsPersisted(Arg.Any<IArt>()).Returns(false);
			var selector = Substitute.For<IAmpacheSelector<AlbumArt>>();
			var art = new AlbumArt();
			art.ArtStream = new MemoryStream();
			selector.SelectBy<AmpacheSong>(Arg.Any<AmpacheSong>()).Returns(new [] {art});
			factory.GetPersistorFor<AlbumArt>().Returns(persistor);
			factory.GetInstanceSelectorFor<AlbumArt>().Returns(selector);
			model.Factory = factory;
			var prefs = new UserConfiguration();
			prefs.CacheArt = false;
			model.Configuration = prefs;
			
			var target = new AlbumArtLoader(model, defaultStream);
			var sng = new AmpacheSong();
			sng.ArtUrl = "test";
			model.PlayingSong = sng;
			
			System.Threading.Thread.Sleep(100);
			Assert.That(model.AlbumArtStream, Is.SameAs(art.ArtStream)); 
		}
		
		[Test()]
		public void AlbumArtLoaderRespectsCachingPerferencesTest ()
		{
			var model = new AmpacheModel();
			var defaultStream = new MemoryStream();
			
			var factory = Substitute.For<AmpacheSelectionFactory>((Handshake)null);
			var persistor = Substitute.For<IPersistor<AlbumArt>>();
			persistor.IsPersisted(Arg.Any<IArt>()).Returns(false);
			int timesCalled = 0;
			persistor.When(x => x.Persist(Arg.Any<AlbumArt>())).Do( x => { ++timesCalled; });
			var selector = Substitute.For<IAmpacheSelector<AlbumArt>>();
			var art = new AlbumArt();
			art.ArtStream = new MemoryStream();
			selector.SelectBy<AmpacheSong>(Arg.Any<AmpacheSong>()).Returns(new [] {art});
			factory.GetPersistorFor<AlbumArt>().Returns(persistor);
			factory.GetInstanceSelectorFor<AlbumArt>().Returns(selector);
			model.Factory = factory;
			var prefs = new UserConfiguration();
			prefs.CacheArt = false;
			model.Configuration = prefs;
			
			var target = new AlbumArtLoader(model, defaultStream);
			var sng = new AmpacheSong();
			sng.ArtUrl = "test";
			model.PlayingSong = sng;
			
			System.Threading.Thread.Sleep(100);
			Assert.That(model.AlbumArtStream, Is.SameAs(art.ArtStream)); 
			Assert.That(timesCalled, Is.EqualTo(0));
		}
		
		[Test()]
		public void AlbumArtLoaderIgnoresCachingPersistedItemTest ()
		{
			var model = new AmpacheModel();
			var defaultStream = new MemoryStream();
			
			var factory = Substitute.For<AmpacheSelectionFactory>((Handshake)null);
			var persistor = Substitute.For<IPersistor<AlbumArt>>();
			persistor.IsPersisted(Arg.Any<IArt>()).Returns(true);
			var art = new AlbumArt();
			persistor.IsPersisted(Arg.Is(art)).Returns(true);
			int timesCalled = 0;
			persistor.When(x => x.Persist(Arg.Any<AlbumArt>())).Do( x => { ++timesCalled; });
			var selector = Substitute.For<IAmpacheSelector<AlbumArt>>();
			art.ArtStream = new MemoryStream();
			selector.SelectBy<AmpacheSong>(Arg.Any<AmpacheSong>()).Returns(new [] {art});
			factory.GetPersistorFor<AlbumArt>().Returns(persistor);
			factory.GetInstanceSelectorFor<AlbumArt>().Returns(selector);
			model.Factory = factory;
			var prefs = new UserConfiguration();
			prefs.CacheArt = true;
			model.Configuration = prefs;
			
			var target = new AlbumArtLoader(model, defaultStream);
			var sng = new AmpacheSong();
			sng.ArtUrl = "test";
			model.PlayingSong = sng;
			
			System.Threading.Thread.Sleep(100);
			Assert.That(model.AlbumArtStream, Is.SameAs(art.ArtStream)); 
			Assert.That(timesCalled, Is.EqualTo(0));
		}
		
		[Test()]
		public void AlbumArtLoaderCachesNewArtTest ()
		{
			var model = new AmpacheModel();
			var defaultStream = new MemoryStream();
			
			var factory = Substitute.For<AmpacheSelectionFactory>((Handshake)null);
			var persistor = Substitute.For<IPersistor<AlbumArt>>();
			persistor.IsPersisted(Arg.Any<IArt>()).Returns(false);
			var art = new AlbumArt();
			persistor.IsPersisted(Arg.Is(art)).Returns(false);
			int timesCalled = 0;
			persistor.When(x => x.Persist(Arg.Any<AlbumArt>())).Do( x => { ++timesCalled; });
			var selector = Substitute.For<IAmpacheSelector<AlbumArt>>();
			art.ArtStream = new MemoryStream();
			selector.SelectBy<AmpacheSong>(Arg.Any<AmpacheSong>()).Returns(new [] {art});
			factory.GetPersistorFor<AlbumArt>().Returns(persistor);
			factory.GetInstanceSelectorFor<AlbumArt>().Returns(selector);
			model.Factory = factory;
			var prefs = new UserConfiguration();
			prefs.CacheArt = true;
			model.Configuration = prefs;
			
			var target = new AlbumArtLoader(model, defaultStream);
			var sng = new AmpacheSong();
			sng.ArtUrl = "test";
			model.PlayingSong = sng;
			
			System.Threading.Thread.Sleep(100);
			Assert.That(model.AlbumArtStream, Is.SameAs(art.ArtStream)); 
			Assert.That(timesCalled, Is.EqualTo(1));
		}
	}
}

