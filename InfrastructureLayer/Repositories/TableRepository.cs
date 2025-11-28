using InfrastructureLayer.Contracts;
using InfrastructureLayer.Exceptions;
using InfrastructureLayer.Models;
using Domains;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
namespace InfrastructureLayer.Repositories
{
    public class GenericRepository<T> : IGenericRepository<T> where T : BaseTable
    {
        private readonly NetflixContext _context;
        private readonly DbSet<T> _dbSet;
        private readonly ILogger<GenericRepository<T>> _logger;

        public GenericRepository(NetflixContext context, ILogger<GenericRepository<T>> logger)
        {
            _context = context;
            _dbSet = _context.Set<T>();
            _logger = logger;
        }


        public IQueryable<T> GetAllQueryable()
        {
            return _dbSet.AsQueryable();
        }



        public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
        {
            try
            {
                return await _dbSet.AnyAsync(predicate);
            }
            catch (Exception ex)
            {
                throw new DataAccessException(ex,$"Error executing AnyAsync: {ex.Message}" ,_logger);
            }
        }


        public async Task<IEnumerable<T>> GetAllAsync(params Expression<Func<T, object>>[] includes)
        {
            IQueryable<T> query = _dbSet.AsQueryable();

            if (includes != null && includes.Any())
            {
                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }

            return await query.ToListAsync();
        }



        public async Task<List<T>> GetAll()
        {
            try
            {
                return await _dbSet.AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                throw new DataAccessException(ex, "", _logger);
            }
        }

        public async Task<T?> GetById(Guid id)
        {
            try
            {
                return await _dbSet.FirstOrDefaultAsync(a => a.Id == id);
            }
            catch (Exception ex)
            {
                throw new DataAccessException(ex, "", _logger);
            }
        }

        public async Task<T?> GetByIdAsNoTracking(Guid id)
        {
            try
            {
                return await _dbSet.AsNoTracking().FirstOrDefaultAsync(a => a.Id == id);
            }
            catch (Exception ex)
            {
                throw new DataAccessException(ex, "", _logger);
            }
        }

        public async Task<(bool, Guid)> Add(T entity)
        {
            try
            {
                entity.CreatedDate = DateTime.Now;
                await _dbSet.AddAsync(entity);
                await _context.SaveChangesAsync();
                return (true, entity.Id);
            }
            catch (Exception ex)
            {
                throw new DataAccessException(ex, ex.Message, _logger);

            }
        }


        public async Task<(bool, Guid)> AddAsync(T entity)
        {
            try
            {
                entity.CreatedDate = DateTime.Now;

                // لو نفس الكيان موجود في الـ ChangeTracker لازم نفصله
                var trackedEntity = _context.ChangeTracker
                    .Entries<T>()
                    .FirstOrDefault(e => e.Entity.Id.Equals(entity.Id));

                if (trackedEntity != null)
                    trackedEntity.State = EntityState.Detached;

                await _dbSet.AddAsync(entity);
                await _context.SaveChangesAsync();

                // بعد الحفظ، فك تتبع النسخة عشان ما يسببش conflict بعدين
                _context.Entry(entity).State = EntityState.Detached;

                return (true, entity.Id);
            }
            catch (Exception ex)
            {
                throw new DataAccessException(ex, "Error during Add() operation", _logger);
            }
        }


