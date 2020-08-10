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

using System;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Constants;
using Rock.Data;
using Rock.Model;
using Rock.Security;
using Rock.Web.UI;
using Rock.Web.UI.Controls;

namespace RockWeb.Blocks.Oidc
{
    /// <summary>
    /// Block for displaying logins.  By default displays all logins, but can be configured to use person context to display logins for a specific person.
    /// </summary>
    [DisplayName( "Open Id Connect Claims" )]
    [Category( "Oidc" )]
    [Description( "Block for displaying and editing available Opend Id Connect claims." )]
    public partial class AuthClaims : RockBlock, ICustomGridColumns, IDetailBlock
    {
        public class PageParameterKey
        {
            public const string ScopeDetailId = "scopeId";
        }

        private int? AuthScopeId
        {
            get
            {
                return PageParameter( PageParameterKey.ScopeDetailId ).AsIntegerOrNull();
            }
        }

        #region Base Control Methods

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Init" /> event.
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnInit( EventArgs e )
        {
            // this event gets fired after block settings are updated. it's nice to repaint the screen if these settings would alter it
            BlockUpdated += Block_BlockUpdated;
            AddConfigurationUpdateTrigger( upnlContent );

            var authScopeId = AuthScopeId;
            if ( authScopeId == null )
            {
                DisplayErrorMessage( "No Auth Scope Id was specified." );
                return;
            }

            if ( authScopeId.Equals( 0 ) )
            {
                Visible = false;
            }

            gfSettings.ApplyFilterClick += gfSettings_ApplyFilterClick;

            gAuthClaims.DataKeyNames = new string[] { "Id" };
            gAuthClaims.Actions.AddClick += gAuthClaims_AddClick;
            gAuthClaims.GridRebind += gAuthClaims_GridRebind;

            bool canEdit = IsUserAuthorized( Authorization.EDIT );
            gAuthClaims.IsDeleteEnabled = canEdit;
            gAuthClaims.Actions.ShowAdd = canEdit;

            if ( canEdit )
            {
                dlgClaimDetails.SaveClick += DlgClaimDetails_SaveClick;
            }

            base.OnInit( e );
        }

        /// <summary>
        /// Raises the <see cref="E:System.Web.UI.Control.Load" /> event.
        /// </summary>
        /// <param name="e">The <see cref="T:System.EventArgs" /> object that contains the event data.</param>
        protected override void OnLoad( EventArgs e )
        {
            if ( !Page.IsPostBack )
            {
                BindFilter();
                BindGrid();
            }
            base.OnLoad( e );
        }

        #endregion

