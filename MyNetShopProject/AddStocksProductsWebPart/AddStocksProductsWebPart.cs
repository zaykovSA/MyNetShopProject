using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using Microsoft.SharePoint;

namespace MyNetShopProject.AddStocksProductsWebPart
{
    [ToolboxItemAttribute(false)]
    public class AddStocksProductsWebPart : WebPart
    {
        readonly UpdatePanel _panel = new UpdatePanel
        {
            ID = "stocksProductsPanel",
            ClientIDMode = ClientIDMode.Static,
            ChildrenAsTriggers = true,
            UpdateMode = UpdatePanelUpdateMode.Conditional
        };

        private readonly HtmlTable _table = new HtmlTable {ID = "mainStocksProductsTable", ClientIDMode = ClientIDMode.Static};


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

            var row = new HtmlTableRow();
            var cell = new HtmlTableCell();

            var mainLiteral = new Literal {Text = "Выберите данные по поставке:"};
            cell.ColSpan = 2;
            cell.Controls.Add(mainLiteral);
            row.Cells.Add(cell);
            _table.Rows.Add(row);

            var web = SPContext.Current.Web;

            var productsList = web.GetList("lists/Products");
            var stocksList = web.GetList("lists/Stocks");

            var stockLiteral = new Literal {Text = "Склад: "};
            var stocksDd = new DropDownList();
            stocksDd.Items.Add("<<---Выберите склад--->>");
            foreach (SPListItem stock in stocksList.Items)
            {
                stocksDd.Items.Add(new ListItem(stock["Title"].ToString(), stock.ID.ToString()));
            }
            row = new HtmlTableRow();
            cell = new HtmlTableCell();
            cell.Controls.Add(stockLiteral);
            row.Cells.Add(cell);
            cell = new HtmlTableCell();
            cell.Controls.Add(stocksDd);
            row.Cells.Add(cell);
            _table.Rows.Add(row);

            var productLiteral = new Literal {Text = "Товар: "};
            var productDd = new DropDownList();
            productDd.Items.Add("<<---Выберите товар--->>");
            foreach (SPListItem product in productsList.Items)
            {
                productDd.Items.Add(new ListItem(product["ProductTitle"].ToString(), product.ID.ToString()));
            }
            row = new HtmlTableRow();
            cell = new HtmlTableCell();
            cell.Controls.Add(productLiteral);
            row.Cells.Add(cell);
            cell = new HtmlTableCell();
            cell.Controls.Add(productDd);
            row.Cells.Add(cell);
            _table.Rows.Add(row);

            var countLiteral = new Literal {Text = " Прибывшее количество: "};

            var countTextBox = new TextBox {ID = "CounterTextBox", ClientIDMode = ClientIDMode.Static};
            countTextBox.Attributes.Add("style", "border-radius: 5px;");

            row = new HtmlTableRow();
            cell = new HtmlTableCell();
            cell.Controls.Add(countLiteral);
            row.Cells.Add(cell);
            cell = new HtmlTableCell();
            cell.Controls.Add(countTextBox);
            row.Cells.Add(cell);
            _table.Rows.Add(row);

            var applyButton = new Button {Text = "Сохранить данные"};
            applyButton.Click += applyButton_Click;
            row = new HtmlTableRow();
            cell = new HtmlTableCell {ColSpan = 2};
            cell.Controls.Add(applyButton);
            row.Cells.Add(cell);
            _table.Rows.Add(row);
            _panel.ContentTemplateContainer.Controls.Add(_table);
            Controls.Add(_panel);

            ChildControlsCreated = true;
        }

        void applyButton_Click(object sender, EventArgs e)
        {
            using (var web = SPContext.Current.Web)
            {
                SPListItem stockItem = null;
                SPListItem productItem = null;
                var count = 0;
                var stockDd = _table.Rows[1].Cells[1].Controls[0] as DropDownList;
                if (stockDd != null)
                {
                    stockItem = web.GetList("Lists/Stocks").GetItemById(Convert.ToInt32(stockDd.SelectedValue));
                }
                var productDd = _table.Rows[2].Cells[1].Controls[0] as DropDownList;
                if (productDd != null)
                {
                    productItem = web.GetList("Lists/Products").GetItemById(Convert.ToInt32(productDd.SelectedValue));
                }
                var countTb = _table.Rows[3].Cells[1].Controls[0] as TextBox;
                
                try
                {
                    if (countTb != null) count = Convert.ToInt32(countTb.Text);
                }
                catch (Exception)
                {
                    Page.ClientScript.RegisterStartupScript(GetType(), "AlertError", "alert(\"Поле 'Количество' должно содержать число!\")");
                    return;
                }

                var stocksProductsList = web.GetList("Lists/StocksProducts");
                foreach (SPListItem stockProduct in stocksProductsList.Items)
                {
                    var productLookupField = new SPFieldLookupValue(stockProduct["Product"].ToString());
                    var stockLookupField = new SPFieldLookupValue(stockProduct["Stock"].ToString());

                    if (stockItem != null && (productItem != null && (productLookupField.LookupId != productItem.ID || stockLookupField.LookupId != stockItem.ID)))
                        continue;
                    var currentCount = Convert.ToInt32(stockProduct["Count"]);
                    currentCount += count;
                    stockProduct["Count"] = currentCount;
                    stockProduct.Update();
                }

                if (stockDd != null) stockDd.ClearSelection();
                if (productDd != null) productDd.ClearSelection();
                if (countTb != null) countTb.Text = "";
                Page.Response.Redirect(Page.Request.RawUrl);
            }
        }
    }
}