using System;
using System.Data.SqlClient;
using System.Linq;
using System.Security.Permissions;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.Workflow;

namespace MyNetShopProject.Recievers.ProductsReciever
{
    /// <summary>
    /// События элемента списка
    /// </summary>
    public class ProductsReciever : SPItemEventReceiver
    {
        SqlConnection sqlConnection = new SqlConnection("Data Source=SP13;Persist Security Info=True;User ID=sa;Password=8*gUe411kE;Initial Catalog=SPCustomData");
        /// <summary>
        /// Добавляется элемент.
        /// </summary>
        public override void ItemAdded(SPItemEventProperties properties)
        {
            var listItem = properties.ListItem;

            var productUID = Guid.NewGuid();
            listItem["Title"] = productUID;
            listItem.Update();

            var categoryList = properties.Web.GetList("lists/ProductsCategories");
            var lookupField = new SPFieldLookupValue(listItem["ProductCategory"].ToString());
            var categoryItem = categoryList.GetItemById(lookupField.LookupId);
            var productCategory = categoryItem["Title"];

            var productTitle = listItem["ProductTitle"];
            var productSoloPrice = listItem["ProductSoloPrice"];
            var productMultyPrice = listItem["ProductMultyPrice"];
            var productMultyPriceCount = listItem["ProductMultyPriceCount"];
            
            SqlCommand sqlCommand;
            try
            {
                sqlConnection.Open();
                sqlCommand = new SqlCommand("Insert into Products (Product_UID, Product_Title, Product_SoloPrice, Product_MultyPrice, Product_MultyPriceCount, Product_Category) values('" + productUID + "','" + productTitle + "','" + productSoloPrice + "','" + productMultyPrice + "','" + productMultyPriceCount + "',(select Categories.Category_ID from Categories where convert(varchar, Categories.Category_Title) = '" + productCategory + "'))", sqlConnection);
                sqlCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {

            }
            finally
            {
                sqlConnection.Close();
            }

            AddProductToStocksProductsList(properties.Web, listItem.ID);

            base.ItemAdded(properties);
        }

        /// <summary>
        /// Обновляется элемент.
        /// </summary>
        public override void ItemUpdating(SPItemEventProperties properties)
        {
            var listItem = properties.ListItem;
            var categoryList = properties.Web.GetList("lists/ProductsCategories");
            var lookupField = new SPFieldLookupValue(properties.AfterProperties["ProductCategory"].ToString());
            var categoryItem = categoryList.GetItemById(lookupField.LookupId);

            var oldProductUID = listItem["Title"];

            var productUID = properties.AfterProperties["Title"];
            var productTitle = properties.AfterProperties["ProductTitle"];
            var productSoloPrice = properties.AfterProperties["ProductSoloPrice"];
            var productMultyPrice = properties.AfterProperties["ProductMultyPrice"];
            var productMultyPriceCount = properties.AfterProperties["ProductMultyPriceCount"];
            var productCategory = categoryItem["Title"];

            SqlCommand sqlCommand;
            try
            {
                sqlConnection.Open();
                sqlCommand = new SqlCommand("Update Products Set Product_UID='" + productUID + "', Product_Title='" + productTitle + "', Product_SoloPrice='" + productSoloPrice + "', Product_MultyPrice='" + productMultyPrice + "', Product_MultyPriceCount='" + productMultyPriceCount + "', Product_Category= (select Categories.Category_ID from Categories where convert(varchar, Categories.Category_Title) = '" + productCategory + "') where convert(varchar, Product_UID)='" + oldProductUID + "'", sqlConnection);
                sqlCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {}
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
            var productUID = listItem["Title"];
            SqlCommand sqlCommand;
            try
            {
                sqlConnection.Open();
                sqlCommand = new SqlCommand("Delete From Products where convert(varchar, Product_UID)='" + productUID + "'", sqlConnection);
                sqlCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {}
            finally
            {
                sqlConnection.Close();
            }

            DeleteProductFromStocksProductsList(properties.Web, listItem.ID);

            base.ItemDeleting(properties);
        }

        public void AddProductToStocksProductsList(SPWeb web, int productId)
        {
            var stocksList = web.GetList("lists/Stocks");
            var stockIndexes = (from SPListItem item in stocksList.Items select item.ID).ToList();

            var stocksProductsList = web.GetList("lists/StocksProducts");
            foreach (var stockIndex in stockIndexes)
            {
                var newItem = stocksProductsList.Items.Add();
                newItem["Product"] = new SPFieldLookupValue { LookupId = productId };
                newItem["Stock"] = new SPFieldLookupValue { LookupId = stockIndex };
                newItem["Count"] = 0;
                newItem.Update();
            }
        }

        public void DeleteProductFromStocksProductsList(SPWeb web, int productId)
        {
            var stocksProductsList = web.GetList("lists/StocksProducts");
            var indexesForDelete = (from SPListItem item in stocksProductsList.Items
                                    let productField = new SPFieldLookupValue(item["Product"].ToString())
                                    where productField.LookupId == productId
                                    select item.ID).ToList();
            foreach (var i in indexesForDelete)
            {
                stocksProductsList.Items.DeleteItemById(i);
            }
        }
    }
}