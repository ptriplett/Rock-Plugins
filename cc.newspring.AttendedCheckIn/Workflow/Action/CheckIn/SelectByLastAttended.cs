﻿// <copyright>
// Copyright 2013 by the Spark Development Network
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
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
using System.ComponentModel.Composition;
using System.Linq;

using Rock.Attribute;
using Rock.CheckIn;
using Rock.Data;
using Rock.Workflow;
using Rock.Workflow.Action.CheckIn;

namespace cc.newspring.AttendedCheckIn.Workflow.Action.CheckIn
{
    /// <summary>
    /// Selects the available grouptype, group, location and schedule if it matches their previous attendance
    /// </summary>
    [Description( "Selects the grouptype, group, location and schedule for each person based on what they last checked into." )]
    [Export( typeof( ActionComponent ) )]
    [ExportMetadata( "ComponentName", "Select By Last Attended" )]
    public class SelectByLastAttended : CheckInActionComponent
    {
        /// <summary>
        /// Executes the specified workflow.
        /// </summary>
        /// <param name="rockContext">The rock context.</param>
        /// <param name="action">The workflow action.</param>
        /// <param name="entity">The entity.</param>
        /// <param name="errorMessages">The error messages.</param>
        /// <returns></returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public override bool Execute( RockContext rockContext, Rock.Model.WorkflowAction action, Object entity, out List<string> errorMessages )
        {
            var checkInState = GetCheckInState( entity, out errorMessages );
            if ( checkInState != null )
            {
                var family = checkInState.CheckIn.Families.Where( f => f.Selected ).FirstOrDefault();
                if ( family != null )
                {
                    foreach ( var person in family.People.Where( f => f.Selected ) )
                    {
                        if ( person.LastCheckIn != null )
                        {
                            var groupType = person.GroupTypes.FirstOrDefault( gt => gt.Selected || gt.LastCheckIn == person.LastCheckIn );
                            if ( groupType != null )
                            {
                                groupType.PreSelected = true;
                                groupType.Selected = true;

                                var group = groupType.Groups.FirstOrDefault( g => g.Selected || g.LastCheckIn == person.LastCheckIn );
                                if ( group != null )
                                {
                                    group.PreSelected = true;
                                    group.Selected = true;

                                    var location = group.Locations.FirstOrDefault( l => l.Selected || l.LastCheckIn == person.LastCheckIn );
                                    if ( location != null )
                                    {
                                        location.PreSelected = true;
                                        location.Selected = true;

                                        var schedule = location.Schedules.FirstOrDefault( s => s.Selected || s.LastCheckIn == person.LastCheckIn );
                                        if ( schedule != null )
                                        {
                                            schedule.PreSelected = true;
                                            schedule.Selected = true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                return true;
            }

            return false;
        }
    }
}