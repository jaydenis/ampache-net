//
// FactoryBase.cs
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
using System.Xml.Linq;
using System.Linq;
using System.Collections.Generic;
using JohnMoore.AmpacheNet.Entities;

namespace JohnMoore.AmpacheNet.DataAccess
{
    internal class PlaylistFactory : FactoryBaseTagable<AmpachePlaylist>, IEntityFactory<AmpachePlaylist>
    {
        #region IEntityFactory[AmpachePlaylist] implementation
        public ICollection<AmpachePlaylist> Construct (ICollection<XElement> raw)
        {
            return new HashSet<AmpachePlaylist>(raw.Select(n=> Construct(n)));
        }
        #endregion

        public AmpachePlaylist Construct(XElement raw)
        {
			if(raw.Name.LocalName.ToLower() != "playlist")
			{
				throw new System.Xml.XmlException(string.Format("{0} can not be processed into an Playlist", raw.Name.LocalName));
			}
            var result = BuildBase(raw);			
            if (!raw.Descendants("name").Any()) 
			{
				throw new System.Xml.XmlException(string.Format("Playlist id {0} has no name defined", result.Id)); 
            }
			result.Name = raw.Descendants ("name").First ().Value;
			if(raw.Descendants("items").Any())
			{
				result.SongCount = int.Parse(raw.Descendants("items").First().Value);
			}
            return result;
        }
    }
}
