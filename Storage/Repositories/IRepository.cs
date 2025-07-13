namespace Storage.Repositories;

public interface IRepository<T> where T : class
{
    Task Update(T order);
    Task<long> Get(T order);
}