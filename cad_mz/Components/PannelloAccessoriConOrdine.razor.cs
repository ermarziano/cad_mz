using Microsoft.AspNetCore.Components;
using static cad_mz.Logic.Entities.Accessorio;
using static cad_mz.Logic.Entities;

namespace cad_mz.Components
{
    public partial class PannelloAccessoriConOrdine
    {
        [CascadingParameter] Selection UserSelection { get; set; } = new()!;
        [CascadingParameter] SQLData UserData { get; set; } = new()!;
        [Parameter] public TipoAccessorio Tipo { get; set; }
        private String Titolo = String.Empty, sortableID = String.Empty, styleinfo = "padding: 4px";
        private List<Accessorio> ListaInterna = new();
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        protected override void OnParametersSet()
        {
            base.OnParametersSet();
            switch (Tipo)
            {
                case TipoAccessorio.Aspirazione:
                    Titolo = "Accessori in Aspirazione";
                    sortableID = "sortAspirazione";
                    styleinfo = "padding: 4px";
                    break;
                case TipoAccessorio.SilAspirazione:
                    Titolo = $"Accessori per {UserSelection.SilenziatoreAspirazione.Codice} con tramoggia associata";
                    sortableID = "sortAspirazioneSil";
                    styleinfo = "padding: 4px"; //; margin-left:60px";
                    break;
                case TipoAccessorio.Mandata:
                    Titolo = "Accessori in Mandata";
                    sortableID = "sortMandata";
                    styleinfo = "padding: 4px";
                    break;
                case TipoAccessorio.SilMandata:
                    Titolo = $"Accessori per {UserSelection.SilenziatoreMandata.Codice} con tramoggia associata";
                    sortableID = "sortMandataSil";
                    styleinfo = "padding: 4px"; //margin-left:60px";
                    break;
            }
            StateHasChanged();
        }
        public void Refresh()
        {
            StateHasChanged();
        }
        protected void ContaECreaLista(Accessorio acc, bool info, int qty)
        {
            var accessoriDelTipo = ListaInterna.Where(x => x.Code.Equals(acc.Code));
            if (accessoriDelTipo.Count() > qty)
                for (int i = 0; i < accessoriDelTipo.Count() - qty; i++) // se ne ho troppi, rimuovo la differenza, partendo da quello con l'indice più alto {
                {
                    var ultimoDelTipo = ListaInterna.Where(x => x.Code.Equals(acc.Code)).OrderByDescending(x => x.Id).First();
                    ListaInterna.Remove(ultimoDelTipo);
                }
            else if (accessoriDelTipo.Count() < qty) // se ne ho pochi, aggiungi la differenza
            {
                for (int i = 1; i <= qty - accessoriDelTipo.Count(); i++)
                {
                    Accessorio newAcc = acc.Clone();
                    newAcc.Id = ListaInterna.Where(x => x.Code.Equals(acc.Code)).Count() + i;
                    ListaInterna.Add(newAcc);
                }
            }
        }
        protected void ConfermaAccessori()
        {
            switch (Tipo)
            {
                case TipoAccessorio.Aspirazione:
                    UserSelection.AccessoriAspirazione = new List<Accessorio>(ListaInterna);
                    break;
                case TipoAccessorio.SilAspirazione:
                    UserSelection.AccessoriAspirazioneSil = new List<Accessorio>(ListaInterna);
                    break;
                case TipoAccessorio.Mandata:
                    UserSelection.AccessoriMandata = new List<Accessorio>(ListaInterna);
                    break;
                case TipoAccessorio.SilMandata:
                    UserSelection.AccessoriMandataSil = new List<Accessorio>(ListaInterna);
                    break;
            }
        }

        private void SortList((int oldIndex, int newIndex) indices)
        {
            var (oldIndex, newIndex) = indices;

            var items = ListaInterna;
            var itemToMove = items[oldIndex];
            items.RemoveAt(oldIndex);
            if (newIndex < items.Count)
            {
                items.Insert(newIndex, itemToMove);
            }
            else
            {
                items.Add(itemToMove);
            }
            StateHasChanged();
        }
    }
}
