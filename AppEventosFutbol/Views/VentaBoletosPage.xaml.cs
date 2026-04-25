using AppEventosFutbol.Controllers;
using AppEventosFutbol.Models;

namespace AppEventosFutbol.Views;

public partial class VentaBoletosPage : ContentPage
{
    private VentaBoletosController _controller;
    private List<Evento> _listaEventos;
    private Evento _eventoSeleccionado;

    public VentaBoletosPage()
	{
		InitializeComponent();
        _controller = new VentaBoletosController();
        CargarEventos();
    }

    // Cada vez que la pantalla aparece, recargamos (útil para cuando vengamos de registrar un evento nuevo)
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        // Llamamos al método asíncrono y esperamos a que termine
        await CargarEventos();
    }

    private async Task CargarEventos()
    {
        try
        {
            // Usamos await para esperar la respuesta de Supabase
            _listaEventos = await _controller.ObtenerEventosAsync();

            pickerEvento.Items.Clear();

            // Verificamos que la lista no sea nula antes de iterar
            if (_listaEventos != null && _listaEventos.Count > 0)
            {
                foreach (var evt in _listaEventos)
                {
                    // Mostramos el partido y el estadio en el selector
                    pickerEvento.Items.Add($"{evt.EquipoLocal} vs {evt.EquipoVisitante} - {evt.EstadioNombre}");
                }
            }
        }
        catch (Exception ex)
        {
            await DisplayAlert("Error", $"No se pudieron cargar los eventos: {ex.Message}", "OK");
        }
    }


    private void OnEventoSeleccionado(object sender, EventArgs e)
    {
        if (pickerEvento.SelectedIndex == -1) return;

        // 1. Obtenemos el evento seleccionado
        _eventoSeleccionado = _listaEventos[pickerEvento.SelectedIndex];

        // 2. Llenamos los datos en la tarjeta
        lblEquipos.Text = $"{_eventoSeleccionado.EquipoLocal} vs {_eventoSeleccionado.EquipoVisitante}";
        lblFecha.Text = _eventoSeleccionado.FechaHora.ToString("dd MMMM yyyy - HH:mm");
        lblDisponibles.Text = _eventoSeleccionado.NumeroBoletos.ToString();
        lblPrecio.Text = $"${_eventoSeleccionado.Precio:N2}";

        // Mostrar Sede
        lblSede.Text = $"📍 {_eventoSeleccionado.EstadioNombre}";

        // Mostrar Totales
        lblBoletosTotales.Text = _eventoSeleccionado.BoletosTotales.ToString("N0"); // Formato con comas

        // Calcular Ventas (Boletos Totales menos los Disponibles que quedan, multiplicado por el precio)
        int boletosVendidos = _eventoSeleccionado.BoletosTotales - _eventoSeleccionado.NumeroBoletos;
        decimal ingresosTotales = boletosVendidos * _eventoSeleccionado.Precio;
        lblTotalVentas.Text = $"${ingresosTotales:N2}";

        // Limpiar el Entry de Total a Pagar al cambiar de evento
        txtTotalPagar.Text = "$0.00";

        // 3. Evaluamos la regla de los 3 días usando el Controlador
        var evaluacion = _controller.EvaluarDisponibilidad(_eventoSeleccionado);

        // Actualizamos la etiqueta de estatus visualmente
        lblEstatus.Text = evaluacion.MensajeEstatus;
        badgeEstatus.BackgroundColor = Color.FromArgb(evaluacion.ColorEstatus == "#006847" ? "#006847" :
                                                      evaluacion.ColorEstatus == "#CE1126" ? "#CE1126" : "#555555");

        // 4. LA MAGIA: Ocultamos o mostramos el formulario de venta según la evaluación
        tarjetaInfo.IsVisible = true;
        panelVenta.IsVisible = evaluacion.PermiteVenta;

        // Limpiamos los campos por si había texto de otro evento
        txtCliente.Text = "";
        txtTelefono.Text = "";
        txtCantidad.Text = "";
    }

    private async void OnComprarClicked(object sender, EventArgs e)
    {
        if (_eventoSeleccionado == null) return;

        // Mandamos los datos al controlador para que haga las validaciones matemáticas
        var resultado = await _controller.ProcesarCompraAsync(_eventoSeleccionado, txtCliente.Text, txtTelefono.Text, txtCantidad.Text);

        if (!resultado.Exito)
        {
            await DisplayAlert("Error", resultado.Mensaje, "Entendido");
        }
        else
        {
            await DisplayAlert("Éxito", resultado.Mensaje, "Aceptar");

            // Actualizamos la etiqueta de boletos disponibles visualmente
            lblDisponibles.Text = _eventoSeleccionado.NumeroBoletos.ToString();

            // Limpiamos los campos
            txtCliente.Text = "";
            txtTelefono.Text = "";
            txtCantidad.Text = "";
        }
    }

    // Este evento se dispara cada vez que el usuario escribe un número en "Cantidad"
    private void OnCantidadChanged(object sender, TextChangedEventArgs e)
    {
        if (_eventoSeleccionado == null) return;

        // Intentamos convertir lo que escribió a un número entero
        if (int.TryParse(e.NewTextValue, out int cantidadDeseada) && cantidadDeseada > 0)
        {
            // Calculamos el precio total
            decimal totalCalculado = cantidadDeseada * _eventoSeleccionado.Precio;
            txtTotalPagar.Text = $"${totalCalculado:N2}";
        }
        else
        {
            // Si borra el texto o pone letras, regresamos a cero
            txtTotalPagar.Text = "$0.00";
        }
    }
}