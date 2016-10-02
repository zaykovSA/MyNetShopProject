using System.ComponentModel;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using Microsoft.SharePoint;

namespace MyNetShopProject.ChoseAvailableOperationsWebPart
{
    [ToolboxItemAttribute(false)]
    public class ChoseAvailableOperationsWebPart : WebPart
    {
        protected override void CreateChildControls()
        {
            var web = SPContext.Current.Web;
            var adminGroup = web.Groups.GetByName("Администраторы");
            var directorGroup = web.Groups.GetByName("Директорат");
            var managerGroup = web.Groups.GetByName("Менеджеры");
            var operatorGroup = web.Groups.GetByName("Операторы");
            var isAdmin = web.IsCurrentUserMemberOfGroup(adminGroup.ID);
            var isDirector = web.IsCurrentUserMemberOfGroup(directorGroup.ID);
            var isManager = web.IsCurrentUserMemberOfGroup(managerGroup.ID);
            var isOperator = web.IsCurrentUserMemberOfGroup(operatorGroup.ID);

            var sb = new System.Text.StringBuilder();
            sb.AppendLine(@"<script language='javascript'>");
            sb.AppendLine(@"var link = document.createElement('link');");
            sb.AppendLine(@"link.href = '_layouts/15/MyNetShopProject/Scripts/Buttons.css';");
            sb.AppendLine(@"link.rel = 'stylesheet';");
            sb.AppendLine(@"document.getElementsByTagName('head')[0].appendChild(link);");
            sb.AppendLine(@"</script>");
            Page.ClientScript.RegisterStartupScript(GetType(), "registerCss", sb.ToString());
            var panel = new Panel { ID = "MainPanel" };
            var table = new HtmlTable { Align = "center" };

            if (isAdmin) //if Operators group
            {
                table.Controls.Add(
                    CreateTableRowWithTwoControls(
                    "Открыть список Городов для редактирования.", 
                    "http://sp13/Lists/CIties/AllItems.aspx", 
                    "Открыть список Складов для редактирования.", 
                    "http://sp13/Lists/Stocks/AllItems.aspx"));

                table.Controls.Add(new HtmlTableRow { Height = "20px" });

                table.Controls.Add(
                    CreateTableRowWithTwoControls(
                    "Открыть список Курьеров для редактирования.",
                    "http://sp13/Lists/Couriers/AllItems.aspx",
                    "Открыть список Товаров для редактирования.",
                    "http://sp13/Lists/Products/AllItems.aspx"));

                table.Controls.Add(new HtmlTableRow { Height = "20px" });

                table.Controls.Add(
                    CreateTableRowWithTwoControls(
                    "Открыть список Категорий товаров для редактирования.",
                    "http://sp13/Lists/ProductsCategories/AllItems.aspx",
                    "Открыть список Клиентов для редактирования.",
                    "http://sp13/Lists/Clients/AllItems.aspx"));

                table.Controls.Add(new HtmlTableRow { Height = "20px" });

                table.Controls.Add(
                    CreateTableRowWithTwoControls(
                    "Открыть список Статусов Заказов для редактирования.",
                    "http://sp13/Lists/OrdersStatuses/AllItems.aspx",
                    "Открыть список Заказов для редактирования.",
                    "http://sp13/Lists/Orders/AllItems.aspx"));

                table.Controls.Add(new HtmlTableRow { Height = "20px" });
            }
            if (isDirector)
            {
                //Задать ссылку
                table.Controls.Add(
                    CreateTableRowWithTwoControls(
                    "Просмотреть отчеты по работе.",
                    "blahBlah",
                    null,
                    null));

                table.Controls.Add(new HtmlTableRow { Height = "20px" });
            }
            if (isManager)
            {
                table.Controls.Add(
                    CreateTableRowWithTwoControls(
                    "Открыть список Городов для редактирования.",
                    "http://sp13/Lists/CIties/AllItems.aspx",
                    "Открыть список Складов для редактирования.",
                    "http://sp13/Lists/Stocks/AllItems.aspx"));

                table.Controls.Add(new HtmlTableRow { Height = "20px" });

                table.Controls.Add(
                    CreateTableRowWithTwoControls(
                    "Открыть список Курьеров для редактирования.",
                    "http://sp13/Lists/Couriers/AllItems.aspx",
                    "Открыть список Товаров для редактирования.",
                    "http://sp13/Lists/Products/AllItems.aspx"));

                table.Controls.Add(new HtmlTableRow { Height = "20px" });

                table.Controls.Add(
                    CreateTableRowWithTwoControls(
                    "Открыть список Категорий товаров для редактирования.",
                    "http://sp13/Lists/ProductsCategories/AllItems.aspx",
                    "Открыть список Клиентов для редактирования.",
                    "http://sp13/Lists/Clients/AllItems.aspx"));

                table.Controls.Add(new HtmlTableRow { Height = "20px" });
                //Задать ссылку
                table.Controls.Add(
                    CreateTableRowWithTwoControls(
                    "Задать пришедшие Поставки на Склады",
                    "http://sp13/SitePages/StocksFiller.aspx",
                    null,
                    null));

                table.Controls.Add(new HtmlTableRow { Height = "20px" });
            }
            if (isOperator)
            {
                table.Controls.Add(
                    CreateTableRowWithTwoControls(
                    "Открыть список Заказов для редактирования.",
                    "http://sp13/Lists/Orders/AllItems.aspx",
                    "Открыть список Клиентов для редактирования.",
                    "http://sp13/Lists/Clients/AllItems.aspx"));

                table.Controls.Add(new HtmlTableRow { Height = "20px" });

                //включить ссылку на перенаправление
                table.Controls.Add(
                    CreateTableRowWithTwoControls(
                    "Открыть список текущих Доставок для редактирования.",
                    "blahBlah",
                    null,
                    null));

                table.Controls.Add(new HtmlTableRow { Height = "20px" });
            }
            panel.Controls.Add(table);
            Controls.Add(panel);
            ChildControlsCreated = true;
        }

        private HtmlTableRow CreateTableRowWithTwoControls(string leftText, string leftUrl, string rightText, string rightUrl)
        {
            var tableRow = new HtmlTableRow();
            var lCell = new HtmlTableCell();
            var mCell = new HtmlTableCell() { Width = "5px" };
            var rCell = new HtmlTableCell();

            if (leftUrl != null)
            {
                var lButton = new HtmlGenericControl("lbut") {InnerText = leftText};
                lButton.Attributes.Add("onclick", "window.location.href = '" + leftUrl + "';");
                lCell.Controls.Add(lButton);
            }
            tableRow.Controls.Add(lCell);
            tableRow.Controls.Add(mCell);
            if (rightUrl != null)
            {
                var rButton = new HtmlGenericControl("rbut") {InnerText = rightText};
                rButton.Attributes.Add("onclick", "window.location.href = '" + rightUrl + "';");
                rCell.Controls.Add(rButton);
            }
            tableRow.Controls.Add(rCell);
            return tableRow;
        }
    }
}
