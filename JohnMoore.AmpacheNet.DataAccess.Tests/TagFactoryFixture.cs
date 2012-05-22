using NUnit.Framework;
using System;
using System.Xml.Linq;
using System.Linq;
using JohnMoore.AmpacheNet.DataAccess;
using JohnMoore.AmpacheNet.Entities;

namespace JohnMoore.AmpacheNet.DataAccess.Tests
{
	[TestFixture()]
	public class TagFactoryFixture
	{
		[Test()]
		public void TagFactoryNormalTest ()
		{
			var target = new TagFactoryHandle();
			var raw = XElement.Parse(@"
<albums id=""1"">
        <name>Back in Black</name>
        <artist id=""129348"">AC/DC</artist>
        <year>1984</year>
        <tracks>12</tracks>
        <disk>1</disk>
        <tag id=""2482"" count=""1"">Rock &amp; Roll</tag>
        <tag id=""2482"" count=""2"">Rock</tag>
        <tag id=""2483"" count=""1"">Roll</tag>
        <art>http://localhost/image.php?id=129348</art>
        <preciserating>3</preciserating>
        <rating>2.9</rating>
</albums>");
			var actual = target.Build(raw);
			Assert.That(actual, Is.Not.Null);
			Assert.That(actual.Tags, Is.Not.Null);
			Assert.That(actual.Tags.Count, Is.EqualTo(3));
			var tag = actual.Tags.FirstOrDefault(t => t.Name == "Rock & Roll");
			Assert.That(tag, Is.Not.Null);
			Assert.That(tag.Id, Is.EqualTo(2482));
			Assert.That(tag.Count, Is.EqualTo(1));
			tag = actual.Tags.FirstOrDefault(t => t.Name == "Rock");
			Assert.That(tag, Is.Not.Null);
			Assert.That(tag.Count, Is.EqualTo(2));
			tag = actual.Tags.FirstOrDefault(t => t.Name == "Roll");
			Assert.That(tag, Is.Not.Null);
		}
		[Test()]
		public void TagFactoryNoTagsTest ()
		{
			var target = new TagFactoryHandle();
			var raw = XElement.Parse(@"
<albums id=""1"">
        <name>Back in Black</name>
        <artist id=""129348"">AC/DC</artist>
        <year>1984</year>
        <tracks>12</tracks>
        <disk>1</disk>
        <art>http://localhost/image.php?id=129348</art>
        <preciserating>3</preciserating>
        <rating>2.9</rating>
</albums>");
			var actual = target.Build(raw);
			Assert.That(actual, Is.Not.Null);
			Assert.That(actual.Tags, Is.Not.Null);
			Assert.That(actual.Tags.Count, Is.EqualTo(0));
		}
		
		[Test()]
		public void TagFactoryMissingCountTest ()
		{
			var target = new TagFactoryHandle();
			var raw = XElement.Parse(@"
<albums id=""1"">
        <name>Back in Black</name>
        <artist id=""129348"">AC/DC</artist>
        <year>1984</year>
        <tracks>12</tracks>
        <disk>1</disk>
        <tag id=""2482"">Rock &amp; Roll</tag>
        <art>http://localhost/image.php?id=129348</art>
        <preciserating>3</preciserating>
        <rating>2.9</rating>
</albums>");
			var actual = target.Build(raw);
			Assert.That(actual, Is.Not.Null);
			Assert.That(actual.Tags, Is.Not.Null);
			Assert.That(actual.Tags.Count, Is.EqualTo(1));
			var tag = actual.Tags.FirstOrDefault(t => t.Name == "Rock & Roll");
			Assert.That(tag, Is.Not.Null);
			Assert.That(tag.Id, Is.EqualTo(2482));
			Assert.That(tag.Count, Is.EqualTo(1));
		}
		
		[Test()]
		public void TagFactoryMissingItTest ()
		{
			var target = new TagFactoryHandle();
			var raw = XElement.Parse(@"
<albums id=""1"">
        <name>Back in Black</name>
        <artist id=""129348"">AC/DC</artist>
        <year>1984</year>
        <tracks>12</tracks>
        <disk>1</disk>
        <tag count=""2482"">Rock &amp; Roll</tag>
        <art>http://localhost/image.php?id=129348</art>
        <preciserating>3</preciserating>
        <rating>2.9</rating>
</albums>");
			var actual = target.Build(raw);
			Assert.That(actual, Is.Not.Null);
			Assert.That(actual.Tags, Is.Not.Null);
			Assert.That(actual.Tags.Count, Is.EqualTo(0));
		}
		
		private class MockEntity : ITagable
		{
			#region ITagable implementation
			public System.Collections.Generic.ICollection<JohnMoore.AmpacheNet.Entities.Tag> Tags { get; set; }
			#endregion
	
			#region IEntity implementation
			public int Id { get; set; }
	
			public string Name { get; set; }
			#endregion
		}
		
		private class TagFactoryHandle : FactoryBaseTagable<MockEntity>
		{
			public MockEntity Build(XElement elm)
			{
				return base.BuildBase(elm);
			}
		}
	}
}

