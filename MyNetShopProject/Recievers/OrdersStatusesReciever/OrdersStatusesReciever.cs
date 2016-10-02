using System;
using System.Data.SqlClient;
using System.Security.Permissions;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.Workflow;

namespace MyNetShopProject.Recievers.OrdersStatusesReciever
{
    /// <summary>
    /// События элемента списка
    /// </summary>
    public class OrdersStatusesReciever : SPItemEventReceiver
    {
        SqlConnection sqlConnection = new SqlConnection("Data Source=SP13;Persist Security Info=True;User ID=sa;Password=8*gUe411kE;Initial Catalog=SPCustomData");
        /// <summary>
        /// Добавляется элемент.
        /// </summary>
        public override void ItemAdded(SPItemEventProperties properties)
        {
            var listItem = properties.ListItem;
            var orderStatusTitle = listItem["Title"];
            SqlCommand sqlCommand;
            try
            {
                sqlConnection.Open();
                sqlCommand = new SqlCommand("Insert into OrdersStatuses (OrderStatus_Title) values('" + orderStatusTitle + "')", sqlConnection);
                sqlCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {

            }
            finally
            {
                sqlConnection.Close();
            }
            base.ItemAdded(properties);
        }

        /// <summary>
        /// Обновляется элемент.
        /// </summary>
        public override void ItemUpdating(SPItemEventProperties properties)
        {
            var listItem = properties.ListItem;
            var oldOrderStatusTitle = listItem["Title"];
            var orderStatusTitle = properties.AfterProperties["Title"];
            SqlCommand sqlCommand;
            try
            {
                sqlConnection.Open();
                sqlCommand = new SqlCommand("Update OrdersStatuses Set OrderStatus_Title='" + orderStatusTitle + "' where convert(varchar, OrderStatus_Title)='" + oldOrderStatusTitle + "'", sqlConnection);
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
            var orderStatusTitle = listItem["Title"];
            SqlCommand sqlCommand;
            try
            {
                sqlConnection.Open();
                sqlCommand = new SqlCommand("Delete From OrdersStatuses where convert(varchar, OrderStatus_Title)='" + orderStatusTitle + "'", sqlConnection);
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