        public async Task<bool> Update(T entity)
        {
            try
            {
                var dbData = await GetById(entity.Id);
                if (dbData == null) return false;

                entity.CreatedDate = dbData.CreatedDate;
                entity.CreatedBy = dbData.CreatedBy;
                entity.CurrentState = dbData.CurrentState;
                entity.UpdatedDate = DateTime.Now;

                //_context.Entry(entity).State = EntityState.Modified;
                _context.Entry(dbData).CurrentValues.SetValues(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new DataAccessException(ex, "", _logger);
            }
        }

        public async Task<bool> Update(Guid id, Action<T> updateAction)
        {
            try
            {
                var entity = await GetById(id);
                if (entity == null)
                    return false;

                updateAction(entity);

                _context.Entry(entity).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new DataAccessException(ex, "", _logger);
            }
        }

        public async Task<bool> Delete(Guid id)
        {
            try
            {
                var entity = await GetById(id);
                if (entity == null) return false;

                _dbSet.Remove(entity);
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new DataAccessException(ex, "", _logger);
            }
        }

        public async Task<bool> ChangeStatus(Guid id, Guid userId, int status = 1)
        {
            try
            {
                var entity = await GetById(id);
                if (entity == null) return false;

                entity.CurrentState = status;
                entity.UpdatedBy = userId;
                entity.UpdatedDate = DateTime.Now;

                _context.Entry(entity).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                throw new DataAccessException(ex, "", _logger);
            }
        }

        public async Task<T?> GetFirstOrDefault(Expression<Func<T, bool>> filter)
        {
            try
            {
                return await _dbSet.Where(filter).AsNoTracking().FirstOrDefaultAsync();
            }
            catch (Exception ex)
            {
                throw new DataAccessException(ex, "", _logger);
            }
        }

        public async Task<List<T>> GetList(Expression<Func<T, bool>> filter)
        {
            try
            {
                return await _dbSet.Where(filter).AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                throw new DataAccessException(ex, "", _logger);
            }
        }
        public async Task<List<T>> GetListWithInclude(
    Expression<Func<T, bool>> filter,
    Func<IQueryable<T>, IQueryable<T>>? include = null)
        {
            try
            {
                IQueryable<T> query = _dbSet.Where(filter);

                if (include != null)
                    query = include(query);

                return await query.AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                throw new DataAccessException(ex, "", _logger);
            }
        }

        public async Task<List<TResult>> GetList<TResult>(
            Expression<Func<T, bool>>? filter = null,
            Expression<Func<T, TResult>>? selector = null,
            Expression<Func<T, object>>? orderBy = null,
            bool isDescending = false,
            params Expression<Func<T, object>>[] includers)
        {
            try
            {
                IQueryable<T> query = _dbSet.AsQueryable();

                foreach (var include in includers)
                    query = query.Include(include);

                if (filter != null)
                    query = query.Where(filter);

                if (orderBy != null)
                {
                    query = isDescending
                        ? query.OrderByDescending(orderBy)
                        : query.OrderBy(orderBy);
                }

                query = query.AsNoTracking();

                if (selector != null)
                    return await query.Select(selector).ToListAsync();

                return await query.Cast<TResult>().ToListAsync();
            }
            catch (Exception ex)
            {
                throw new DataAccessException(ex, "", _logger);
            }
        }



        public async Task<PagedResult<T>> GetPagedListAsync(
        int pageNumber,
        int pageSize,
        Expression<Func<T, bool>>? filter = null,
        Expression<Func<T, object>>? orderBy = null,
        bool isDescending = false,
        Func<IQueryable<T>, IQueryable<T>>? include = null
    )
        {
            IQueryable<T> query = _dbSet.AsQueryable();

            if (include != null)
                query = include(query);

            if (filter != null)
                query = query.Where(filter);

            int totalCount = await query.CountAsync();

            if (orderBy != null)
                query = isDescending ? query.OrderByDescending(orderBy) : query.OrderBy(orderBy);

            var items = await query.AsNoTracking()
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<T>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };
        }


        public async Task<PagedResult<TResult>> GetPagedList<TResult>(
            int pageNumber,
            int pageSize,
            Expression<Func<T, bool>>? filter = null,
            Expression<Func<T, TResult>>? selector = null,
            Expression<Func<T, object>>? orderBy = null,
            bool isDescending = false,
            params Expression<Func<T, object>>[] includers)
        {
            try
            {
                IQueryable<T> query = _dbSet.AsQueryable();

                foreach (var include in includers)
                    query = query.Include(include);

                if (filter != null)
                    query = query.Where(filter);

                int totalCount = await query.CountAsync();

                if (orderBy != null)
                {
                    query = isDescending
                        ? query.OrderByDescending(orderBy)
                        : query.OrderBy(orderBy);
                }

                query = query
                    .AsNoTracking()
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize);

                var items = selector != null
                    ? await query.Select(selector).ToListAsync()
                    : await query.Cast<TResult>().ToListAsync();

                return new PagedResult<TResult>
                {
                    Items = items,
                    PageNumber = pageNumber,
                    PageSize = pageSize,
                    TotalCount = totalCount,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                };
            }
            catch (Exception ex)
            {
                throw new DataAccessException(ex, "", _logger);
            }
        }
    }

}
