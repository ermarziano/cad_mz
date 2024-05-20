using Microsoft.AspNetCore.Mvc.Infrastructure;
using System.Runtime.Intrinsics.Arm;
using static cad_mz.Logic.Entities;

namespace cad_mz.Logic
{
    public class Entities
    {
        public class Selection
        {
            public Serie SelectedSerie { get; set; } = new("Selezione Serie");
            public Ventilatore SelectedFan { get; set; } = new Ventilatore() { Model = "Seleziona Modello"};
            public TipoTrasmissione TipoTrasm { get; set; } 
            public KeyValuePair<String, TipoTrasmissione> Esecuzione { get; set; }
            public KeyValuePair<String, int> Rotazione { get; set; }
            public List<Motore> MotoriFiltrati { get; set; } = new();
            public List<Accessorio> AccessoriChiocciola { get; set; } = new();
            public List<Accessorio> AccessoriMandata { get; set; } = new();
            public List<Accessorio> AccessoriAspirazione { get; set; } = new();
            public List<Accessorio> AccessoriMandataSil { get; set; } = new();
            public List<Accessorio> AccessoriAspirazioneSil { get; set; } = new();
            public (string Codice, int Lunghezza) SilenziatoreMandata { get; set; }
            public (string Codice, int Lunghezza) SilenziatoreAspirazione { get; set; }
            public string GetH()
            {
                return Rotazione.Key switch
                {
                    "RD180" => "H1",
                    "LG180" => "H1",
                    _ => "H0"
                };
            }
        }
        public class Ventilatore
        {
            public int Id { get; set; }
            public string Model { get; set; } = string.Empty;
            public string MotorSize { get; set; } = string.Empty;
            public double Kw { get; set; }
            public int Poles { get; set; }
            public Motore MotoreSelezionato { get; set; } = new();
            public Chiocciola Casing_data { get; set; } = new();
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
        public class Fan
        {
            public int Id { get; set; }
            public string Series { get; set; } = string.Empty;
            public string Model { get; set; } = string.Empty;
            public bool IsTransmission { get; set; } = false;
            public KeyValuePair<String, bool> Construction { get; set; }
            public Motore InstalledMotor { get; set; } = new()!;
            public string AspSilencer { get; set; } = string.Empty;
            public string ManSilencer { get; set; } = string.Empty;
            public int Poles, Ch_Size;
            public bool Ch_TipoBocca, SupportoRichiesto;
            public List<Motore> MotorsList = new();
            public List<Accessorio> AccessoriSelezionati = new();
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
            public bool Disabled { get; set; } = false;
            public string Code { get; set; }
            public Boolean Selected { get; set; } = false;
            public Accessorio Clone()
            {
                return (Accessorio)this.MemberwiseClone();
            }
            public int Qty { get; set; } = 0;
            public int Id { get; set; }
            public Boolean Visible { get; set; } = true;
            public string Direction { get; set; } = ""!;
            public override string ToString()
            {
                return Code + " - " + Id;
            }
            public string Drop { get; set; } = "Drop";
            public bool Is(string codice)
            {
                return Code.Equals(codice);
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
            public List<(string Bocca, List<(string Codice, int Lunghezza)>)> Silenziatori { get; set; } = new();
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
    }
}
