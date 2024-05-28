using Microsoft.AspNetCore.Mvc.Infrastructure;
using Newtonsoft.Json;
using System.Runtime.Intrinsics.Arm;
using System.Transactions;
using static cad_mz.Logic.Entities;

namespace cad_mz.Logic
{
    public class Entities
    {
        public class Selection
        {
            [JsonIgnore]
            public Serie SelectedSerie { get; set; } = new("Selezione Serie");
            public Ventilatore SelectedFan { get; set; } = new Ventilatore() { Model = "Seleziona Modello"};
            public TipoTrasmissione TipoTrasm { get; set; } 
            public KeyValuePair<String, TipoTrasmissione> Esecuzione { get; set; }
            public KeyValuePair<String, int> Rotazione { get; set; }
            public List<Motore> MotoriFiltrati { get; set; } = new();
            public List<Accessorio> AccessoriChiocciola { get; set; } = new();
            public List<Accessorio> AccessoriMandata { get; set; } = new();
            public List<Accessorio> AccessoriAspirazione { get; set; } = new();
            public Silenziatore? SilenziatoreMandata { get; set; }
            public Silenziatore? SilenziatoreAspirazione { get; set; }
            public string Mail { get; set; } = "eraldo.marziano@configuratori.com";
            public bool Caricato { get; set; } = false;
            public string GetH()
            {
                return Rotazione.Key switch
                {
                    "RD180" => "H1",
                    "LG180" => "H1",
                    _ => "H0"
                };
            }
            public bool IsPortelloA()
            {
                return Rotazione.Value switch
                {
                    0 => true,
                    45 => true,
                    315 => true,
                    _ => false
                };
            }
        }
        public class Ventilatore
        {
            public int Id { get; set; }
            public string Serie { get; set; } = "";
            public string Model { get; set; } = string.Empty;
            public string MotorSize { get; set; } = string.Empty;
            public double Kw { get; set; }
            public int Poles { get; set; }
            public Motore MotoreSelezionato { get; set; } = new();
            public Chiocciola DatiChiocciola { get; set; } = new();
        }
        public class Serie
        {
            public Serie()
            {

            }
            public Serie(string _name)
            {
                Name = _name;
            }
            public string Name { get; set; } = string.Empty;
            public List<Ventilatore> DimensionalDatas { get; set; } = new List<Ventilatore>();
        }
        public class Motore
        {
            public string Codice { get; set; } = string.Empty;
            public double Kw { get; set; }
            public int Poli { get; set; }
            public int Taglia {  get; set; }
            public string Descrizione { get; set; } = string.Empty;
            public Motore Clone()
            {
                return (Motore)this.MemberwiseClone();
            }
            public override string ToString()
            {
                return Descrizione;
            }
        }
        public class Accessorio
        {
            public Accessorio(int _Id, string _Code, TipoAccessorio _type)
            {
                Id = _Id;
                Code = _Code;
                TypeOfOptional = _type; 
            }
            public int max = 100;
            public bool Disabled { get; set; } = false;
            public string Code { get; set; }
            public Boolean Selected { get; set; } = false;
            public Accessorio Clone()
            {
                return (Accessorio)this.MemberwiseClone();
            }
            public int Qty { get; set; } = 0;
            public int Id { get; set; }
            public override string ToString()
            {
                return Code + " - " + Id;
            }
            public TipoAccessorio TypeOfOptional; 

