using Microsoft.Data.SqlClient;
using System.Resources;
using System.Security.AccessControl;
using static cad_mz.Logic.Entities;

namespace cad_mz.Logic
{
    public class SQL
    {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        #region Caricamento
        public static async Task<List<KeyValuePair<String, TipoTrasmissione>>> CaricaEsecuzioni()
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
                        result.Add(new KeyValuePair<String, TipoTrasmissione>(dr.GetString(1), tipo));
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
                        motore.Codice = dr.GetString(0);
                        motore.Kw = (double)dr.GetDecimal(1);
                        motore.Poli = dr.GetByte(2);
                        motore.Taglia = dr.GetInt16(3);
                        motore.Descrizione = dr.GetString(4);
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
                                    dimensionalData.Id = drSerie.GetInt32(0);
                                    dimensionalData.Model = drSerie.GetString(1);
                                    dimensionalData.MotorSize = drSerie.GetString(2);
                                    dimensionalData.Kw = (double)drSerie.GetDecimal(3);
                                    dimensionalData.Poles = drSerie.GetByte(4);
                                    dimensionalData.Casing_data = new();
                                    dimensionalData.Casing_data.Code = drSerie.GetString(5);
                                    dimensionalData.Casing_data.Size = drSerie.GetInt16(6);
                                    dimensionalData.Casing_data.Outlet = drSerie.GetString(7);
                                    dimensionalData.Casing_data.Mouth_Type = drSerie.GetBoolean(8);
                                    dimensionalData.Casing_data.Mouth_Code = drSerie.GetString(9);
                                    dimensionalData.Casing_data.Port = drSerie.GetString(10);
                                    dimensionalData.Casing_data.CodeE05 = drSerie.GetString(11);
                                    dimensionalData.Casing_data.DiscE05 = drSerie.GetString(12);
                                    dimensionalData.Casing_data.PortE05 = drSerie.GetString(13);
                                    dimensionalData.Casing_data.ChairE04 = drSerie.GetString(14);
                                    dimensionalData.Casing_data.ChairOther = drSerie.GetString(15);
                                    dimensionalData.Casing_data.ChairE08 = drSerie.GetString(16);
                                    dimensionalData.Casing_data.Support = drSerie.GetString(17);
                                    dimensionalData.Casing_data.Support_Mandatory = drSerie.GetBoolean(18);
                                    dimensionalData.Casing_data.AV = drSerie.GetString(19);
                                    dimensionalData.Casing_data.PAV = drSerie.GetString(20);
                                    if (!drSerie.IsDBNull(21))
                                        dimensionalData.Casing_data.MB = drSerie.GetString(21);
                                    dimensionalData.Casing_data.Inlet = drSerie.GetString(22);
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
        internal static async Task<List<(string, List<(string, int)>)>> CaricaSilenziatori()
        {
            List<(string Bocca, List<(string Codice, int Lunghezza)>)> list = new();
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
                            listaInt.Add((dr.GetString(i + 2), Int32.Parse(dr.GetString(i + 3))));
                        }
                        list.Add((dr.GetString(1), listaInt));
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
        #endregion

        #region DatiPerJson
        internal static async Task<List<KeyValuePair<string, double>>> CaricaDatiChiocciola(string codice, int taglia, string boccaMandata)
        {
            List<KeyValuePair<string, double>> result = new();
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
                            result.Add(new KeyValuePair<string, double>(item.ColumnName, value));
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
                            result.Add(new KeyValuePair<string, double>($"bocca_{item.ColumnName}", value));
                    }
                }
                conn.Close();
            }
            catch
            {

            }
            return result;
        }
        internal static async Task<List<KeyValuePair<string, double>>> CaricaDatiSedia(string codiceSedia)
        {
            List<KeyValuePair<string, double>> result = new();
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
                            result.Add(new KeyValuePair<string, double>(item.ColumnName, value));
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
        internal static async Task<List<KeyValuePair<string, double>>> CaricaSupporto(string codiceSupporto)
        {
            List<KeyValuePair<string, double>> result = new();
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
                            result.Add(new KeyValuePair<string, double>(item.ColumnName, value));
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
        internal static async Task<List<KeyValuePair<string, double>>> CaricaBoccaAspirazione(string codiceBocca)
        {
            List<KeyValuePair<string, double>> result = new();
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
                            result.Add(new KeyValuePair<string, double>(item.ColumnName, value));
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
