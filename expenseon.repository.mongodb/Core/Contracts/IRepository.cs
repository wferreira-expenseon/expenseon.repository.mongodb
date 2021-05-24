using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ExpenseOn.Repository.MongoDB.Core
{
    public interface IRepository<TDocument> where TDocument : class
    {
        bool Any();
        bool Any(Expression<Func<TDocument, bool>> predicate);
        long Count();
        long Count(Expression<Func<TDocument, bool>> predicate);
        TDocument Insert(TDocument entity);
        bool Insert(ICollection<TDocument> entities);
        IReadOnlyList<TDocument> GetAll();
        (IReadOnlyList<TDocument> entities, long count) GetAll(int skip, int take);
        IReadOnlyList<TDocument> Get(Expression<Func<TDocument, bool>> predicate);
        (IReadOnlyList<TDocument> entities, long count) Get(Expression<Func<TDocument, bool>> predicate, int skip, int take);
        TDocument Find(object key);
        TDocument FirstOrDefault();
        TDocument FirstOrDefault(Expression<Func<TDocument, bool>> predicate);
        bool Upsert(TDocument entity);
        bool Upsert(ICollection<TDocument> entities);
        bool Update(TDocument entity);
        bool Update(ICollection<TDocument> entities);
        bool UpdateField<TField>(Expression<Func<TDocument, TField>> fieldSelector, TField value);
        bool UpdateField<TField>(Expression<Func<TDocument, bool>> predicate, Expression<Func<TDocument, TField>> fieldSelector, TField value);
        bool UpdateFields<TField>(params (Expression<Func<TDocument, TField>> fieldSelector, TField value)[] fieldsToUpdate);
        bool UpdateFields<TField>(Expression<Func<TDocument, bool>> predicate, params (Expression<Func<TDocument, TField>> fieldSelector, TField value)[] fieldsToUpdate);
        bool Delete(object key);
        bool Delete(TDocument entity);
        bool Delete(Expression<Func<TDocument, bool>> predicate);

        Task<bool> AnyAsync();
        Task<bool> AnyAsync(Expression<Func<TDocument, bool>> predicate);
        Task<long> CountAsync();
        Task<long> CountAsync(Expression<Func<TDocument, bool>> predicate);
        Task<TDocument> InsertAsync(TDocument entity);
        Task<bool> InsertAsync(ICollection<TDocument> entities);
        Task<IReadOnlyList<TDocument>> GetAllAsync();
        Task<(IReadOnlyList<TDocument> entities, long count)> GetAllAsync(int skip, int take);
        Task<IReadOnlyList<TDocument>> GetAsync(Expression<Func<TDocument, bool>> predicate);
        Task<(IReadOnlyList<TDocument> entities, long count)> GetAsync(Expression<Func<TDocument, bool>> predicate, int skip, int take);
        Task<TDocument> FindAsync(object key);
        Task<TDocument> FirstOrDefaultAsync();
        Task<TDocument> FirstOrDefaultAsync(Expression<Func<TDocument, bool>> predicate);
        Task<bool> UpsertAsync(TDocument entity);
        Task<bool> UpsertAsync(ICollection<TDocument> entities);
        Task<bool> UpdateAsync(TDocument entity);
        Task<bool> UpdateAsync(ICollection<TDocument> entities);
        Task<bool> UpdateFieldAsync<TField>(Expression<Func<TDocument, TField>> fieldSelector, TField value);
        Task<bool> UpdateFieldAsync<TField>(Expression<Func<TDocument, bool>> predicate, Expression<Func<TDocument, TField>> fieldSelector, TField value);
        Task<bool> UpdateFieldsAsync<TField>(params (Expression<Func<TDocument, TField>> fieldSelector, TField value)[] fieldsToUpdate);
        Task<bool> UpdateFieldsAsync<TField>(Expression<Func<TDocument, bool>> predicate, params (Expression<Func<TDocument, TField>> fieldSelector, TField value)[] fieldsToUpdate);
        Task<bool> DeleteAsync(object key);
        Task<bool> DeleteAsync(TDocument entity);
        Task<bool> DeleteAsync(Expression<Func<TDocument, bool>> predicate);
    }
}