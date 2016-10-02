using System;
using System.ComponentModel;
using System.Linq;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using DevExpress.Charts.Model;
using DevExpress.Web.Internal;
using DevExpress.XtraCharts;
using DevExpress.XtraCharts.Web;
using DevExpress.XtraCharts.Web.Designer;
using Microsoft.SharePoint;

namespace MyNetShopProject.DeliveriesFormWebPart
{
    [ToolboxItemAttribute(false)]
    public class DeliveriesFormWebPart : WebPart
    {
        private HtmlTable _table;
        private DropDownList _orderDropDownList; 
        private DropDownList _couriersDropDownList;
        private TextBox _addressTB;
        private WebChartControl _chart;
        protected override void CreateChildControls()
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine(@"<script language='javascript'>");
            sb.AppendLine(@"var link = document.createElement('link');");
            sb.AppendLine(@"link.href = '_layouts/15/MyNetShopProject/Scripts/DropDown.css';");
            sb.AppendLine(@"link.rel = 'stylesheet';");
            sb.AppendLine(@"document.getElementsByTagName('head')[0].appendChild(link);");
            sb.AppendLine(@"</script>");
            Page.ClientScript.RegisterStartupScript(GetType(), "registerCss", sb.ToString());

            var mainTable = new HtmlTable {ID = "formatingTable", ClientIDMode = ClientIDMode.Static};
            var mainRow = new HtmlTableRow();
            var mainCell = new HtmlTableCell();

            _table = new HtmlTable { ID = "mainDeliveryTable", ClientIDMode = ClientIDMode.Static };
            _orderDropDownList = new DropDownList();
            _couriersDropDownList = new DropDownList();
            _addressTB = new TextBox();

            var row = new HtmlTableRow();
            var cell = new HtmlTableCell();
            var mainLiteral = new Literal { Text = "Заполните карточку доставки:" };
            cell.ColSpan = 2;
            cell.Align = "center";
            cell.Controls.Add(mainLiteral);
            row.Cells.Add(cell);
            _table.Rows.Add(row);

            using (var web = SPContext.Current.Site.OpenWeb("/"))
            {
                _orderDropDownList.AutoPostBack = true;
                _orderDropDownList.SelectedIndexChanged += _orderDropDownList_SelectedIndexChanged;
                _orderDropDownList.Items.Add(new ListItem("<--Выберите заказ-->"));
                var ordersList = web.GetList("lists/orders");
                foreach (
                    var item in
                        ordersList.Items.Cast<SPListItem>()
                            .Where(
                                item =>
                                    new SPFieldLookupValue(item["orderStatus"].ToString()).LookupValue ==
                                    "Оформлен Клиентом"))
                    _orderDropDownList.Items.Add(new ListItem(item["Title"].ToString(), item["Title"].ToString()));
                if (_orderDropDownList.Items.Count == 1)
                {
                    Controls.Add(new LiteralControl("Все заказы уже распределены по курьерам!"));
                    return;
                }
                else
                {
                    row = new HtmlTableRow();
                    cell = new HtmlTableCell();
                    cell.Controls.Add(new Literal {Text = "Выберите UID заказа:     "});
                    row.Cells.Add(cell);
                    cell = new HtmlTableCell();
                    cell.Controls.Add(_orderDropDownList);
                    row.Cells.Add(cell);
                    _table.Rows.Add(row);

                    row = new HtmlTableRow();
                    cell = new HtmlTableCell();
                    cell.Controls.Add(new Literal { Text = "Выберите курьера на доставку:     " });
                    row.Cells.Add(cell);

                    cell = new HtmlTableCell();
                    _couriersDropDownList = new DropDownList();
                    _couriersDropDownList.Items.Add(new ListItem("<--Выберите курьера-->"));

                    cell.Controls.Add(_couriersDropDownList);
                    row.Cells.Add(cell);
                    _table.Rows.Add(row);

                    row = new HtmlTableRow();
                    cell = new HtmlTableCell();
                    cell.Controls.Add(new Literal { Text = "Введите адрес для доставки:     " });
                    row.Cells.Add(cell);

                    cell = new HtmlTableCell();
                    _addressTB = new TextBox();
                    _addressTB.Attributes.Add("style", "border-radius: 5px;");
                    cell.Controls.Add(_addressTB);
                    row.Cells.Add(cell);
                    _table.Rows.Add(row);

                    row = new HtmlTableRow();
                    cell = new HtmlTableCell();
                    var button = new Button
                    {
                        Text = "Сохранить настройки доставки"
                    };
                    button.Click += button_Click;
                    button.Attributes.Add("style", "border-radius: 15px;");
                    cell.Controls.Add(button);
                    cell.ColSpan = 2;
                    row.Cells.Add(cell);
                    _table.Rows.Add(row);
                    mainCell.Controls.Add(_table);
                }
                mainRow.Cells.Add(mainCell);

                

                mainCell = new HtmlTableCell();
                _chart = new WebChartControl {Visible = false};
                mainCell.Controls.Add(_chart);
                mainRow.Cells.Add(mainCell);
                mainTable.Rows.Add(mainRow);
                Controls.Add(mainTable);
            }
        }

