﻿//
// Jice.cs
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

using System;
using System.Collections.Generic;
using System.Linq;

namespace Jice
{
    public sealed class JiceContainer : IDisposable
    {
        internal static readonly string DEFAULT_INSTANCE_NAME = Guid.NewGuid().ToString();
        private readonly Dictionary<Type, Dictionary<string, ResolutionInfoBase>> _resolutionMap = new Dictionary<Type, Dictionary<string, ResolutionInfoBase>>();

        public JiceContainer()
        {
            Register<JiceContainer>().To(this);
        }

        /// <summary>
        /// Begins the registration syntax, must invoke the To operation to persist the registration
        /// </summary>
        public JiceRegistationContext<TType> Register<TType>()
        {
            return new RegistrationContext<TType>(this);
        }

        /// <summary>
        /// Returns the default implementation of <typeparamref name="TType"/> 
        /// </summary>
        public TType Resolve<TType>()
        {
            return Resolve<TType>(DEFAULT_INSTANCE_NAME);
        }
        
        /// <summary>
        /// Returns the named implementation of <typeparamref name="TType"/>
        /// </summary>
        public TType Resolve<TType>(string name)
        {
            if (!_resolutionMap.ContainsKey(typeof(TType)))
            {
                if(typeof(TType).IsInterface || typeof(TType).IsAbstract)
                {
                    throw new ResolutionException(string.Format("The requested type is not available: {0}", typeof(TType).Name), typeof(TType));
                }
                Register<TType>().To<TType>(); // we will try to register the new type and see what happens
            }
            if (!_resolutionMap[typeof(TType)].ContainsKey(name))
            {
                throw new ResolutionException(string.Format("The requested type, {0}, is not available with name {1}", typeof(TType).Name, name), typeof(TType));
            }
            return ((ResolutionInfo<TType>)_resolutionMap[typeof(TType)][name]).LifeCycleManager.GetConcrete();
        }

        /// <summary>
        /// Returns a lazily loaded collection of all registrations of <typeparamref name="TType"/> with the
        /// default implementation as the first in the collection
        /// </summary>
        public IEnumerable<TType> ResolveAll<TType>()
        {
            if (_resolutionMap.ContainsKey(typeof(TType)) && _resolutionMap[typeof(TType)].ContainsKey(DEFAULT_INSTANCE_NAME))
            {
                yield return Resolve<TType>();
            }
            var names = _resolutionMap[typeof(TType)].Keys.Where(k => k != DEFAULT_INSTANCE_NAME).ToList();
            foreach (var n in names)
            {
                yield return Resolve<TType>(n);
            }
        }

        /// <summary>
        /// releases all objects held by the container and clears all registrations
        /// </summary>
        public void Dispose()
        {
            foreach (var obj in _resolutionMap.Where(m => m.Key != typeof(JiceContainer)).SelectMany(m => m.Value.Values))
            {
                obj.ObjectManager.Dispose();
            }
            _resolutionMap.Clear();
        }

        private TInterface ReflectionBuild<TInterface, TType>(ResolutionInfoBase ri) where TType : TInterface
        {
            if (ri.ConstructorInfo == null)
            {
                ri.ConstructorInfo = typeof(TType).GetConstructors()
                                                  .OrderByDescending(c => c.GetParameters().Length)
                                                  .FirstOrDefault(c => c.GetParameters().All(p => _resolutionMap.ContainsKey(p.ParameterType)));
                if (ri.ConstructorInfo != null)
                {
                    ri.Parameters = ri.ConstructorInfo.GetParameters();
                }
                else
                {
                    throw new ResolutionException(string.Format("Unable to resolve dependencies for type: {0}", typeof(TType).Name), typeof(TType));
                }
            }
            var parameters = ri.Parameters.Select(p => _resolutionMap[p.ParameterType][DEFAULT_INSTANCE_NAME].ObjectManager.GetObject()).ToArray();
            return (TInterface)ri.ConstructorInfo.Invoke(parameters);
        }

