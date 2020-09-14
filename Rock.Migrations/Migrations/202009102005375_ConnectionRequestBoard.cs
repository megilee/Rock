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
namespace Rock.Migrations
{
    /// <summary>
    ///
    /// </summary>
    public partial class ConnectionRequestBoard : RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn( "dbo.ConnectionType", "DefaultView", c => c.Int( nullable: false ) );
            AddColumn( "dbo.ConnectionType", "RequestHeaderLava", c => c.String() );
            AddColumn( "dbo.ConnectionType", "RequestBadgeLava", c => c.String() );
            AddColumn( "dbo.ConnectionType", "Order", c => c.Int( nullable: false ) );
            AddColumn( "dbo.ConnectionOpportunity", "Order", c => c.Int( nullable: false ) );
            AddColumn( "dbo.ConnectionRequest", "Order", c => c.Int( nullable: false ) );
            AddColumn( "dbo.ConnectionStatus", "Order", c => c.Int( nullable: false ) );
            AddColumn( "dbo.ConnectionStatus", "HighlightColor", c => c.String( maxLength: 50 ) );

            CmsChangesUp();
        }

        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            CmsChangesDown();

            DropColumn( "dbo.ConnectionStatus", "HighlightColor" );
            DropColumn( "dbo.ConnectionStatus", "Order" );
            DropColumn( "dbo.ConnectionRequest", "Order" );
            DropColumn( "dbo.ConnectionOpportunity", "Order" );
            DropColumn( "dbo.ConnectionType", "Order" );
            DropColumn( "dbo.ConnectionType", "RequestBadgeLava" );
            DropColumn( "dbo.ConnectionType", "RequestHeaderLava" );
            DropColumn( "dbo.ConnectionType", "DefaultView" );
        }

        /// <summary>
        /// CMSs the changes up.
        /// </summary>
        private void CmsChangesUp()
        {
            // Add Page Connections to Site:Rock RMS
            RockMigrationHelper.AddPage( true, "48242949-944A-4651-B6CC-60194EDE08A0", "D65F783D-87A9-4CC9-8110-E83466A0EADB", "Connections", "", SystemGuid.Page.CONNECTIONS_BOARD, "" );
            RockMigrationHelper.MovePage( SystemGuid.Page.CONNECTIONS, SystemGuid.Page.CONNECTIONS_BOARD );
            // Add/Update BlockType Connection Request Board
            RockMigrationHelper.UpdateBlockType( "Connection Request Board", "Display the Connection Requests for a selected Connection Opportunity as a list or board view.", "~/Blocks//Connection/ConnectionRequestBoard.ascx", "Connection", "28DBE708-E99B-4879-A64D-656C030D25B5" );
            RockMigrationHelper.AddBlock( true, SystemGuid.Page.CONNECTIONS_BOARD.AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "11630BB9-E685-4582-91F8-620448AA34B0".AsGuid(), "Add Campaign Requests", "Main", @"", @"", 0, "06944871-258E-436E-8F57-D1BA950328C4" );
            // Add Block Connection Request Board to Page: Connections, Site: Rock RMS
            RockMigrationHelper.AddBlock( true, "4FBCEB52-8892-4035-BDEA-112A494BE81F".AsGuid(), null, "C2D29296-6A87-47A9-A753-EE4E9159C4C4".AsGuid(), "28DBE708-E99B-4879-A64D-656C030D25B5".AsGuid(), "Connection Request Board", "Main", @"", @"", 0, "7908EAD6-832B-4E38-9EDA-5FC40115DA0E" );
        }

        /// <summary>
        /// CMSs the changes down.
        /// </summary>
        private void CmsChangesDown()
        {
            // Delete BlockType Connection Request Board
            RockMigrationHelper.DeleteBlockType( "28DBE708-E99B-4879-A64D-656C030D25B5" ); // Connection Request Board  
            RockMigrationHelper.MovePage( SystemGuid.Page.CONNECTIONS, SystemGuid.Page.ENGAGEMENT );
            // Delete Page Connections from Site:Rock RMS
            RockMigrationHelper.DeletePage( Rock.SystemGuid.Page.CONNECTIONS_BOARD ); //  Page: Connections, Layout: Full Width, Site: Rock RMS  
        }
    }
}
