﻿<%@ Control Language="C#" AutoEventWireup="true" CodeFile="GroupAttendanceList.ascx.cs" Inherits="RockWeb.Blocks.Groups.GroupAttendanceList" %>

<asp:UpdatePanel ID="pnlContent" runat="server">
    <ContentTemplate>

        <div class="grid grid-panel">
            <Rock:Grid ID="gOccurrences" runat="server" DisplayType="Full" AllowSorting="true" RowItemText="Occurrence" OnRowSelected="gOccurrences_Edit" >
                <Columns>
                    <Rock:DateField DataField="StartDateTime" HeaderText="Date" ItemStyle-HorizontalAlign="Left" />
                    <Rock:BoolField DataField="AttendanceEntered" HeaderText="Attendance Entered" />
                    <Rock:BoolField DataField="DidNotOccur" HeaderText="Didn't Meet" />
                    <Rock:RockBoundField DataField="NumberAttended" HeaderText="Attendance Count" ItemStyle-HorizontalAlign="Right" DataFormatString="{0:N0}" />
                    <Rock:EditField IconCssClass="fa fa-check-square-o" OnClick="gOccurrences_Edit" ToolTip="Enter Attendance" />
                </Columns>
            </Rock:Grid>
        </div>

    </ContentTemplate>
</asp:UpdatePanel>
