using Microsoft.Extensions.DependencyInjection;
using AppEventosFutbol.Services;

namespace AppEventosFutbol
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
            _ = SupabaseConfig.Inicializar();
            MainPage = new Views.MainTabbedPage();
        }

        protected override Window CreateWindow(IActivationState? activationState)
        {
            return new Window(MainPage);
        }
    }
}