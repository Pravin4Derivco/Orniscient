using System.Collections.Generic;
using System.Linq;

namespace Derivco.Orniscient.Proxy.Grains.Models
{
    public class DiffModel
    {
        public DiffModel()
        {
            NewGrains = new List<UpdateModel>();
            RemovedGrains = new List<string>();
        }
        public List<UpdateModel> NewGrains { get; set; }
        public List<string> RemovedGrains { get; set; }
        public List<TypeCounter> TypeCounts { get; set; }
        public bool SummaryView { get; set; }
    }
}