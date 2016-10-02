using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;
using System;
using System.Collections.Generic;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;

namespace MyNetShopProject
{
    class OrderFieldControl : TextField
    {
        private HtmlTable _table;

        private readonly HiddenField _hiddenFormedString = new HiddenField
        {
            ID = "returnString",
            ClientIDMode = ClientIDMode.Static
        };

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);
            if ((SPContext.Current.FormContext.FormMode == SPControlMode.New) ||
                (SPContext.Current.FormContext.FormMode == SPControlMode.Edit))
            {
                SPContext.Current.FormContext.OnSaveHandler += myCustomSaveHandler;
            }
        }

        protected override void CreateChildControls()
        {
            base.CreateChildControls();
            _table = new HtmlTable();
            HtmlTableRow row;
            HtmlTableCell cell;

            _table.ID = "MainProductsTable";
            _table.ClientIDMode = ClientIDMode.Static;
            _table.CellPadding = 0;
            _table.CellSpacing = 0;
            _table.Style["border-collapse"] = "collapse";

            if (ControlMode == SPControlMode.Edit)
            {
                var currentValue = (string) ItemFieldValue;
                _hiddenFormedString.Value = currentValue;
                if (!string.IsNullOrEmpty(currentValue))
                {
                    row = new HtmlTableRow();
                    cell = new HtmlTableCell
                    {
                        ColSpan = 2,
                        InnerText = "Вы выбрали следующие товары:"
                    };
                    row.Cells.Add(cell);
                    _table.Rows.Add(row);

                    var splitedSelectedItems = currentValue.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var item in splitedSelectedItems)
                    {
                        var trimmedItem = item.Trim();
                        if (string.IsNullOrEmpty(trimmedItem))
                            continue;
                        var product = trimmedItem.Split('x')[0].Trim();
                        var count = trimmedItem.Split('x')[1].Trim();

                        row = new HtmlTableRow();
                        cell = new HtmlTableCell {ColSpan = 2};
                        if (string.IsNullOrEmpty(count))
                            cell.InnerText = "• " + product + " в количестве 0 штук.";
                        else
                            cell.InnerText = "• " + product + " в количестве " + count;
                        row.Cells.Add(cell);
                        _table.Rows.Add(row);
                    }
                    row = new HtmlTableRow();
                    cell = new HtmlTableCell() {InnerText = "  "};
                    row.Cells.Add(cell);
                    _table.Rows.Add(row);
                    row = new HtmlTableRow();
                    cell = new HtmlTableCell {ColSpan = 2};
                    cell.Attributes["class"] = "ms-formdescription";
                    cell.InnerText =
                        "Заказ уже сформирован, чтобы изменить его содержимое - удалите заказ и задайте новый!";
                    row.Cells.Add(cell);
                    _table.Rows.Add(row);
                }
                Controls.Clear();
                Controls.Add(_table);
            }

            if (ControlMode == SPControlMode.New)
            {
                var productsList = SPContext.Current.Web.GetList("/Lists/Products");
                var productsItems = productsList.Items;
                var productsItemsList = new List<ListItem>();
                foreach (SPListItem product in productsItems)
                {
                    productsItemsList.Add(new ListItem(product["ProductTitle"].ToString(),
                        product["ProductTitle"].ToString()));
                }

                row = new HtmlTableRow();
                cell = new HtmlTableCell {ColSpan = 2};
                cell.Attributes["class"] = "ms-formdescription";
                cell.InnerText = "Выберите товары и их количество:";
                row.Cells.Add(cell);
                _table.Rows.Add(row);

                row = new HtmlTableRow();
                cell = new HtmlTableCell();
                var productDd = new DropDownList {ID = "DD_0"};
                productDd.Items.Add("Выберите товар");
                productDd.Items.AddRange(productsItemsList.ToArray());
                productDd.Attributes.Add(
                    "onchange",
                    "var t = document.getElementById('MainProductsTable');" +
                    "var resultString = '';" +
                    "for (var i = 1; i < t.rows.length; i++){" +
                    "var r = t.rows(i);" +
                    "var ddcell = r.cells(0);" +
                    "var dd = ddcell.childNodes[0];" +
                    "var tbcell = r.cells(1);" +
                    "var tb = tbcell.childNodes[0];" +
                    "var count = tb.value;" +
                    "var o = dd.options[dd.selectedIndex].value;" +
                    "if(o != 'Выберите товар'){" +
                    "resultString = resultString + o + ' x ' + count + 'шт.; ';}}" +
                    "var hid = document.getElementById('returnString');" +
                    "hid.value = resultString;");
                cell.Controls.Add(productDd);
                row.Cells.Add(cell);

                cell = new HtmlTableCell();
                var productCount = new TextBox {Width = 10};
                productCount.Attributes.Add(
                    "onchange",
                    "var t = document.getElementById('MainProductsTable');" +
                    "var resultString = '';" +
                    "for (var i = 1; i < t.rows.length; i++){" +
                    "var r = t.rows(i);" +
                    "var ddcell = r.cells(0);" +
                    "var dd = ddcell.childNodes[0];" +
                    "var tbcell = r.cells(1);" +
                    "var tb = tbcell.childNodes[0];" +
                    "var count = tb.value;" +
                    "var o = dd.options[dd.selectedIndex].value;" +
                    "if(o != 'Выберите товар'){" +
                    "resultString = resultString + o + ' x ' + count + 'шт.; ';}}" +
                    "var hid = document.getElementById('returnString');" +
                    "hid.value = resultString;");
                cell.Controls.Add(productCount);
                row.Cells.Add(cell);
                _table.Rows.Add(row);

                var button = new Button {Text = "Добавить еще один товар"};
                button.Attributes.Add(
                    "onclick",
                    "var t = document.getElementById('MainProductsTable');" +
                    "var rToClone = t.rows(t.rows.length - 1);" +
                    "var ddcell = rToClone.cells(0);" +
                    "var dd = ddcell.childNodes[0];" +
                    "var optLength = dd.options.length - 1;" +
                    "var tLength = t.rows.length - 1;" +
                    "if (!(dd.selectedIndex === 0) && tLength < optLength)" +
                    "{var clone = rToClone.cloneNode(true);" +
                    "var cloneddcell = clone.cells(0);" +
                    "var clonedd = cloneddcell.childNodes[0];" +
                    "clonedd.selectedIndex = 0;" +
                    "var clonetbcell = clone.cells(1);" +
                    "var clonetb = clonetbcell.childNodes[0];" +
                    "clonetb.innerText = '';" +
                    "t.appendChild(clone);}return false;");

                Controls.Clear();
                Controls.Add(_hiddenFormedString);
                Controls.Add(_table);
                Controls.Add(button);
            }

            if (ControlMode == SPControlMode.Display)
            {
                var currentValue = (String) ItemFieldValue;
                if (!string.IsNullOrEmpty(currentValue))
                {
                    row = new HtmlTableRow();
                    cell = new HtmlTableCell {ColSpan = 2};
                    cell.Attributes["class"] = "ms-formdescription";
                    cell.InnerText = "Выбранные товары и их количество:";
                    row.Cells.Add(cell);
                    _table.Rows.Add(row);

                    var splitedSelectedItems = currentValue.Split(';');
                    foreach (var item in splitedSelectedItems)
                    {
                        if (string.IsNullOrEmpty(item.Trim()))
                            continue;
                        var trimmedItem = item.Trim();
                        var product = trimmedItem.Split('x')[0];
                        var count = trimmedItem.Split('x')[1];

                        row = new HtmlTableRow();
                        cell = new HtmlTableCell();
                        cell.Controls.Add(new Literal {Text = string.Format("{0} в количестве {1}", product, count)});
                        row.Controls.Add(cell);
                        _table.Controls.Add(row);
                    }
                }
                else
                {
                    row = new HtmlTableRow();
                    cell = new HtmlTableCell {ColSpan = 2};
                    cell.Attributes["class"] = "ms-formdescription";
                    cell.InnerText = "Вы не выбрали ни одного товара!";
                    row.Cells.Add(cell);
                    _table.Rows.Add(row);
                }
                Controls.Clear();
                Controls.Add(_table);
            }
            ChildControlsCreated = true;
        }

        protected void myCustomSaveHandler(object sender, EventArgs e)
        {
            ItemFieldValue = _hiddenFormedString.Value;
            SPContext.Current.ListItem.Update();
        }
    }
}
