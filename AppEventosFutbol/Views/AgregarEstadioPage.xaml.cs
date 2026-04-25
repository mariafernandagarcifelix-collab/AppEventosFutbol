using AppEventosFutbol.Controllers;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;

namespace AppEventosFutbol.Views;

public partial class AgregarEstadioPage : ContentPage
{
    private AgregarEstadioController _controller;
    private double? _latitudSeleccionada;
    private double? _longitudSeleccionada;

    public AgregarEstadioPage()
	{
		InitializeComponent();
        _controller = new AgregarEstadioController();

        // Mover la cámara a México por defecto al abrir
        Location ubicacionMexico = new Location(23.6345, -102.5528);
        MapSpan zoom = MapSpan.FromCenterAndRadius(ubicacionMexico, Distance.FromKilometers(1500));
        mapaEstadio.MoveToRegion(zoom);
    }

    // Evento que se dispara al tocar el mapa
    private void OnMapClicked(object sender, MapClickedEventArgs e)
    {
        // 1. Guardamos las coordenadas
        _latitudSeleccionada = e.Location.Latitude;
        _longitudSeleccionada = e.Location.Longitude;

        // 2. Limpiamos pines anteriores
        mapaEstadio.Pins.Clear();

        // 3. Creamos y agregamos el nuevo Pin
        Pin pinEstadio = new Pin
        {
            Label = "Nuevo Estadio",
            Address = "Ubicación seleccionada",
            Type = PinType.Place,
            Location = e.Location
        };
        mapaEstadio.Pins.Add(pinEstadio);

        // 4. Actualizamos el texto en la pantalla
        lblCoordenadas.Text = $"Lat: {_latitudSeleccionada:F4}, Lon: {_longitudSeleccionada:F4}";
    }

    private async void OnGuardarClicked(object sender, EventArgs e)
    {
        // Pasamos los datos al controlador
        var resultado = await _controller.ValidarYGuardarEstadio(txtNombreEstadio.Text, _latitudSeleccionada, _longitudSeleccionada);

        if (!resultado.EsValido)
        {
            await DisplayAlert("Error", resultado.Mensaje, "OK");
        }
        else
        {
            await DisplayAlert("Éxito", resultado.Mensaje, "OK");

            // Regresamos a la pantalla anterior (Registro de Eventos)
            await Navigation.PopModalAsync();
        }
    }

    private async void OnCancelarClicked(object sender, EventArgs e)
    {
        // Cerramos la pantalla sin hacer nada
        await Navigation.PopModalAsync();
    }
}