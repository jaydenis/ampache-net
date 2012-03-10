using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using System;
using System.Linq;
using System.Collections.Generic;
using JohnMoore.AmpacheNet.Logic;
using JohnMoore.AmpacheNet.Entities;
using JohnMoore.AmpacheNet.DataAccess;
using NSubstitute;

namespace JohnMoore.AmpacheNet.Logic.Tests
{
	[TestFixture()]
	public class LookupFixture
	{
		[Test()]
		public void LookupCachedEntitiesReturnsNullByDefaultTest ()
		{
			var target = new Lookup<MockEntity>(TimeSpan.Zero, null);
			var actual = target.CachedEntites;
			Assert.That(actual, Is.Null);
		}
		[Test()]
		public void LookupCachedEntitiesRespectsTimeToLiveParameterTest ()
		{
			var target = new LookupHandle(TimeSpan.Zero, null);
			target.SetEntities(new List<MockEntity>());
			target.SetLookupTime(DateTime.Now.AddSeconds(-1));
			var actual = target.CachedEntites;
			Assert.That(actual, Is.Null);
		}
		[Test()]
		public void LookupCachedEntitiesReturnsLiveDataTest ()
		{
			var target = new LookupHandle(TimeSpan.FromDays(2), null);
			var ent = new List<MockEntity>();
			target.SetEntities(ent);
			target.SetLookupTime(DateTime.Today);
			var actual = target.CachedEntites;
			Assert.That(actual, Is.SameAs(ent));
		}
		
		[Test()]
		public void LoadEntitiesReturnsCachedDataTest ()
		{
			var target = new LookupHandle(TimeSpan.FromDays(2), null);
			var ent = new List<MockEntity>();
			target.SetEntities(ent);
			target.SetLookupTime(DateTime.Today);
			var actual = target.LoadAll();
			Assert.That(actual, Is.SameAs(ent));
		}
		
		[Test()]
		public void LoadEntitiesLoadsAndCachedDataTest ()
		{
			var model = new AmpacheModel();
			var factory = Substitute.For<AmpacheSelectionFactory>((Handshake)null);
			model.Factory = factory;
			var selector = Substitute.For<IAmpacheSelector<MockEntity>>();
			factory.GetInstanceSelectorFor<MockEntity>().Returns(selector);
			var ent = new List<MockEntity>();
			selector.SelectAll().Returns(ent);
			
			var target = new LookupHandle(TimeSpan.FromMilliseconds(100), model);
			var actual = target.LoadAll();
			var cache = target.CachedEntites;
			Assert.That(actual, Is.Not.SameAs(ent));
			Assert.That(actual, Is.Not.Null);
			Assert.That(cache, Is.Not.Null);
		}
		
		[Test()]
		public void SearchEntitiesUsesCachedDataTest ()
		{
			var target = new LookupHandle(TimeSpan.FromDays(2), null);
			var ent = new List<MockEntity>();
			target.SetEntities(ent);
			target.SetLookupTime(DateTime.Today);
			var actual = target.Search("test");
			Assert.That(actual, Is.Not.SameAs(ent));
		}
		
		[Test()]
		public void SeachGoesToServerWhenNoDataCachedDataTest ()
		{
			var model = new AmpacheModel();
			var factory = Substitute.For<AmpacheSelectionFactory>((Handshake)null);
			model.Factory = factory;
			var selector = Substitute.For<IAmpacheSelector<MockEntity>>();
			factory.GetInstanceSelectorFor<MockEntity>().Returns(selector);
			var ent = new List<MockEntity>();
			string text = "test";
			selector.SelectBy(text).Returns(ent);
			
			var target = new LookupHandle(TimeSpan.FromDays(2), model);
			var actual = target.Search(text);
			var cache = target.CachedEntites;
			Assert.That(actual, Is.Not.SameAs(ent));
			Assert.That(cache, Is.Null);
		}
		
		private class LookupHandle : Lookup<MockEntity>
		{
			public LookupHandle (TimeSpan t, AmpacheModel m): base (t, m)
			{}
			
			public void SetEntities(ICollection<MockEntity> ent)
			{
				_cachedEntities = ent;
			}
			public void SetLookupTime(DateTime time)
			{
				_cacheLoadTime = time;
			}
		}
		
		public class MockEntity : IEntity
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
	}
}

