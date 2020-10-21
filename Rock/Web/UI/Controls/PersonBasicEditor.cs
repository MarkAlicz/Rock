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
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Dynamic;
using System.Web.UI.WebControls;

using Rock.Model;
using Rock.Web.Cache;

namespace Rock.Web.UI.Controls
{
    /// <summary>
    /// A general purpose editor for editing basic person information 
    /// </summary>
    public class PersonBasicEditor : CompositeControl, IHasValidationGroup
    {
        #region ViewStateKey

        private static class ViewStateKey
        {
            public const string PersonLabelPrefix = "PersonLabelPrefix";
            public const string ShowInColumns = "ShowInColumns";
            public const string RequireGender = "RequireGender";
        }

        #endregion

        #region Controls

        private Panel _pnlRow;
        private Panel _pnlCol1;
        private Panel _pnlCol2;
        private Panel _pnlCol3;
        private DefinedValuePicker _dvpPersonTitle;
        private RockTextBox _tbPersonFirstName;
        private RockTextBox _tbPersonLastName;
        private DefinedValuePicker _dvpPersonSuffix;
        private DefinedValuePicker _dvpPersonConnectionStatus;
        private RockRadioButtonList _rblPersonRole;
        private RockRadioButtonList _rblPersonGender;
        private DatePicker _dpPersonBirthDate;
        private GradePicker _ddlGradePicker;
        private DefinedValuePicker _dvpPersonMaritalStatus;
        private EmailBox _ebPersonEmail;
        private PhoneNumberBox _pnbMobilePhoneNumber;

        #endregion Controls

        #region Properties

