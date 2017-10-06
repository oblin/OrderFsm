using OrderFsm.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderFsm.Models
{
    public class StateRepository : IStateRepository<OrderProcess>
    {
        private readonly OrderContext _context;

        public StateRepository(OrderContext context)
        {
            _context = context;
        }

        public int Add(OrderProcess entity)
        {
            _context.OrderProcesses.Add(entity);
            return _context.SaveChanges();
        }

        public int Update(OrderProcess entity)
        {
            _context.OrderProcesses.Update(entity);
            return _context.SaveChanges();
        }
    }
}
