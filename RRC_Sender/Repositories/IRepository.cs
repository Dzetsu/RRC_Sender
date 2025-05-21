using RRC_Sender.Entities;

namespace RRC_Sender.Repositories;

public interface IRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAll(CancellationToken cancellationToken);
}
