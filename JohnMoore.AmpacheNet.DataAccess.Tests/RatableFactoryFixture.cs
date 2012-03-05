using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using System;
using System.Xml.Linq;
using System.Linq;
using JohnMoore.AmpacheNet.DataAccess;
using JohnMoore.AmpacheNet.Entities;

namespace JohnMoore.AmpacheNet.DataAccess.Tests
{
	[TestFixture()]
	public class RatableFactoryFixture
	{
		[Test()]
		public void RatableFactoryNormalTest ()
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
			Assert.That(actual.Rating, Is.EqualTo(3));
			Assert.That(actual.PerciseRating, Is.EqualTo(3));
		}
		[Test()]
		public void RatableFactoryMissingRatingTest ()
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
</albums>");
			var actual = target.Build(raw);
			Assert.That(actual, Is.Not.Null);
			Assert.That(actual.Rating, Is.EqualTo(0));
			Assert.That(actual.PerciseRating, Is.EqualTo(3));
		}
		[Test()]
		public void RatableFactoryMissingPreciseRatingTest ()
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
        <rating>2.9</rating>
</albums>");
			var actual = target.Build(raw);
			Assert.That(actual, Is.Not.Null);
			Assert.That(actual.Rating, Is.EqualTo(3));
			Assert.That(actual.PerciseRating, Is.EqualTo(0));
		}
		
		
		private class MockEntity : IRatable
		{
			#region ITagable implementation
			public System.Collections.Generic.ICollection<JohnMoore.AmpacheNet.Entities.Tag> Tags { get; set; }
			#endregion
	
			#region IEntity implementation
			public int Id { get; set; }
	
			public string Name { get; set; }
			#endregion

			#region IRatable implementation
			public int PerciseRating { get; set; }
			public int Rating { get; set; }
			#endregion
		}
		
		private class TagFactoryHandle : FactoryBaseRatable<MockEntity>
		{
			public MockEntity Build(XElement elm)
			{
				return base.BuildBase(elm);
			}
		}
	}
}

