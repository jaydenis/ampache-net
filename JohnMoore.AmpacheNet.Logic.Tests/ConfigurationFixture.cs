//
// ConfigurationFixture.cs
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
	public class ConfigurationFixture
	{
		[Test()]
		public void ConfigurationPerformTestSuccessfulTest ()
		{
			var model = new AmpacheModel();
			var factory = Substitute.For<AmpacheSelectionFactory>();
			model.Factory = factory;			
			factory.AuthenticationTest(Arg.Any<string>(),Arg.Any<string>(),Arg.Any<string>()).Returns((Authenticate)null);
			
			var target = new Configuration(model);
			Assert.That(model.UserMessage, Is.Null);
			target.PerformTest(string.Empty, string.Empty, string.Empty);
			Assert.That(model.UserMessage, Is.Not.Null);
		}
		
		[Test()]
		public void ConfigurationPerformTestErrorTest ()
		{
			var model = new AmpacheModel();
			var factory = Substitute.For<AmpacheSelectionFactory>();
			model.Factory = factory;			
			var message = "error message";
			factory.When(x => x.AuthenticationTest(Arg.Any<string>(),Arg.Any<string>(),Arg.Any<string>())).Do((obj) => { throw new Exception(message); });
			
			var target = new Configuration(model);
			Assert.That(model.UserMessage, Is.Null);
			target.PerformTest(string.Empty, string.Empty, string.Empty);
			Assert.That(model.UserMessage, Is.SameAs(message));
		}
		
		[Test]
		public void ConfigurationTrySaveConfigurationFailsByDefault()
		{
			var target = new Configuration(null);
			var res = target.TrySaveConfiguration(new UserConfiguration());
			Assert.That(res, Is.False);
		}
		[Test()]
		public void ConfigurationTrySaveConfigurationSuccessfulTest ()
		{
			var model = new AmpacheModel();
			var factory = Substitute.For<AmpacheSelectionFactory>();
			model.Factory = factory;			
			factory.AuthenticateToServer(Arg.Any<string>(),Arg.Any<string>(),Arg.Any<string>()).Returns((Authenticate)null);
			
			var target = new Configuration(model);
			Assert.That(model.UserMessage, Is.Null);
			var conf = new UserConfiguration();
			var actual = target.TrySaveConfiguration(conf);
			Assert.That(actual, Is.True);
			Assert.That(model.UserMessage, Is.Not.Null);
			Assert.That(model.Configuration, Is.SameAs(conf));
		}
		
		[Test()]
		public void ConfigurationTrySaveConfigurationErrorTest ()
		{
			var model = new AmpacheModel();
			var factory = Substitute.For<AmpacheSelectionFactory>();
			model.Factory = factory;			
			var message = "error message";
			factory.When(x => x.AuthenticateToServer(Arg.Any<string>(),Arg.Any<string>(),Arg.Any<string>())).Do((obj) => { throw new Exception(message); });
			
			var target = new Configuration(model);
			Assert.That(model.UserMessage, Is.Null);
			var conf = new UserConfiguration();
			var actual = target.TrySaveConfiguration(conf);
			Assert.That(actual, Is.False);
			Assert.That(model.UserMessage, Is.SameAs(message));
			Assert.That(model.Configuration, Is.Null);
		}
	}
}

