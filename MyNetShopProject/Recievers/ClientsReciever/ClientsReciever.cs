using System;
using System.Data.SqlClient;
using System.Security.Permissions;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.Workflow;

namespace MyNetShopProject.Recievers.ClientsReciever
{
    /// <summary>
    /// Поля: Title - ФИО, ClientPhone - Телефон Клиента
    /// </summary>
    public class ClientsReciever : SPItemEventReceiver
    {
        SqlConnection sqlConnection = new SqlConnection("Data Source=SP13;Persist Security Info=True;User ID=sa;Password=8*gUe411kE;Initial Catalog=SPCustomData");
        /// <summary>
        /// Добавляется элемент.
        /// </summary>
        public override void ItemAdded(SPItemEventProperties properties)
        {
            var listItem = properties.ListItem;
            var clientFIO = listItem["Title"];
            var clientPhone = listItem["ClientPhone"];
            var cityLookupField = new SPFieldLookupValue(listItem["ClientCity"].ToString());
            var clientCity = cityLookupField.LookupValue;
            SqlCommand sqlCommand;
            try
            {
                sqlConnection.Open();
                sqlCommand = new SqlCommand("Insert into Clients (Client_FIO, Client_Phone, Client_City) values('" + clientFIO + "','" + clientPhone + "', (select Cities.City_ID from Cities where convert(varchar, Cities.City_Title) = convert(varchar, '" + clientCity + "')))", sqlConnection);
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
            var oldClientFIO = listItem["Title"];
            var oldClientPhone = listItem["ClientPhone"];

            var clientFIO = properties.AfterProperties["Title"];
            var clientPhone = properties.AfterProperties["ClientPhone0"];
            var cityLookupField = new SPFieldLookupValue(properties.AfterProperties["ClientCity"].ToString());
            

            var citiesList = properties.Web.GetList("lists/cities");
            var cityItem = citiesList.GetItemById(cityLookupField.LookupId);

            var clientCity = cityItem.Title;

            SqlCommand sqlCommand;
            try
            {
                sqlConnection.Open();
                sqlCommand = new SqlCommand("Update Clients Set Client_FIO='" + clientFIO + "', Client_Phone='" + clientPhone + "', Client_City=(select Cities.City_ID from Cities where convert(varchar, Cities.City_Title) = convert(varchar, '" + clientCity + "')) where convert(varchar, Client_FIO)='" + oldClientFIO + "' and convert(varchar, Client_Phone)='" + oldClientPhone + "'", sqlConnection);
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
            var clientFIO = listItem["Title"];
            var clientPhone = listItem["ClientPhone"];
            SqlCommand sqlCommand;
            try
            {
                sqlConnection.Open();
                sqlCommand = new SqlCommand("Delete From Clients where convert(varchar, Client_FIO)='" + clientFIO + "' and convert(varchar, Client_Phone)='" + clientPhone + "'", sqlConnection);
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