using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Supabase;

namespace AppEventosFutbol.Services
{
    public class SupabaseConfig
    {
        public static Client Cliente { get; private set; }

        public static async Task Inicializar()
        {
            if (Cliente == null)
            {
                var options = new SupabaseOptions { AutoConnectRealtime = true };
                Cliente = new Client(Secretos.SupabaseUrl, Secretos.SupabaseKey, options);
                await Cliente.InitializeAsync();
            }
        }
    }
}
