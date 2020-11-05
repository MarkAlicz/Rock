﻿// <copyright>
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
using System.Linq;
using Rock.Bus.Message;
using Rock.Bus.Queue;
using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Transactions
{
    /// <summary>
    /// Transaction to process achievements for updated source entities
    /// </summary>
    public class AchievementsProcessingTransaction : BusStartedTransaction<AchievementsProcessingTransaction.Message>
    {
        #region Instance Properties

        /// <summary>
        /// Gets or sets the source entities.
        /// </summary>
        /// <value>
        /// The source entities.
        /// </value>
        public IEnumerable<IEntity> SourceEntities { get; set; }

        #endregion Instance Properties

        #region Abstract Implementation

        /// <summary>
        /// Executes this instance.
        /// </summary>
        public override void Execute( Message message )
        {
            if ( message == null )
            {
                return;
            }

            var type = Type.GetType( message.EntityTypeName );
            var entity = Reflection.GetIEntityForEntityType( type, message.EntityGuid );

            if ( entity == null )
            {
                return;
            }

            AchievementTypeCache.ProcessAchievements( entity );
        }

        /// <summary>
        /// Generate messages from the instance properties.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public override IEnumerable<Message> GetMessagesToSend()
        {
            return SourceEntities?.Select( e => new Message
            {
                EntityGuid = e.Guid,
                EntityTypeName = e.TypeName
            } );
        }

        #endregion Abstract Implementation

        #region Helper Classes

        /// <summary>
        /// Message Class
        /// </summary>
        public sealed class Message : ICommandMessage<StartTaskQueue>
        {
            /// <summary>
            /// Gets or sets the entity type name.
            /// </summary>
            /// <value>
            /// The entity type identifier.
            /// </value>
            public string EntityTypeName { get; set; }

            /// <summary>
            /// Gets or sets the entity guid.
            /// </summary>
            /// <value>
            /// The entity identifier.
            /// </value>
            public Guid EntityGuid { get; set; }
        }

        #endregion Helper Classes
    }
}