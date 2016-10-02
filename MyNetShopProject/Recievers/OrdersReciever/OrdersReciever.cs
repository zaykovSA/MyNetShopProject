using System;
using System.Data.SqlClient;
using System.Security.Permissions;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.Workflow;

namespace MyNetShopProject.Recievers.OrdersReciever
{
    /// <summary>
    /// События элемента списка
    /// </summary>
    public class OrdersReciever : SPItemEventReceiver
    {
        SqlConnection sqlConnection = new SqlConnection("Data Source=SP13;Persist Security Info=True;User ID=sa;Password=8*gUe411kE;Initial Catalog=SPCustomData");
        /// <summary>
        /// Добавляется элемент.
        /// </summary>
        public override void ItemAdded(SPItemEventProperties properties)
        {
            var listItem = properties.ListItem;
            var clientLookupField = new SPFieldLookupValue(listItem["orderClient"].ToString());
            var statusLookupField = new SPFieldLookupValue(listItem["orderStatus"].ToString());

            var clientList = properties.Web.GetList("lists/clients");
            var clientItem = clientList.GetItemById(clientLookupField.LookupId);
            var orderClient = clientItem["Title"];

            var statusList = properties.Web.GetList("lists/OrdersStatuses");
            var statustItem = statusList.GetItemById(statusLookupField.LookupId);
            var orderStatus = statustItem["Title"];

            var orderUID = Guid.NewGuid();
            listItem["Title"] = orderUID;
            listItem.Update();

            SqlCommand sqlCommand;
            try
            {
                sqlConnection.Open();
                sqlCommand = new SqlCommand("Insert into Orders (Order_UID,Order_Client,Order_Status) values('" + orderUID + "',(select Clients.Client_ID from Clients where convert(varchar, Clients.Client_FIO) = '" + orderClient + "'),(select OrdersStatuses.OrderStatus_ID from OrdersStatuses where convert(varchar, OrdersStatuses.OrderStatus_Title) = '" + orderStatus + "'))", sqlConnection);
                sqlCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {

            }
            finally
            {
                sqlConnection.Close();
            }

            var productsString = listItem["111"].ToString();
            var splitedProductsWithCount = productsString.Split(new[] {";"}, StringSplitOptions.RemoveEmptyEntries);
            foreach (var s in splitedProductsWithCount)
            {
                var splitedObject = s.Trim().Split('x');
                var productName = splitedObject[0].Trim();
                var productCount = splitedObject[1].Trim();
                productCount = productCount.Remove(productCount.Length - 3);

                try
                {
                    sqlConnection.Open();
                    sqlCommand = new SqlCommand(
                        "Insert into ProductsInOrders " +
                        "(ProductInOrder_Product,ProductInOrder_Order,ProductInOrder_Count) " +
                        "values(" +
                        "(select Products.Product_ID from Products where convert(varchar, Products.Product_Title) = '" + productName + "')," +
                        "(select Orders.Order_ID from Orders where convert(varchar, Orders.Order_UID) = convert(varchar,'" + orderUID + "'))," +
                        "'" + productCount + "')", sqlConnection);
                    sqlCommand.ExecuteNonQuery();
                }
                catch (Exception ex)
                {

                }
                finally
                {
                    sqlConnection.Close();
                }
            }
            base.ItemAdded(properties);
        }

        /// <summary>
        /// Обновляется элемент.
        /// </summary>
        public override void ItemUpdating(SPItemEventProperties properties)
        {
            var listItem = properties.ListItem;
            var orderUID = listItem["Title"];
            var clientLookupField = 
                properties.AfterProperties["orderClient0"] != null 
                ? new SPFieldLookupValue(properties.AfterProperties["orderClient0"].ToString()) 
                : new SPFieldLookupValue(listItem["orderClient0"].ToString());
            var statusLookupField = new SPFieldLookupValue(properties.AfterProperties["orderStatus0"].ToString());

            var clientList = properties.Web.GetList("lists/clients");
            var clientItem = clientList.GetItemById(clientLookupField.LookupId);
            var orderClient = clientItem["Title"];

            var statusList = properties.Web.GetList("lists/OrdersStatuses");
            var statustItem = statusList.GetItemById(statusLookupField.LookupId);
            var orderStatus = statustItem["Title"];

            SqlCommand sqlCommand;
            try
            {
                sqlConnection.Open();
                sqlCommand = new SqlCommand("Update Orders Set "+
                    "Order_Client=(select Clients.Client_ID from Clients where convert(varchar, Clients.Client_FIO) = '" + orderClient + "')," +
                    "Order_Status=(select OrdersStatuses.OrderStatus_ID from OrdersStatuses where convert(varchar, OrdersStatuses.OrderStatus_Title) = '" + orderStatus + "') " +
                    "where convert(varchar, Order_UID)=convert(varchar, '" + orderUID + "')", sqlConnection);
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

            var orderUID = listItem["Title"];
            SqlCommand sqlCommand;
            try
            {
                sqlConnection.Open();
                sqlCommand = new SqlCommand("Delete From ProductsInOrders where convert(varchar, ProductInOrder_Order)=(select Orders.Order_ID from Orders where convert(varchar, Orders.Order_UID) = convert(varchar,'" + orderUID + "'))", sqlConnection);
                sqlCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {

            }
            finally
            {
                sqlConnection.Close();
            }
            try
            {
                sqlConnection.Open();
                sqlCommand = new SqlCommand("Delete From Orders where convert(varchar, Order_UID)=convert(varchar,'" + orderUID + "')", sqlConnection);
                sqlCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {

            }
            finally
            {
                sqlConnection.Close();
            }
            base.ItemDeleting(properties);
        }
    }
}