using NUnit.Framework;
using System;
using System.Xml.Linq;
using System.Linq;
using JohnMoore.AmpacheNet.DataAccess;
using JohnMoore.AmpacheNet.Entities;

namespace JohnMoore.AmpacheNet.DataAccess.Tests
{
	[TestFixture()]
	public class PlaylistFactoryFixture
	{
		[Test()]
		[ExpectedException(typeof(System.Xml.XmlException))]
		public void PlaylistFactoryConstructBadXmlTest ()
		{
			var target = new PlaylistFactory();
			var raw = XElement.Parse(@"
<albums>
        <name>Back in Black</name>
        <artist id=""129348"">AC/DC</artist>
        <year>1984</year>
        <tracks>12</tracks>
        <disk>1</disk>
        <tag id=""2482"" count=""1"">Rock &amp; Roll</tag>
        <tag id=""2482"" count=""1"">Rock</tag>
        <tag id=""2483"" count=""1"">Roll</tag>
        <art>http://localhost/image.php?id=129348</art>
        <preciserating>3</preciserating>
        <rating>2.9</rating>
</albums>");
			var actual = target.Construct(raw);
			Assert.Fail();
		}
		[Test()]
		[ExpectedException(typeof(System.Xml.XmlException))]
		public void PlaylistFactoryConstructMissingNameTest ()
		{
			var target = new PlaylistFactory();
			var raw = XElement.Parse(@"
<playlist id=""1234"">
        <owner>Karl Vollmer</owner>
        <items>50</items>
        <tag id=""2481"" count=""2"">Rock &amp; Roll</tag>
        <tag id=""2482"" count=""2"">Rock</tag>
        <tag id=""2483"" count=""1"">Roll</tag>
        <type>Public</type>
</playlist>");
			var actual = target.Construct(raw);
			Assert.Fail();
		}
		
		[Test]
		public void PlaylistFactoryConstructNormalTest()
		{
			var target = new PlaylistFactory();
			var raw = XElement.Parse(@"
<playlist id=""1234"">
        <name>The Good Stuff</name>
        <owner>Karl Vollmer</owner>
        <items>50</items>
        <tag id=""2481"" count=""2"">Rock &amp; Roll</tag>
        <tag id=""2482"" count=""2"">Rock</tag>
        <tag id=""2483"" count=""1"">Roll</tag>
        <type>Public</type>
</playlist>");
			var actual = target.Construct(raw);
			Assert.That(actual.Id, Is.EqualTo(1234));
			Assert.That(actual.Name, Is.EqualTo("The Good Stuff"));
			Assert.That(actual.SongCount, Is.EqualTo(50));
			Assert.That(actual.Tags.Count, Is.EqualTo(3));
		}
		
		[Test]
		public void PlaylistFactoryConstructMissingTagsTest()
		{
			var target = new PlaylistFactory();
			var raw = XElement.Parse(@"
<playlist id=""1234"">
        <name>The Good Stuff</name>
        <owner>Karl Vollmer</owner>
        <items>50</items>
        <type>Public</type>
</playlist>");
			var actual = target.Construct(raw);
			Assert.That(actual.Id, Is.EqualTo(1234));
			Assert.That(actual.Name, Is.EqualTo("The Good Stuff"));
			Assert.That(actual.SongCount, Is.EqualTo(50));
			Assert.That(actual.Tags.Count, Is.EqualTo(0));
		}
		
		[Test]
		public void PlaylistFactoryConstructMissingOwnerTest()
		{
			var target = new PlaylistFactory();
			var raw = XElement.Parse(@"
<playlist id=""1234"">
        <name>The Good Stuff</name>
        <items>50</items>
        <tag id=""2481"" count=""2"">Rock &amp; Roll</tag>
        <tag id=""2482"" count=""2"">Rock</tag>
        <tag id=""2483"" count=""1"">Roll</tag>
        <type>Public</type>
</playlist>");
			var actual = target.Construct(raw);
			Assert.That(actual.Id, Is.EqualTo(1234));
			Assert.That(actual.Name, Is.EqualTo("The Good Stuff"));
			Assert.That(actual.SongCount, Is.EqualTo(50));
			Assert.That(actual.Tags.Count, Is.EqualTo(3));
		}
		
		[Test]
		public void PlaylistFactoryConstructMissingTypeTest()
		{
			var target = new PlaylistFactory();
			var raw = XElement.Parse(@"
<playlist id=""1234"">
        <name>The Good Stuff</name>
        <owner>Karl Vollmer</owner>
        <items>50</items>
        <tag id=""2481"" count=""2"">Rock &amp; Roll</tag>
        <tag id=""2482"" count=""2"">Rock</tag>
        <tag id=""2483"" count=""1"">Roll</tag>
</playlist>");
			var actual = target.Construct(raw);
			Assert.That(actual.Id, Is.EqualTo(1234));
			Assert.That(actual.Name, Is.EqualTo("The Good Stuff"));
			Assert.That(actual.SongCount, Is.EqualTo(50));
			Assert.That(actual.Tags.Count, Is.EqualTo(3));
		}
		
		[Test]
		public void PlaylistFactoryConstructMissingItemCountTest()
		{
			var target = new PlaylistFactory();
			var raw = XElement.Parse(@"
<playlist id=""1234"">
        <name>The Good Stuff</name>
        <owner>Karl Vollmer</owner>
        <tag id=""2481"" count=""2"">Rock &amp; Roll</tag>
        <tag id=""2482"" count=""2"">Rock</tag>
        <tag id=""2483"" count=""1"">Roll</tag>
        <type>Public</type>
</playlist>");
			var actual = target.Construct(raw);
			Assert.That(actual.Id, Is.EqualTo(1234));
			Assert.That(actual.Name, Is.EqualTo("The Good Stuff"));
			Assert.That(actual.SongCount, Is.EqualTo(0));
			Assert.That(actual.Tags.Count, Is.EqualTo(3));
		}
	}
}

