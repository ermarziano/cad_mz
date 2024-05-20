using Microsoft.AspNetCore.Components;
using static cad_mz.Logic.Entities;

namespace cad_mz.Components
{
    public partial class SelettoreAccessorio
    {
        [Parameter] public Accessorio Acc { get; set; } = null!;
        [Parameter] public TipoSelettore Ts { get; set; }
        [Parameter] public EventCallback<(bool, int)> reload { get; set; }

        protected async void AggiornaBool(bool newValue)
        {
            Acc.Selected = newValue;
            await reload.InvokeAsync((newValue, 1));
        }
        protected async void AggiornaInt(int newValue)
        {
            Acc.Qty = newValue;
            bool selected = newValue > 0;
            await reload.InvokeAsync((selected, newValue));
        }
        public enum TipoSelettore
        {
            Check,
            Numeric
        }
    }
}