        /// <summary>
        /// Gets or sets a value indicating whether [show title].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show title]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowTitle
        {
            get
            {
                EnsureChildControls();
                return _dvpPersonTitle.Visible;
            }

            set
            {
                EnsureChildControls();
                _dvpPersonTitle.Visible = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show suffix].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show suffix]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowSuffix
        {
            get
            {
                EnsureChildControls();
                return _dvpPersonSuffix.Visible;
            }

            set
            {
                EnsureChildControls();
                _dvpPersonSuffix.Visible = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show grade].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show grade]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowGrade
        {
            get
            {
                EnsureChildControls();
                return _ddlGradePicker.Visible;
            }

            set
            {
                EnsureChildControls();
                _ddlGradePicker.Visible = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show birthdate].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show birthdate]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowBirthdate
        {
            get
            {
                EnsureChildControls();
                return _dpPersonBirthDate.Visible;
            }

            set
            {
                EnsureChildControls();
                _dpPersonBirthDate.Visible = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [require birthdate].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [require birthdate]; otherwise, <c>false</c>.
        /// </value>
        public bool RequireBirthdate
        {
            get
            {
                EnsureChildControls();
                return _dpPersonBirthDate.Required;
            }

            set
            {
                EnsureChildControls();
                _dpPersonBirthDate.Required = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show email].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show email]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowEmail
        {
            get
            {
                EnsureChildControls();
                return _ebPersonEmail.Visible;
            }

            set
            {
                EnsureChildControls();
                _ebPersonEmail.Visible = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [require email].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [require email]; otherwise, <c>false</c>.
        /// </value>
        public bool RequireEmail
        {
            get
            {
                EnsureChildControls();
                return _ebPersonEmail.Required;
            }

            set
            {
                EnsureChildControls();
                _ebPersonEmail.Required = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show mobile phone].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show mobile phone]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowMobilePhone
        {
            get
            {
                EnsureChildControls();
                return _pnbMobilePhoneNumber.Visible;
            }

            set
            {
                EnsureChildControls();
                _pnbMobilePhoneNumber.Visible = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [require mobile phone].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [require mobile phone]; otherwise, <c>false</c>.
        /// </value>
        public bool RequireMobilePhone
        {
            get
            {
                EnsureChildControls();
                return _pnbMobilePhoneNumber.Required;
            }

            set
            {
                EnsureChildControls();
                _pnbMobilePhoneNumber.Required = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show person role].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show person role]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowPersonRole
        {
            get
            {
                EnsureChildControls();
                return _rblPersonRole.Visible;
            }

            set
            {
                EnsureChildControls();
                _rblPersonRole.Visible = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show connection status].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show connection status]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowConnectionStatus
        {
            get
            {
                EnsureChildControls();
                return _dvpPersonConnectionStatus.Visible;
            }

            set
            {
                EnsureChildControls();
                _dvpPersonConnectionStatus.Visible = value;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show marital status].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show marital status]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowMaritalStatus
        {
            get
            {
                EnsureChildControls();
                return _dvpPersonMaritalStatus.Visible;
            }

            set
            {
                EnsureChildControls();
                _dvpPersonMaritalStatus.Visible = value;
            }
        }

        /// <summary>
        /// If Required, the "Unknown" option won't be displayed
        /// </summary>
        /// <value>
        ///   <c>true</c> if [require gender]; otherwise, <c>false</c>.
        /// </value>
        public bool RequireGender
        {
            get
            {
                return ViewState[ViewStateKey.RequireGender] as bool? ?? false;
            }

            set
            {
                ViewState[ViewStateKey.RequireGender] = value;

                var listItemUnknown = _rblPersonGender.Items.OfType<ListItem>().FirstOrDefault( x => x.Value == Gender.Unknown.ConvertToInt().ToString() );

                if ( value )
                {
                    if ( listItemUnknown == null )
                    {
                        _rblPersonGender.Items.Add( new ListItem( Gender.Unknown.ConvertToString(), Gender.Unknown.ConvertToInt().ToString() ) );
                    }
                }
                else
                {
                    if ( listItemUnknown != null )
                    {
                        _rblPersonGender.Items.Remove( listItemUnknown );
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the first name.
        /// </summary>
        /// <value>
        /// The first name.
        /// </value>
        public string FirstName
        {
            get
            {
                EnsureChildControls();
                return _tbPersonFirstName.Text;
            }

            set
            {
                EnsureChildControls();
                _tbPersonFirstName.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets the last name.
        /// </summary>
        /// <value>
        /// The last name.
        /// </value>
        public string LastName
        {
            get
            {
                EnsureChildControls();
                return _tbPersonLastName.Text;
            }

            set
            {
                EnsureChildControls();
                _tbPersonLastName.Text = value;
            }
        }

        /// <summary>
        /// Gets or sets the person title value identifier.
        /// </summary>
        /// <value>
        /// The person title value identifier.
        /// </value>
        public int? PersonTitleValueId
        {
            get
            {
                EnsureChildControls();
                return _dvpPersonTitle.SelectedDefinedValueId;
            }

            set
            {
                EnsureChildControls();
                _dvpPersonTitle.SelectedDefinedValueId = value;
            }
        }

        /// <summary>
        /// Gets or sets the person suffix value identifier.
        /// </summary>
        /// <value>
        /// The person suffix value identifier.
        /// </value>
        public int? PersonSuffixValueId
        {
            get
            {
                EnsureChildControls();
                return _dvpPersonSuffix.SelectedDefinedValueId;
            }

            set
            {
                EnsureChildControls();
                _dvpPersonSuffix.SelectedDefinedValueId = value;
            }
        }

        /// <summary>
        /// Gets or sets the person marital status value identifier.
        /// </summary>
        /// <value>
        /// The person marital status value identifier.
        /// </value>
        public int? PersonMaritalStatusValueId
        {
            get
            {
                EnsureChildControls();
                return _dvpPersonMaritalStatus.SelectedDefinedValueId;
            }

            set
            {
                EnsureChildControls();
                _dvpPersonMaritalStatus.SelectedDefinedValueId = value;
            }
        }

        /// <summary>
        /// Gets or sets the person grade offset.
        /// </summary>
        /// <value>
        /// The person grade offset.
        /// </value>
        public int? PersonGradeOffset
        {
            get
            {
                EnsureChildControls();
                return _ddlGradePicker.SelectedGradeOffset;
            }

            set
            {
                EnsureChildControls();
                _ddlGradePicker.SelectedGradeOffset = value;
            }
        }

        /// <summary>
        /// Gets or sets the person group role identifier.
        /// </summary>
        /// <value>
        /// The person group role identifier.
        /// </value>
        public int PersonGroupRoleId
        {
            get
            {
                EnsureChildControls();
                return _rblPersonRole.SelectedValue.AsInteger();
            }

            set
            {
                EnsureChildControls();
                _rblPersonRole.SetValue( value );
            }
        }

        /// <summary>
        /// Gets or sets the person connection status value identifier.
        /// </summary>
        /// <value>
        /// The person connection status value identifier.
        /// </value>
        public int? PersonConnectionStatusValueId
        {
            get
            {
                EnsureChildControls();
                return _dvpPersonConnectionStatus.SelectedDefinedValueId;
            }

            set
            {
                EnsureChildControls();
                _dvpPersonConnectionStatus.SelectedDefinedValueId = value;
            }
        }

        /// <summary>
        /// Gets or sets the person gender.
        /// </summary>
        /// <value>
        /// The person gender.
        /// </value>
        public Gender PersonGender
        {
            get
            {
                EnsureChildControls();
                return _rblPersonGender.SelectedValueAsEnum<Gender>();
            }

            set
            {
                EnsureChildControls();
                _rblPersonGender.SetValue( ( int ) value );
            }
        }

        /// <summary>
        /// Gets or sets the person birth date.
        /// </summary>
        /// <value>
        /// The person birth date.
        /// </value>
        public DateTime? PersonBirthDate
        {
            get
            {
                EnsureChildControls();
                return _dpPersonBirthDate.SelectedDate;
            }

            set
            {
                EnsureChildControls();
                _dpPersonBirthDate.SelectedDate = value;
            }
        }

        /// <summary>
        /// Gets a value indicating whether [person birth date is valid].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [person birth date is valid]; otherwise, <c>false</c>.
        /// </value>
        public bool PersonBirthDateIsValid
        {
            get
            {
                EnsureChildControls();
                return _dpPersonBirthDate.IsValid;
            }
        }

        /// <summary>
        /// Gets or sets the validation group.
        /// </summary>
        /// <value>
        /// The validation group.
        /// </value>
        public string ValidationGroup { get; set; }

        /// <summary>
        /// Gets or sets the label prefix. For example "Spouse", would label things as "Spouse First Name", "Spouse Last Name", etc
        /// </summary>
        /// <value>
        /// The label prefix.
        /// </value>
        public string PersonLabelPrefix
        {
            set
            {
                EnsureChildControls();
                ViewState[ViewStateKey.PersonLabelPrefix] = value;
                _dvpPersonTitle.Label = AddLabelPrefix( "Title" );
                _tbPersonFirstName.Label = AddLabelPrefix( "First Name" );
                _tbPersonLastName.Label = AddLabelPrefix( "Last Name" );
                _dvpPersonSuffix.Label = AddLabelPrefix( "Suffix" );
                _dvpPersonConnectionStatus.Label = AddLabelPrefix( "Connection Status" );
                _rblPersonRole.Label = AddLabelPrefix( "Role" );
                _rblPersonGender.Label = AddLabelPrefix( "Gender" );
                _dpPersonBirthDate.Label = AddLabelPrefix( "Birthdate" );
                _ddlGradePicker.Label = AddLabelPrefix( "Grade" );
                _dvpPersonMaritalStatus.Label = AddLabelPrefix( "Marital Status" );
                _ebPersonEmail.Label = AddLabelPrefix( "Email" );
                _pnbMobilePhoneNumber.Label = AddLabelPrefix( "MobilePhone" );
            }

            get
            {
                return ViewState[ViewStateKey.PersonLabelPrefix] as string;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether [show in columns].
        /// </summary>
        /// <value>
        ///   <c>true</c> if [show in columns]; otherwise, <c>false</c>.
        /// </value>
        public bool ShowInColumns
        {
            set
            {
                EnsureChildControls();
                ViewState[ViewStateKey.ShowInColumns] = value;
                if ( value )
                {
                    _pnlRow.AddCssClass( "row" );
                    _pnlCol1.AddCssClass( "col-sm-4" );
                    _pnlCol2.AddCssClass( "col-sm-4" );
                    _pnlCol3.AddCssClass( "col-sm-4" );
                }
                else
                {
                    _pnlRow.RemoveCssClass( "row" );
                    _pnlCol1.RemoveCssClass( "col-sm-4" );
                    _pnlCol2.RemoveCssClass( "col-sm-4" );
                    _pnlCol3.RemoveCssClass( "col-sm-4" );
                }
            }

            get
            {
                return ViewState[ViewStateKey.ShowInColumns] as bool? ?? false;
            }
        }

        #endregion Properties

        #region Events

        /// <summary>
        /// Called by the ASP.NET page framework to notify server controls that use composition-based implementation to create any child controls they contain in preparation for posting back or rendering.
        /// </summary>
        protected override void CreateChildControls()
        {
            base.CreateChildControls();

            _pnlRow = new Panel { ID = "pnlRow", CssClass = "row" };

            this.Controls.Add( _pnlRow );

            _pnlCol1 = new Panel { ID = "pnlCol1", CssClass = "col-sm-4" };

            _pnlRow.Controls.Add( _pnlCol1 );
            _dvpPersonTitle = new DefinedValuePicker
            {
                ID = "_dvpPersonTitle",
                DefinedTypeId = DefinedTypeCache.GetId( Rock.SystemGuid.DefinedType.PERSON_TITLE.AsGuid() ),
                Label = "Title",
                CssClass = "input-width-md",
                ValidationGroup = ValidationGroup
            };

            _pnlCol1.Controls.Add( _dvpPersonTitle );

            _tbPersonFirstName = new RockTextBox
            {
                ID = "tbPersonFirstName",
                Label = "First Name",
                Required = true,
                AutoCompleteType = AutoCompleteType.None,
                ValidationGroup = ValidationGroup
            };

            _pnlCol1.Controls.Add( _tbPersonFirstName );

            _tbPersonLastName = new RockTextBox
            {
                ID = "tbPersonLastName",
                Label = "Last Name",
                Required = true,
                AutoCompleteType = AutoCompleteType.None,
                ValidationGroup = ValidationGroup
            };

            _pnlCol1.Controls.Add( _tbPersonLastName );

            _dvpPersonSuffix = new DefinedValuePicker
            {
                ID = "dvpPersonSuffix",
                DefinedTypeId = DefinedTypeCache.GetId( Rock.SystemGuid.DefinedType.PERSON_SUFFIX.AsGuid() ),
                Label = "Suffix",
                CssClass = "input-width-md",
                ValidationGroup = ValidationGroup
            };

            _pnlCol1.Controls.Add( _dvpPersonSuffix );

            _pnlCol2 = new Panel { ID = "pnlCol2", CssClass = "col-sm-4" };

            _pnlRow.Controls.Add( _pnlCol2 );

            _dvpPersonConnectionStatus = new DefinedValuePicker
            {
                ID = "dvpPersonConnectionStatus",
                DefinedTypeId = DefinedTypeCache.GetId( Rock.SystemGuid.DefinedType.PERSON_CONNECTION_STATUS.AsGuid() ),
                Label = "Connection Status",
                Required = true,
                ValidationGroup = ValidationGroup
            };

            _pnlCol2.Controls.Add( _dvpPersonConnectionStatus );

            _rblPersonRole = new RockRadioButtonList
            {
                ID = "rblPersonRole",
                Label = "Role",
                RepeatDirection = RepeatDirection.Horizontal,
                DataTextField = "Name",
                DataValueField = "Id",
                Required = true,
                ValidationGroup = ValidationGroup
            };

            _pnlCol2.Controls.Add( _rblPersonRole );

            _rblPersonGender = new RockRadioButtonList
            {
                ID = "rblPersonGender",
                Label = "Gender",
                RepeatDirection = RepeatDirection.Horizontal,
                Required = true,
                ValidationGroup = ValidationGroup
            };

            _pnlCol2.Controls.Add( _rblPersonGender );

            _pnlCol3 = new Panel { ID = "pnlCol3", CssClass = "col-sm-4" };
            _pnlRow.Controls.Add( _pnlCol3 );

            _dpPersonBirthDate = new DatePicker
            {
                ID = "dpPersonBirthDate",
                Label = "Birthdate",
                AllowFutureDateSelection = false,
                ForceParse = false,
                ValidationGroup = ValidationGroup
            };

            _pnlCol3.Controls.Add( _dpPersonBirthDate );

            _ddlGradePicker = new GradePicker
            {
                ID = "ddlGradePicker",
                Label = "Grade",
                UseAbbreviation = true,
                UseGradeOffsetAsValue = true,
                ValidationGroup = ValidationGroup
            };

            _pnlCol3.Controls.Add( _ddlGradePicker );

            _dvpPersonMaritalStatus = new DefinedValuePicker
            {
                ID = "dvpPersonMaritalStatus",
                Label = "Marital Status",
                DefinedTypeId = DefinedTypeCache.GetId( Rock.SystemGuid.DefinedType.PERSON_MARITAL_STATUS.AsGuid() ),
                ValidationGroup = ValidationGroup
            };

            _pnlCol3.Controls.Add( _dvpPersonMaritalStatus );

            _ebPersonEmail = new EmailBox
            {
                ID = "ebPersonEmail",
                Label = "Email",
                ValidationGroup = ValidationGroup
            };

            _pnlCol3.Controls.Add( _ebPersonEmail );

            _pnbMobilePhoneNumber = new PhoneNumberBox
            {
                Label = "Mobile Phone",
                ID = "pnbMobilePhoneNumber",
            };

            var groupType = GroupTypeCache.GetFamilyGroupType();
            _rblPersonRole.DataSource = groupType.Roles.OrderBy( r => r.Order ).ToList();
            _rblPersonRole.DataBind();

            _rblPersonGender.Items.Clear();
            _rblPersonGender.Items.Add( new ListItem( Gender.Male.ConvertToString(), Gender.Male.ConvertToInt().ToString() ) );
            _rblPersonGender.Items.Add( new ListItem( Gender.Female.ConvertToString(), Gender.Female.ConvertToInt().ToString() ) );
            _rblPersonGender.Items.Add( new ListItem( Gender.Unknown.ConvertToString(), Gender.Unknown.ConvertToInt().ToString() ) );
        }


        private string AddLabelPrefix( string labelText )
        {
            if ( PersonLabelPrefix.IsNullOrWhiteSpace() )
            {
                return labelText;
            }

            return $"{PersonLabelPrefix} {labelText}";
        }

        /// <summary>
        /// Updates the person fields based on what the values in the PersonBasicEditor are
        /// </summary>
        /// <param name="person">The new person.</param>
        public void UpdatePerson( Person person )
        {
            person.TitleValueId = this.PersonTitleValueId;
            person.FirstName = this.FirstName;
            person.NickName = this.FirstName;
            person.LastName = this.LastName;
            person.SuffixValueId = this.PersonSuffixValueId;
            person.Gender = this.PersonGender;
            person.MaritalStatusValueId = this.PersonMaritalStatusValueId;
            person.SetBirthDate( this.PersonBirthDate );
            person.GradeOffset = this.PersonGradeOffset;
            person.ConnectionStatusValueId = this.PersonConnectionStatusValueId;
        }

        /// <summary>
        /// Updates the PersonEditor values based on the specified person
        /// </summary>
        /// <param name="person">The person.</param>
        public void SetFromPerson( Person person )
        {
            // if a null person is specified, use whatever the defaults are for a new Person object
            person = person ?? new Person();
            this.PersonTitleValueId = person.TitleValueId;
            this.FirstName = person.FirstName;
            this.FirstName = person.NickName;
            this.LastName = person.LastName;
            this.PersonSuffixValueId = person.SuffixValueId;
            this.PersonGender = person.Gender;
            this.PersonMaritalStatusValueId = person.MaritalStatusValueId;
            this.PersonBirthDate = person.BirthDate;
            this.PersonGradeOffset = person.GradeOffset;
            this.PersonConnectionStatusValueId = person.ConnectionStatusValueId;
        }

        /// <summary>
        /// Gets the validation results.
        /// </summary>
        /// <value>
        /// The validation results.
        /// </value>
        public ValidationResult[] ValidationResults { get; private set; } = new ValidationResult[0];

        /// <summary>
        /// Returns true if the edited values are valid, otherwise returns false and populates <see cref="ValidationResults"/>
        /// </summary>
        /// <returns>
        ///   <c>true</c> if this instance is valid; otherwise, <c>false</c>.
        /// </returns>
        public bool IsValid()
        {
            var validationResults = new List<ValidationResult>();
            bool isValid = true;

            DateTime? birthdate = this.PersonBirthDate;
            if ( !this.PersonBirthDateIsValid )
            {
                validationResults.Add( new ValidationResult( "Birth date is not valid." ) );
                isValid = false;
            }

            this.ValidationResults = validationResults.ToArray();

            return isValid;
        }

        #endregion Events
    }
}
