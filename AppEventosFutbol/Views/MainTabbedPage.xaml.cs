using Microsoft.Maui.Controls.PlatformConfiguration.AndroidSpecific;
using TabbedPage = Microsoft.Maui.Controls.TabbedPage; // Para evitar conflictos de nombres

namespace AppEventosFutbol.Views;

public partial class MainTabbedPage : TabbedPage
{
    public MainTabbedPage()
    {
        InitializeComponent();
    }

    private void OnCurrentPageChanged(object sender, EventArgs e)
    {
        if (CurrentPage is MapaReportePage)
        {
            this.On<Microsoft.Maui.Controls.PlatformConfiguration.Android>().SetIsSwipePagingEnabled(false);
        }
        else
        {
            this.On<Microsoft.Maui.Controls.PlatformConfiguration.Android>().SetIsSwipePagingEnabled(true);
        }
    }
}