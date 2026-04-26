using AppEventosFutbol.Controllers;
using AppEventosFutbol.Models;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using System.Text.Json;

namespace AppEventosFutbol.Views;

public partial class AgregarEstadioPage : ContentPage
{
    private AgregarEstadioController _controller;
    private double? _latitudSeleccionada;
    private double? _longitudSeleccionada;
    public string NombreNuevoEstadio { get; private set; }

    private HttpClient _httpClient;
    private CancellationTokenSource _cts;

    public class LugarSugerencia
    {
        public string Nombre { get; set; }
        public double Latitud { get; set; }
        public double Longitud { get; set; }
    }

    public AgregarEstadioPage()
	{
		InitializeComponent();
        _controller = new AgregarEstadioController();
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "AppEventosFutbol/1.0");

        // Mover la cámara a México por defecto al abrir
        Location ubicacionMexico = new Location(23.6345, -102.5528);
        MapSpan zoom = MapSpan.FromCenterAndRadius(ubicacionMexico, Distance.FromKilometers(1500));
        mapaEstadio.MoveToRegion(zoom);
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CargarEstadiosExistentesAsync();
    }

    private async Task CargarEstadiosExistentesAsync()
    {
        try
        {
            var estadiosCargados = await new RegistroEventoController().ObtenerEstadiosAsync();
            foreach (var estadio in estadiosCargados)
            {
                var pin = new Pin
                {
                    Label = estadio.Nombre,
                    Type = PinType.SavedPin,
                    Location = new Location(estadio.Latitud, estadio.Longitud)
                };
                mapaEstadio.Pins.Add(pin);
            }
        }
        catch { }
    }

    // Evento que se dispara al tocar el mapa
    private void OnMapClicked(object sender, MapClickedEventArgs e)
    {
        // 1. Guardamos las coordenadas
        _latitudSeleccionada = e.Location.Latitude;
        _longitudSeleccionada = e.Location.Longitude;

        // 2. Limpiamos solo el pin de nuevo estadio
        var pinAnterior = mapaEstadio.Pins.FirstOrDefault(p => p.Label == "Nuevo Estadio");
        if (pinAnterior != null)
        {
            mapaEstadio.Pins.Remove(pinAnterior);
        }

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
            
            NombreNuevoEstadio = txtNombreEstadio.Text;

            // Regresamos a la pantalla anterior (Registro de Eventos)
            await Navigation.PopModalAsync();
        }
    }

    private async void OnCancelarClicked(object sender, EventArgs e)
    {
        // Cerramos la pantalla sin hacer nada
        await Navigation.PopModalAsync();
    }

    private async void OnBuscarUbicacion(object sender, EventArgs e)
    {
        string query = searchBarUbicacion.Text;
        if (string.IsNullOrWhiteSpace(query)) return;

        gridCargando.IsVisible = true;
        try
        {
            var locations = await Geocoding.Default.GetLocationsAsync(query);
            var location = locations?.FirstOrDefault();

            if (location != null)
            {
                mapaEstadio.MoveToRegion(MapSpan.FromCenterAndRadius(location, Distance.FromKilometers(2)));
                
                // Poner el pin temporal de nuevo estadio ahí mismo
                OnMapClicked(this, new MapClickedEventArgs(location));
            }
            else
            {
                await DisplayAlert("Sin resultados", "No se encontró la ubicación.", "OK");
            }
        }
        catch (Exception)
        {
            await DisplayAlert("Error", "Problema al buscar la ubicación. Verifica tu conexión a internet.", "OK");
        }
        finally
        {
            gridCargando.IsVisible = false;
        }
    }

    private async void OnSearchBarTextChanged(object sender, TextChangedEventArgs e)
    {
        string query = e.NewTextValue?.Trim();
        if (string.IsNullOrWhiteSpace(query) || query.Length < 3)
        {
            borderSugerencias.IsVisible = false;
            return;
        }

        _cts?.Cancel();
        _cts = new CancellationTokenSource();
        var token = _cts.Token;

        try
        {
            await Task.Delay(500, token); // Debounce de 500ms

            string url = $"https://nominatim.openstreetmap.org/search?format=json&q={Uri.EscapeDataString(query)}&countrycodes=mx&limit=5";

            var response = await _httpClient.GetStringAsync(url, token);
            
            using var doc = JsonDocument.Parse(response);
            var root = doc.RootElement;
            
            var sugerencias = new List<LugarSugerencia>();
            
            foreach (var item in root.EnumerateArray())
            {
                sugerencias.Add(new LugarSugerencia
                {
                    Nombre = item.GetProperty("display_name").GetString(),
                    Latitud = double.Parse(item.GetProperty("lat").GetString(), System.Globalization.CultureInfo.InvariantCulture),
                    Longitud = double.Parse(item.GetProperty("lon").GetString(), System.Globalization.CultureInfo.InvariantCulture)
                });
            }

            if (sugerencias.Any())
            {
                cvSugerencias.ItemsSource = sugerencias;
                borderSugerencias.IsVisible = true;
            }
            else
            {
                borderSugerencias.IsVisible = false;
            }
        }
        catch (TaskCanceledException)
        {
            // Ignorado, el usuario siguió escribiendo
        }
        catch
        {
            // Error de red
        }
    }

    private void OnSugerenciaSeleccionada(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is LugarSugerencia sugerencia)
        {
            var shortName = sugerencia.Nombre.Split(',')[0];
            searchBarUbicacion.Text = shortName;
            borderSugerencias.IsVisible = false;
            cvSugerencias.SelectedItem = null;
            
            // Centrar mapa
            var loc = new Location(sugerencia.Latitud, sugerencia.Longitud);
            mapaEstadio.MoveToRegion(MapSpan.FromCenterAndRadius(loc, Distance.FromKilometers(2)));

            // Poner el pin automáticamente
            OnMapClicked(this, new MapClickedEventArgs(loc));
        }
    }
}