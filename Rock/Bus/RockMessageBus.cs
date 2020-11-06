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

using MassTransit;
using Rock.Bus.Consumer;
using Rock.Bus.Message;
using Rock.Bus.Queue;
using Rock.Bus.Transport;
using Rock.Model;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Rock.Bus
{
    /// <summary>
    /// Rock Bus Process Controls: Start the bus
    /// </summary>
    public static class RockMessageBus
    {
        /// <summary>
        /// Is the bus started?
        /// </summary>
        private static bool _isBusStarted = false;

        /// <summary>
        /// The bus
        /// </summary>
        private static IBusControl _bus = null;

        /// <summary>
        /// The transport component
        /// </summary>
        private static TransportComponent _transportComponent = null;

        /// <summary>
        /// The Rock instance unique identifier
        /// </summary>
        public static readonly Guid RockInstanceGuid = Guid.NewGuid();

        /// <summary>
        /// Starts this bus.
        /// </summary>
        public static async Task StartAsync()
        {
            var components = TransportContainer.Instance.Components.Select( c => c.Value.Value );
            var inMemoryTransport = components.FirstOrDefault( c => c is InMemory );

            // If the in-memory transport is active, then it will be the transport. Admins will need to
            // explicitly deactivate in-memory for another transport to work. This is to ensure Rock
            // instances without explicit config (most) will function with the in-memory transport.
            if ( inMemoryTransport.IsActive )
            {
                _transportComponent = inMemoryTransport;
            }
            else
            {
                // If there is no active transport, in-memory is used anyway. Effectively, in-memory cannot
                // be deactivated because it would cause Rock to not function.
                _transportComponent = components.FirstOrDefault( c => c.IsActive ) ?? inMemoryTransport;
            }

            try
            {
                await ConfigureBusAsync();
            }
            catch ( Exception e )
            {
                // An error occured with the other transport so try one more time with in-memory to see if Rock
                // can still start to allow UI based configuration
                if ( _transportComponent == inMemoryTransport )
                {
                    // Already tried in-memory and it is what threw the exception
                    throw;
                }

                // Switch to in-memory
                var originalTransport = _transportComponent;
                _transportComponent = inMemoryTransport;

                // Start-up with in-memory
                await ConfigureBusAsync();

                // Log that the original transport did not work
                ExceptionLogService.LogException( new ConfigurationException( $"Could not start the message bus transport: {originalTransport.GetType().Name}", e ) );
            }
        }

        /// <summary>
        /// Publishes the message.
        /// </summary>
        /// <param name="message">The message.</param>
        public static async Task PublishAsync<TQueue, TMessage>( TMessage message )
            where TQueue : IPublishEventQueue, new()
            where TMessage : class, IEventMessage<TQueue>
        {
            await PublishAsync( message, typeof( TMessage ) );
        }

        /// <summary>
        /// Publishes the message.
        /// </summary>
        /// <typeparam name="TQueue">The type of the queue.</typeparam>
        /// <param name="message">The message.</param>
        /// <param name="messageType">Type of the message.</param>
        public static async Task PublishAsync<TQueue>( IEventMessage<TQueue> message, Type messageType )
            where TQueue : IPublishEventQueue, new()
        {
            if ( !IsReady() )
            {
                ExceptionLogService.LogException( $"A message was published before the message bus was ready: {RockMessage.GetLogString( message )}" );
                return;
            }

            await _bus.Publish( message, messageType, context =>
            {
                context.TimeToLive = RockQueue.GetTimeToLive<TQueue>();
            } );
        }

        /// <summary>
        /// Sends the message.
        /// </summary>
        /// <param name="message">The message.</param>
        public static async Task SendAsync<TQueue, TMessage>( TMessage message )
            where TQueue : ISendCommandQueue, new()
            where TMessage : class, ICommandMessage<TQueue>
        {
            await SendAsync( message, typeof( TMessage ) );
        }

        /// <summary>
        /// Sends the command message.
        /// </summary>
        /// <typeparam name="TQueue">The type of the queue.</typeparam>
        /// <param name="message">The message.</param>
        /// <param name="messageType">Type of the message.</param>
        public static async Task SendAsync<TQueue>( ICommandMessage<TQueue> message, Type messageType )
            where TQueue : ISendCommandQueue, new()
        {
            if ( !IsReady() )
            {
                ExceptionLogService.LogException( $"A message was sent before the message bus was ready: {RockMessage.GetLogString( message )}" );
                return;
            }

            var queue = RockQueue.Get<TQueue>();
            var endpoint = _transportComponent.GetSendEndpoint( _bus, queue.NameForConfiguration );

            await endpoint.Send( message, messageType, context =>
            {
                context.TimeToLive = RockQueue.GetTimeToLive( queue );
            } );
        }

        /// <summary>
        /// Configures the bus.
        /// </summary>
        /// <returns></returns>
        private async static Task ConfigureBusAsync()
        {
            if ( _transportComponent == null )
            {
                throw new ConfigurationException( "An active transport component is required for Rock to run correctly" );
            }

            _bus = _transportComponent.GetBusControl( RockConsumer.ConfigureRockConsumers );

            // Allow the bus to try to connect for some seconds at most
            var task = _bus.StartAsync();
            var secondsToWait = 15;

            if ( await Task.WhenAny( task, Task.Delay( TimeSpan.FromSeconds( secondsToWait ) ) ) == task )
            {
                // Task completed within timeout.
                // Consider that the task may have faulted or been canceled.
                // We re-await the task so that any exceptions/cancellation is rethrown.
                // https://stackoverflow.com/a/11191070/13215483
                await task;
            }
            else
            {
                // The bus did not connect after some seconds
                throw new ConfigurationException( $"The bus failed to connect using {_transportComponent.GetType().Name} within {secondsToWait} seconds" );
            }

            _isBusStarted = true;
        }

        /// <summary>
        /// Determines whether this instance is ready.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance is ready; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsReady()
        {
            return _isBusStarted && _transportComponent != null && _bus != null;
        }

        /// <summary>
        /// Gets a completed task. This is a helper to be used for when there is a Task
        /// return type and no task is really necessary. The task returned is immediately
        /// resolved.
        /// </summary>
        /// <returns></returns>
        internal static Task GetCompletedTask()
        {
            return Task.Delay( 0 );
        }
    }
}
