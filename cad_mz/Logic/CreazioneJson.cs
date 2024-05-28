using static cad_mz.Logic.Entities;
using CADServer.Shared.Common.Models;
using System.Text.RegularExpressions;

namespace cad_mz.Logic
{
    public class CreazioneJson
    {
        readonly Selection selection;
        bool haSupporto = true;
        private string modelloBoccaAspirazione = "", connessioneSilenziatore = "", dimensioneFlangia= "", connessioneSilenziatoreMandata = "";
        int numeroFori = -1;

        SQLData data;
        List<string> DatiCaricati = new();
        List<JobCommand> commands;
        private double estrusioneCavallotto, diametroAlbero, altezzaAsse, diametroForo, diametroPrigionieri, diametroAttacco, diametroMandata;

        public CreazioneJson(Selection _selection, SQLData _data)
        {
            selection = _selection;
            data = _data;
            commands = new List<JobCommand>();
        }
        public async Task<JobRequest> Crea()
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
            await CaricaAccessoriAspirazione();
            await CaricaSilenziatoreAspirazioneEAccessori();
            await CaricaAccessoriMandata();
            await CaricaSilenziatoreMandataEAccessori();
            await CaricaAccessoriChiocciola();
            ScriviParametroSingolo("Chiocciola.ipt:1", "Rotazione", -1 * selection.Rotazione.Value);
            SalvaEChiudi();
            var jobData = new JobRequest()
            {
                Commands = commands,
                Parameters = new ProcessingSettings()
                {
                    CADName = CADNames.Inventor,
                    CreateNewInstance = true,
                    AppVisible = true,
                    NotificationSettings = new NotificationSettings()
                    {
                        Type = CADServer.Shared.Common.Notification.NotificationType.Email,
                        ToEmail = selection.Mail
                    }
                }
            };
            return jobData;
        }
       
