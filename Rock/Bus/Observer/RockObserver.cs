// <copyright>
// Copyright by the Spark Development Network
//
// Licensed under the Rock Community License (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.rockrms.com/license
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
// </copyright>
//

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using MassTransit;
using Rock.Bus.Message;
using Rock.Bus.Queue;

namespace Rock.Bus.Observer
{
    /// <summary>
    /// Rock Observer Interface.
    /// </summary>
    public interface IRockObserver
    {
        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        IRockObserver Instance { get; }
    }

    /// <summary>
    /// Rock Observer Interface
    /// </summary>
    public interface IRockObserver<TQueue, TMessage> : IRockObserver
        where TQueue : IRockQueue, new()
        where TMessage : class, IRockMessage<TQueue>
    {
        /// <summary>
        /// Observes the specified message.
        /// </summary>
        /// <param name="receiveContext">The receive context.</param>
        /// <param name="messageEnvelope">The message envelope.</param>
        /// <param name="message">The message.</param>
        void Observe( ReceiveContext receiveContext, MessageEnvelope messageEnvelope, TMessage message );
    }

    /// <summary>
    /// Rock Observer
    /// </summary>
    /// <typeparam name="TQueue">The type of the queue.</typeparam>
    /// <typeparam name="TMessage">The type of the message.</typeparam>
    /// <seealso cref="Rock.Bus.Observer.IRockObserver{TQueue, TMessage}" />
    public abstract class RockObserver<TQueue, TMessage> : IRockObserver<TQueue, TMessage>
        where TQueue : IRockQueue, new()
        where TMessage : class, IRockMessage<TQueue>
    {
        /// <summary>
        /// Gets the receive context.
        /// </summary>
        /// <value>
        /// The receive context.
        /// </value>
        protected ReceiveContext ReceiveContext { get; private set; } = null;

        /// <summary>
        /// Gets the message envelope.
        /// </summary>
        /// <value>
        /// The message envelope.
        /// </value>
        protected MessageEnvelope MessageEnvelope { get; private set; } = null;

        /// <summary>
        /// Observes the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        public abstract void Observe( TMessage message );

        /// <summary>
        /// Observes the specified receive context.
        /// </summary>
        /// <param name="receiveContext">The receive context.</param>
        /// <param name="messageEnvelope">The message envelope.</param>
        /// <param name="message">The message.</param>
        public virtual void Observe( ReceiveContext receiveContext, MessageEnvelope messageEnvelope, TMessage message )
        {
            MessageEnvelope = messageEnvelope;
            ReceiveContext = receiveContext;
            Observe( message );
        }

        /// <summary>
        /// Gets an instance of the queue.
        /// </summary>
        /// <returns></returns>
        public static IRockQueue GetQueue()
        {
            return RockQueue.Get<TQueue>();
        }

        /// <summary>
        /// Gets the instance.
        /// </summary>
        /// <value>
        /// The instance.
        /// </value>
        public virtual IRockObserver Instance => Activator.CreateInstance( GetType() ) as IRockObserver;
    }

    /// <summary>
    /// Rock Message Bus Observer Helpers
    /// </summary>
    public static class RockObserver
    {
        private static readonly Type _genericInterfaceType = typeof( IRockObserver<,> );
        private static ConcurrentDictionary<string, ConcurrentBag<Tuple<Type, Type, IRockObserver>>> _observersByMessageType = null;

        /// <summary>
        /// Configures the rock Observers.
        /// </summary>
        /// <param name="bus">The bus.</param>
        public static void ConfigureRockObservers( IBusControl bus )
        {
            // Gather observers in assemblies by message type
            var observerTypesByMessageType = GetObserverTypes()
                .GroupBy( o => GetMessageType( o ) )
                .ToDictionary( g => g.Key, g => g.ToList() );

            var messageTypes = observerTypesByMessageType.Keys;
            _observersByMessageType = new ConcurrentDictionary<string, ConcurrentBag<Tuple<Type, Type, IRockObserver>>>();

            foreach ( var messageType in messageTypes )
            {
                var messageUrn = MessageUrn.ForType( messageType );
                var messageTypeId = messageUrn.ToString();

                var observerTypes = observerTypesByMessageType[messageType];
                var observers = new ConcurrentBag<Tuple<Type, Type, IRockObserver>>();
                _observersByMessageType[messageTypeId] = observers;

                foreach ( var observerType in observerTypes )
                {
                    var queueType = GetQueueType( observerType );
                    var observer = Activator.CreateInstance( observerType ) as IRockObserver;
                    observers.Add( new Tuple<Type, Type, IRockObserver>( queueType, messageType, observer ) );
                }
            }

            // Connect the router, which observes all received messages
            var router = new RockObserverRouter();
            bus.ConnectReceiveObserver( router );
        }

