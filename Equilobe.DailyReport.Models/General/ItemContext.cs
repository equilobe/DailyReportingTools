using Equilobe.DailyReport.Models.General;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Equilobe.DailyReport.Models
{
    public class ItemContext : ItemContext<long>
    {
        public ItemContext(long id)
            : base(id)
        {

        }
    }

    public class ItemContext<T> : UserContext
    {
        public ItemContext(T id)
        {
            Id = id;
        }

        public ItemContext()
        {

        }

        public T Id { get; set; }
    }
}
