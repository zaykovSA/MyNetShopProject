using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Permissions;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Portal.Audience.AdminUI;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.Workflow;

namespace MyNetShopProject.Recievers.StocksReciever
{
    /// <summary>
    /// События элемента списка
    /// </summary>
    public class StocksReciever : SPItemEventReceiver
    {
        SqlConnection sqlConnection = new SqlConnection("Data Source=SP13;Persist Security Info=True;User ID=sa;Password=8*gUe411kE;Initial Catalog=SPCustomData");
        /// <summary>
        /// Добавляется элемент.
        /// </summary>
        public override void ItemAdded(SPItemEventProperties properties)
        {
            var cityList = properties.Web.GetList("lists/cities");
            var listItem = properties.ListItem;
            var lookupField = new SPFieldLookupValue(listItem["StockCity"].ToString());
            var cityItem = cityList.GetItemById(lookupField.LookupId);

            var stockTitle = listItem["Title"];
            var stockAddress = listItem["StockAddress"];
            var stockPhone = listItem["StockPhone"];
            var stockCity = cityItem["Title"];

            SqlCommand sqlCommand;
            try
            {
                sqlConnection.Open();
                sqlCommand = new SqlCommand("Insert into Stocks (Stock_Title,Stock_Address,Stock_Phone,Stock_City) values('" + stockTitle + "','" + stockAddress + "','" + stockPhone + "',(select Cities.City_ID from Cities where convert(varchar, Cities.City_Title) = '" + stockCity + "'))", sqlConnection);
                sqlCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {

            }
            finally
            {
                sqlConnection.Close();
            }

            AddStockToStocksProductsList(properties.Web, listItem.ID);
            base.ItemAdded(properties);
        }

        /// <summary>
        /// Обновляется элемент.
        /// </summary>
        public override void ItemUpdating(SPItemEventProperties properties)
        {
            var listItem = properties.ListItem;
            var cityList = properties.Web.GetList("lists/cities");
            var lookupField = new SPFieldLookupValue(properties.AfterProperties["StockCity"].ToString());
            var cityItem = cityList.GetItemById(lookupField.LookupId);

            var oldStockTitle = listItem["Title"];
            var oldStockAddress = listItem["StockAddress"];
            var oldStockPhone = listItem["StockPhone"];

            var stockTitle = properties.AfterProperties["Title"];
            var stockAddress = properties.AfterProperties["StockAddress"];
            var stockPhone = properties.AfterProperties["StockPhone"];
            var stockCity = cityItem["Title"];

            SqlCommand sqlCommand;
            try
            {
                sqlConnection.Open();
                sqlCommand = new SqlCommand("Update Stocks Set Stock_Title='" + stockTitle + "', Stock_Address='" + stockAddress + "', Stock_Phone='" + stockPhone + "', Stock_City= (select Cities.City_ID from Cities where convert(varchar, Cities.City_Title) = '" + stockCity + "') where convert(varchar, Stock_Title)='" + oldStockTitle + "' and convert(varchar, [Stock_Address])='" + oldStockAddress + "' and convert(varchar, [Stock_Phone])='" + oldStockPhone + "'", sqlConnection);
                sqlCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {

            }
            finally
            {
                sqlConnection.Close();
            }
            base.ItemUpdating(properties);
        }

        /// <summary>
        /// Удаляется элемент.
        /// </summary>
        public override void ItemDeleting(SPItemEventProperties properties)
        {
            var listItem = properties.ListItem;
            var stockTitle = listItem["Title"];
            var stockAddress = listItem["StockAddress"];
            var stockPhone = listItem["StockPhone"];

            SqlCommand sqlCommand;
            try
            {
                sqlConnection.Open();
                sqlCommand = new SqlCommand("Delete From Stocks where convert(varchar, Stock_Title)='" + stockTitle + "' and convert(varchar, Stock_Address)='" + stockAddress + "' and convert(varchar, Stock_Phone)='" + stockPhone + "'", sqlConnection);
                sqlCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {

            }
            finally
            {
                sqlConnection.Close();
            }

            DeleteStockFromStocksProductsList(properties.Web, listItem.ID);

            base.ItemDeleting(properties);
        }

        public void AddStockToStocksProductsList(SPWeb web, int stockId)
        {
            var productList = web.GetList("lists/Products");
            var productIndexes = (from SPListItem item in productList.Items select item.ID).ToList();

            var stocksProductsList = web.GetList("lists/StocksProducts");
            foreach (var productIndex in productIndexes)
            {
                var newItem = stocksProductsList.Items.Add();
                newItem["Product"] = new SPFieldLookupValue { LookupId = productIndex };
                newItem["Stock"] = new SPFieldLookupValue { LookupId = stockId };
                newItem["Count"] = 0;
                newItem.Update();
            }
        }

        public void DeleteStockFromStocksProductsList(SPWeb web, int stockId)
        {
            var stocksProductsList = web.GetList("lists/StocksProducts");
            var indexesForDelete = (from SPListItem item in stocksProductsList.Items
                let stockField = new SPFieldLookupValue(item["Stock"].ToString())
                where stockField.LookupId == stockId
                select item.ID).ToList();
            foreach (var i in indexesForDelete)
            {
                stocksProductsList.Items.DeleteItemById(i);
            }
        }
    }
}