        #region Accessori
        private async Task CaricaAccessoriAspirazione()
        {
            string connettiA = modelloBoccaAspirazione;
            foreach (Accessorio accessorio in selection.AccessoriAspirazione)
            {
                string nomeNuovoAccessorio = $"{accessorio.Code}.ipt:{accessorio.Id}";
                DatiDimensionali dati = await SQL.CaricaDatiAccessorio(accessorio.Code, diametroAttacco);
                dati.Add("NumeroFori", numeroFori);
                AggiungiEAggancia(connettiA, nomeNuovoAccessorio, "UCSOUT", "UCSIN");
                ScriviParametri(nomeNuovoAccessorio, dati);
                connettiA = nomeNuovoAccessorio;
                connessioneSilenziatore = nomeNuovoAccessorio;
            }
        }
        private async Task CaricaAccessoriMandata()
        {
            string connettiA = "Chiocciola.ipt:1";
            dimensioneFlangia = selection.SelectedFan.DatiChiocciola.Outlet;
            foreach (Accessorio accessorio in selection.AccessoriMandata)
            {
                string nomeNuovoAccessorio = $"{accessorio.Code}.ipt:{accessorio.Id}";
                DatiDimensionali dati = await SQL.CaricaDatiAccessorio(accessorio.Code, -1, dimensioneFlangia);
                AggiungiEAggancia(connettiA, nomeNuovoAccessorio);
                ScriviParametri(nomeNuovoAccessorio , dati);
                connettiA = nomeNuovoAccessorio;
                connessioneSilenziatoreMandata = nomeNuovoAccessorio;
            }
        }
        private async Task CaricaSilenziatoreMandataEAccessori()
        {
            if (selection.SilenziatoreMandata !=null)
            {
                string vecchio = "SilenziatoreMandata.ipt:1", nuovo = string.Empty;
                AggiungiEAggancia(connessioneSilenziatoreMandata, vecchio);
                DatiDimensionali dati = await SQL.CaricaDatiSilenziatoreMandata(dimensioneFlangia, selection.SilenziatoreMandata.Codice);
                diametroMandata = dati.Get("Tram_Flan2_d1");
                foreach (var Accessorio in selection.SilenziatoreMandata.Accessori)
                {
                    nuovo = $"SILM{Accessorio.Code}.ipt:{Accessorio.Id}";
                    dati = await SQL.CaricaDatiAccessorio(Accessorio.Code, diametroMandata);
                    dati.Add("NumeroFori", numeroFori);
                    AggiungiEAggancia(vecchio, nuovo);
                    ScriviParametri(nuovo, dati);
                    vecchio = nuovo;
                }
            }
        }
        private async Task CaricaSilenziatoreAspirazioneEAccessori()
        {
            if (selection.SilenziatoreAspirazione!=null)
            {
                //Silenziatore e Tramoggia
                AggiungiEAggancia(connessioneSilenziatore, "SilenziatoreAspirazione.ipt:1");
                DatiDimensionali dati = await SQL.CaricaDatiSilenziatoreAspirazione(diametroAttacco, selection.SilenziatoreAspirazione.Codice);
                diametroAttacco = dati.Get("Tram_Fla2_D1");
                numeroFori = (int)dati.Get("Tram_Fla2_NFori");
                //Supporto
                if (selection.SilenziatoreAspirazione.Accessori.Exists(a => a.Code.Equals("SS")))
                {
                    var temp = await SQL.CaricaDatiSupporto(selection.SelectedFan.DatiChiocciola.Support, true);
                    dati.Append(temp.dati);
                }
                else
                {
                    dati.Add("SupportoRichiesto", 1);
                }
                dati.Add("Tram_Fla1_NFori", numeroFori);
                dati.Add("Tram_L", selection.SilenziatoreAspirazione.LunghezzaTramoggia);
                ScriviParametri("SilenziatoreAspirazione.ipt:1", dati);
                string vecchio = "SileniatoreAspirazione.ipt:1", nuovo = string.Empty;
                foreach (var Accessorio in selection.SilenziatoreAspirazione.Accessori.Where(a => !a.Code.Equals("SS")))
                {
                    nuovo = $"SILA{Accessorio.Code}.ipt:{Accessorio.Id}";
                    dati = await SQL.CaricaDatiAccessorio(Accessorio.Code, diametroAttacco);
                    dati.Add("NumeroFori", numeroFori);
                    AggiungiEAggancia(vecchio, nuovo);
                    ScriviParametri(nuovo, dati);
                    vecchio = nuovo;
                }
            }
        }
        private async Task CaricaAccessoriChiocciola()
        {
            bool SupportoSilenziatore = selection.SilenziatoreAspirazione!= null && selection.SilenziatoreAspirazione.Accessori.Contiene("SS");
            int numeroAV = SupportoSilenziatore ? 8 : 6;
            if (selection.AccessoriChiocciola.Contiene("GC") || selection.AccessoriChiocciola.Contiene("TN"))
            {
                if (selection.Esecuzione.Key.Equals("E04"))
                {
                    DatiDimensionali dati = await SQL.CaricaGasCaldiTenuta("E04", selection.SelectedFan.MotoreSelezionato.Taglia, selection.SelectedFan.DatiChiocciola.Size);
                    AggiungiEAggancia("MotoreB3.ipt:1", "reteGC.ipt:1", "UCS1", "UCS1");
                    double deltaCaldi = dati.Pop("DeltaGasCaldi");
                    double deltaTenuta = dati.Pop("DeltaTenuta");
                    double distanza = selection.AccessoriChiocciola.Contiene("GC") ? deltaCaldi : deltaTenuta;
                    ScriviParametroSingolo("MotoreB3.ipt:1", "DistanzaDaChiocciola", distanza); // sposta motore
                    ScriviParametroSingolo("Sedia.ipt:1", "EstrusioneCavallotto", dati.Pop("SpostamentoCavallotto") + estrusioneCavallotto); // sposta sedia
                    dati.Add("profondita", distanza);
                    dati.Add("AlberoMotore", diametroAlbero);
                    dati.Add("AltezzaAsse", altezzaAsse);
                    ScriviParametri("reteGC.ipt:1", dati);
                }
                if (selection.Esecuzione.Key.Equals("E05"))
                {
                    DatiDimensionali dati = await SQL.CaricaGasCaldiTenuta("E05", selection.SelectedFan.MotoreSelezionato.Taglia, selection.SelectedFan.DatiChiocciola.Size);
                    AggiungiEAggancia("MotoreB5.ipt:1", "GC_B5.ipt:1", "UCS1", "UCS1");
                    ScriviParametroSingolo("MotoreB5.ipt:1", "DistanzaDaChiocciola", dati.Get("AltezzaPalafitta")); // sposta motore
                    dati.Remove("TagliaVentilatore");
                    dati.Remove("TagliaMotore");
                    ScriviParametri("GC_B5.ipt:1", dati);
                }
            }
            if (selection.AccessoriChiocciola.Contiene("AV"))
            {
                DatiDimensionali dati = await SQL.CaricaAV(selection.SelectedFan.DatiChiocciola.AV);
                AggiungiEAggancia("Sedia.ipt:1", "AV.ipt:1", "UCSA1", "UCS1");
                AggiungiEAggancia("Sedia.ipt:1", "AV.ipt:2", "UCSA2", "UCS1");
                AggiungiEAggancia("Sedia.ipt:1", "AV.ipt:3", "UCSA3", "UCS1");
                AggiungiEAggancia("Sedia.ipt:1", "AV.ipt:4", "UCSA4", "UCS1");
                if (selection.AccessoriChiocciola.Contiene("SA"))
                {
                    AggiungiEAggancia("Supporto.ipt:1", "AV.ipt:5", "UCSA1", "UCS1");
                    AggiungiEAggancia("Supporto.ipt:1", "AV.ipt:6", "UCSA2", "UCS1");
                    if (SupportoSilenziatore)
                    {
                        AggiungiEAggancia("SilenziatoreAspirazione.ipt:1", "AV.ipt:7", "UCSA1", "UCS1");
                        AggiungiEAggancia("SilenziatoreAspirazione.ipt:1", "AV.ipt:8", "UCSA1", "UCS1");
                    }
                } else if (SupportoSilenziatore)
                {
                    AggiungiEAggancia("SilenziatoreAspirazione.ipt:1", "AV.ipt:5", "UCSA1", "UCS1");
                    AggiungiEAggancia("SilenziatoreAspirazione.ipt:1", "AV.ipt:6", "UCSA1", "UCS1");
                }
                ScriviParametri("AV.ipt:1", dati);
            }
            if (selection.AccessoriChiocciola.Contiene("PAV") && selection.Esecuzione.Key.Equals("E04"))
            {

                if (selection.AccessoriChiocciola.Contiene("SA"))
                    numeroAV -= 2;

                DatiDimensionali dati = await SQL.CaricaPAV(selection.SelectedFan.DatiChiocciola.PAV);
                for (int i = 0; i < numeroAV; i++)
                {
                    AggiungiEAggancia($"AV.ipt:{i + 1}", $"PAV.ipt:{i + 1}", "UCS2", "UCS1");
                }
                ScriviParametri("PAV.ipt:1", dati);
            }
            if (selection.AccessoriChiocciola.Contiene("PI"))
            {
                ScriviParametroSingolo("Chiocciola.ipt:1", "PortelloA", selection.IsPortelloA());
                ScriviParametroSingolo("Chiocciola.ipt:1", "PortelloB", !selection.IsPortelloA());
            } else
            {
                ScriviParametroSingolo("Chiocciola.ipt:1", "PortelloA", false);
                ScriviParametroSingolo("Chiocciola.ipt:1", "PortelloB", false);
            }
        }
        #endregion

