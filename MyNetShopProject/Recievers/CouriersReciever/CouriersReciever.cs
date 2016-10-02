using System;
using System.Data.SqlClient;
using System.Security.Permissions;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.Workflow;

namespace MyNetShopProject.Recievers.CouriersReciever
{
    /// <summary>
    /// События элемента списка
    /// </summary>
    public class CouriersReciever : SPItemEventReceiver
    {
        SqlConnection sqlConnection = new SqlConnection("Data Source=SP13;Persist Security Info=True;User ID=sa;Password=8*gUe411kE;Initial Catalog=SPCustomData");
        /// <summary>
        /// Добавляется элемент.
        /// </summary>
        public override void ItemAdded(SPItemEventProperties properties)
        {
            var cityList = properties.Web.GetList("lists/cities");
            var listItem = properties.ListItem;
            var courierTitle = listItem["Title"];
            var courierPhone = listItem["CourierPhone"];
            var lookupField = new SPFieldLookupValue(listItem["CourierCity"].ToString());
            var cityItem = cityList.GetItemById(lookupField.LookupId);
            var courierCity = cityItem["Title"];
            SqlCommand sqlCommand;
            try
            {
                sqlConnection.Open();
                sqlCommand = new SqlCommand("Insert into Couriers (Courier_FIO,Courier_Phone,Courier_City) values('" + courierTitle + "','" + courierPhone + "',(select Cities.City_ID from Cities where convert(varchar, Cities.City_Title) = '" + courierCity +"'))", sqlConnection);
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
            var cityList = properties.Web.GetList("lists/cities");
            var lookupField = new SPFieldLookupValue(properties.AfterProperties["CourierCity"].ToString());
            var cityItem = cityList.GetItemById(lookupField.LookupId);

            var oldCourierFIO = listItem["Title"];
            var oldCourierPhone = listItem["CourierPhone"];

            var courierFIO = properties.AfterProperties["Title"];
            var courierPhone = properties.AfterProperties["CourierPhone"];
            var courierCity = cityItem["Title"];

            SqlCommand sqlCommand;
            try
            {
                sqlConnection.Open();
                sqlCommand = new SqlCommand("Update Couriers Set Courier_FIO='" + courierFIO + "', Courier_Phone='" + courierPhone + "', Courier_City= (select Cities.City_ID from Cities where convert(varchar, Cities.City_Title) = '" + courierCity + "') where convert(varchar, Courier_FIO)='" + oldCourierFIO + "' and convert(varchar, [Courier_Phone])='" + oldCourierPhone + "'", sqlConnection);
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
            var courierFIO = listItem["Title"];
            var courierPhone = listItem["CourierPhone"];
            SqlCommand sqlCommand;
            try
            {
                sqlConnection.Open();
                sqlCommand = new SqlCommand("Delete From Couriers where convert(varchar, Courier_FIO)='" + courierFIO + "' and convert(varchar, [Courier_Phone])='" + courierPhone + "'", sqlConnection);
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