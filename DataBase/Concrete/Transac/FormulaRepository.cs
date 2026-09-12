using System.Linq;

namespace Concrete
{
    public class FormulaRepository : FGA.Concrete.Repository<FGA.Models.Formulas>
    {
        public FormulaRepository()
        {

        }

        public override FGA.Models.Formulas Get(string id)
        {
            int idFormula = int.Parse(id);
            return DbSet.FirstOrDefault(o => o.Id == idFormula);
        }
    }
}