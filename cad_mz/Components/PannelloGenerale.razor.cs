using cad_mz.Logic;
using Microsoft.AspNetCore.Components;
using MudBlazor;
using Newtonsoft.Json.Linq;
using static cad_mz.Logic.Entities;

namespace cad_mz.Components
{
    public partial class PannelloGenerale
    {
        [CascadingParameter] MyCascadingValues Values { get; set; } = new()!;
        public Serie defaultSerie = null;
        public Ventilatore defaultVentilatore = null;
        public bool VisualizzaPannelloMotore = false;
        public MudSelect<Ventilatore> ms;
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        protected override void OnInitialized()
        {
            base.OnInitialized();
            if (Values.UserData.Rotazioni.Count > 0)
            {
                Values.UserSelection.Esecuzione = Values.UserData.Esecuzioni.Where(x => x.Value == TipoTrasmissione.DirettamenteAccoppiato).First();
                Values.UserSelection.Rotazione = Values.UserData.Rotazioni.First();
            }
        }
        protected void LoadEsecuzioni(TipoTrasmissione Trasmissione)
        {
            Values.UserSelection.TipoTrasm = Trasmissione;
            Values.UserSelection.Esecuzione = Values.UserData.Esecuzioni.Find((x => x.Value == Values.UserSelection.TipoTrasm));
            StateHasChanged();
        }
        protected void CaricaSerie(Serie serie)
        {
            Values.UserSelection.SelectedSerie = serie;
            defaultSerie = serie;
            Values.UserSelection.SelectedFan = serie.DimensionalDatas.First();
            StateHasChanged();
        }
        protected void LoadMotori(KeyValuePair<String, TipoTrasmissione> Esecuzione)
        {
            Values.UserSelection.Esecuzione = (KeyValuePair<string, TipoTrasmissione>)Esecuzione;
            if (Values.UserSelection.Esecuzione.Value == TipoTrasmissione.DirettamenteAccoppiato)
            {
                var TagliePossibili = Values.UserData.CodiciSedie.Where(x => x.Item1.StartsWith(Values.UserSelection.SelectedFan.DatiChiocciola.ChairE04)).Select(x => x.Item2).Distinct(); //cerca le taglie possibili dall'elenco delle sedie
                Values.UserSelection.MotoriFiltrati = Values.UserData.Motori.Where(x => TagliePossibili.Contains(x.Taglia)).ToList();
            }
        }
        protected void SelectVentilatore(Ventilatore Fan)
        {
            if (Fan != null)
            {
                defaultVentilatore = Fan;
                Values.UserSelection.SelectedFan = Fan;
                Values.ValuesChanged.Invoke();
                string inizioDescrizione = $"{Values.UserSelection.SelectedFan.MotorSize} {Values.UserSelection.SelectedFan.Kw}";
                Values.UserSelection.SelectedFan.MotoreSelezionato = Values.UserData.Motori.Find(x => x.Descrizione.StartsWith(inizioDescrizione));
                VisualizzaPannelloMotore = true;
                StateHasChanged();
            }
        }
        protected async Task ScegliMotore()
        {
            var parameters = new DialogParameters<DialogMotore>();
            DialogOptions maxWidth = new DialogOptions() { MaxWidth = MaxWidth.Small, FullWidth = true };
            parameters.Add(x => x.selection, Values.UserSelection);
            parameters.Add(x => x.data, Values.UserData);
            var dialog = await DialogService.ShowAsync<DialogMotore>("Scegli Motore", parameters, maxWidth);
            var result = await dialog.Result;
            StateHasChanged();
        }
    }
}
