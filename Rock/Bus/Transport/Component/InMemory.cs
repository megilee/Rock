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
using System.ComponentModel;
using System.ComponentModel.Composition;
using MassTransit;

namespace Rock.Bus.Transport.Component
{
    /// <summary>
    /// In-Memory Transport
    /// </summary>
    /// <seealso cref="BusTransportComponent" />
 
    [Description( "In-Memory Message Bus Transport" )]
    [Export( typeof( BusTransportComponent ) )]
    [ExportMetadata( "ComponentName", "In-Memory" )]

    public class InMemory : BusTransportComponent
    {
        /// <summary>
        /// Create the transport
        /// </summary>
        public override IBusControl Create<T>()
        {
            var consumerType = typeof( T );

            return MassTransit.Bus.Factory.CreateUsingInMemory( configurator =>
            {
                configurator.ReceiveEndpoint( "in-memory-queue", ep =>
                {
                    ep.Consumer( consumerType, type => Activator.CreateInstance( consumerType ) );
                } );
            } );
        }
    }
}
