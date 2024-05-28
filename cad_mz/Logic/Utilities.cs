using cad_mz.Components;
using static cad_mz.Logic.Entities;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace cad_mz.Logic
{
    public static class Utilities
    {
        public static double getValue(this List<KeyValuePair<string, double>> list, string key)
        {
            return list.Find(x => x.Key.Equals(key)).Value;
        }
        public static bool Contiene(this List<Accessorio> list, string key)
        {
            return list.Exists(x=>x.Code.Equals(key));
        }
        public static bool Rimpiazza(this List<Accessorio> list, Accessorio acc, List<Accessorio>? lista = null)
        {
            if (list.Any(x => x.Code.Equals(acc.Code) && x.TypeOfOptional == acc.TypeOfOptional))
            {
                int index = list.FindIndex(x => x.Code.Equals(acc.Code) && x.TypeOfOptional == acc.TypeOfOptional);
                list.RemoveAt(index);
                list.Insert(index, acc);
                if (lista!=null)
                {
                    lista.Add(acc);
                }
                return true;
            }
            return false;
        }
    }
}
