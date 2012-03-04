//
// IPersistor.cs
//
// Author:
//       John Moore <jcwmoore@gmail.com>
//
// Copyright (c) 2012 John Moore
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
using JohnMoore.AmpacheNet.Entities;


namespace JohnMoore.AmpacheNet.DataAccess
{
	public interface IPersistor<TEntity> where TEntity : IEntity
	{
		/// <summary>
		/// Determines whether this instance has persisted the specified entity.
		/// </summary>
		/// <returns>
		/// <c>true</c> if this instance is persisted; otherwise, <c>false</c>.
		/// </returns>
		/// <param name='entity'>
		/// entity to check for persistence
		/// </param>
		bool IsPersisted(TEntity entity);
		
		/// <summary>
		/// Determines whether this instance has persisted the art for the specified entity.
		/// </summary>
		/// <returns>
		/// <c>true</c> if this instance is persisted the specified entity; otherwise, <c>false</c>.
		/// </returns>
		/// <param name='entity'>
		/// If set to <c>true</c> entity.
		/// </param>
		bool IsPersisted (IArt entity);
		
		/// <summary>
		/// Persist the specified entity.
		/// </summary>
		/// <param name='entity'>
		/// Entity.
		/// </param>
		void Persist(TEntity entity);
		
		/// <summary>
		/// Remove the specified entity from the backing store.
		/// </summary>
		/// <param name='entity'>
		/// Entity.
		/// </param>
		void Remove(TEntity entity);
	}
}

