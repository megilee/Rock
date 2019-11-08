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
namespace Rock.Plugin.HotFixes
{
    /// <summary>
    /// Plugin Migration. The migration number jumps to 83 because 75-82 were moved to EF migrations and deleted.
    /// </summary>
    [MigrationNumber( 92, "1.9.0" )]
    public class DiscProfileAttributeFix : Migration
    {
        /// <summary>
        /// The commands to run to migrate plugin to the specific version
        /// </summary>
        public override void Up()
        {
            //FixDiscProfilePersonAttribute();
        }

        /// <summary>
        /// The commands to undo a migration from a specific version
        /// </summary>
        public override void Down()
        {
            // Not yet used by hotfix migrations.
        }

        /// <summary>
        /// ED: Add the Defined Type "DISC Results" to the "DISC Profile" person attribute.
        /// </summary>
        private void FixDiscProfilePersonAttribute()
        {
            Sql( $@"
                DECLARE @DiscProfileAttributeId INT = (SELECT [Id] FROM Attribute WHERE [Guid] = '{Rock.SystemGuid.Attribute.PERSON_DISC_PROFILE}')
                DECLARE @DiscResultsDefinedTypeId INT = (SELECT [Id] FROM [DefinedType] WHERE [Guid] = '{Rock.SystemGuid.DefinedType.DISC_RESULTS_TYPE}')
                DECLARE @DefinedTypeAttributeQualifierId INT = (SELECT [Id] FROM [AttributeQualifier] WHERE [AttributeId] = @DiscProfileAttributeId AND [Key] = 'definedtype')

                IF (@DefinedTypeAttributeQualifierId IS NULL)
                BEGIN
	                -- We don't have he qualifier so insert it.
	                INSERT INTO AttributeQualifier([IsSystem], [AttributeId], [Key], [Value], [Guid])
	                VALUES(1, @DiscProfileAttributeId, 'definedtype', @DiscResultsDefinedTypeId, NEWID())
                END
                ELSE
                BEGIN
	                -- We have a qualifier so make sure the value is correct
	                UPDATE [AttributeQualifier]
	                SET [Value] = @DiscResultsDefinedTypeId
	                WHERE [Id] = @DefinedTypeAttributeQualifierId
                END" );
        }
    }
}