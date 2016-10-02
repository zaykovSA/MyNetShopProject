using System;
using System.Data.SqlClient;
using System.Security.Permissions;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.Workflow;

namespace MyNetShopProject.Recievers.CitiesReciever
{
    /// <summary>
    /// События элемента списка
    /// </summary>
    public class CitiesReciever : SPItemEventReceiver
    {
        SqlConnection sqlConnection = new SqlConnection("Data Source=SP13;Persist Security Info=True;User ID=sa;Password=8*gUe411kE;Initial Catalog=SPCustomData");
        /// <summary>
        /// Добавляется элемент.
        /// </summary>
        public override void ItemAdded(SPItemEventProperties properties)
        {
            var listItem = properties.ListItem;
            var cityTitle = listItem["Title"];
            var cityPhoneCode = listItem["Phone_Code"];
            SqlCommand sqlCommand;
            try
            {
                sqlConnection.Open();
                sqlCommand = new SqlCommand("Insert into Cities (City_Title,City_PhoneCode) values('" + cityTitle + "','" + cityPhoneCode.ToString() + "')", sqlConnection);
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
            var oldCityTitle = listItem["Title"];
            var oldCityPhoneCode = listItem["Phone_Code"];
            var cityTitle = properties.AfterProperties["Title"];
            var cityPhone = properties.AfterProperties["Phone_Code"];
            SqlCommand sqlCommand;
            try
            {
                sqlConnection.Open();
                sqlCommand = new SqlCommand("Update Cities Set City_Title='" + cityTitle + "',City_PhoneCode='" + cityPhone.ToString() + "' where convert(varchar, City_Title)='" + oldCityTitle + "' and convert(varchar, [City_PhoneCode])='" + oldCityPhoneCode + "'", sqlConnection);
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
            var cityTitle = listItem["Title"];
            var cityPhoneCode = listItem["Phone_Code"];
            SqlCommand sqlCommand;
            try
            {
                sqlConnection.Open();
                sqlCommand = new SqlCommand("Delete From Cities where convert(varchar, City_Title)='" + cityTitle + "' and convert(varchar, [City_PhoneCode])='" + cityPhoneCode + "'", sqlConnection);
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