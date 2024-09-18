using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechXpress.Entities.Repositories
{
    public interface IUnitOfWork  : IDisposable
    {
        ICategoryRepository Category{ get; }
        int complete();
    }
}
