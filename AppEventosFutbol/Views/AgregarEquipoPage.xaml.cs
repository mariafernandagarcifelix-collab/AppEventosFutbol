using AppEventosFutbol.Controllers;
using System;

namespace AppEventosFutbol.Views;

public partial class AgregarEquipoPage : ContentPage
{
    private AgregarEquipoController _controller;

    public string NombreNuevoEquipo { get; private set; }

    public AgregarEquipoPage()
    {
        InitializeComponent();
        _controller = new AgregarEquipoController();
    }

    private async void OnGuardarClicked(object sender, EventArgs e)
    {
        var resultado = await _controller.GuardarEquipoAsync(txtNombreEquipo.Text);

        if (resultado.Exito)
        {
            await DisplayAlert("Éxito", resultado.Mensaje, "OK");
            NombreNuevoEquipo = txtNombreEquipo.Text;
            await Navigation.PopModalAsync();
        }
        else
        {
            await DisplayAlert("Error", resultado.Mensaje, "OK");
        }
    }

    private async void OnCancelarClicked(object sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}
