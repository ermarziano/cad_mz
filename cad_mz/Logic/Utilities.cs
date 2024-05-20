namespace cad_mz.Logic
{
    public static class Utilities
    {
        public static double getValue(this List<KeyValuePair<string, double>> list, string key)
        {
            return list.Find(x => x.Key.Equals(key)).Value;
        }
    }
}
