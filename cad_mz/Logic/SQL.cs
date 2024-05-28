using Microsoft.Data.SqlClient;
using System.Diagnostics;
using System.Resources;
using System.Security.AccessControl;
using static cad_mz.Logic.Entities;

namespace cad_mz.Logic
{
    public class SQL
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        #region Caricamento
        public static async Task<List<KeyValuePair<string, TipoTrasmissione>>> CaricaEsecuzioni()
        {
            List<KeyValuePair<String, TipoTrasmissione>> result = new();
            try
            {
                using SqlConnection conn = new(Resources.Resource.SQLSviluppo);
                SqlCommand cmd = new(@"SELECT * FROM Esecuzioni", conn);
                await conn.OpenAsync();
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        TipoTrasmissione tipo = dr.GetString(2).Equals("Diretto") ? TipoTrasmissione.DirettamenteAccoppiato : TipoTrasmissione.Trasmissione;
                        result.Add(new KeyValuePair<string, TipoTrasmissione>(dr.GetString(1), tipo));
                    }
                }
                dr.Close();
                conn.Close();
            }
            catch (Exception ex)
            {
                Logger.Error(ex.Message);
                //display error message
            }
            return result;
        }
        internal static async Task<List<Accessorio>> CaricaAccessori()
        {
            List<Accessorio> result = new();
            try
            {
                using SqlConnection conn = new(Resources.Resource.SQLSviluppo);
                SqlCommand cmd = new(@"SELECT * FROM Accessori", conn);
                await conn.OpenAsync();
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        result.Add(new Accessorio((int)dr.GetByte(0), dr.GetString(1), Accessorio.TipoAccessorio.Chiocciola));
                    }
                }
                cmd.CommandText = @"SELECT * FROM AccessoriAspirazione";
                dr.Close();
                dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        result.Add(new Accessorio((int)dr.GetInt32(0), dr.GetString(1), Accessorio.TipoAccessorio.Aspirazione));
                    }
                }
                cmd.CommandText = @"SELECT * FROM AccessoriMandata";
                dr.Close();
                dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        result.Add(new Accessorio((int)dr.GetInt32(0), dr.GetString(1), Accessorio.TipoAccessorio.Mandata));
                    }
                }
                cmd.CommandText = @"SELECT * FROM AccessoriAspirazioneSil";
                dr.Close();
                dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        result.Add(new Accessorio((int)dr.GetInt32(0), dr.GetString(1), Accessorio.TipoAccessorio.SilAspirazione));
                    }
                }
                cmd.CommandText = @"SELECT * FROM AccessoriMandataSil";
                dr.Close();
                dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        result.Add(new Accessorio((int)dr.GetInt32(0), dr.GetString(1), Accessorio.TipoAccessorio.SilMandata));
                    }
                }
                dr.Close();
                conn.Close();
            }
            catch (Exception ex)
            {
                Logger.Error("Exception: " + ex.Message);
            }
            result.ForEach(acc =>
            {
                if (acc.Code.Equals("SS"))
                    acc.max = 1;
            });
            var newlist = result.OrderBy(x => x.TypeOfOptional).ThenBy(y => y.Id).ToList();
            return newlist;
        }
        internal static async Task<List<(string, int)>> CaricaCodiciSedie()
        {
            List<(string, int)> list = new();
            try
            {
                using SqlConnection conn = new(Resources.Resource.SQLSviluppo);
                SqlCommand cmd = new("SELECT CodiceSedia, tagliaMotore FROM Sedie", conn);
                await conn.OpenAsync();
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        list.Add((dr.GetString(0), Int32.Parse(dr.GetString(1))));
                    }
                }
                dr.Close();
                conn.Close();
            }
            catch
            {

            }
            return list;
        }
        internal static async Task<List<Motore>> CaricaMotori()
        {
            List<Motore> list = new();
            try
            {
                using SqlConnection conn = new(Resources.Resource.SQLSviluppo);
                SqlCommand cmd = new("SELECT * FROM MotoriAssociazione", conn);
                await conn.OpenAsync();
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        Motore motore = new();
                        motore.Codice = dr.GetString(0).Trim();
                        motore.Kw = (double)dr.GetDecimal(1);
                        motore.Poli = dr.GetByte(2);
                        motore.Taglia = dr.GetInt16(3);
                        motore.Descrizione = dr.GetString(4).Trim();
                        list.Add(motore);
                    }
                }
                dr.Close();
                conn.Close();
            }
            catch
            {

            }
            return list;
        }
        internal static async Task<List<Serie>> CaricaSerie()
        {
            List<Serie> result = new();
            try
            {
                using SqlConnection conn = new(Resources.Resource.SQLSviluppo);
                SqlCommand cmd = new(@"SELECT * FROM Serie", conn);
                await conn.OpenAsync();
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    SqlDataReader drSerie;
                    SqlCommand cmdSerie;
                    while (dr.Read())
                    {
                        string SerieName = dr.GetString(1);
                        Serie newSerie = new(SerieName);
                        cmdSerie = new($"select se.*, bo.DiametroPosizionePrigionieri from Serie_{SerieName} se " +
                            $"left join BoccaAspirazione bo " +
                            $"on se.Ch_CodiceBocca = bo.Codice", conn);
                        try
                        {
                            drSerie = cmdSerie.ExecuteReader();
                            if (drSerie.HasRows)
                            {
                                while (drSerie.Read())
                                {
                                    Ventilatore dimensionalData = new();
                                    dimensionalData.Serie = SerieName;
                                    dimensionalData.Id = drSerie.GetInt32(0);
                                    dimensionalData.Model = drSerie.GetString(1);
                                    dimensionalData.MotorSize = drSerie.GetString(2);
                                    dimensionalData.Kw = (double)drSerie.GetDecimal(3);
                                    dimensionalData.Poles = drSerie.GetByte(4);
                                    dimensionalData.DatiChiocciola = new();
                                    dimensionalData.DatiChiocciola.Code = drSerie.GetString(5);
                                    dimensionalData.DatiChiocciola.Size = drSerie.GetInt16(6);
                                    dimensionalData.DatiChiocciola.Outlet = drSerie.GetString(7);
                                    dimensionalData.DatiChiocciola.Mouth_Type = drSerie.GetBoolean(8);
                                    dimensionalData.DatiChiocciola.Mouth_Code = drSerie.GetString(9);
                                    dimensionalData.DatiChiocciola.Port = drSerie.GetString(10);
                                    dimensionalData.DatiChiocciola.CodeE05 = drSerie.GetString(11);
                                    dimensionalData.DatiChiocciola.DiscE05 = drSerie.GetString(12);
                                    dimensionalData.DatiChiocciola.PortE05 = drSerie.GetString(13);
                                    dimensionalData.DatiChiocciola.ChairE04 = drSerie.GetString(14);
                                    dimensionalData.DatiChiocciola.ChairOther = drSerie.GetString(15);
                                    dimensionalData.DatiChiocciola.ChairE08 = drSerie.GetString(16);
                                    dimensionalData.DatiChiocciola.Support = drSerie.GetString(17);
                                    dimensionalData.DatiChiocciola.Support_Mandatory = drSerie.GetBoolean(18);
                                    dimensionalData.DatiChiocciola.AV = drSerie.GetString(19);
                                    dimensionalData.DatiChiocciola.PAV = drSerie.GetString(20);
                                    if (!drSerie.IsDBNull(21))
                                        dimensionalData.DatiChiocciola.MB = drSerie.GetString(21);
                                    dimensionalData.DatiChiocciola.Inlet = drSerie.GetString(22);
                                    newSerie.DimensionalDatas.Add(dimensionalData);

                                }
                                result.Add(newSerie);
                            }
                            drSerie.Close();
                        }
                        catch (Exception ex)
                        {
                            //display error message
                            Console.WriteLine("Exception: " + ex.Message);
                        }
                    }
                }
                dr.Close();
                conn.Close();
            }
            catch (Exception ex)
            {
                //display error message
                Console.WriteLine("Exception: " + ex.Message);
            }
            return result;
        }
        internal static async Task<List<Silenziatore>> CaricaSilenziatori()
        {
            List<Silenziatore> list = new();
            try
            {
                using SqlConnection conn = new(Resources.Resource.SQLSviluppo);
                SqlCommand cmd = new($"select * from Silenziatori_AssociazioneTramoggeTre", conn);
                await conn.OpenAsync();
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        List<(string, int)> listaInt = new();
                        for (int i = 0; i < 18; i += 2)
                        {
                            if (dr.GetString(i + 2).Equals("--"))
                                break;
                            list.Add(new(dr.GetString(1), dr.GetString(i + 2), Int32.Parse(dr.GetString(i + 3))));
                        }
                    }
                }
                dr.Close();
                cmd.CommandText = $"select codice from Silenziatori_DatiSilenziatore";
                List<string> listaSilenziatoriEsistenti = new();
                dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    while (dr.Read())
                    {
                        listaSilenziatoriEsistenti.Add(dr.GetString(0));
                    }    
                }
                dr.Close();
                conn.Close();
                list = list.Where(x => listaSilenziatoriEsistenti.Contains(x.Codice)).ToList();
            }
            catch
            {

            }
            return list;
        }
        #endregion

        #region DatiPerJson
        internal static async Task<DatiDimensionali> CaricaDatiChiocciola(string codice, int taglia, string boccaMandata)
        {
            DatiDimensionali result = new();
            try
            {
                using SqlConnection conn = new(Resources.Resource.SQLSviluppo);
                SqlCommand cmd = new($"select c.dim_X, c.dim_Y, c.dim_Z, c.dim_W, c.Scalino, c.DiametroForoGirante, cd.DiametroDisco, cd.DiametroPrigionieriForo, cd.NumeroPrigionieri, cd.SpessoreDisco" +
                                     $" from Chiocciole c, ChioccioleDimensioni cd" +
                                     $" where c.Codice = '{codice}' and cd.Taglia = '{taglia}'", conn);
                await conn.OpenAsync();
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    dr.Read();
                    foreach (var item in dr.GetColumnSchema())
                    {
                        double value = -1;
                        if (Double.TryParse(dr.GetString((int)item.ColumnOrdinal), out value))
                            result.Add(item.ColumnName, value);
                    }
                }
                dr.Close();
                cmd.CommandText = $"SELECT a, b, a1, a2, N1, N2, Passo, phi, E FROM BoccaMandata where axb = '{boccaMandata}'";
                dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    dr.Read();
                    foreach (var item in dr.GetColumnSchema())
                    {
                        double value = -1;
                        if (Double.TryParse(dr.GetString((int)item.ColumnOrdinal), out value))
                            result.Add($"bocca_{item.ColumnName}", value);
                    }
                }
                conn.Close();
            }
            catch
            {

            }
            return result;
        }
        internal static async Task<DatiDimensionali> CaricaDatiSedia(string codiceSedia)
        {
            DatiDimensionali result = new();
            try
            {
                using SqlConnection conn = new(Resources.Resource.SQLSviluppo);
                SqlCommand cmd = new($"SELECT * FROM Sedie where CodiceSedia = '{codiceSedia}'", conn);
                await conn.OpenAsync();
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    dr.Read();
                    foreach (var item in dr.GetColumnSchema())
                    {
                        double value = -1;
                        if (Double.TryParse(dr.GetString((int)item.ColumnOrdinal), out value))
                            result.Add(item.ColumnName, value);
                    }
                }
                dr.Close();
                conn.Close();
            }
            catch
            {

            }
            return result;
        }
        internal static async Task<DatiDimensionali> CaricaDatiSupporto(string codiceSupporto, bool silenziatore)
        {
            DatiDimensionali result = new();
            try
            {
                using SqlConnection conn = new(Resources.Resource.SQLSviluppo);
                SqlCommand cmd = new($"SELECT * FROM Supporti where Codice = '{codiceSupporto}'", conn);
                await conn.OpenAsync();
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    dr.Read();
                    foreach (var item in dr.GetColumnSchema())
                    {
                        double value = -1;
                        if (Double.TryParse(dr.GetString((int)item.ColumnOrdinal), out value))
                            result.Add(item.ColumnName, value);
                    }
                }
                dr.Close();
                conn.Close();
            }
            catch
            {

            }
            if (silenziatore)
            {
                result.Remove("DForoPerPassaggioGirante");
                result.Remove("LarghezzaSupporto");
                result.Remove("InterasseFori");
                result.Remove("DForo");
                result.Remove("NFori");
                result.Rename("HSupporto", "Sup_Altezza");
                result.Rename("SpessoreLamiera", "Sup_Spessore");
                result.Rename("LarghezzaSpallaSupporto", "Sup_LarghezzaSpallaSupporto");
                result.Rename("InterasseForiBase", "Sup_InterasseForoBase");
                result.Rename("DistForoBase", "Sup_DistForoBase");
                result.Rename("PosizioneForoBase", "Sup_PosizioneForoBase");
                result.Add("SupportoRichiesto", 1);
            }
            return result;
        }
        internal static async Task<DatiDimensionali> CaricaBoccaAspirazione(string codiceBocca)
        {
            DatiDimensionali result = new();
            try
            {
                using SqlConnection conn = new(Resources.Resource.SQLSviluppo);
                SqlCommand cmd = new($"SELECT * FROM BoccaAspirazione where Codice = '{codiceBocca}'", conn);
                await conn.OpenAsync();
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    dr.Read();
                    foreach (var item in dr.GetColumnSchema())
                    {
                        double value = -1;
                        if (Double.TryParse(dr.GetString((int)item.ColumnOrdinal), out value))
                            result.Add(item.ColumnName, value);
                    }
                }
                dr.Close();
                conn.Close();
            }
            catch
            {

            }
            return result;
        }
        internal async static Task<DatiDimensionali> CaricaMotore(string grandezza)
        {
            DatiDimensionali result = new();
            try
            {
                using SqlConnection conn = new(Resources.Resource.SQLSviluppo);
                SqlCommand cmd = new($"Select * From Motori where Codice ='{grandezza}'", conn);
                await conn.OpenAsync();
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    dr.Read();
                    foreach (var item in dr.GetColumnSchema())
                    {
                        double value = -1;
                        if (Double.TryParse(dr.GetString((int)item.ColumnOrdinal), out value))
                            result.Add(item.ColumnName, value);
                    }
                }
                dr.Close();
                conn.Close();
            }
            catch
            {

            }
            return result;
        }
        internal async static Task<DatiDimensionali> CaricaDatiAccessorio(string code, double diametroAttacco, string axb = "")
        {
            DatiDimensionali result = new();
            try
            {
                using SqlConnection conn = new(Resources.Resource.SQLSviluppo);
                string query = diametroAttacco > 0 ? $"Select * From {code} where d1 ='{diametroAttacco}'" : $"Select * From {code} where axb ='{axb}'";
                SqlCommand cmd = new(query, conn);

                await conn.OpenAsync();
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    dr.Read();
                    foreach (var item in dr.GetColumnSchema())
                    {
                        if (item.ColumnName.ToLower().Equals("_id"))
                            continue;
                        double value = -1;
                        if (Double.TryParse(dr.GetString((int)item.ColumnOrdinal), out value))
                            result.Add(item.ColumnName, value);
                    }
                }
                dr.Close();
                conn.Close();
            }
            catch
            {

            }
            return result;
        }
        internal async static Task<DatiDimensionali> CaricaDatiSilenziatoreAspirazione(double diametroAttacco, string codice)
        {
            DatiDimensionali result = new();
            try
            {
                using SqlConnection conn = new(Resources.Resource.SQLSviluppo);
                SqlCommand cmd = new($"Select * From CFA where d1 = '{diametroAttacco}'", conn);
                await conn.OpenAsync();
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    dr.Read();
                    foreach (var item in dr.GetColumnSchema())
                    {
                        if (item.ColumnName.ToLower().Equals("_id"))
                            continue;
                        string nomeParametro = item.ColumnName.ToUpper();
                        if (nomeParametro.Equals("PHI")) nomeParametro = "phi";
                        if (nomeParametro.Equals("E1")) nomeParametro = "E";
                        double value = -1;
                        if (Double.TryParse(dr.GetString((int)item.ColumnOrdinal), out value))
                            result.Add($"Tram_Fla1_{nomeParametro}", value);
                    }
                }
                dr.Close();
                cmd.CommandText = $"Select * From Silenziatori_DatiSilenziatore where Codice ='{codice}'";
                dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    dr.Read();
                    foreach (var item in dr.GetColumnSchema())
                    {
                        if (item.ColumnName.ToLower().Equals("_id"))
                            continue;
                        double value = -1;
                        if (Double.TryParse(dr.GetString((int)item.ColumnOrdinal), out value))
                            result.Add(item.ColumnName, value);
                    }
                }
                conn.Close();
            }
            catch
            {

            }
            return result;
        }
        internal async static Task<DatiDimensionali> CaricaDatiSilenziatoreMandata(string axb, string codice)
        {
            DatiDimensionali result = new();
            try
            {
                using SqlConnection conn = new(Resources.Resource.SQLSviluppo);
                SqlCommand cmd = new($"Select * From CFP where axb = '{axb}'", conn);
                await conn.OpenAsync();
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    dr.Read();
                    foreach (var item in dr.GetColumnSchema())
                    {
                        if (item.ColumnName.ToLower().Equals("_id") || item.ColumnName.ToLower().Equals("H1"))
                            continue;
                        double value = -1;
                        if (Double.TryParse(dr.GetString((int)item.ColumnOrdinal), out value))
                            result.Add($"Tram_Flan1_{item.ColumnName}", value);
                    }
                }
                dr.Close();
                cmd.CommandText = $"Select * From Silenziatori_DatiSilenziatore where Codice ='{codice}'";
                dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    dr.Read();
                    foreach (var item in dr.GetColumnSchema())
                    {
                        if (item.ColumnName.ToLower().Equals("_id"))
                            continue;
                        double value = -1;
                        if (Double.TryParse(dr.GetString((int)item.ColumnOrdinal), out value))
                            result.Add(item.ColumnName, value);
                    }
                }
                result.Rename("Tram_Fla2_D", "Tram_Flan2_d");
                result.Rename("Tram_Fla2_D1", "Tram_Flan2_d1");
                result.Rename("Tram_Fla2_D2", "Tram_Flan2_d2");
                result.Rename("Tram_Fla2_NFori", "Tram_Flan2_NFori");
                result.Rename("Tram_Fla2_phi", "Tram_Flan2_phi");
                result.Rename("Sil_L", "Tram_Sil_Lunghezza");
                conn.Close();
            }
            catch
            {

            }
            return result;
        }
        internal async static Task<DatiDimensionali> CaricaGasCaldiTenuta(string esecuzione, int tagliaMotore, int tagliaVentilatore)
        {
            DatiDimensionali result = new();
            try
            {
                string query = "";
                using SqlConnection conn = new(Resources.Resource.SQLSviluppo);
                if (esecuzione.Equals("E04"))
                {
                    query = "SELECT DeltaTenuta, DeltaGasCaldi, SpostamentoCavallotto, altezza, larghezza from GC_TN" +
                           " JOIN ReteGC ON GC_TN.TagliaMotore = ReteGC.TagliaMotore" +
                           $" WHERE GC_TN.TagliaMotore = '{tagliaMotore}' AND GC_TN.TagliaVentilatore = '{tagliaVentilatore}'";
                }
                else if (esecuzione.Equals("E05"))
                {
                    query = $"SELECT * FROM GC_B5 WHERE TagliaMotore = '{tagliaMotore}' AND GC_TN.TagliaVentilatore = '{tagliaVentilatore}'";
                }
                SqlCommand cmd = new(query, conn);
                await conn.OpenAsync();
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    dr.Read();
                    foreach (var item in dr.GetColumnSchema())
                    {
                        if (item.ColumnName.ToLower().Equals("_id"))
                            continue;
                        double value = -1;
                        if (Double.TryParse(dr.GetString((int)item.ColumnOrdinal), out value))
                            result.Add(item.ColumnName, value);
                    }
                }
                dr.Close();
                conn.Close();
            }
            catch
            {

            }
            return result;
        }
        internal async static Task<DatiDimensionali> CaricaAV(string codiceAV)
        {
            DatiDimensionali result = new();
            try
            {
                using SqlConnection conn = new(Resources.Resource.SQLSviluppo);
                string query = $"SELECT * from AV where Codice = '{codiceAV}'";
                SqlCommand cmd = new(query, conn);
                await conn.OpenAsync();
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    dr.Read();
                    foreach (var item in dr.GetColumnSchema())
                    {
                        if (item.ColumnName.ToLower().Equals("_id"))
                            continue;
                        double value = -1;
                        if (Double.TryParse(dr.GetString((int)item.ColumnOrdinal), out value))
                            result.Add(item.ColumnName, value);
                    }
                }
                dr.Close();
                conn.Close();
            }
            catch
            {

            }
            return result;
        }
        internal async static Task<DatiDimensionali> CaricaPAV(string codicePAV)
        {
            DatiDimensionali result = new();
            try
            {
                using SqlConnection conn = new(Resources.Resource.SQLSviluppo);
                string query = $"SELECT * from PAV where Codice = '{codicePAV}'";
                SqlCommand cmd = new(query, conn);
                await conn.OpenAsync();
                SqlDataReader dr = cmd.ExecuteReader();
                if (dr.HasRows)
                {
                    dr.Read();
                    foreach (var item in dr.GetColumnSchema())
                    {
                        if (item.ColumnName.ToLower().Equals("_id"))
                            continue;
                        double value = -1;
                        if (Double.TryParse(dr.GetString((int)item.ColumnOrdinal), out value))
                            result.Add(item.ColumnName, value);
                    }
                }
                dr.Close();
                conn.Close();
            }
            catch
            {

            }
            return result;
        }
        #endregion
    }
}
