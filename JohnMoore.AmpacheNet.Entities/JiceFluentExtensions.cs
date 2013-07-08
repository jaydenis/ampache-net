//
// JiceFluentExtensions.cs
//
// Author:
//       John Moore <jcwmoore@gmail.com>
//
// Copyright (c) 2013 John Moore
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

namespace Jice
{
    public static class JiceRegistationContextExtensions
    {
        /// <summary>
        /// Registers a type to its self
        /// </summary>
        public static JiceRegistration<TInterface, TInterface> ToItsSelf<TInterface>(this JiceRegistationContext<TInterface> rc) where TInterface : class
        {
            return rc.To<TInterface>();
        }

        /// <summary>
        /// Assigns a name to this registration
        /// </summary>
        /// <param name="name">Name for the registration</param>
        /// <returns>Original object for Fluent API</returns>
        public static JiceRegistationContext<TInterface> Named<TInterface>(this JiceRegistationContext<TInterface> rc, string name)
        {
            rc.ResolutionInfo.Name = name;
            return rc;
        }
    }

    public static class JiceRegistrationExtensions
    {
        /// <summary>
        /// Specifies this Registration as a Singleton
        /// </summary>
        public static void AsSingleton<TInterface, TImplementation>(this JiceRegistration<TInterface, TImplementation> jr) where TImplementation : TInterface
        {
            jr.ResolutionInfo.LifeCycleManager = new JiceContainer.SingletonLifecycle<TInterface>(jr.ResolutionInfo, default(TImplementation));
        }

        /// <summary>
        /// Specifies this Registration as a Singleton
        /// </summary>
        /// <param name="obj">Singleton object</param>
        public static void AsSingleton<TInterface, TImplementation>(this JiceRegistration<TInterface, TImplementation> jr, TImplementation obj) where TImplementation : TInterface
        {
            jr.ResolutionInfo.LifeCycleManager = new JiceContainer.SingletonLifecycle<TInterface>(jr.ResolutionInfo, obj);
        }

        /// <summary>
        /// Specifies this Registration as a Transient
        /// </summary>
        public static void AsTransient<TInterface, TImplementation>(this JiceRegistration<TInterface, TImplementation> jr) where TImplementation : TInterface
        {
            jr.ResolutionInfo.LifeCycleManager = new JiceContainer.TransientLifecycle<TInterface>(jr.ResolutionInfo);
        }
    }
}