        #region CorpoVentilatore
        private async Task GeneraVentilatore()
        {
            switch (selection.Esecuzione.Key)
            {
                case "E01":
                    await ScriviSedia($"{selection.SelectedFan.DatiChiocciola.ChairOther}{selection.GetH():000}");
                    await ScriviChiocciola(selection.SelectedFan.DatiChiocciola.Code, selection.SelectedFan.DatiChiocciola.Size, selection.SelectedFan.DatiChiocciola.Outlet);
                    await ScriviSupporto();
                    //CaricaBoccaAspirazione();
                    //CaricaMonoblocco();
                    //break;
                    break;
                case "E04":
                    await ScriviSedia($"{selection.SelectedFan.DatiChiocciola.ChairE04}{selection.GetH()}G{selection.SelectedFan.MotoreSelezionato.Taglia:000}");
                    await ScriviChiocciola(selection.SelectedFan.DatiChiocciola.Code, selection.SelectedFan.DatiChiocciola.Size, selection.SelectedFan.DatiChiocciola.Outlet);
                    await ScriviSupporto();
                    await ScriviBoccaAspirazione(haSupporto ? "Supporto.ipt:1" : "Chiocciola.ipt:1", haSupporto ? "UCS2" : "UCS1");
                    await ScriviMotore("MotoreB3.ipt:1", selection.SelectedFan.MotoreSelezionato.Codice);
                    break;
                case "E05":
                    await ScriviChiocciola(selection.SelectedFan.DatiChiocciola.CodeE05, selection.SelectedFan.DatiChiocciola.Size, selection.SelectedFan.DatiChiocciola.Outlet);
                    await ScriviBoccaAspirazione("Chiocciola.ipt:1", "UCS1");
                    await ScriviMotore("MotoreB5.ipt:1", selection.SelectedFan.MotoreSelezionato.Codice);
                    break;
            }
        }
        private async Task ScriviMotore(string TipoMotore, string Grandezza)
        {
            commands.Add(new()
            {
                Method = "ModifyParameter",
                Params = new Dictionary<string, object>
                {
                        { "componentPath", TipoMotore },
                        { "parameterName", "DistanzaDaChiocciola" },
                        { "parameterValue", "5" }
                    },
                OperationId = commands.Count
            });
            DatiDimensionali valori = await SQL.CaricaMotore(Grandezza);
            diametroAlbero = valori.Get("Dalbero");
            altezzaAsse = valori.Get("AltezzaAsse");
            ScriviParametri(TipoMotore, valori);
        }
        private async Task ScriviBoccaAspirazione(string connessaA, string UCS)
        {
            DatiDimensionali valori = await SQL.CaricaBoccaAspirazione(selection.SelectedFan.DatiChiocciola.Mouth_Code);
            modelloBoccaAspirazione = valori.Get("HLatoRettilineo") > 0 ? "BoccaASP2.ipt:1" : "BoccaASP1.ipt:1";
            numeroFori = valori.Get("HLatoRettilineo") > 0 ? (int)valori.Get("NumeroPrigionieri") : (int)valori.Get("NumeroFori");
            diametroPrigionieri = valori.Get("DiametroPosizionePrigionieri");
            diametroForo = valori.Get("DiametroPosizioneFori");
            diametroAttacco = diametroPrigionieri > 0 ? diametroPrigionieri : diametroForo;
            AggiungiEAggancia(connessaA, modelloBoccaAspirazione, UCS, "UCSIN");
            ScriviParametri(modelloBoccaAspirazione, valori);
            connessioneSilenziatore = modelloBoccaAspirazione;
        }
        private async Task ScriviSupporto()
        {
            if (selection.AccessoriChiocciola.Exists(x => x.Code.Equals("SA") && x.Selected))
            {
                DatiDimensionali valori = await SQL.CaricaDatiSupporto(selection.SelectedFan.DatiChiocciola.Support, false);
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
            DatiDimensionali valori = await SQL.CaricaDatiChiocciola(Codice, Taglia, BoccaMandata);
            ScriviParametri("Chiocciola.ipt:1", valori);
        }
        private async Task ScriviSedia(string CodiceSedia)
        {
            DatiDimensionali valori = await SQL.CaricaDatiSedia(CodiceSedia);
            estrusioneCavallotto = valori.Get("EstrusioneCavallotto");
            ScriviParametri("Sedia.ipt:1", valori);
        }
        #endregion

        #region Generali
        private void AggiungiEAggancia(string componente1, string componente2, string UCSComponente1 = "UCSOUT", string UCSComponente2 = "UCSIN")
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
        private void ScriviParametri(string Item, DatiDimensionali valori)
        {
            if (!DatiCaricati.Contains(Item.Split(".ipt")[0]))
            {
                DatiCaricati.Add(Item.Split(".ipt")[0]);
                foreach (var parametro in valori.dati.Where(x => x.valore > 0))
                {
                    ScriviParametroSingolo(Item, parametro.nome, parametro.valore);
                }
            }
        }
        private void ScriviParametroSingolo(string Item, string Nome, double Valore)
        {
            commands.Add(new()
            {
                Method = "ModifyParameter",
                Params = new Dictionary<string, object>
                    {
                        { "componentPath", Item },
                        { "parameterName", Nome },
                        { "parameterValue", Valore }
                    },
                OperationId = commands.Count
            });
        }
        private void ScriviParametroSingolo(string Item, string Nome, bool Valore)
        {
            commands.Add(new()
            {
                Method = "ModifyParameter",
                Params = new Dictionary<string, object>
                    {
                        { "componentPath", Item },
                        { "parameterName", Nome },
                        { "parameterValue", Valore }
                    },
                OperationId = commands.Count
            });
        }
        private void SalvaEChiudi()
        {
            commands.Add(new()
            {
                Method = "Export",
                Params = new Dictionary<string, object>
                    {
                        { "modelNewName", "Export.stp" }
                    },
                OperationId = commands.Count
            });
            commands.Add(new()
            {
                Method = "SaveAs",
                Params = new Dictionary<string, object>
                    {
                        { "modelNewName", $"{selection.Esecuzione.Key}.iam" }
                    },
                OperationId = commands.Count
            });
            commands.Add(new()
            {
                Method = "Close",
                Params = new Dictionary<string, object>
                    {
                        { "saveChanges", false }
                    },
                OperationId = commands.Count
            });
        }
        #endregion
    }
}
