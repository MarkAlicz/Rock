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
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Rock.Bus.Observer
{
    /// <summary>
    /// Message Envelope
    /// </summary>
    public sealed class MessageEnvelope
    {
        /// <summary>
        /// Gets or sets the message identifier.
        /// </summary>
        /// <value>
        /// The message identifier.
        /// </value>
        public string MessageId { get; set; }

        /// <summary>
        /// Gets or sets the request identifier.
        /// </summary>
        /// <value>
        /// The request identifier.
        /// </value>
        public string RequestId { get; set; }

        /// <summary>
        /// Gets or sets the correlation identifier.
        /// </summary>
        /// <value>
        /// The correlation identifier.
        /// </value>
        public string CorrelationId { get; set; }

        /// <summary>
        /// Gets or sets the conversation identifier.
        /// </summary>
        /// <value>
        /// The conversation identifier.
        /// </value>
        public string ConversationId { get; set; }

        /// <summary>
        /// Gets or sets the initiator identifier.
        /// </summary>
        /// <value>
        /// The initiator identifier.
        /// </value>
        public string InitiatorId { get; set; }

        /// <summary>
        /// Gets or sets the source address.
        /// </summary>
        /// <value>
        /// The source address.
        /// </value>
        public string SourceAddress { get; set; }

        /// <summary>
        /// Gets or sets the destination address.
        /// </summary>
        /// <value>
        /// The destination address.
        /// </value>
        public string DestinationAddress { get; set; }

        /// <summary>
        /// Gets or sets the response address.
        /// </summary>
        /// <value>
        /// The response address.
        /// </value>
        public string ResponseAddress { get; set; }

        /// <summary>
        /// Gets or sets the fault address.
        /// </summary>
        /// <value>
        /// The fault address.
        /// </value>
        public string FaultAddress { get; set; }

        /// <summary>
        /// Gets or sets the type of the message.
        /// </summary>
        /// <value>
        /// The type of the message.
        /// </value>
        public List<string> MessageType { get; set; }

        /// <summary>
        /// Gets or sets the message.
        /// </summary>
        /// <value>
        /// The message.
        /// </value>
        public JObject Message { get; set; }

        /// <summary>
        /// Gets or sets the expiration time.
        /// </summary>
        /// <value>
        /// The expiration time.
        /// </value>
        public DateTime? ExpirationTime { get; set; }

        /// <summary>
        /// Gets or sets the sent time.
        /// </summary>
        /// <value>
        /// The sent time.
        /// </value>
        public DateTime? SentTime { get; set; }

        /// <summary>
        /// Gets or sets the headers.
        /// </summary>
        /// <value>
        /// The headers.
        /// </value>
        public IDictionary<string, object> Headers { get; set; }
    }
}