        internal class TransientLifecycle<TInterface> : IJiceLifeCycleManager<TInterface>
        {
            private readonly ResolutionInfo<TInterface> _info;

            public TransientLifecycle(ResolutionInfo<TInterface> info)
            {
                _info = info;
            }

            public TInterface GetConcrete()
            {
                return _info.BuildInstance(_info.Container);
            }

            public object GetObject()
            {
                return GetConcrete();
            }

            public void Dispose() { }
        }

        internal class SingletonLifecycle<TInterface> : IJiceLifeCycleManager<TInterface>
        {
            private readonly ResolutionInfo<TInterface> _info;
            private TInterface _instance;

            public SingletonLifecycle(ResolutionInfo<TInterface> info, TInterface instance)
            {
                _info = info;
                _instance = instance;
            }

            public TInterface GetConcrete()
            {
                if(_instance == null)
                {
                    _instance = _info.BuildInstance(_info.Container);
                }
                return _instance;
            }

            public object GetObject()
            {
                return GetConcrete();
            }

            public void Dispose() 
            {
                var dis = _instance as IDisposable;
                if (dis != null) 
                {
                    dis.Dispose();
                }
            }
        }

        private class RegistrationContext<TInterface> : JiceRegistationContext<TInterface>
        {
            private readonly JiceContainer _container;
            private readonly ResolutionInfo<TInterface> _info = new ResolutionInfo<TInterface>();
            internal override ResolutionInfo<TInterface> ResolutionInfo { get { return _info; } }

            public RegistrationContext(JiceContainer container)
            {
                _container = container;
                _info.ParentContainer = _container;
                _info.Name = DEFAULT_INSTANCE_NAME;
                _info.Container = _container;
            }

            private void BuildBase<TImplementation>() where TImplementation : TInterface
            {
                if (typeof(TImplementation).IsAbstract)
                {
                    throw new RegistrationException(string.Format("Cannot register {0} to abstract type {1}", typeof(TInterface).Name, typeof(TImplementation).Name), typeof(TImplementation));
                }
                _info.BuildInstance = (j) => _container.ReflectionBuild<TInterface, TImplementation>(_info);
                if (!_container._resolutionMap.ContainsKey(typeof(TInterface)))
                {
                    _container._resolutionMap.Add(typeof(TInterface), new Dictionary<string, ResolutionInfoBase>());
                }
                if (_container._resolutionMap[typeof(TInterface)].ContainsKey(_info.Name))
                {
                    _container._resolutionMap[typeof(TInterface)].Remove(_info.Name);
                }
                _container._resolutionMap[typeof(TInterface)].Add(_info.Name, _info);
            }

            public override JiceRegistration<TInterface, TImplementation> To<TImplementation>()
            {
                BuildBase<TImplementation>();
                _info.LifeCycleManager = new TransientLifecycle<TInterface>(_info);
                return new Registration<TInterface, TImplementation>(_info);
            }

            public override JiceRegistration<TInterface, TImplementation> To<TImplementation>(TImplementation ti)
            {
                BuildBase<TImplementation>();
                _info.LifeCycleManager = new SingletonLifecycle<TInterface>(_info, ti);
                return new Registration<TInterface, TImplementation>(_info);
            }
        }

        private class Registration<TInterface, TImplementation> : JiceRegistration<TInterface, TImplementation> where TImplementation : TInterface
        {
            private readonly ResolutionInfo<TInterface> _info;
            private Func<JiceContainer, TImplementation> _fun;
            internal override ResolutionInfo<TInterface> ResolutionInfo { get { return _info; } }

            public Registration(ResolutionInfo<TInterface> info)
            {
                _info = info;
            }
            
            public override JiceRegistration<TInterface, TImplementation> ConstructAs(Func<JiceContainer, TImplementation> fun)
            {
                _fun = fun;
                _info.BuildInstance = (c) => _fun(c);
                return this;
            }
        }

