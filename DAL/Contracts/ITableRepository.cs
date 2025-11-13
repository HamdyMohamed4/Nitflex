using DAL.Models;
using Domains;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Contracts
{
    public interface IGenericRepository<T> where T : BaseTable
    {
        Task<List<T>> GetAll();
        Task<T?> GetById(Guid id);
        Task<T?> GetByIdAsNoTracking(Guid id);

        Task<(bool, Guid)> Add(T entity);

        Task<bool> Update(T entity);
        Task<bool> Update(Guid id, Action<T> updateAction);

        Task<bool> Delete(Guid id);
        Task<bool> ChangeStatus(Guid id, Guid userId, int status = 1);

        Task<T?> GetFirstOrDefault(Expression<Func<T, bool>> filter);
        Task<List<T>> GetList(Expression<Func<T, bool>> filter);

        Task<List<TResult>> GetList<TResult>(
            Expression<Func<T, bool>>? filter = null,
            Expression<Func<T, TResult>>? selector = null,
            Expression<Func<T, object>>? orderBy = null,
            bool isDescending = false,
            params Expression<Func<T, object>>[] includers);

        Task<PagedResult<TResult>> GetPagedList<TResult>(
            int pageNumber,
            int pageSize,
            Expression<Func<T, bool>>? filter = null,
            Expression<Func<T, TResult>>? selector = null,
            Expression<Func<T, object>>? orderBy = null,
            bool isDescending = false,
            params Expression<Func<T, object>>[] includers);
    }
}
