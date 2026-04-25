using AppEventosFutbol.Controllers;
using AppEventosFutbol.Models;

namespace AppEventosFutbol.Views;

public partial class RegistroEventoPage : ContentPage
{
    private RegistroEventoController _controller;
    private List<Estadio> _listaEstadios;

    public RegistroEventoPage()
	{
		InitializeComponent();
        _controller = new RegistroEventoController();
        _listaEstadios = new List<Estadio>();
    }

    // EL TRUCO ESTRELLA: OnAppearing se ejecuta cada vez que la pantalla se vuelve visible.
    // Así, si vas al mapa a agregar un estadio y regresas, el Picker se actualiza solo.
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CargarEstadiosAsync();
    }

    private async Task CargarEstadiosAsync()
    {
        // Descargamos de la nube
        _listaEstadios = await _controller.ObtenerEstadiosAsync();

        pickerEstadio.Items.Clear();

        foreach (var estadio in _listaEstadios)
        {
            pickerEstadio.Items.Add(estadio.Nombre);
        }

        pickerEstadio.Items.Add("➕ Agregar nuevo estadio...");
    }

    private async void OnEstadioSeleccionado(object sender, EventArgs e)
    {
        if (pickerEstadio.SelectedIndex == -1) return;

        string seleccion = pickerEstadio.SelectedItem.ToString();

        if (seleccion == "➕ Agregar nuevo estadio...")
        {
            pickerEstadio.SelectedIndex = -1;
            // Abrimos el mapa de agregar estadio
            await Navigation.PushModalAsync(new AgregarEstadioPage());
        }
    }

    private async void OnGuardarClicked(object sender, EventArgs e)
    {
        Estadio estadioSeleccionado = null;
        if (pickerEstadio.SelectedIndex >= 0 && pickerEstadio.SelectedIndex < _listaEstadios.Count)
        {
            estadioSeleccionado = _listaEstadios[pickerEstadio.SelectedIndex];
        }

        // Llamamos al nuevo método asíncrono
        var resultado = await _controller.ValidarYGuardarEventoAsync(
            txtEquipoLocal.Text,
            txtEquipoVisitante.Text,
            txtBoletos.Text,
            txtPrecio.Text,
            dpFecha.Date ?? DateTime.Now,
            tpHora.Time ?? TimeSpan.Zero,
            estadioSeleccionado
        );

        if (!resultado.EsValido)
        {
            await DisplayAlert("Error", resultado.MensajeError, "Entendido");
        }
        else
        {
            await DisplayAlert("Éxito", resultado.MensajeError, "OK");

            // Limpiar formulario tras guardar exitosamente
            txtEquipoLocal.Text = "";
            txtEquipoVisitante.Text = "";
            txtBoletos.Text = "";
            txtPrecio.Text = "";
            pickerEstadio.SelectedIndex = -1;
        }
    }
}