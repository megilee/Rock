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
    using System;
    using System.Data.Entity.Migrations;
    
    /// <summary>
    ///
    /// </summary>
    public partial class ConnectionRequestBoard : Rock.Migrations.RockMigration
    {
        /// <summary>
        /// Operations to be performed during the upgrade process.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.ConnectionType", "DefaultView", c => c.Int(nullable: false));
            AddColumn("dbo.ConnectionType", "RequestHeaderLava", c => c.String());
            AddColumn("dbo.ConnectionType", "RequestBadgeLava", c => c.String());
            AddColumn("dbo.ConnectionType", "Order", c => c.Int(nullable: false));
            AddColumn("dbo.ConnectionOpportunity", "Order", c => c.Int(nullable: false));
            AddColumn("dbo.ConnectionRequest", "Order", c => c.Int(nullable: false));
            AddColumn("dbo.ConnectionStatus", "Order", c => c.Int(nullable: false));
            AddColumn("dbo.ConnectionStatus", "HighlightColor", c => c.String(maxLength: 50));
        }
        
        /// <summary>
        /// Operations to be performed during the downgrade process.
        /// </summary>
        public override void Down()
        {
            DropColumn("dbo.ConnectionStatus", "HighlightColor");
            DropColumn("dbo.ConnectionStatus", "Order");
            DropColumn("dbo.ConnectionRequest", "Order");
            DropColumn("dbo.ConnectionOpportunity", "Order");
            DropColumn("dbo.ConnectionType", "Order");
            DropColumn("dbo.ConnectionType", "RequestBadgeLava");
            DropColumn("dbo.ConnectionType", "RequestHeaderLava");
            DropColumn("dbo.ConnectionType", "DefaultView");
        }
    }
}