        internal abstract class ResolutionInfoBase
        {
            public JiceContainer Container { get; set; }
            public string Name { get; set; }
            public abstract IJiceObjectManager ObjectManager { get; }
            public JiceContainer ParentContainer { get; set; }
            public System.Reflection.ConstructorInfo ConstructorInfo { get; set; }
            public System.Reflection.ParameterInfo[] Parameters { get; set; }
        }

        internal class ResolutionInfo<TInterface> : ResolutionInfoBase
        {
            public TInterface Singleton { get; set; }
            public Func<JiceContainer, TInterface> BuildInstance { get; set; }
            public IJiceLifeCycleManager<TInterface> LifeCycleManager { get; set; }
            public override IJiceObjectManager ObjectManager { get { return LifeCycleManager; } }
        }
    }
    
    /// <summary>
    /// This exception is thrown when the container is unable to complete the requested type Resolution
    /// </summary>
    public sealed class ResolutionException : Exception
    {
        public Type AttemptedType { get; set; }
        internal ResolutionException(string message, Type attemptedType) : this(message, attemptedType, null) { }
        internal ResolutionException(string message, Type attemptedType, Exception inner) : base(message, inner)
        {
            AttemptedType = attemptedType;
        }
    }

    /// <summary>
    /// This exception is thrown when the container fails to complete the requested registration
    /// </summary>
    public sealed class RegistrationException : Exception
    {
        public Type AttemptedType { get; set; }
        internal RegistrationException(string message, Type attemptedType) : this(message, attemptedType, null) { }
        internal RegistrationException(string message, Type attemptedType, Exception inner) : base(message, inner)
        {
            AttemptedType = attemptedType;
        }
    }

    /// <summary>
    /// This is the pre registration object, any pre-registration operations (i.e. naming) will happen on this object
    /// </summary>
    /// <typeparam name="TInterface">Interface/Abstract type</typeparam>
    public abstract class JiceRegistationContext<TInterface>
    {
        internal abstract JiceContainer.ResolutionInfo<TInterface> ResolutionInfo { get; }

        /// <summary>
        /// Assigns the registration to a concert type as a transient
        /// </summary>
        public abstract JiceRegistration<TInterface, TImplementation> To<TImplementation>() where TImplementation : TInterface;

        /// <summary>
        /// Assigns the registration to a concert type with a predefined object as a singleton
        /// </summary>
        public abstract JiceRegistration<TInterface, TImplementation> To<TImplementation>(TImplementation ti) where TImplementation : TInterface;
    }

    /// <summary>
    /// Post registration object, once you have this object the registration is persisted and you can adjust 
    /// any post registration configuration options, i.e. life cycle management.
    /// </summary>
    /// <typeparam name="TInterface">Interface/Abstract type</typeparam>
    /// <typeparam name="TImplementation">Concrete type</typeparam>
    public abstract class JiceRegistration<TInterface, TImplementation> where TImplementation : TInterface
    {
        internal abstract JiceContainer.ResolutionInfo<TInterface> ResolutionInfo { get; }

        /// <summary>
        /// Provides the registration with a direct method for building an instance
        /// </summary>
        /// <returns>the original object</returns>
        public abstract JiceRegistration<TInterface, TImplementation> ConstructAs(Func<JiceContainer, TImplementation> fun);
    }

    /// <summary>
    /// This is the base (non-generic) interface for object life cycle managers, custom life cycles should always implement <see cref="IJiceLifeCycleManager"/>
    /// </summary>
    public interface IJiceObjectManager : IDisposable
    {
        /// <summary>
        /// returns a non-generic object instance
        /// </summary>
        object GetObject();
    }

    /// <summary>
    /// Interface for all life cycle managers.  The life cycle manager for a <see cref="ResolutionInfo"/> can be set by an extension method on the <see cref="JiceRegistration"/> class.
    /// </summary>
    /// <typeparam name="TInterface">Interface type, this is not the same as the concrete implementation type</typeparam>
    public interface IJiceLifeCycleManager<TInterface> : IJiceObjectManager
    {
        /// <summary>
        /// Provides a generic object instance, as the interface type, for this Lifecycle
        /// </summary>
        TInterface GetConcrete();
    }
}