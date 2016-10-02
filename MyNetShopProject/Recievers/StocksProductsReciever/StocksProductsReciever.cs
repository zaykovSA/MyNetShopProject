using System;
using System.Data.SqlClient;
using Microsoft.SharePoint;

namespace MyNetShopProject.Recievers.StocksProductsReciever
{
    public class StocksProductsReciever : SPItemEventReceiver
    {
        readonly SqlConnection _sqlConnection = new SqlConnection("Data Source=SP13;Persist Security Info=True;User ID=sa;Password=8*gUe411kE;Initial Catalog=SPCustomData");

        public override void ItemUpdating(SPItemEventProperties properties)
        {
            var listItem = properties.ListItem;
            var productLookupField = new SPFieldLookupValue(listItem["Product"].ToString());
            var stockLookupField = new SPFieldLookupValue(listItem["Stock"].ToString());

            var newCount = properties.AfterProperties["Count"];

            try
            {
                _sqlConnection.Open();
                var sqlCommand = new SqlCommand("Update StocksWithProducts Set " +
                                                       "StockWithProduct_Count='" + newCount + "'" +
                                                       "where convert(varchar, StockWithProduct_Stock)=convert(varchar, (select Stocks.Stock_ID from Stocks where convert(varchar, Stocks.Stock_Title) = '" +
                                                       stockLookupField.LookupValue + "'))" +
                                                       "and convert(varchar, StockWithProduct_Product)=convert(varchar, (select Products.Product_ID from Products where convert(varchar, Products.Product_Title) = '" +
                                                       productLookupField.LookupValue + "'))", _sqlConnection);
                sqlCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {

            }
            finally
            {
                _sqlConnection.Close();
            }
            base.ItemUpdating(properties);
        }

        public override void ItemDeleting(SPItemEventProperties properties)
        {
            var listItem = properties.ListItem;

            var productLookupField = new SPFieldLookupValue(listItem["Product"].ToString());
            var stockLookupField = new SPFieldLookupValue(listItem["Stock"].ToString());

            try
            {
                _sqlConnection.Open();
                var sqlCommand = new SqlCommand("Delete From StocksWithProducts " +
                                                       "where convert(varchar, StockWithProduct_Stock)" +
                                                       "=convert(varchar, (select Stocks.Stock_ID from Stocks where convert(varchar, Stocks.Stock_Title) = '" +
                                                       stockLookupField.LookupValue + "'))" +
                                                       "and convert(varchar, StockWithProduct_Product)" +
                                                       "=convert(varchar, (select Products.Product_ID from Products where convert(varchar, Products.Product_Title) = '" +
                                                       productLookupField.LookupValue + "'))", _sqlConnection);
                sqlCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {

            }
            finally
            {
                _sqlConnection.Close();
            }
            base.ItemDeleting(properties);
        }

        public override void ItemAdded(SPItemEventProperties properties)
        {
            var listItem = properties.ListItem;

            var productLookupField = new SPFieldLookupValue(listItem["Product"].ToString());
            var stockLookupField = new SPFieldLookupValue(listItem["Stock"].ToString());
            var count = listItem["Count"];

            var productsList = properties.Web.GetList("lists/Products");
            var productItem = productsList.GetItemById(productLookupField.LookupId);
            var product = productItem["ProductTitle"];

            var stocksList = properties.Web.GetList("lists/Stocks");
            var stockItem = stocksList.GetItemById(stockLookupField.LookupId);
            var stock = stockItem["Title"];
            try
            {
                _sqlConnection.Open();
                var sqlCommand = new SqlCommand("Insert into StocksWithProducts (StockWithProduct_Product,StockWithProduct_Stock,StockWithProduct_Count) values((select Products.Product_ID from Products where convert(varchar, Products.Product_Title) = '" + product + "'),(select Stocks.Stock_ID from Stocks where convert(varchar, Stocks.Stock_Title) = '" + stock + "'), '" + count + "')", _sqlConnection);
                sqlCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {

            }
            finally
            {
                _sqlConnection.Close();
            }
            base.ItemAdded(properties);
        }
    }
}