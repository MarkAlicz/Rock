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
using System.Data.Entity;
using System.Linq;
using System.Runtime.Serialization;

using Rock.Data;
using Rock.Model;

namespace Rock.Web.Cache
{
    /// <summary>
    /// Cached WorkflowActionForm
    /// </summary>
    [Serializable]
    [DataContract]
    public class WorkflowActionFormCache : ModelCache<WorkflowActionFormCache, WorkflowActionForm>
    {

        #region Properties

        private readonly object _obj = new object();

        /// <summary>
        /// Gets or sets the notification communication email identifier.
        /// </summary>
        /// <value>
        /// The notification system communication identifier.
        /// </value>
        [DataMember]
        public int? NotificationSystemCommunicationId { get; private set; }


        /// <summary>
        /// Gets or sets the notification system email identifier.
        /// </summary>
        /// <value>
        /// The notification system email identifier.
        /// </value>
        [DataMember]
        [Obsolete( "Use NotificationSystemCommunicationId instead." )]
        [RockObsolete( "1.10" )]
        public int? NotificationSystemEmailId { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether [include actions in notification].
        /// </summary>
        /// <value>
        /// <c>true</c> if [include actions in notification]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool IncludeActionsInNotification { get; private set; }

        /// <summary>
        /// Gets or sets the header.
        /// </summary>
        /// <value>
        /// The header.
        /// </value>
        [DataMember]
        public string Header { get; private set; }

        /// <summary>
        /// Gets or sets the footer.
        /// </summary>
        /// <value>
        /// The footer.
        /// </value>
        [DataMember]
        public string Footer { get; private set; }

        /// <summary>
        /// Gets or sets the delimited list of action buttons and actions.
        /// </summary>
        /// <value>
        /// The actions.
        /// </value>
        [DataMember]
        public string Actions { get; private set; }

        /// <summary>
        /// An optional text attribute that will be updated with the action that was selected
        /// </summary>
        /// <value>
        /// The action attribute unique identifier.
        /// </value>
        [DataMember]
        public Guid? ActionAttributeGuid { get; private set; }

        /// <summary>
        /// Gets or sets whether Notes can be entered
        /// </summary>
        /// <value>
        /// The allow notes.
        /// </value>
        [DataMember]
        public bool? AllowNotes { get; private set; }

        #region Person entry related Entity Properties

        /// <summary>
        /// Gets or sets a value indicating whether a new person (and spouse) can be added
        /// </summary>
        /// <value>
        ///   <c>true</c> if [allow person entry]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool AllowPersonEntry { get; private set; }

        /// <summary>
        /// Gets or sets the person entry preHTML.
        /// </summary>
        /// <value>
        /// The person entry preHTML.
        /// </value>
        [DataMember]
        public string PersonEntryPreHtml { get; private set; }

        /// <summary>
        /// Gets or sets the person entry post HTML.
        /// </summary>
        /// <value>
        /// The person entry post HTML.
        /// </value>
        [DataMember]
        public string PersonEntryPostHtml { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether [person entry show campus].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [person entry show campus]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool PersonEntryCampusIsVisible { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether Person Entry should auto-fill with the CurrentPerson
        /// </summary>
        /// <value>
        ///   <c>true</c> if [person entry auto-fill current person]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool PersonEntryAutofillCurrentPerson { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether Person Entry should be hidden if the CurrentPerson is known
        /// </summary>
        /// <value>
        ///   <c>true</c> if [person entry hide if current person known]; otherwise, <c>false</c>.
        /// </value>
        [DataMember]
        public bool PersonEntryHideIfCurrentPersonKnown { get; private set; }

        /// <summary>
        /// Gets or sets the person entry spouse entry option.
        /// </summary>
        /// <value>
        /// The person entry spouse entry option.
        /// </value>
        [DataMember]
        public WorkflowActionFormPersonEntryOption PersonEntrySpouseEntryOption { get; private set; } = WorkflowActionFormPersonEntryOption.Hidden;

        /// <summary>
        /// Gets or sets the person entry email entry option.
        /// </summary>
        /// <value>
        /// The person entry email entry option.
        /// </value>
        [DataMember]
        public WorkflowActionFormPersonEntryOption PersonEntryEmailEntryOption { get; private set; } = WorkflowActionFormPersonEntryOption.Required;

        /// <summary>
        /// Gets or sets the person entry mobile phone entry option.
        /// </summary>
        /// <value>
        /// The person entry mobile phone entry option.
        /// </value>
        [DataMember]
        public WorkflowActionFormPersonEntryOption PersonEntryMobilePhoneEntryOption { get; private set; } = WorkflowActionFormPersonEntryOption.Hidden;

        /// <summary>
        /// Gets or sets the person entry birthdate entry option.
        /// </summary>
        /// <value>
        /// The person entry birthdate entry option.
        /// </value>
        [DataMember]
        public WorkflowActionFormPersonEntryOption PersonEntryBirthdateEntryOption { get; private set; } = WorkflowActionFormPersonEntryOption.Hidden;

        /// <summary>
        /// Gets or sets the person entry address entry option.
        /// </summary>
        /// <value>
        /// The person entry address entry option.
        /// </value>
        [DataMember]
        public WorkflowActionFormPersonEntryOption PersonEntryAddressEntryOption { get; private set; } = WorkflowActionFormPersonEntryOption.Hidden;

        /// <summary>
        /// Gets or sets the person entry marital status entry option.
        /// </summary>
        /// <value>
        /// The person entry marital entry option.
        /// </value>
        [DataMember]
        public WorkflowActionFormPersonEntryOption PersonEntryMaritalStatusEntryOption { get; private set; } = WorkflowActionFormPersonEntryOption.Hidden;

        /// <summary>
        /// Gets or sets the person entry spouse label.
        /// </summary>
        /// <value>
        /// The person entry spouse label.
        /// </value>
        [DataMember]
        public string PersonEntrySpouseLabel { get; private set; } = "Spouse";

        /// <summary>
        /// Gets or sets the person entry connection status value identifier.
        /// </summary>
        /// <value>
        /// The person entry connection status value identifier.
        /// </value>
        [DataMember]
        public int? PersonEntryConnectionStatusValueId { get; private set; }

        /// <summary>
        /// Gets or sets the person entry record status value identifier.
        /// </summary>
        /// <value>
        /// The person entry record status value identifier.
        /// </value>
        [DataMember]
        public int? PersonEntryRecordStatusValueId { get; private set; }

        /// <summary>
        /// Gets or sets the person entry address type value identifier.
        /// </summary>
        /// <value>
        /// The person entry address type value identifier.
        /// </value>
        [DataMember]
        public int? PersonEntryAddressTypeValueId { get; private set; }

        /// <summary>
        /// Gets or sets the person entry person attribute identifier.
        /// </summary>
        /// <value>
        /// The person entry person attribute identifier.
        /// </value>
        [DataMember]
        public int? PersonEntryPersonAttributeId { get; private set; }

        /// <summary>
        /// Gets or sets the person entry spouse attribute identifier.
        /// </summary>
        /// <value>
        /// The person entry spouse attribute identifier.
        /// </value>
        [DataMember]
        public int? PersonEntrySpouseAttributeId { get; private set; }

        /// <summary>
        /// Gets or sets the person entry family attribute identifier.
        /// </summary>
        /// <value>
        /// The person entry family attribute identifier.
        /// </value>
        [DataMember]
        public int? PersonEntryFamilyAttributeId { get; private set; }

        #endregion Person entry related Entity Properties

        /// <summary>
        /// Gets the defined values.
        /// </summary>
        /// <value>
        /// The defined values.
        /// </value>
        public List<WorkflowActionFormAttributeCache> FormAttributes
        {
            get
            {
                var formAttributes = new List<WorkflowActionFormAttributeCache>();

                if ( _formAttributeIds == null )
                {
                    lock ( _obj )
                    {
                        if ( _formAttributeIds == null )
                        {
                            using ( var rockContext = new RockContext() )
                            {
                                _formAttributeIds = new WorkflowActionFormAttributeService( rockContext )
                                    .Queryable().AsNoTracking()
                                    .Where( a => a.WorkflowActionFormId == Id )
                                    .Select( a => a.Id )
                                    .ToList();
                            }
                        }
                    }
                }

                if ( _formAttributeIds == null ) return formAttributes;

                foreach ( var id in _formAttributeIds )
                {
                    var formAttribute = WorkflowActionFormAttributeCache.Get( id );
                    if ( formAttribute != null && formAttribute.Attribute != null )
                    {
                        formAttributes.Add( formAttribute );
                    }
                }

                return formAttributes;
            }
        }
        private List<int> _formAttributeIds;

        /// <summary>
        /// Gets the buttons.
        /// </summary>
        /// <value>
        /// The buttons.
        /// </value>
        public List<WorkflowActionForm.LiquidButton> Buttons => WorkflowActionForm.GetActionButtons( Actions );

        #endregion

        #region Public Methods

        /// <summary>
        /// Copies from model.
        /// </summary>
        /// <param name="entity">The entity.</param>
        public override void SetFromEntity( IEntity entity )
        {
            base.SetFromEntity( entity );

            var workflowActionForm = entity as WorkflowActionForm;
            if ( workflowActionForm == null ) return;

            NotificationSystemCommunicationId = workflowActionForm.NotificationSystemCommunicationId;

#pragma warning disable CS0618 // Type or member is obsolete
            NotificationSystemEmailId = workflowActionForm.NotificationSystemEmailId;
#pragma warning disable CS0618 // Type or member is obsolete

            IncludeActionsInNotification = workflowActionForm.IncludeActionsInNotification;
            Header = workflowActionForm.Header;
            Footer = workflowActionForm.Footer;
            Actions = workflowActionForm.Actions;
            ActionAttributeGuid = workflowActionForm.ActionAttributeGuid;
            AllowNotes = workflowActionForm.AllowNotes;
            AllowPersonEntry = workflowActionForm.AllowPersonEntry;

            // set formAttributeIds to null so it load them all at once on demand
            _formAttributeIds = null;
        }

        #endregion

        #region Static Methods

        /// <summary>
        /// Removes a WorkflowActionForm from cache.
        /// </summary>
        /// <param name="id">The identifier.</param>
        public new static void Remove( int id )
        {
            var actionForm = Get( id );
            if ( actionForm != null )
            {
                foreach ( var formAttribute in actionForm.FormAttributes )
                {
                    WorkflowActionFormAttributeCache.Remove( formAttribute.Id );
                }
            }

            Remove( id.ToString() );
        }

        #endregion

    }

}