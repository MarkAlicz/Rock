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
using System.Configuration;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Rock.Bus.Message;
using Rock.Data;
using Rock.Model;

namespace Rock.WebFarm
{
    /// <summary>
    /// Web Farm
    /// </summary>
    public static class RockWebFarm
    {
        #region Helper Classes

        /// <summary>
        /// Event Types
        /// </summary>
        internal static class EventType
        {
            /// <summary>
            /// Startup
            /// </summary>
            public const string Startup = "Startup";

            /// <summary>
            /// Shutdown
            /// </summary>
            public const string Shutdown = "Shutdown";

            /// <summary>
            /// Error
            /// </summary>
            public const string Warning = "Warning";

            /// <summary>
            /// Ping
            /// </summary>
            public const string Ping = "Ping";

            /// <summary>
            /// Pong
            /// </summary>
            public const string Pong = "Pong";
        }

        #endregion Helper Classes

        #region State

        /// <summary>
        /// Do debug logging
        /// </summary>
        private const bool DEBUG = true;

        /// <summary>
        /// The web farm enabled
        /// </summary>
        private static bool _webFarmEnabled = false;

        /// <summary>
        /// The start stage
        /// </summary>
        private static int _startStage = 0;

        /// <summary>
        /// The node name
        /// </summary>
        private static string _nodeName = null;

        /// <summary>
        /// The node identifier
        /// </summary>
        private static int _nodeId = 0;

        /// <summary>
        /// Was this instance pinged?
        /// </summary>
        private static bool _wasPinged = false;

        /// <summary>
        /// The polling interval seconds
        /// </summary>
        private static decimal _pollingIntervalSeconds = WebFarmSettings.DefaultLeadershipPollingIntervalUpperLimitSeconds;

        /// <summary>
        /// The polling interval
        /// </summary>
        private static IntervalAction _pollingInterval;

        #endregion State

        #region Startup and Shutdown

