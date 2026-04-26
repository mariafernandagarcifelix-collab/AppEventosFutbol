using AppEventosFutbol.Controllers;
using AppEventosFutbol.Models;
using Microsoft.Maui.Controls.Maps;
using Microsoft.Maui.Maps;
using System.Text.Json;
using System.Linq;

namespace AppEventosFutbol.Views;

public partial class MapaReportePage : ContentPage
{
    private ReportesController _controller;
    private List<Evento> _eventosProximos;
    // NUEVO: Diccionario para enlazar cada Pin visual con sus datos (Evento)
    private Dictionary<Pin, Evento> _diccionarioPines;
    private Evento _eventoSeleccionado;
    private List<Estadio> _estadiosCargados = new List<Estadio>();

    public MapaReportePage()
	{
		InitializeComponent();
        _controller = new ReportesController();
        _diccionarioPines = new Dictionary<Pin, Evento>(); // Inicializamos el diccionario
    }

    // Usamos OnAppearing para que el mapa se actualice siempre que cambiamos a esta pestaña
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        // Llamamos al método asíncrono (es buena práctica agregarle la palabra Async al final)
        await CargarPinesEnMapaAsync();
    }

    private async Task CargarPinesEnMapaAsync()
    {
        try
        {
            // 1. Limpiamos el mapa, elementos y el diccionario
            mapaReportes.Pins.Clear();
            mapaReportes.MapElements.Clear();
            _diccionarioPines.Clear();
            tarjetaEvento.IsVisible = false;
            carouselEstadios.Children.Clear();

            // 2. MAGIA: Usamos 'await' para traer los datos reales de Supabase
            _eventosProximos = await _controller.ObtenerEventosProximosMapaAsync();

            try {
                _estadiosCargados = await new RegistroEventoController().ObtenerEstadiosAsync();
            } catch { }

            // Verificamos que la lista no venga vacía o nula
            if (_eventosProximos != null && _eventosProximos.Count > 0)
            {
                // 3. Dibujamos los pines
                foreach (var evento in _eventosProximos)
                {
                    var pin = new Pin
                    {
                        Label = $"{evento.EquipoLocal} vs {evento.EquipoVisitante}",
                        Address = evento.EstadioNombre,
                        Type = PinType.Place,
                        Location = new Location(evento.Latitud, evento.Longitud)
                    };

                    pin.MarkerClicked += OnPinClicked;

                    mapaReportes.Pins.Add(pin);

                    // Guardamos la relación en el diccionario (Pin -> Evento)
                    _diccionarioPines.Add(pin, evento);

                    // Agregar botón al carrusel de favoritos
                    var btnChip = new Button
                    {
                        Text = $"🏟️ {evento.EstadioNombre}",
                        HeightRequest = 35,
                        Padding = new Thickness(12, 0),
                        FontSize = 12,
                        CornerRadius = 17,
                        BackgroundColor = Colors.White,
                        TextColor = Colors.Black,
                        FontAttributes = FontAttributes.Bold
                    };

                    btnChip.Clicked += (s, args) => 
                    {
                        // Mover cámara y mostrar tarjeta
                        mapaReportes.MoveToRegion(MapSpan.FromCenterAndRadius(pin.Location, Distance.FromKilometers(2)));
                        MostrarTarjetaEvento(evento);
                    };

                    carouselEstadios.Children.Add(btnChip);
                }
            }

            // 4. Centramos la cámara en México
            Location centroMexico = new Location(23.6345, -102.5528);
            mapaReportes.MoveToRegion(MapSpan.FromCenterAndRadius(centroMexico, Distance.FromKilometers(1200)));
        }
        catch (Exception ex)
        {
            // Protegemos la app: si se cae el internet, mostramos un mensaje en vez de que la app "crashee"
            await DisplayAlert("Error", $"No se pudieron cargar los pines en el mapa: {ex.Message}", "OK");
        }
    }

    private void OnPinClicked(object sender, PinClickedEventArgs e)
    {
        // Ocultamos el cuadro feo por defecto
        e.HideInfoWindow = true;

        if (sender is Pin pinTocado)
        {
            // CORRECCIÓN 4: Buscamos el evento usando el pin como "llave" del diccionario
            if (_diccionarioPines.TryGetValue(pinTocado, out Evento eventoSeleccionado))
            {
                MostrarTarjetaEvento(eventoSeleccionado);
            }
        }
    }

    private void MostrarTarjetaEvento(Evento eventoSeleccionado)
    {
        _eventoSeleccionado = eventoSeleccionado;

        // Llenamos nuestra tarjeta personalizada con los datos que recuperamos
        lblSede.Text = $"🏟️ {eventoSeleccionado.EstadioNombre}";
        lblEquipos.Text = $"{eventoSeleccionado.EquipoLocal} vs {eventoSeleccionado.EquipoVisitante}";
        lblFecha.Text = eventoSeleccionado.FechaHora.ToString("dd MMMM, HH:mm 'hrs'");
        lblBoletos.Text = (eventoSeleccionado.BoletosTotales - eventoSeleccionado.NumeroBoletos).ToString("N0");
        lblPrecio.Text = $"${eventoSeleccionado.Precio:N2}";

        // Mostramos la tarjeta con animación
        tarjetaEvento.Opacity = 0;
        tarjetaEvento.IsVisible = true;
        tarjetaEvento.FadeTo(1, 250);
    }

    private async void OnCerrarTarjetaClicked(object sender, EventArgs e)
    {
        // Ocultamos la tarjeta con animación
        await tarjetaEvento.FadeTo(0, 200);
        tarjetaEvento.IsVisible = false;
        _eventoSeleccionado = null;
    }

    private async void OnComoLlegarClicked(object sender, EventArgs e)
    {
        if (_eventoSeleccionado != null)
        {
            // Ocultamos la tarjeta actual y pedimos al usuario que seleccione el origen
            await tarjetaEvento.FadeTo(0, 200);
            tarjetaEvento.IsVisible = false;
            
            searchContainer.IsVisible = true;
            searchBarUbicacion.Text = "";
            searchBarUbicacion.Focus();
        }
    }

    private void OnMapClicked(object sender, MapClickedEventArgs e)
    {
        // Ya no usamos el toque en el mapa para marcar origen, usamos el buscador
    }

    private async Task TrazarRutaNativaAsync(Location origen, Location destino)
    {
        try
        {
            // Usamos OSRM público con HTTPS para evitar bloqueos de red en Android
            string url = $"https://router.project-osrm.org/route/v1/driving/{origen.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)},{origen.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)};{destino.Longitude.ToString(System.Globalization.CultureInfo.InvariantCulture)},{destino.Latitude.ToString(System.Globalization.CultureInfo.InvariantCulture)}?overview=full&geometries=geojson";

            using var httpClient = new HttpClient();
            var response = await httpClient.GetStringAsync(url);
            
            using JsonDocument doc = JsonDocument.Parse(response);
            var root = doc.RootElement;
            
            if (root.GetProperty("code").GetString() == "Ok")
            {
                var routes = root.GetProperty("routes");
                if (routes.GetArrayLength() > 0)
                {
                    var geometry = routes[0].GetProperty("geometry");
                    var coordinates = geometry.GetProperty("coordinates");
                    
                    // Extraer distancia y duración
                    double distanceMeters = routes[0].GetProperty("distance").GetDouble();
                    double durationSeconds = routes[0].GetProperty("duration").GetDouble();
                    
                    double distanceKm = Math.Round(distanceMeters / 1000.0, 1);
                    double durationMin = Math.Ceiling(durationSeconds / 60.0);

                    lblRutaDistancia.Text = $"{distanceKm:0.#} km";
                    if (durationMin >= 60)
                    {
                        int hours = (int)(durationMin / 60);
                        int mins = (int)(durationMin % 60);
                        lblRutaTiempo.Text = $"{hours} h {mins} min";
                    }
                    else
                    {
                        lblRutaTiempo.Text = $"{durationMin} min";
                    }

                    // Obtener nombre de la calle usando Geocoding
                    try
                    {
                        var placemarks = await Geocoding.Default.GetPlacemarksAsync(origen.Latitude, origen.Longitude);
                        var placemark = placemarks?.FirstOrDefault();
                        if (placemark != null)
                        {
                            string nombreLugar = placemark.Thoroughfare ?? placemark.FeatureName;
                            if (string.IsNullOrEmpty(nombreLugar))
                                nombreLugar = placemark.Locality;
                                
                            lblRutaOrigen.Text = string.IsNullOrEmpty(nombreLugar) ? "Ubicación seleccionada" : nombreLugar;
                        }
                        else
                        {
                            lblRutaOrigen.Text = "Ubicación seleccionada";
                        }
                    }
                    catch
                    {
                        lblRutaOrigen.Text = "Ubicación seleccionada";
                    }

                    // Mostrar tarjeta de info
                    tarjetaRutaInfo.IsVisible = true;
                    
                    var polyline = new Polyline
                    {
                        StrokeColor = Color.FromArgb("#FFD700"), // Color Dorado de la marca
                        StrokeWidth = 6
                    };

                    // Añadir el punto exacto de origen para que la línea lo toque
                    polyline.Geopath.Add(origen);

                    foreach (var coord in coordinates.EnumerateArray())
                    {
                        double lon = coord[0].GetDouble();
                        double lat = coord[1].GetDouble();
                        polyline.Geopath.Add(new Location(lat, lon));
                    }

                    // Añadir el punto exacto de destino para que la línea lo toque
                    polyline.Geopath.Add(destino);

                    // Limpiar rutas previas
                    mapaReportes.MapElements.Clear();
                    
                    // Añadir un Pin para el origen
                    var pinOrigen = new Pin 
                    { 
                        Label = "Tu Ubicación", 
                        Type = PinType.SearchResult, 
                        Location = origen 
                    };
                    mapaReportes.Pins.Add(pinOrigen);

                    // Añadir la ruta al mapa
                    mapaReportes.MapElements.Add(polyline);

                    // Centrar la cámara para ver toda la ruta
                    var centerLat = (origen.Latitude + destino.Latitude) / 2;
                    var centerLon = (origen.Longitude + destino.Longitude) / 2;
                    var distance = Location.CalculateDistance(origen, destino, DistanceUnits.Kilometers);
                    
                    mapaReportes.MoveToRegion(MapSpan.FromCenterAndRadius(new Location(centerLat, centerLon), Distance.FromKilometers(distance / 2 + 5)));
                }
            }
        }
        catch (Exception)
        {
            await DisplayAlert("Error", "No se pudo calcular la ruta. Verifica tu conexión a internet.", "OK");
        }
    }

    private void OnCerrarRutaClicked(object sender, EventArgs e)
    {
        // Ocultar tarjeta de ruta
        tarjetaRutaInfo.IsVisible = false;
        
        // Limpiar la línea de la ruta
        mapaReportes.MapElements.Clear();
        
        // Quitar el pin de origen
        var pinOrigen = mapaReportes.Pins.FirstOrDefault(p => p.Label == "Tu Ubicación");
        if (pinOrigen != null)
        {
            mapaReportes.Pins.Remove(pinOrigen);
        }
    }

    private async void OnBuscarUbicacion(object sender, EventArgs e)
    {
        string query = searchBarUbicacion.Text;
        if (string.IsNullOrWhiteSpace(query)) return;

        borderSugerencias.IsVisible = false;

        if (_eventoSeleccionado != null)
        {
            searchContainer.IsVisible = false;
            gridCargando.IsVisible = true;

            try
            {
                var locations = await Geocoding.Default.GetLocationsAsync(query);
                var location = locations?.FirstOrDefault();

                if (location != null)
                {
                    Location origen = location;
                    Location destino = new Location(_eventoSeleccionado.Latitud, _eventoSeleccionado.Longitud);
                    await TrazarRutaNativaAsync(origen, destino);
                }
                else
                {
                    await DisplayAlert("Sin resultados", "No se encontró la ubicación ingresada.", "OK");
                    searchContainer.IsVisible = true;
                }
            }
            catch (Exception)
            {
                await DisplayAlert("Error", "Problema al buscar la ubicación. Verifica tu conexión a internet.", "OK");
                searchContainer.IsVisible = true;
            }
            finally
            {
                gridCargando.IsVisible = false;
            }
        }
    }

    private void OnSearchBarTextChanged(object sender, TextChangedEventArgs e)
    {
        string query = e.NewTextValue?.ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(query))
        {
            borderSugerencias.IsVisible = false;
            return;
        }

        var sugerencias = _estadiosCargados.Where(x => x.Nombre.ToLowerInvariant().Contains(query)).ToList();
        
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

    private async void OnSugerenciaSeleccionada(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is Estadio estadioSeleccionado)
        {
            searchBarUbicacion.Text = estadioSeleccionado.Nombre;
            borderSugerencias.IsVisible = false;
            cvSugerencias.SelectedItem = null;
            
            if (_eventoSeleccionado != null)
            {
                searchContainer.IsVisible = false;
                gridCargando.IsVisible = true;

                Location origen = new Location(estadioSeleccionado.Latitud, estadioSeleccionado.Longitud);
                Location destino = new Location(_eventoSeleccionado.Latitud, _eventoSeleccionado.Longitud);
                await TrazarRutaNativaAsync(origen, destino);

                gridCargando.IsVisible = false;
            }
        }
    }
}