            public override bool Equals(object? obj)
            {
                if (obj == null) return false;

                if (obj is Accessorio acc)
                    return this.ToString().Equals(acc.ToString());
                return false;
            }
            public enum TipoAccessorio
            {
                Chiocciola,
                Aspirazione,
                Mandata,
                SilAspirazione,
                SilMandata
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(this.Code, this.Id);
            }
        }
        public struct ProprietaParametro
        {
            public string nome;
            public double valore;
            public ProprietaParametro(string nome, double valore)
            {
                this.nome = nome;
                this.valore = valore;
            }
            public override string? ToString()
            {
                return $"{nome} - {valore}";
            }
        }
        public class DatiDimensionali
        {
            public List<ProprietaParametro> dati;
            public DatiDimensionali()
            {
                dati = new();
            }
            public void Add(string nome, double valore)
            {
                dati.Add(new ProprietaParametro(nome, valore));
            }
            public double Get(string nome)
            {
                return dati.Find(x => x.nome == nome).valore;
            }
            public bool Rename(string vecchio, string nuovo)
            {
                if (dati.Any(x => x.nome == vecchio))
                {
                    int position = dati.FindIndex(x => x.nome == vecchio);
                    if (position > -1)
                    {
                        ProprietaParametro mod = new() { nome = nuovo, valore = dati[position].valore };
                        dati.RemoveAt(position);
                        dati.Insert(position, mod);
                    }
                    return true;
                }
                return false;
            }
            public bool Remove(string Nome)
            {
                if (dati.Any(x => x.nome == Nome))
                {
                    dati.RemoveAt(dati.FindIndex(x => x.nome == Nome));
                    return true;
                }
                return false;
            }
            public void Append(List<ProprietaParametro> listAdd)
            {
                this.dati.AddRange(listAdd);
            }
            public double Pop(string nome)
            {
                double result = Get(nome);
                Remove(nome);
                return result;
            }
        }
        public class Chiocciola
        {
            public string Code { get; set; } = string.Empty;
            public string Outlet { get; set; } = string.Empty;
            public string Inlet { get; set; } = string.Empty;
            public string Port { get; set; } = string.Empty;
            public string Mouth_Code { get; set; } = string.Empty;
            public string CodeE05 { get; set; } = string.Empty;
            public string DiscE05 { get; set; } = string.Empty;
            public string PortE05 { get; set; } = string.Empty;
            public string ChairE04 { get; set; } = string.Empty;
            public string ChairE08 { get; set; } = string.Empty;
            public string ChairOther { get; set; } = string.Empty;
            public string Support { get; set; } = string.Empty;
            public bool Support_Mandatory { get; set; }
            public bool Mouth_Type { get; set; }
            public string AV { get; set; } = string.Empty;
            public string PAV { get; set; } = string.Empty;
            public string MB { get; set; } = string.Empty;
            public int Size { get; set; }
        }
        public class SQLData
        {
            public List<KeyValuePair<string, TipoTrasmissione>> Esecuzioni { get; set; } = new();
            public List<Accessorio> Accessori { get; set; } = new();
            public List<Serie> Serie { get; set; } = new();
            public List<KeyValuePair<string, int>> Rotazioni { get; set; } = new();
            public List<Motore> Motori { get; set; } = new();
            public List<(String, int)> CodiciSedie { get; set; } = new();
            public List<Silenziatore> Silenziatori { get; set; } = new();
            public async Task SQLDataLoad()
            {
                Serie = await SQL.CaricaSerie();
                Esecuzioni = await SQL.CaricaEsecuzioni();
                Accessori = await SQL.CaricaAccessori();
                Motori = await SQL.CaricaMotori();
                CodiciSedie = await SQL.CaricaCodiciSedie();
                CreaRotazioni();
                Silenziatori = await SQL.CaricaSilenziatori();
            }
            protected void CreaRotazioni()
            {
                Rotazioni.Add(new KeyValuePair<string, int>("Zero", 0));
                for (int i = 0; i < 16; i++)
                {
                    string prefix = "RD";
                    int loop = i * 45;
                    if (i > 7)
                    {
                        loop -= 360;
                        prefix = "LG";
                    }
                    Rotazioni.Add(new KeyValuePair<string, int>($"{prefix}{loop}", loop));
                }
            }
        }
        public enum TipoTrasmissione
        {
            Trasmissione = 1,  //true
            DirettamenteAccoppiato = 0  //false
        }
        public class Silenziatore
        {
            public Silenziatore(string dimensioneBocca, string codice, double lunghezzaTramoggia)
            {
                Codice = codice;
                LunghezzaTramoggia = lunghezzaTramoggia;
                DimensioneBocca = dimensioneBocca;
            }

            public string Codice { get; set; } = "";
            public double LunghezzaTramoggia { get; set; }
            public string DimensioneBocca { get; set; } = "";
            public List<Accessorio> Accessori { get; set; } = new();
        }
    }
}