        /// <summary>
        /// Stage 1 Start.
        /// Called before Hot load caches and start-up activities( EF migrations etc )
        /// </summary>
        public static void StartStage1()
        {
            if ( _startStage != 0 )
            {
                LogException( $"Web Farm cannot start stage 1 when at stage {_startStage}" );
                return;
            }

            Debug( "Start Stage 1" );

            using ( var rockContext = new RockContext() )
            {
                // Check that the WebFarmEnable = true.If yes, continue
                var webFarmSettingsService = new WebFarmSettingsService( rockContext );
                var webFarmSettings = webFarmSettingsService.Queryable().FirstOrDefault();

                if ( webFarmSettings?.WebFarmEnabled != true )
                {
                    return;
                }

                // Check license key in the SystemSetting
                if ( !HasValidKey() )
                {
                    return;
                }

                _webFarmEnabled = true;

                // Find node record in DB using node name, if not found create a new record
                _nodeName = GetNodeName();
                var webFarmNodeService = new WebFarmNodeService( rockContext );
                var webFarmNode = webFarmNodeService.Queryable().FirstOrDefault( wfn => wfn.NodeName == _nodeName );
                var isNewNode = webFarmNode == null;

                if ( isNewNode )
                {
                    webFarmNode = new WebFarmNode
                    {
                        NodeName = _nodeName
                    };

                    webFarmNodeService.Add( webFarmNode );
                    rockContext.SaveChanges();
                }

                _nodeId = webFarmNode.Id;

                // Determine leadership polling interval. If provided in database( ConfiguredLeadershipPollingIntervalSeconds ) use that
                // otherwise randomly select a number between upper and lower limits( number = seconds * 10 ) -Check to see that no one
                // else has registered the same number in the database. If so re - choose. Possibly this could be a SQL Function that would
                // ensure that only unique values were handed out.
                const int maxGenerationAttempts = 50;
                var generationAttempts = 1;

                _pollingIntervalSeconds = webFarmNode.ConfiguredLeadershipPollingIntervalSeconds ?? GeneratePollingIntervalSeconds(
                    webFarmSettings.LeadershipPollingIntervalLowerLimitSeconds,
                    webFarmSettings.LeadershipPollingIntervalUpperLimitSeconds );

                var isPollingIntervalInUse = IsPollingIntervalInUse( rockContext, _nodeName, _pollingIntervalSeconds );

                while ( generationAttempts < maxGenerationAttempts && isPollingIntervalInUse )
                {
                    generationAttempts++;

                    _pollingIntervalSeconds = GeneratePollingIntervalSeconds(
                        webFarmSettings.LeadershipPollingIntervalLowerLimitSeconds,
                        webFarmSettings.LeadershipPollingIntervalUpperLimitSeconds );

                    isPollingIntervalInUse = IsPollingIntervalInUse( rockContext, _nodeName, _pollingIntervalSeconds );
                }

                if ( isPollingIntervalInUse )
                {
                    LogException( $"Web farm node {_nodeName} did not successfully pick a polling interval after {maxGenerationAttempts} attempts" );
                    return;
                }

                // If StoppedDateTime is currently null then write to ClusterNodeLog -"Detected previous abrupt shutdown on load."
                if ( !isNewNode && !webFarmNode.StoppedDateTime.HasValue )
                {
                    AddLog( rockContext, webFarmNode.Id, EventType.Warning, "Detected previous abrupt shutdown on load." );
                }

                // Save the polling internval
                // If web.config set to run jobs make IsCurrentJobRunner = true
                // Set StoppedDateTime to null
                // Update LastRestartDateTime to now
                webFarmNode.CurrentLeadershipPollingIntervalSeconds = _pollingIntervalSeconds;
                webFarmNode.IsCurrentJobRunner = IsCurrentJobRunner();
                webFarmNode.StoppedDateTime = null;
                webFarmNode.LastRestartDateTime = RockDateTime.Now;
                webFarmNode.LastSeenDateTime = RockDateTime.Now;
                webFarmNode.IsActive = false;

                // Write to ClusterNodeLog -Startup Message
                AddLog( rockContext, webFarmNode.Id, EventType.Startup );

                rockContext.SaveChanges();
            }

            _startStage = 1;
            Debug( "Done with Stage 1" );
        }

        /// <summary>
        /// Start Stage 2.
        /// Called after Hot load caches and start-up activities( EF migrations etc )
        /// </summary>
        public static void StartStage2()
        {
            if ( !_webFarmEnabled )
            {
                return;
            }

            if ( _startStage != 1 )
            {
                LogException( $"Web Farm cannot start stage 2 when at stage {_startStage}" );
                return;
            }

            Debug( "Start Stage 2" );

            using ( var rockContext = new RockContext() )
            {
                // Mark IsActive true
                // Update LastSeenDateTime = now
                var webFarmNode = GetNode( rockContext, _nodeName );
                webFarmNode.IsActive = true;
                webFarmNode.LastSeenDateTime = RockDateTime.Now;
                rockContext.SaveChanges();
            }

            // Start the polling cycle
            _pollingInterval = IntervalAction.Start( DoLeadershipPollAsync, TimeSpan.FromSeconds( decimal.ToDouble( _pollingIntervalSeconds ) ) );

            // Annouce startup to EventBus
            PublishEvent( EventType.Startup );
            _startStage = 2;

            Debug( "Done with Stage 2" );
        }

