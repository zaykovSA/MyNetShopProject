using System;
using System.Data.SqlClient;
using System.Security.Permissions;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.Workflow;

namespace MyNetShopProject.Recievers.DeliveriesReceiver
{
    /// <summary>
    /// События элемента списка
    /// </summary>
    public class DeliveriesReceiver : SPItemEventReceiver
    {
        SqlConnection sqlConnection = new SqlConnection("Data Source=SP13;Persist Security Info=True;User ID=sa;Password=8*gUe411kE;Initial Catalog=SPCustomData");
        public override void ItemAdded(SPItemEventProperties properties)
        {
            var listItem = properties.ListItem;
            var courierLookupField = new SPFieldLookupValue(listItem["DeliveryCourier"].ToString());
            var orderLookupField = new SPFieldLookupValue(listItem["DeliveryOrder"].ToString());

            var courierList = properties.Web.GetList("lists/couriers");
            var courierItem = courierList.GetItemById(courierLookupField.LookupId);
            var deliveryCourier = courierItem["Title"];

            var orderList = properties.Web.GetList("lists/orders");
            var orderItem = orderList.GetItemById(orderLookupField.LookupId);
            var deliveryOrder = orderItem["Title"];

            var orderUID = listItem["Title"].ToString();
            var orderAddress = listItem["DeliveryAddress"].ToString();
            SqlCommand sqlCommand;
            try
            {
                sqlConnection.Open();
                sqlCommand = new SqlCommand("Insert into Deliveries (Delivery_Courier,Delivery_Order,Delivery_Address) values((select Couriers.Courier_ID from Couriers where convert(varchar, Couriers.Courier_FIO) = '" + deliveryCourier + "'),(select Orders.Order_ID from Orders where convert(varchar, Orders.Order_UID) = convert(varchar, '" + deliveryOrder + "')),'" + orderAddress + "')", sqlConnection);
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
        public override void ItemUpdating(SPItemEventProperties properties)
        {
            var listItem = properties.ListItem;
            var orderLookupField = new SPFieldLookupValue(listItem["DeliveryOrder"].ToString());
            var newOrderLookupField =
                properties.AfterProperties["DeliveryOrder"] != null
                ? new SPFieldLookupValue(properties.AfterProperties["DeliveryOrder"].ToString())
                : new SPFieldLookupValue(listItem["DeliveryOrder"].ToString());

            var courierLookupField = new SPFieldLookupValue(listItem["DeliveryCourier"].ToString());
            var newCourierLookupField =
                properties.AfterProperties["DeliveryCourier"] != null
                ? new SPFieldLookupValue(properties.AfterProperties["DeliveryCourier"].ToString())
                : new SPFieldLookupValue(listItem["DeliveryCourier"].ToString());

            var orderList = properties.Web.GetList("lists/Orders");
            var orderItem = orderList.GetItemById(orderLookupField.LookupId);
            var deliveryOrder = orderItem["Title"];

            var newOrderItem = orderList.GetItemById(newOrderLookupField.LookupId);
            var newDeliveryOrder = newOrderItem["Title"];

            var courierList = properties.Web.GetList("lists/Couriers");
            var newCourierItem = courierList.GetItemById(newCourierLookupField.LookupId);
            var newDeliveryCourier = newCourierItem["Title"];

            var deliveryAddress = properties.AfterProperties["DeliveryAddress"] != null
                ? properties.AfterProperties["DeliveryAddress"].ToString()
                : listItem["DeliveryAddress"];

            SqlCommand sqlCommand;
            try
            {
                sqlConnection.Open();
                sqlCommand = new SqlCommand("Update Deliveries Set " +
                "Delivery_Courier=(select Couriers.Courier_ID from Couriers where convert(varchar, Couriers.Courier_FIO) = '" +
                                            newDeliveryCourier + "')," +
                "Delivery_Order=(select Orders.Order_ID from Orders where convert(varchar, Orders.Order_UID) = '" +
                                            newDeliveryOrder + "')," +
                "Delivery_Address='" + deliveryAddress + "'" +
                "where convert(varchar, Delivery_Order)=(select Orders.Order_ID from Orders where convert(varchar, Orders.Order_UID) = '" +
                                            deliveryOrder + "')", sqlConnection);
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
        public override void ItemDeleting(SPItemEventProperties properties)
        {
            base.ItemDeleting(properties);
        }
    }
}