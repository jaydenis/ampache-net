//
// ArtistFactory.cs
//
// Author:
//       John Moore <jcwmoore@gmail.com>
//
// Copyright (c) 2010 John Moore
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Linq;
using System.Xml.Linq;
using System.Collections.Generic;
using JohnMoore.AmpacheNet.Entities;

namespace JohnMoore.AmpacheNet.DataAccess
{
    internal class ArtistFactory : FactoryBaseRatable<AmpacheArtist>, IEntityFactory<AmpacheArtist>
    {
        public ICollection<AmpacheArtist> Construct(ICollection<XElement> raw)
        {
            return new HashSet<AmpacheArtist>(raw.Select(n=>Construct(n)));
        }

        public AmpacheArtist Construct(XElement raw)
        {
			if(raw.Name.LocalName.ToLower() != "artist")
			{
				throw new System.Xml.XmlException(string.Format("{0} can not be processed into an Artist", raw.Name.LocalName));
			}
            var result = this.BuildBase(raw);
			if(!raw.Descendants("name").Any())
			{
				throw new System.Xml.XmlException(string.Format("Artist id {0} has no name defined", result.Id));
			}
            result.Name = raw.Descendants("name").First().Value;
			if(raw.Descendants("albums").Any())
			{
				result.AlbumCount = int.Parse(raw.Descendants("albums").First().Value);
			}
			if(raw.Descendants("songs").Any())
			{
				result.SongCount = int.Parse(raw.Descendants("songs").First().Value);
			}
            return result;
        }
    }
}