        /// <summary>
        /// Gets the observers.
        /// </summary>
        /// <param name="messageType">Type of the message.</param>
        /// <returns></returns>
        public static IEnumerable<Tuple<Type, Type, IRockObserver>> GetQueueMessageAndObservers( string messageType )
        {
            if ( _observersByMessageType.ContainsKey( messageType ) )
            {
                return _observersByMessageType[messageType];
            }

            return new List<Tuple<Type, Type, IRockObserver>>();
        }

        /// <summary>
        /// Gets the Observer types.
        /// </summary>
        /// <returns></returns>
        public static List<Type> GetObserverTypes()
        {
            var observerTypes = new Dictionary<string, Type>();
            var assemblies = Reflection.GetRockAndPluginAssemblies();
            var types = assemblies
                .SelectMany( a => a.GetTypes()
                .Where( t => t.IsClass && ( t.IsPublic || t.IsNestedPublic ) ) );

            foreach ( var type in types )
            {
                if ( IsRockObserver( type ) )
                {
                    observerTypes.AddOrIgnore( type.FullName, type );
                }
            }

            var observerTypeList = observerTypes.Select( kvp => kvp.Value ).ToList();
            return observerTypeList;
        }

        /// <summary>
        /// Determines if the type is a Rock Observer type.
        /// </summary>
        /// <param name="type">The type.</param>
        /// <returns>
        ///   <c>true</c> if [is rock Observer] [the specified type]; otherwise, <c>false</c>.
        /// </returns>
        public static bool IsRockObserver( Type type )
        {
            if ( type.IsAbstract || type.ContainsGenericParameters )
            {
                return false;
            }

            var typeInterfaces = type.GetInterfaces().Where( i => i.IsGenericType );

            foreach ( var typeInterface in typeInterfaces )
            {
                var genericInterface = typeInterface.GetGenericTypeDefinition();

                if ( genericInterface == _genericInterfaceType )
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Gets the queue for the Observer type.
        /// </summary>
        /// <param name="observerType">Type of the Observer.</param>
        /// <returns></returns>
        public static IRockQueue GetQueue( Type observerType )
        {
            var queueType = GetQueueType( observerType );
            return RockQueue.Get( queueType );
        }

        /// <summary>
        /// Gets the queue type for the Observer type.
        /// </summary>
        /// <param name="observerType">Type of the Observer.</param>
        /// <returns></returns>
        public static Type GetQueueType( Type observerType )
        {
            var queueInterface = typeof( IRockQueue );
            var typeInterfaces = observerType.GetInterfaces().Where( i => i.IsGenericType );

            foreach ( var typeInterface in typeInterfaces )
            {
                var genericInterface = typeInterface.GetGenericTypeDefinition();

                if ( genericInterface == _genericInterfaceType )
                {
                    // There are two generic type arguments. The first is the queue and the second is the message
                    return typeInterface.GenericTypeArguments[0];
                }
            }

            return null;
        }

        /// <summary>
        /// Gets the message type for the Observer type.
        /// </summary>
        /// <param name="observerType">Type of the Observer.</param>
        /// <returns></returns>
        public static Type GetMessageType( Type observerType )
        {
            var messageInterface = typeof( IRockMessage<> );
            var typeInterfaces = observerType.GetInterfaces().Where( i => i.IsGenericType );

            foreach ( var typeInterface in typeInterfaces )
            {
                var genericInterface = typeInterface.GetGenericTypeDefinition();

                if ( genericInterface == _genericInterfaceType )
                {
                    // There are two generic type arguments. The first is the queue and the second is the message
                    return typeInterface.GenericTypeArguments[1];
                }
            }

            return null;
        }
    }
}