        /// <summary>
        /// Stops this instance.
        /// </summary>
        public static void Shutdown()
        {
            if ( !_webFarmEnabled )
            {
                return;
            }

            if ( _startStage != 2 )
            {
                LogException( $"Web Farm cannot shutdown properly when at stage {_startStage}" );
                return;
            }

            Debug( "Shutdown" );

            // Stop the polling interval
            _pollingInterval.Stop();

            // Announce to stop EventBus
            PublishEvent( EventType.Shutdown );

            using ( var rockContext = new RockContext() )
            {
                // Update IsActive = false, StoppedDateTime = now
                // If IsCurrentJobRunning = true set this to false
                var webFarmNode = GetNode( rockContext, _nodeName );
                webFarmNode.IsActive = false;
                webFarmNode.StoppedDateTime = RockDateTime.Now;
                webFarmNode.IsCurrentJobRunner = false;

                // Write to ClusterNodeLog shutdown message
                AddLog( rockContext, webFarmNode.Id, EventType.Shutdown );
                rockContext.SaveChanges();
            }

            _startStage = 0;
        }

        #endregion Startup and Shutdown

        #region Event Handlers

        /// <summary>
        /// Called when [ping].
        /// </summary>
        /// <param name="senderNodeName">Name of the node that pinged.</param>
        internal static void OnReceivedPing( string senderNodeName )
        {
            if ( senderNodeName == _nodeName )
            {
                // Don't talk to myself
                return;
            }

            Debug( $"Got a Ping from {senderNodeName}" );
            _wasPinged = true;

            // Reply to the leader (sender of the ping)
            PublishEvent( EventType.Pong, senderNodeName );
        }

        /// <summary>
        /// Called when [pong].
        /// </summary>
        /// <param name="senderNodeName">Name of the node that ponged.</param>
        /// <param name="recipientNodeName">Name of the recipient node.</param>
        internal static void OnReceivedPong( string senderNodeName, string recipientNodeName )
        {
            if ( senderNodeName == _nodeName )
            {
                // Don't talk to myself
                return;
            }

            if ( !recipientNodeName.IsNullOrWhiteSpace() && recipientNodeName != _nodeName )
            {
                // This message is not for me
                return;
            }

            Debug( $"Got a Pong from {senderNodeName}" );

            using ( var rockContext = new RockContext() )
            {
                var node = GetNode( rockContext, senderNodeName );

                // Write to log if the server was thought to be inactive but responded
                if ( !node.IsActive )
                {
                    AddLog( rockContext, node.Id, EventType.Warning, $"{node.NodeName} was marked inactive but responded to a ping" );
                }

                node.LastSeenDateTime = RockDateTime.Now;
                node.IsActive = true;
                rockContext.SaveChanges();
            }
        }

        /// <summary>
        /// Does the leadership poll.
        /// </summary>
        internal static async Task DoLeadershipPollAsync()
        {
            // If another node pinged this node, then that node is the leader, not this one
            if ( _wasPinged )
            {
                Debug( "My time to poll. I was pinged, so I'm not the leader" );
                _wasPinged = false;
                return;
            }

            Debug( "My time to poll. I was not pinged, so I'm starting leadership duties" );

            // Ping other nodes
            var pollingTime = RockDateTime.Now;
            PublishEvent( EventType.Ping );

            // Assert this node's leadership in the database
            using ( var rockContext = new RockContext() )
            {
                var webFarmNodeService = new WebFarmNodeService( rockContext );
                var nodes = webFarmNodeService.Queryable().ToList();
                var thisNode = nodes.FirstOrDefault( wfn => wfn.NodeName == _nodeName );
                var otherNodes = nodes.Where( wfn => wfn.NodeName != _nodeName );

                thisNode.IsLeader = true;
                thisNode.LastSeenDateTime = pollingTime;

                foreach ( var otherNode in otherNodes )
                {
                    otherNode.IsLeader = false;
                }

                rockContext.SaveChanges();
            }

            // Wait a maximum of 1 second for responses
            await Task.Delay( TimeSpan.FromSeconds( 1 ) ).ContinueWith( t =>
            {
                Debug( "Checking for unresponsive nodes" );

                using ( var rockContext = new RockContext() )
                {
                    var webFarmNodeService = new WebFarmNodeService( rockContext );
                    var unresponsiveNodes = webFarmNodeService.Queryable()
                        .Where( wfn =>
                            wfn.LastSeenDateTime < pollingTime &&
                            wfn.IsActive &&
                            wfn.NodeName != _nodeName )
                        .ToList();

                    Debug( $"I found {unresponsiveNodes.Count} unresponsive nodes" );

                    foreach ( var node in unresponsiveNodes )
                    {
                        // Write to log if the server was thought to be active but did not respond
                        AddLog( rockContext, node.Id, EventType.Warning, $"{node.NodeName} was marked active but did not respond to a ping" );
                        node.IsActive = false;
                    }

                    rockContext.SaveChanges();
                }
            } );
        }

