using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderFsm.Interfaces
{
    public interface IStateRepository<T>  where T: IStateEntity
    {
        int Add(T entity);
        int Update(T entity);
    }

    public class IStateEntity
    {
        public int Id { get; set; }
        public string Key { get; set; }
        /// <summary>
        /// 狀態：不允許外部改變
        /// </summary>
        public string State { get; protected set; }
        public bool IsComplete { get; set; }
    }
}
