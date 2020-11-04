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

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Rock.Data;
using Rock.Web.Cache;

namespace Rock.Model
{
    /// <summary>
    /// Represents WebFarmSettings in Rock.
    /// </summary>
    [RockDomain( "WebFarm" )]
    [Table( "WebFarmSettings" )]
    [DataContract]
    public partial class WebFarmSettings : Model<WebFarmSettings>
    {
        #region Constants

        /// <summary>
        /// The default leadership polling interval lower limit seconds
        /// </summary>
        public const int DefaultLeadershipPollingIntervalLowerLimitSeconds = 50;

        /// <summary>
        /// The default leadership polling interval upper limit seconds
        /// </summary>
        public const int DefaultLeadershipPollingIntervalUpperLimitSeconds = 70;

        #endregion Constants

        #region Entity Properties

        /// <summary>
        /// Is the Web Farm enabled. This property is required.
        /// </summary>
        [DataMember( IsRequired = true )]
        [Required]
        public bool WebFarmEnabled { get; set; }

        /// <summary>
        /// Gets or sets the leadership polling interval lower limit seconds.
        /// </summary>
        /// <value>
        /// The leadership polling interval lower limit seconds.
        /// </value>
        [DataMember]
        public int LeadershipPollingIntervalLowerLimitSeconds { get; set; } = DefaultLeadershipPollingIntervalLowerLimitSeconds;

        /// <summary>
        /// Gets or sets the leadership polling interval upper limit seconds.
        /// </summary>
        /// <value>
        /// The leadership polling interval upper limit seconds.
        /// </value>
        [DataMember]
        public int LeadershipPollingIntervalUpperLimitSeconds { get; set; } = DefaultLeadershipPollingIntervalUpperLimitSeconds;

        #endregion Entity Properties

        #region Entity Configuration

        /// <summary>
        /// WebFarmSettings Configuration class.
        /// </summary>
        public partial class WebFarmSettingsConfiguration : EntityTypeConfiguration<WebFarmSettings>
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="WebFarmSettingsConfiguration"/> class.
            /// </summary>
            public WebFarmSettingsConfiguration()
            {
            }
        }

        #endregion Entity Configuration

        #region Overrides

        /// <summary>
        /// Gets a value indicating whether this instance is valid.
        /// </summary>
        public override bool IsValid
        {
            get
            {
                var isValid = base.IsValid;

                if ( LeadershipPollingIntervalLowerLimitSeconds < 1 )
                {
                    var message = $"{nameof( LeadershipPollingIntervalLowerLimitSeconds )} cannot be less than 1";
                    ValidationResults.Add( new ValidationResult( message ) );
                    isValid = false;
                }

                if ( LeadershipPollingIntervalUpperLimitSeconds < 1 )
                {
                    var message = $"{nameof( LeadershipPollingIntervalUpperLimitSeconds )} cannot be less than 1";
                    ValidationResults.Add( new ValidationResult( message ) );
                    isValid = false;
                }

                if ( LeadershipPollingIntervalUpperLimitSeconds <= LeadershipPollingIntervalLowerLimitSeconds )
                {
                    var message = $"{nameof( LeadershipPollingIntervalUpperLimitSeconds )} cannot be less than or equal to the {nameof( LeadershipPollingIntervalLowerLimitSeconds )}";
                    ValidationResults.Add( new ValidationResult( message ) );
                    isValid = false;
                }

                return isValid;
            }
        }

        #endregion Overrides
    }
}