        /// <summary>
        /// Called when [received startup].
        /// </summary>
        /// <param name="senderNodeName">Name of the sender node.</param>
        internal static void OnReceivedStartup( string senderNodeName )
        {
            Debug( $"I heard that {senderNodeName} started" );
        }

        /// <summary>
        /// Called when [received shutdown].
        /// </summary>
        /// <param name="senderNodeName">Name of the sender node.</param>
        internal static void OnReceivedShutdown( string senderNodeName )
        {
            Debug( $"I heard that {senderNodeName} shutdown" );
        }

        /// <summary>
        /// Called when [received warning].
        /// </summary>
        /// <param name="senderNodeName">Name of the sender node.</param>
        /// <param name="payload">The payload.</param>
        internal static void OnReceivedWarning( string senderNodeName, string payload )
        {
            Debug( $"I heard that {senderNodeName} warned '{payload}'" );
        }

        #endregion Event Handlers

        #region Helper Methods

        /// <summary>
        /// Adds the log.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="subjectNodeId">The subject node identifier.</param>
        /// <param name="eventType">Type of the event.</param>
        /// <param name="text">The text.</param>
        private static void AddLog( RockContext rockContext, int subjectNodeId, string eventType, string text = "" )
        {
            var webFarmNodeLogService = new WebFarmNodeLogService( rockContext );

            webFarmNodeLogService.Add( new WebFarmNodeLog
            {
                WriterWebFarmNodeId = _nodeId,
                WebFarmNodeId = subjectNodeId,
                Message = text,
                EventType = eventType
            } );

            Debug( $"Logged {eventType} {text}" );
        }

        /// <summary>
        /// Gets the name of the node.
        /// </summary>
        /// <returns></returns>
        private static string GetNodeName()
        {
            var webFarmNodeName = ConfigurationManager.AppSettings["WebFarmNodeName"];

            if ( webFarmNodeName.IsNullOrWhiteSpace() )
            {
                webFarmNodeName = Environment.MachineName;
            }

            return webFarmNodeName;
        }

        /// <summary>
        /// Determines whether this Rock instance has a valid web farm key.
        /// </summary>
        /// <returns>
        ///   <c>true</c> if [has valid key]; otherwise, <c>false</c>.
        /// </returns>
        private static bool HasValidKey()
        {
            var webFarmKey = ConfigurationManager.AppSettings["WebFarmKey"];

            if ( webFarmKey.IsNullOrWhiteSpace() )
            {
                return false;
            }

            // BJW TODO
            return true;
        }

        /// <summary>
        /// Determines whether [is current job runner].
        /// </summary>
        /// <returns>
        ///   <c>true</c> if [is current job runner]; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsCurrentJobRunner()
        {
            var webFarmJobRunnerString = ConfigurationManager.AppSettings["WebFarmJobRunner"];
            return webFarmJobRunnerString.AsBoolean();
        }

