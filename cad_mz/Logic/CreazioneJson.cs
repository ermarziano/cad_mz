using static cad_mz.Logic.Entities;
using CADServer.Shared.Common.Models;
using System.Diagnostics;
using System.ComponentModel;

namespace cad_mz.Logic
{
    public class CreazioneJson
    {
        readonly Selection selection;
        bool haSupporto = true;
        string modelloBoccaAspirazione = "", connessioneSilenziatore = "";
        int numeroFori = -1;

        SQLData data;
        List<JobCommand> commands;
        public CreazioneJson(Selection _selection, SQLData _data)
        {
            selection = _selection;
            data = _data;
            commands = new List<JobCommand>();
        }
        public async Task Crea()
        {
            //Apri File
            commands.Add(new JobCommand()
            {
                Method = "Open",
                Params = new Dictionary<string, object>
                {
                    { "modelName", Resources.Resource.Percorso + selection.Esecuzione.Key + ".iam"  }
                },
                OperationId = 0
            });
            await GeneraVentilatore();

        }
        private async Task GeneraVentilatore()
        {
            switch (selection.Esecuzione.Key)
            {
                case "E04":
                    await ScriviSedia($"{selection.SelectedFan.Casing_data.ChairE04}{selection.GetH()}G{selection.SelectedFan.MotoreSelezionato.Taglia:000}");
                    await ScriviChiocciola(selection.SelectedFan.Casing_data.Code, selection.SelectedFan.Casing_data.Size, selection.SelectedFan.Casing_data.Outlet);
                    await ScriviSupporto();
                    await ScriviBoccaAspirazione(haSupporto? "Supporto.ipt:1" : "Chiocciola.ipt:1", haSupporto? "UCS2" : "UCS1");
                    //ScriviMotore();
                    break;
            }
        }
        private async Task ScriviBoccaAspirazione(string connessaA, string UCS)
        {
            List<KeyValuePair<string, double>> valori = await SQL.CaricaBoccaAspirazione(selection.SelectedFan.Casing_data.Mouth_Code);
            modelloBoccaAspirazione = valori.getValue("HLatoRettilineo") > 0 ? "BoccaASP2.ipt:1" : "BoccaASP1.ipt:1";
            numeroFori = valori.getValue("HLatoRettilineo") > 0 ? (int) valori.getValue("NumeroPrigionieri") : (int) valori.getValue("NumeroFori");
            AggiungiEAggancia(connessaA, modelloBoccaAspirazione, UCS, "UCSIN");
            ScriviParametri(modelloBoccaAspirazione, valori);
            connessioneSilenziatore = modelloBoccaAspirazione;
        }
        private async Task ScriviSupporto()
        {
            if (selection.AccessoriChiocciola.Exists(x => x.Code.Equals("SA") && x.Selected))
            {
                List<KeyValuePair<string, double>> valori = await SQL.CaricaSupporto(selection.SelectedFan.Casing_data.Support);
                ScriviParametri("Supporto.ipt:1", valori);
            }
            else
            {
                commands.Add(new JobCommand
                {
                    Method = "DeleteComponent",
                    Params = new Dictionary<string, object>
                    {
                        { "componentPath", "Supporto.ipt:1"},
                    },
                    OperationId = commands.Count
                });
                haSupporto = false;
            }
        }
        private async Task ScriviChiocciola(string Codice, int Taglia, string BoccaMandata)
        {
            List<KeyValuePair<string, double>> valori = await SQL.CaricaDatiChiocciola(Codice, Taglia, BoccaMandata);
            ScriviParametri("Chiocciola.ipt:1", valori);
        }

        private async Task ScriviSedia(string CodiceSedia)
        {
            List<KeyValuePair<string, double>> valori = await SQL.CaricaDatiSedia(CodiceSedia);
            ScriviParametri("Sedia.ipt:1", valori);
        }
        private void AggiungiEAggancia(string componente1, string componente2, string UCSComponente1, string UCSComponente2)
        {
            commands.Add(new JobCommand
            {
                Method = "AddComponent",
                Params = new Dictionary<string, object>
                {
                    { "newComponentName", $"{Resources.Resource.Percorso}{componente2.Split(".ipt")[0]}.ipt" },
                    { "startingFromAsmPath", ""}
                },
                OperationId = commands.Count
            });
            commands.Add(new JobCommand
            {
                Method = "MateCoordinateSystems",
                Params = new Dictionary<string, object>
                {
                    { "componentPath1", componente1},
                    { "componentPath2", componente2},
                    { "coordinateSystemName1", UCSComponente1},
                    { "coordinateSystemName2", UCSComponente2},
                },
                OperationId = commands.Count
            });
        }
        private void ScriviParametri(string Item, List<KeyValuePair<string, double>> valori)
        {
            foreach (var parametro in valori.Where(x => x.Value > 0)) 
            {
                commands.Add(new()
                {
                    Method = "ModifyParameter",
                    Params = new Dictionary<string, object>
                    {
                        { "componentPath", Item },
                        { "parameterName", parametro.Key },
                        { "parameterValue", parametro.Value }
                    },
                    OperationId = commands.Count
                });
            }
        }
    }
}
