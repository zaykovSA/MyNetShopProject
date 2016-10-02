using Microsoft.SharePoint;
using Microsoft.SharePoint.WebControls;

namespace MyNetShopProject
{
    public class OrderField : SPFieldText
    {
        public OrderField(SPFieldCollection fields, string name)
            : base(fields, name)
        {

        }

        public OrderField(SPFieldCollection fields, string typename, string name)
            : base(fields, typename, name)
        {

        }

        public override BaseFieldControl FieldRenderingControl
        {
            get
            {
                BaseFieldControl fieldControl = new OrderFieldControl();

                fieldControl.FieldName = InternalName;

                return fieldControl;
            }
        }
    }
}
