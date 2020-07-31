﻿<%@ Control Language="C#" AutoEventWireup="true" CodeFile="Person.ascx.cs" Inherits="RockWeb.Blocks.CheckIn.Manager.Person" %>

<script type="text/javascript">
    Sys.Application.add_load(function () {
        $(".photo a").fluidbox();

        $('.js-cancel-checkin').on('click', function (event) {
            event.stopImmediatePropagation();
            var personName = $('h1.js-checkin-person-name').first().text();
            return Rock.dialogs.confirmDelete(event, 'Checkin for ' + personName);
        });
    });
</script>

<asp:UpdatePanel ID="upnlContent" runat="server">
    <ContentTemplate>

        <div class="row">
            <div class="col-sm-3 col-xs-12">

                <!-- Photo, Name & Campus, Phone & Email, Attributes -->
                <div class="panel panel-block">
                    <asp:Literal ID="lPhoto" runat="server" />

                    <div class="d-flex flex-column align-items-center p-2 pb-3">
                        <h1 class="title name js-checkin-person-name mt-0 text-center"><asp:Literal ID="lName" runat="server"></asp:Literal></h1>
                        <Rock:HighlightLabel ID="hlCampus" runat="server" LabelType="Campus" />
                    </div>

                    <asp:Panel ID="pnlContact" runat="server" CssClass="border-top border-gray-400 p-2">
                        <div class="pnlSms">
                            <!-- Shows the results of the message send -->
                            <Rock:NotificationBox ID="nbResult" runat="server" Visible="false" Dismissable="true" />

                            <!-- During entry -->
                            <textarea runat="server" rows="3" cols="30" id="tbSmsMessage" visible="false" placeholder="Your SMS message here..." class="form-control js-sms-message mb-2"></textarea>
                            <asp:LinkButton runat="server" Visible="false" ID="btnSmsSend" CssClass="btn btn-xs btn-primary js-btn-send mb-3" OnClick="btnSend_Click">Send</asp:LinkButton>
                            <asp:LinkButton runat="server" Visible="false" ID="btnSmsCancel" CssClass="btn btn-xs btn-link mb-3" OnClick="btnSmsCancel_Click">Cancel</asp:LinkButton>
                        </div>

                        <asp:Repeater ID="rptrPhones" runat="server" OnItemDataBound="rptrPhones_ItemDataBound">
                            <ItemTemplate>
                                <div class="mb-2">
                                    <div>
                                        <%# Eval("NumberFormatted") %>
                                        <a class="text-color" href="tel:<%# Eval("Number") %>"><i class="fa fa-phone-square"></i></a>
                                        <asp:LinkButton ID="btnSms" runat="server" Visible="false" OnClick="btnSms_Click" Text="<i class='fa fa-sms'></i>" />
                                    </div>
                                    <span class="d-block small text-muted">
                                        <%# Eval("NumberTypeValue.Value") %>
                                    </span>
                                </div>
                            </ItemTemplate>
                        </asp:Repeater>

                        <asp:Literal ID="lEmail" runat="server" />
                    </asp:Panel>

                    <div ID="pnlAdultFields" runat="server" class="adult-attributes border-top border-gray-400 p-2">
                        <Rock:AttributeValuesContainer ID="avcAdultAttributes" runat="server" ShowCategoryLabel="false" />
                    </div>
                    <div ID="pnlChildFields" runat="server" class="child-attributes border-top border-gray-400 p-2">
                        <Rock:AttributeValuesContainer ID="avcChildAttributes" runat="server" ShowCategoryLabel="false" />
                    </div>

                </div>

                <!-- Family -->
                <asp:Panel ID="pnlFamily" runat="server" CssClass="panel panel-block flex-row flex-wrap justify-content-around p-2">
                    <asp:Repeater ID="rptrFamily" runat="server" OnItemDataBound="rptrFamily_ItemDataBound">
                        <ItemTemplate>
                            <a class="text-center" href='<%# Eval("Url") %>'>
                                <div class="border border-gray-400 rounded">
                                    <asp:Literal ID="lFamilyPhoto" runat="server" />
                                </div>
                                <%# Eval("NickName") %>
                            </a>
                        </ItemTemplate>
                    </asp:Repeater>
                </asp:Panel>

                <!-- Relationships -->
                <asp:Panel ID="pnlRelationships" runat="server" CssClass="panel panel-block flex-row flex-wrap justify-content-around p-2">
                    <asp:Repeater ID="rptrRelationships" runat="server" OnItemDataBound="rptrRelationships_ItemDataBound">
                        <ItemTemplate>
                            <a class="text-center" href='<%# Eval("Url") %>'>
                                <div class="border border-gray-400 rounded">
                                    <asp:Literal ID="lRelationshipPhoto" runat="server" />
                                </div>
                                <%# Eval("NickName") %>
                            </a>
                        </ItemTemplate>
                    </asp:Repeater>
                </asp:Panel>

            </div>
            <div class="col-sm-9 col-xs-12">

                <!-- Gender, Age & Reprint Labels -->
                <div class="panel panel-block">
                    <div class="row d-sm-flex align-items-sm-center">

                        <div class="col-sm-6 col-xs-12 d-flex justify-content-center justify-content-sm-start align-items-center">
                            <div class="p-2 text-center">
                                <asp:Literal ID="lGender" runat="server" />
                            </div>
                            <div class="p-2 text-center">
                                <asp:Literal ID="lAge" runat="server" />
                            </div>
                            <div class="p-2 text-center">
                                <asp:Literal ID="lGrade" runat="server" />
                            </div>
                        </div>

                        <div class="col-sm-6 col-xs-12 d-flex flex-column justify-content-center align-items-center p-2">
                            <Rock:NotificationBox ID="nbReprintMessage" runat="server" Visible="false"></Rock:NotificationBox>
                            <Rock:ModalAlert ID="maNoLabelsFound" runat="server"></Rock:ModalAlert>
                            <asp:HiddenField ID="hfCurrentAttendanceIds" runat="server" />
                            <asp:HiddenField ID="hfPersonId" runat="server" />
                            <asp:LinkButton ID="btnReprintLabels" runat="server" OnClick="btnReprintLabels_Click" CssClass="btn btn-default">Reprint Labels</asp:LinkButton>
                            <Rock:ModalDialog ID="mdReprintLabels" runat="server" ValidationGroup="vgReprintLabels" Title="Label Reprints" OnSaveClick="mdReprintLabels_PrintClick" SaveButtonText="Print" Visible="false">
                                <Content>
                                    <Rock:NotificationBox ID="nbReprintLabelMessages" runat="server" NotificationBoxType="Validation"></Rock:NotificationBox>
                                    <Rock:RockCheckBoxList ID="cblLabels" runat="server" Label="Labels" DataTextField="Name" DataValueField="FileGuid"></Rock:RockCheckBoxList>
                                    <Rock:RockDropDownList ID="ddlPrinter" runat="server" Label="Printers" DataTextField="Name" DataValueField="IPAddress"></Rock:RockDropDownList>
                                </Content>
                            </Rock:ModalDialog>
                        </div>

                    </div>
                </div>

                <!-- Check-in History -->
                <asp:Panel ID="pnlCheckinHistory" runat="server" CssClass="panel panel-block">
                    <Rock:Grid ID="gHistory" runat="server" DisplayType="Light" UseFullStylesForLightGrid="true" AllowPaging="false" CssClass="table-condensed">
                        <Columns>
                            <Rock:RockTemplateField HeaderText="When">
                                <ItemTemplate>
                                    <span><%# ((DateTime)Eval("Date")).ToShortDateString() %></span>
                                    <span class="d-block small text-muted"><%# Eval("Schedule") %></span>
                                    <asp:Literal ID="lWhoCheckedIn" runat="server"></asp:Literal>
                                </ItemTemplate>
                            </Rock:RockTemplateField>
                            <Rock:RockTemplateField HeaderText="Location">
                                <ItemTemplate>
                                    <span><%# Eval("Location") %></span>
                                    <span class="d-block small text-muted"><%# Eval("Group") %></span>
                                </ItemTemplate>
                            </Rock:RockTemplateField>
                            <Rock:RockTemplateField HeaderText="Code" ItemStyle-CssClass="align-middle">
                                <ItemTemplate>
                                    <%# Eval("Code") %> <asp:Literal ID="lActive" runat="server"></asp:Literal><br />
                                </ItemTemplate>
                            </Rock:RockTemplateField>
                            <Rock:DeleteField OnClick="gHistory_Delete" ButtonCssClass="js-cancel-checkin btn btn-xs btn-danger" Tooltip="Delete This Checkin" />
                        </Columns>
                    </Rock:Grid>
                </asp:Panel>

            </div>
        </div>

        <%-- Ensures that a blank SMS cannot be sent --%>
        <script type="text/javascript">
            function setSmsSendDisabled(boolean) {
                $('.js-btn-send').attr('disabled', boolean);
            }

            Sys.Application.add_load(function () {
                setSmsSendDisabled(true);
                $('.js-sms-message').on('input', function (e) {
                    var tbValue = $(this).val();
                    setSmsSendDisabled(!tbValue.trim());
                });

                $('.js-btn-sms').on('click', function (e) {
                    setSmsSendDisabled(true);
                });
            });
        </script>

    </ContentTemplate>
</asp:UpdatePanel>
