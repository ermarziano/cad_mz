using static cad_mz.Logic.Entities;

namespace cad_mz.Logic
{
    public class MyCascadingValues
    {
        public Selection UserSelection = new();
        public SQLData UserData = new();
        public Action ValuesChanged;
    }
}