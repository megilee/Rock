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
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Linq.Dynamic;
using System.Web.UI;
using System.Web.UI.WebControls;
using Rock;
using Rock.Attribute;
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
    [DisplayName( "Open Id Connect Clients" )]
    [Category( "Oidc" )]
    [Description( "Block for displaying and editing available Opend Id Connect clients." )]
    [LinkedPage( "Detail Page", Key = AttributeKey.DetailPage )]
    public partial class AuthClientList : RockBlock, ICustomGridColumns
    {
        public class AttributeKey
        {
            public const string DetailPage = "DetailPage";
        }

        public class PageParameterKey
        {
            public const string AuthClientId = "AuthClientId";
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

            gfSettings.ApplyFilterClick += gfSettings_ApplyFilterClick;

            gAuthClients.DataKeyNames = new string[] { "Id" };
            gAuthClients.Actions.AddClick += gAuthClients_AddClick;
            gAuthClients.GridRebind += gAuthClients_GridRebind;

            bool canEdit = IsUserAuthorized( Authorization.EDIT );
            gAuthClients.IsDeleteEnabled = canEdit;
            gAuthClients.Actions.ShowAdd = canEdit;

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
        private void gAuthClients_AddClick( object sender, EventArgs e )
        {
            NavigateToAuthScopeDetailPage( 0 );
        }

        private void NavigateToAuthScopeDetailPage( int authClaimId )
        {
            var parms = new Dictionary<string, string>();
            parms.Add( PageParameterKey.AuthClientId, authClaimId.ToString() );
            NavigateToLinkedPage( AttributeKey.DetailPage, parms );
        }

        /// <summary>
        /// Handles the ApplyFilterClick event of the gfSettings control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected void gfSettings_ApplyFilterClick( object sender, EventArgs e )
        {
            gfSettings.SaveUserPreference( "Name", tbName.Text );

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
        protected void gAuthClients_Delete( object sender, RowEventArgs e )
        {
            bool canEdit = IsUserAuthorized( Authorization.EDIT );

            if ( canEdit )
            {
                using ( var rockContext = new RockContext() )
                {
                    var authClientService = new AuthClientService( rockContext );
                    var authScope = authClientService.Get( e.RowKeyId );
                    if ( authScope != null )
                    {
                        authClientService.Delete( authScope );
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
        private void gAuthClients_GridRebind( object sender, EventArgs e )
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

        protected void gAuthClients_RowDataBound( object sender, GridViewRowEventArgs e )
        {
            var authClaim = e.Row.DataItem as AuthClaim;
            if ( authClaim == null )
            {
                return;
            }

            var deleteFieldColumn = gAuthClients.ColumnsOfType<DeleteField>().FirstOrDefault();
            if ( deleteFieldColumn == null || !deleteFieldColumn.Visible )
            {
                return;
            }

            var deleteFieldColumnIndex = gAuthClients.GetColumnIndex( deleteFieldColumn );
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
            var rockContext = new RockContext();
            var authClientService = new AuthClientService( rockContext );
            var authClientQuery = authClientService.Queryable().AsNoTracking();

            if ( tbName.Text.IsNotNullOrWhiteSpace() )
            {
                authClientQuery = authClientQuery.Where( s => s.Name.Contains( tbName.Text ) );
            }

            if ( ddlActiveFilter.SelectedIndex > -1 )
            {
                switch ( ddlActiveFilter.SelectedValue )
                {
                    case "active":
                        authClientQuery = authClientQuery.Where( s => s.IsActive );
                        break;
                    case "inactive":
                        authClientQuery = authClientQuery.Where( s => !s.IsActive );
                        break;
                }
            }

            // Sort
            var sortProperty = gAuthClients.SortProperty;
            if ( sortProperty == null )
            {
                sortProperty = new SortProperty( new GridViewSortEventArgs( "Name", SortDirection.Ascending ) );
            }
            authClientQuery = authClientQuery.Sort( sortProperty );

            gAuthClients.SetLinqDataSource( authClientQuery );
            gAuthClients.DataBind();
        }

        #endregion

        protected void gAuthClients_RowSelected( object sender, RowEventArgs e )
        {
            NavigateToAuthScopeDetailPage( e.RowKeyId );
        }
    }
}