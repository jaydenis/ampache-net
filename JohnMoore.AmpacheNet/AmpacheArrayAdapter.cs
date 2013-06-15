//
// AmpacheArrayAdapter.cs
//
// Author:
//       John Moore <jcwmoore@gmail.com>
//
// Copyright (c) 2011 John Moore
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
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace JohnMoore.AmpacheNet
{
	class AmpacheArrayAdapter<TEntity> : BaseAdapter<TEntity>
	{
		private readonly LayoutInflater _inflator;
		private readonly int _viewId;
		private readonly Func<TEntity, View, View> _hydrate;
        private readonly IList<TEntity> _data;
		
		public AmpacheArrayAdapter (Func<TEntity, View, View> hydrate, LayoutInflater inflator, Context ctx, int viewId, IList<TEntity> data)
		{
			_hydrate = hydrate;
			_inflator = inflator;
			_viewId = viewId;
            _data = data.ToList();
		}
		
		public override View GetView (int position, View convertView, ViewGroup parent)
		{
			View row;
			if(convertView == null)
			{
				row = _inflator.Inflate(_viewId, null);
			}
			else
			{
				row = convertView;
			}
			return _hydrate(this[position], row);
		}

        public override TEntity this[int position]
        {
            get { return _data[position]; }
        }

        public override int Count
        {
            get { return _data.Count; }
        }

        public override long GetItemId(int position)
        {
            return (long)position;
        }

        internal int GetPosition(TEntity ampacheSong)
        {
            return _data.IndexOf(ampacheSong);
        }
    }
}