        #region Events
        /// <summary>
        /// Handles the AddClick event of the gRestKeyList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void gAuthClaims_AddClick( object sender, EventArgs e )
        {
            ShowDetail( 0 );
        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the gfSettings control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gfSettings_ApplyFilterClick( object sender, EventArgs e )
        {
            gfSettings.SaveUserPreference( "Name", tbName.Text );
            gfSettings.SaveUserPreference( "Public Name", tbPublicName.Text );

            if ( ddlActiveFilter.SelectedValue == "all" )
            {
                gfSettings.SaveUserPreference( "Active Status", string.Empty );
            }
            else
            {
                gfSettings.SaveUserPreference( "Active Status", ddlActiveFilter.SelectedValue );
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the Delete event of the gUserLogins control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RowEventArgs" /> instance containing the event data.</param>
        protected void gAuthClaims_Delete( object sender, RowEventArgs e )
        {
            bool canEdit = IsUserAuthorized( Authorization.EDIT );

            if ( canEdit )
            {
                using ( var rockContext = new RockContext() )
                {
                    var authClaimService = new AuthClaimService( rockContext );
                    var authClaim = authClaimService.Get( e.RowKeyId );
                    if ( authClaim != null )
                    {
                        authClaimService.Delete( authClaim );
                        rockContext.SaveChanges();
                    }
                }
            }

            BindGrid();
        }

        /// <summary>
        /// Handles the GridRebind event of the gUserLogins control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs" /> instance containing the event data.</param>
        private void gAuthClaims_GridRebind( object sender, EventArgs e )
        {
            BindGrid();
        }

        /// <summary>
        /// Handles the BlockUpdated event of the UserLogins control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void Block_BlockUpdated( object sender, EventArgs e )
        {
        }

        protected void gAuthClaims_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            var authClaim = e.Row.DataItem as AuthClaim;
            if ( authClaim == null )
            {
                return;
            }

            var deleteFieldColumn = gAuthClaims.ColumnsOfType<DeleteField>().FirstOrDefault();
            if ( deleteFieldColumn == null || !deleteFieldColumn.Visible )
            {
                return;
            }

            var deleteFieldColumnIndex = gAuthClaims.GetColumnIndex( deleteFieldColumn );
            var deleteButton = e.Row.Cells[deleteFieldColumnIndex].ControlsOfTypeRecursive<LinkButton>().FirstOrDefault();
            if ( deleteButton == null )
            {
                return;
            }

            deleteButton.Visible = !authClaim.IsSystem;
        }
        #endregion

        #region Methods

        /// <summary>
        /// Binds the filter.
        /// </summary>
        private void BindFilter()
        {
            tbName.Text = gfSettings.GetUserPreference( "Name" );
            tbPublicName.Text = gfSettings.GetUserPreference( "PublicName" );

            // Set the Active Status
            var itemActiveStatus = ddlActiveFilter.Items.FindByValue( gfSettings.GetUserPreference( "Active Status" ) );
            if ( itemActiveStatus != null )
            {
                itemActiveStatus.Selected = true;
            }
        }

        /// <summary>
        /// Binds the grid.
        /// </summary>
        private void BindGrid()
        {
            var authScopeId = AuthScopeId;
            if ( authScopeId == null )
            {
                return;
            }

            using ( var rockContext = new RockContext() )
            {
                var authClaimService = new AuthClaimService( rockContext );
                var authClaimQuery = authClaimService
                    .Queryable()
                    .AsNoTracking()
                    .Where( ac => ac.ScopeId == authScopeId );

                if ( tbName.Text.IsNotNullOrWhiteSpace() )
                {
                    authClaimQuery = authClaimQuery.Where( s => s.Name.Contains( tbName.Text ) );
                }

                if ( tbPublicName.Text.IsNotNullOrWhiteSpace() )
                {
                    authClaimQuery = authClaimQuery.Where( s => s.PublicName.Contains( tbPublicName.Text ) );
                }

                if ( ddlActiveFilter.SelectedIndex > -1 )
                {
                    switch ( ddlActiveFilter.SelectedValue )
                    {
                        case "active":
                            authClaimQuery = authClaimQuery.Where( s => s.IsActive );
                            break;
                        case "inactive":
                            authClaimQuery = authClaimQuery.Where( s => !s.IsActive );
                            break;
                    }
                }

                // Sort
                var sortProperty = gAuthClaims.SortProperty;
                if ( sortProperty == null )
                {
                    sortProperty = new SortProperty( new GridViewSortEventArgs( "Name", SortDirection.Ascending ) );
                }
                authClaimQuery = authClaimQuery.Sort( sortProperty );

                gAuthClaims.SetLinqDataSource( authClaimQuery );
                gAuthClaims.DataBind();
            }
        }

        private void DisplayErrorMessage( string message )
        {
            nbWarningMessage.Text = message;
            nbWarningMessage.NotificationBoxType = Rock.Web.UI.Controls.NotificationBoxType.Danger;
            nbWarningMessage.Visible = true;
            gAuthClaims.Visible = false;
            gfSettings.Visible = false;
        }
        #endregion

        protected void gAuthClaims_RowSelected( object sender, RowEventArgs e )
        {
            ShowDetail( e.RowKeyId );
        }

        public void ShowDetail( int authClaimId )
        {
            AuthClaim authClaim = null;
            var isNew = authClaimId.Equals( 0 );

            if ( !isNew )
            {
                dlgClaimDetails.Title = ActionTitle.Edit( "Claim" ).FormatAsHtmlTitle();
                using ( var rockContext = new RockContext() )
                {
                    authClaim = new AuthClaimService( rockContext ).Get( authClaimId );
                }
            }
            else
            {
                dlgClaimDetails.Title = ActionTitle.Add( "Claim" ).FormatAsHtmlTitle();
            }

            if ( authClaim == null )
            {
                if ( !isNew )
                {
                    DisplayErrorMessage( "The Auth Claim with the specified Id was found." );
                    return;
                }

                authClaim = new AuthClaim { Id = 0 };
            }

            hfAuthClaimId.Value = authClaim.Id.ToString();

            tbClaimName.Text = authClaim.Name;
            tbClaimPublicName.Text = authClaim.PublicName;
            tbClaimValue.Text = authClaim.Value;
            cbClaimActive.Checked = authClaim.IsActive;
            cbClaimIsSystem.Checked = authClaim.IsSystem;

            if ( authClaim.IsSystem )
            {
                tbClaimName.Enabled = false;
                tbClaimValue.Enabled = false;
            }

            dlgClaimDetails.Show();
        }

        private void DlgClaimDetails_SaveClick( object sender, EventArgs e )
        {
            var claimId = hfAuthClaimId.Value.AsIntegerOrNull();
            if ( claimId == null )
            {
                DisplayErrorMessage( "No Auth Claim Id was specified." );
                return;
            }

            SaveAuthClaim( claimId.Value );

        }

        private void SaveAuthClaim( int authClaimId )
        {
            var isNew = authClaimId.Equals( 0 );
            var authScopeId = AuthScopeId;
            if ( authScopeId == null )
            {
                DisplayErrorMessage( "The auth scope id is required to create a auth claim." );
                return;
            }

            var authClaim = new AuthClaim();

            var editAllowed = authClaim.IsAuthorized( Authorization.EDIT, CurrentPerson );
            if ( !editAllowed )
            {
                DisplayErrorMessage( "The current user is not authorized to make changes." );
                return;
            }

            using ( var rockContext = new RockContext() )
            {
                var authClaimService = new AuthClaimService( rockContext );
                if ( isNew )
                {
                    authClaim.ScopeId = authScopeId.Value;
                    authClaimService.Add( authClaim );
                }
                else
                {
                    authClaim = authClaimService.Get( authClaimId );
                }

                if ( authClaim == null )
                {
                    DisplayErrorMessage( "The Auth Claim with the specified Id was found." );
                    return;
                }

                if ( !authClaim.IsSystem )
                {
                    authClaim.Name = tbClaimName.Text;
                    authClaim.Value = tbClaimValue.Text;
                }

                authClaim.PublicName = tbClaimPublicName.Text;
                authClaim.IsActive = cbClaimActive.Checked;

                rockContext.SaveChanges();
            }
            dlgClaimDetails.Hide();
            BindGrid();
        }
    }
}