        void _orderDropDownList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_orderDropDownList.SelectedIndex < 1)
            {
                _orderDropDownList.SelectedIndex = 0;
                return;
            }

            var clientCity = string.Empty;
            var selectedOrderUID = _orderDropDownList.SelectedItem.ToString();
            var selectedOrderClient = string.Empty;

            using (var web = SPContext.Current.Site.OpenWeb("/"))
            {
                var orderList = web.GetList("lists/Orders");
                var selectedOrder =
                    orderList.Items.Cast<SPListItem>().FirstOrDefault(order => order.Title == selectedOrderUID);
                var clientList = web.GetList("lists/Clients");

                var selectedClient = clientList.GetItemById(new SPFieldLookupValue(selectedOrder["orderClient0"].ToString()).LookupId);
                clientCity = new SPFieldLookupValue(selectedClient["ClientCity"].ToString()).LookupValue;
                var courierList = web.GetList("lists/Couriers");
                foreach (
                    var item in
                        courierList.Items.Cast<SPListItem>()
                            .Where(
                                item => new SPFieldLookupValue(item["CourierCity"].ToString()).LookupValue == clientCity)
                    )
                    _couriersDropDownList.Items.Add(new ListItem(item["Title"].ToString(), item.ID.ToString()));

                var deliveriesList = web.GetList("lists/Deliveries");
                foreach (ListItem item in _couriersDropDownList.Items)
                {
                    if (string.IsNullOrEmpty(item.Value))
                        continue;

                    var activeOrders = orderList.Items.Cast<SPListItem>().Select(order => new SPFieldLookupValue(order["orderStatus0"].ToString()).LookupId != 4 || new SPFieldLookupValue(order["orderStatus0"].ToString()).LookupId != 5 || new SPFieldLookupValue(order["orderStatus0"].ToString()).LookupId != 6);
                }
            }

        }

        void button_Click(object sender, EventArgs e)
        {
            var selectedOrderUID = string.Empty;
            var selectedCourierFIO = string.Empty;
            var address = string.Empty;

            if (_orderDropDownList.SelectedItem != null)
                selectedOrderUID = _orderDropDownList.SelectedItem.ToString();
            else
                return;
            if (_couriersDropDownList.SelectedItem != null)
                selectedCourierFIO = _couriersDropDownList.SelectedItem.ToString();
            else
                return;
            if (!string.IsNullOrEmpty(_addressTB.Text))
                address = _addressTB.Text;
            else
                return;

            using (var web = SPContext.Current.Site.OpenWeb("/"))
            {
                var orderList = web.GetList("lists/Orders");
                var courierList = web.GetList("lists/Couriers");
                var deliveriesList = web.GetList("lists/Deliveries");

                var selectedOrder = orderList
                    .Items
                    .Cast<SPListItem>()
                    .FirstOrDefault(item => string.Equals(item.Title, selectedOrderUID));
                var selectedCourier = courierList
                    .Items
                    .Cast<SPListItem>()
                    .FirstOrDefault(item => string.Equals(item.Title, selectedCourierFIO));

                var newDelivery = deliveriesList.Items.Add();
                newDelivery["Title"] = Guid.NewGuid().ToString();
                newDelivery["DeliveryCourier"] = selectedCourier.ID;
                newDelivery["DeliveryOrder"] = selectedOrder.ID;
                newDelivery["DeliveryAddress"] = address;
                newDelivery.Update();

                selectedOrder["orderStatus0"] = 4;
                selectedOrder.Update();
                Controls.Clear();
                Controls.Add(new LiteralControl("Доставка успешно сформирована! <br>Обновите страницу для задания остальных доставок."));
            }
        }
    }
}
