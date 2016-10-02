using System.ComponentModel;
using System.Web.UI.WebControls.WebParts;
using DevExpress.XtraCharts;
using DevExpress.XtraCharts.Web;

namespace MyNetShopProject.SalesCountWebPart
{
    [ToolboxItemAttribute(false)]
    public class SalesCountWebPart : WebPart
    {
        protected override void CreateChildControls()
        {
            var chart = new WebChartControl();
            var series1 = new Series("My Series", ViewType.Line);

            series1.Points.Add(new SeriesPoint("A", 2));
            series1.Points.Add(new SeriesPoint("B", 7));
            series1.Points.Add(new SeriesPoint("C", 5));
            series1.Points.Add(new SeriesPoint("D", 9));

            chart.Series.Add(series1);

            var series2 = new Series("My Series2", ViewType.Line);
            series2.Points.Add(new SeriesPoint("A", 9));
            series2.Points.Add(new SeriesPoint("B", 5));
            series2.Points.Add(new SeriesPoint("C", 7));
            series2.Points.Add(new SeriesPoint("D", 2));

            chart.Series.Add(series2);

            Controls.Add(chart);
        }
    }
}
