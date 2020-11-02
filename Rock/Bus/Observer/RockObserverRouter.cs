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
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using MassTransit;
using MassTransit.Serialization;
using MassTransit.Util;
using Newtonsoft.Json;
using Rock.Bus.Message;
using Rock.Bus.Queue;
using Rock.Model;

namespace Rock.Bus.Observer
{
    /// <summary>
    /// Rock Observer Router. Observes messages received by the transport and then routes to RockObservers.
    /// </summary>
    /// <seealso cref="MassTransit.IReceiveObserver" />
    internal sealed class RockObserverRouter : IReceiveObserver
    {
        /// <summary>
        /// The deserializer
        /// </summary>
        private static readonly JsonSerializer _deserializer = new JsonSerializer();

        /// <summary>
        /// The object type deserializer
        /// </summary>
        private static readonly IObjectTypeDeserializer _objectTypeDeserializer = new ObjectTypeDeserializer( _deserializer );

        /// <summary>
        /// Called when a message being consumed produced a fault
        /// </summary>
        /// <typeparam name="T">The message type</typeparam>
        /// <param name="context">The message consume context</param>
        /// <param name="duration">The consumer duration</param>
        /// <param name="consumerType">The consumer type</param>
        /// <param name="exception">The exception from the consumer</param>
        /// <returns></returns>
        public Task ConsumeFault<T>( ConsumeContext<T> context, TimeSpan duration, string consumerType, Exception exception ) where T : class
        {
            return RockMessageBus.GetCompletedTask();
        }

        /// <summary>
        /// Called when a message has been consumed by a consumer
        /// </summary>
        /// <typeparam name="T">The message type</typeparam>
        /// <param name="context">The message consume context</param>
        /// <param name="duration">The consumer duration</param>
        /// <param name="consumerType">The consumer type</param>
        /// <returns></returns>
        public Task PostConsume<T>( ConsumeContext<T> context, TimeSpan duration, string consumerType ) where T : class
        {
            return RockMessageBus.GetCompletedTask();
        }

        /// <summary>
        /// Called when the message has been received and acknowledged on the transport
        /// </summary>
        /// <param name="receiveContext">The receive context of the message</param>
        /// <returns></returns>
        public Task PostReceive( ReceiveContext receiveContext )
        {
            var envelope = GetMessageEnvelope( receiveContext );

            if ( envelope == null )
            {
                return RockMessageBus.GetCompletedTask();
            }

            foreach ( var messageTypeString in envelope.MessageType )
            {
                var queueMessageAndObservers = RockObserver.GetQueueMessageAndObservers( messageTypeString );

                foreach ( var queueMessageAndObserver in queueMessageAndObservers )
                {
                    var queueType = queueMessageAndObserver.Item1;
                    var messageType = queueMessageAndObserver.Item2;
                    var observer = queueMessageAndObserver.Item3;

                    var message = envelope.Message.ToObject( messageType );

                    typeof( IRockObserver<,> )
                        .MakeGenericType( queueType, messageType )
                        .GetMethod( "Observe" )
                        .Invoke( observer, new object[] { receiveContext, envelope, message } );
                }
            }

            return RockMessageBus.GetCompletedTask();
        }

        /// <summary>
        /// Called when a message has been delivered by the transport is about to be received by the endpoint
        /// </summary>
        /// <param name="context">The receive context of the message</param>
        /// <returns></returns>
        public Task PreReceive( ReceiveContext context )
        {
            return RockMessageBus.GetCompletedTask();
        }

        /// <summary>
        /// Called when the transport receive faults
        /// </summary>
        /// <param name="context">The receive context of the message</param>
        /// <param name="exception">The exception that was thrown</param>
        /// <returns></returns>
        public Task ReceiveFault( ReceiveContext context, Exception exception )
        {
            return RockMessageBus.GetCompletedTask();
        }

        /// <summary>
        /// Gets the message envelope.
        /// </summary>
        /// <param name="receiveContext">The receive context.</param>
        /// <returns></returns>
        private static MessageEnvelope GetMessageEnvelope( ReceiveContext receiveContext )
        {
            var messageEncoding = GetMessageEncoding( receiveContext );

            using ( var body = receiveContext.GetBodyStream() )
            using ( var reader = new StreamReader( body, messageEncoding, false, 1024, true ) )
            using ( var jsonReader = new JsonTextReader( reader ) )
            {
                var envelope = _deserializer.Deserialize<MessageEnvelope>( jsonReader );
                return envelope;
            }
        }

        /*
        /// <summary>
        /// Gets the type of the message.
        /// </summary>
        /// <param name="messageType">Type of the message.</param>
        /// <returns></returns>
        private static Type GetMessageType(string messageType)
        {
            // urn:message:Rock.Bus.Message:EntityWasUpdatedMessage[[Rock.Model:Person]]
        }
        */

        /// <summary>
        /// Gets the message encoding.
        /// https://github.com/MassTransit/MassTransit/blob/1f6a8f0509698705a8d892dd10e3cc2a96e14ed3/src/MassTransit/Serialization/JsonMessageDeserializer.cs#L75
        /// </summary>
        /// <param name="receiveContext">The receive context.</param>
        /// <returns></returns>
        private static Encoding GetMessageEncoding( ReceiveContext receiveContext )
        {
            var contentEncoding = receiveContext.TransportHeaders.Get( "Content-Encoding", default( string ) );
            return string.IsNullOrWhiteSpace( contentEncoding ) ? Encoding.UTF8 : Encoding.GetEncoding( contentEncoding );
        }
    }
}
