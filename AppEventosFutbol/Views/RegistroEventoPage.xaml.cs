using AppEventosFutbol.Controllers;
using AppEventosFutbol.Models;

namespace AppEventosFutbol.Views;

public partial class RegistroEventoPage : ContentPage
{
    private RegistroEventoController _controller;
    private List<Estadio> _listaEstadios;
    private List<Equipo> _listaEquipos;
    private bool _actualizandoPickers = false;

    public RegistroEventoPage()
	{
		InitializeComponent();
        _controller = new RegistroEventoController();
        _listaEstadios = new List<Estadio>();
        _listaEquipos = new List<Equipo>();

        // Configurar la hora y fecha inicial a la actual
        tpHora.Time = DateTime.Now.TimeOfDay;
        dpFecha.Date = DateTime.Now.Date;
    }

    // EL TRUCO ESTRELLA: OnAppearing se ejecuta cada vez que la pantalla se vuelve visible.
    // Así, si vas al mapa a agregar un estadio y regresas, el Picker se actualiza solo.
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await CargarEstadiosAsync();
        await CargarEquiposAsync();
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
            // Abrimos el mapa de agregar estadio y recargamos al cerrar
            var modalPage = new AgregarEstadioPage();
            modalPage.Disappearing += async (s, args) => 
            {
                await CargarEstadiosAsync();
                if (!string.IsNullOrEmpty(modalPage.NombreNuevoEstadio))
                {
                    pickerEstadio.SelectedItem = modalPage.NombreNuevoEstadio;
                }
            };
            await Navigation.PushModalAsync(modalPage);
        }
    }

    private async Task CargarEquiposAsync()
    {
        _listaEquipos = await _controller.ObtenerEquiposAsync();
        ActualizarPickersEquipos();
    }

    private void ActualizarPickersEquipos()
    {
        // Si ya estamos actualizando, no hacemos nada para romper el bucle
        if (_actualizandoPickers) return;

        // Ponemos el candado
        _actualizandoPickers = true;

        try
        {
            string localSeleccionado = pickerEquipoLocal.SelectedItem?.ToString();
            string visitanteSeleccionado = pickerEquipoVisitante.SelectedItem?.ToString();

            pickerEquipoLocal.Items.Clear();
            pickerEquipoVisitante.Items.Clear();

            foreach (var equipo in _listaEquipos)
            {
                if (equipo.Nombre != visitanteSeleccionado)
                    pickerEquipoLocal.Items.Add(equipo.Nombre);

                if (equipo.Nombre != localSeleccionado)
                    pickerEquipoVisitante.Items.Add(equipo.Nombre);
            }

            pickerEquipoLocal.Items.Add("➕ Agregar nuevo equipo...");
            pickerEquipoVisitante.Items.Add("➕ Agregar nuevo equipo...");

            // Restaurar selecciones (Esto es lo que causaba el bucle)
            if (!string.IsNullOrEmpty(localSeleccionado) && pickerEquipoLocal.Items.Contains(localSeleccionado))
                pickerEquipoLocal.SelectedItem = localSeleccionado;

            if (!string.IsNullOrEmpty(visitanteSeleccionado) && pickerEquipoVisitante.Items.Contains(visitanteSeleccionado))
                pickerEquipoVisitante.SelectedItem = visitanteSeleccionado;
        }
        finally
        {
            // Quitamos el candado SIEMPRE, incluso si hay un error
            _actualizandoPickers = false;
        }
    }

    private async void OnEquipoLocalSeleccionado(object sender, EventArgs e)
    {
        // Agregamos la validación del candado aquí
        if (_actualizandoPickers || pickerEquipoLocal.SelectedIndex == -1) return;

        string seleccion = pickerEquipoLocal.SelectedItem.ToString();
        if (seleccion == "➕ Agregar nuevo equipo...")
        {
            pickerEquipoLocal.SelectedIndex = -1;
            var modalPage = new AgregarEquipoPage();
            modalPage.Disappearing += async (s, args) => 
            {
                await CargarEquiposAsync();
                if (!string.IsNullOrEmpty(modalPage.NombreNuevoEquipo))
                {
                    pickerEquipoLocal.SelectedItem = modalPage.NombreNuevoEquipo;
                }
            };
            await Navigation.PushModalAsync(modalPage);
        }
        else
        {
            await Task.Delay(100);
            ActualizarPickersEquipos();
        }
    }

    private async void OnEquipoVisitanteSeleccionado(object sender, EventArgs e)
    {
        // Agregamos la validación del candado aquí
        if (_actualizandoPickers || pickerEquipoVisitante.SelectedIndex == -1) return;

        string seleccion = pickerEquipoVisitante.SelectedItem.ToString();
        if (seleccion == "➕ Agregar nuevo equipo...")
        {
            pickerEquipoVisitante.SelectedIndex = -1;
            var modalPage = new AgregarEquipoPage();
            modalPage.Disappearing += async (s, args) => 
            {
                await CargarEquiposAsync();
                if (!string.IsNullOrEmpty(modalPage.NombreNuevoEquipo))
                {
                    pickerEquipoVisitante.SelectedItem = modalPage.NombreNuevoEquipo;
                }
            };
            await Navigation.PushModalAsync(modalPage);
        }
        else
        {
            await Task.Delay(100);
            ActualizarPickersEquipos();
        }
    }

    private async void OnGuardarClicked(object sender, EventArgs e)
    {
        Estadio estadioSeleccionado = null;
        if (pickerEstadio.SelectedIndex >= 0 && pickerEstadio.SelectedItem.ToString() != "➕ Agregar nuevo estadio...")
        {
            // Buscar por nombre ya que la lista de estadios tiene el mismo orden inicialmente, pero mejor buscar por nombre.
            estadioSeleccionado = _listaEstadios.FirstOrDefault(e => e.Nombre == pickerEstadio.SelectedItem.ToString());
        }

        string equipoLocal = pickerEquipoLocal.SelectedItem?.ToString();
        string equipoVisitante = pickerEquipoVisitante.SelectedItem?.ToString();

        // Llamamos al nuevo método asíncrono
        var resultado = await _controller.ValidarYGuardarEventoAsync(
            equipoLocal,
            equipoVisitante,
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
            pickerEquipoLocal.SelectedIndex = -1;
            pickerEquipoVisitante.SelectedIndex = -1;
            txtBoletos.Text = "";
            txtPrecio.Text = "";
            pickerEstadio.SelectedIndex = -1;
            ActualizarPickersEquipos();
        }
    }
}