        /// <summary>
        /// Gets the node record, potentially creating it.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="nodeName">Name of the node.</param>
        /// <param name="pollingInterval">The polling interval.</param>
        /// <returns>
        ///   <c>true</c> if [is polling interval in use] [the specified node name]; otherwise, <c>false</c>.
        /// </returns>
        private static bool IsPollingIntervalInUse( RockContext rockContext, string nodeName, decimal pollingInterval )
        {
            Debug( $"Checking poll interval {pollingInterval}" );

            var webFarmNodeService = new WebFarmNodeService( rockContext );
            var anyMatches = webFarmNodeService.Queryable().Any( wfn =>
                wfn.NodeName != nodeName &&
                wfn.CurrentLeadershipPollingIntervalSeconds == pollingInterval );

            return anyMatches;
        }

        /// <summary>
        /// Generates a polling interval in seconds.
        /// </summary>
        /// <returns></returns>
        private static decimal GeneratePollingIntervalSeconds( int minSeconds, int maxSeconds )
        {
            // Calculations are done in deciseconds (ds) since we get a random integer
            const int dsPerSecond = 10;

            // A configured value is always used
            var configuredValueString = ConfigurationManager.AppSettings["WebFarmNodePollingValue"];
            var configuredValueDeciSeconds = configuredValueString.AsIntegerOrNull();

            if ( configuredValueDeciSeconds.HasValue )
            {
                var configuredValueSeconds = decimal.Divide( configuredValueDeciSeconds.Value, dsPerSecond );
                return configuredValueSeconds;
            }

            // No configured value, so choose randomly
            var minDs = minSeconds * dsPerSecond;
            var maxDs = maxSeconds * dsPerSecond;

            var random = new Random();
            int randomDs = random.Next( minDs, maxDs );

            var randomSeconds = decimal.Divide( randomDs, dsPerSecond );
            return randomSeconds;
        }

        /// <summary>
        /// Gets the node.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="nodeName">Name of the node.</param>
        /// <returns></returns>
        private static WebFarmNode GetNode( RockContext rockContext, string nodeName )
        {
            var webFarmNodeService = new WebFarmNodeService( rockContext );
            var webFarmNode = webFarmNodeService.Queryable().FirstOrDefault( wfn => wfn.NodeName == nodeName );
            return webFarmNode;
        }

        /// <summary>
        /// Publishes the event.
        /// </summary>
        /// <param name="eventType">Type of the event.</param>
        /// <param name="recipientNodeName">Name of the recipient node. Omit if meant for all nodes.</param>
        /// <param name="payload">The payload.</param>
        private static void PublishEvent( string eventType, string recipientNodeName = "", string payload = "" )
        {
            Debug( $"Sending {eventType} to {( recipientNodeName.IsNullOrWhiteSpace() ? "all" : recipientNodeName )}" );
            WebFarmWasUpdatedMessage.Publish( _nodeName, eventType, recipientNodeName, payload );
        }

        /// <summary>
        /// Logs the exception.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <param name="callerMethod">The caller method.</param>
        private static void LogException( string message, [CallerMemberName] string callerMethod = "" )
        {
            if ( !_nodeName.IsNullOrWhiteSpace() && !callerMethod.IsNullOrWhiteSpace() )
            {
                ExceptionLogService.LogException( $"Web Farm Exception ({_nodeName}) from {callerMethod}: {message}" );
                return;
            }

            if ( !_nodeName.IsNullOrWhiteSpace() )
            {
                ExceptionLogService.LogException( $"Web Farm Exception ({_nodeName}): {message}" );
                return;
            }

            if ( !callerMethod.IsNullOrWhiteSpace() )
            {
                ExceptionLogService.LogException( $"Web Farm Exception from {callerMethod}: {message}" );
                return;
            }

            ExceptionLogService.LogException( $"Web Farm Exception: {message}" );
            return;
        }

        /// <summary>
        /// Debugs the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        private static void Debug( string message )
        {
            if ( DEBUG )
            {
                System.Diagnostics.Debug.WriteLine( $"\tFARM {RockDateTime.Now:mm.ss.f} {message}" );
            }
        }

        #endregion Helper Methods
    }
}
