using Microsoft.AspNetCore.Components;
using MudBlazor;
using static cad_mz.Logic.Entities;

namespace cad_mz.Components
{
    public partial class PannelloGenerale
    {
        [CascadingParameter] Selection UserSelection { get; set; } = new()!;
        [CascadingParameter] SQLData UserData { get; set; } = new()!;
        [Parameter] public EventCallback refreshAction { get; set; }
        private bool VisualizzaPannelloMotore = false;
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        protected void loadEsecuzioni(TipoTrasmissione Trasmissione)
        {
            UserSelection.TipoTrasm = Trasmissione;
            UserSelection.Esecuzione = UserData.Esecuzioni.Find((x => x.Value == UserSelection.TipoTrasm));
            StateHasChanged();
        }
        protected void loadVentilatori(Serie serie)
        {
            UserSelection.SelectedSerie = serie;
            UserSelection.SelectedFan = serie.DimensionalDatas.First();
            StateHasChanged();
        }
        protected void loadMotori(KeyValuePair<String, TipoTrasmissione> Esecuzione)
        {
            UserSelection.Esecuzione = Esecuzione;
            if (Esecuzione.Value == TipoTrasmissione.DirettamenteAccoppiato)
            {
                var TagliePossibili = UserData.CodiciSedie.Where(x => x.Item1.StartsWith(UserSelection.SelectedFan.Casing_data.ChairE04)).Select(x => x.Item2).Distinct(); //cerca le taglie possibili dall'elenco delle sedie
                UserSelection.MotoriFiltrati = UserData.Motori.Where(x => TagliePossibili.Contains(x.Taglia)).ToList();
            }
        }
        protected void selectVentilatore(Ventilatore Fan)
        {
            UserSelection.SelectedFan = Fan;
            string inizioDescrizione = $"{UserSelection.SelectedFan.MotorSize} {UserSelection.SelectedFan.Kw}";
            UserSelection.SelectedFan.MotoreSelezionato = UserData.Motori.Find(x => x.Descrizione.StartsWith(inizioDescrizione));
            VisualizzaPannelloMotore = true;
            refreshAction.InvokeAsync();
        }

        protected override void OnParametersSet()
        {
            base.OnInitialized();
            if (UserData.Serie.Count > 0)
            {
                UserSelection.SelectedSerie = UserData.Serie.First();
                UserSelection.SelectedFan = UserSelection.SelectedSerie.DimensionalDatas.First();
                UserSelection.Esecuzione = UserData.Esecuzioni.Find((x => x.Value == UserSelection.TipoTrasm));
                UserSelection.Rotazione = UserData.Rotazioni.First();
                string inizioDescrizione = $"{UserSelection.SelectedFan.MotorSize} {UserSelection.SelectedFan.Kw}";
                UserSelection.SelectedFan.MotoreSelezionato = UserData.Motori.Find(x => x.Descrizione.StartsWith(inizioDescrizione));
                VisualizzaPannelloMotore = true;
            }
            StateHasChanged();
        }
        protected async Task ScegliMotore()
        {
            var parameters = new DialogParameters<DialogMotore>();
            DialogOptions maxWidth = new DialogOptions() { MaxWidth = MaxWidth.Small, FullWidth = true };
            parameters.Add(x => x.selection, UserSelection);
            parameters.Add(x => x.data, UserData);
            var dialog = await DialogService.ShowAsync<DialogMotore>("Scegli Motore", parameters, maxWidth);
            var result = await dialog.Result;
            StateHasChanged();
        }
    }
}
