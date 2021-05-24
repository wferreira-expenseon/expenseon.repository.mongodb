using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using ExpenseOn.Repository.MongoDB.Extensions;
using MongoDB.Driver;

namespace ExpenseOn.Repository.MongoDB.Core 
{
    public class Repository<TDocument> : IRepository<TDocument> where TDocument : class
    {
        protected IMongoCollection<TDocument> DbSet { get; }

        public Repository(IMongoDatabase mongoDatabase)
        {
            if (mongoDatabase == null)
                throw new ArgumentNullException(nameof(mongoDatabase));

            DbSet = mongoDatabase.GetCollection<TDocument>(typeof(TDocument).Name);
        }

        public bool Any()
        {
            return DbSet.Find().Any();
        }

        public bool Any(Expression<Func<TDocument, bool>> predicate)
        {
            return DbSet.Find(predicate).Any();
        }

        public long Count()
        {
            return DbSet.EstimatedDocumentCount();
        }

        public long Count(Expression<Func<TDocument, bool>> predicate)
        {
            return DbSet.CountDocuments(predicate);
        }

        public TDocument Insert(TDocument entity)
        {
            DbSet.InsertOne(entity);

            return entity;
        }

        public bool Insert(ICollection<TDocument> entities)
        {
            var entityBatches = entities.BatchesOf(2000).ToList();

            var insertCount = 0L;

            foreach (var batch in entityBatches)
            {
                var upserts = GetBulkInsertWriteModels(batch);
                var writeResult = DbSet.BulkWrite(upserts, new BulkWriteOptions { IsOrdered = false });

                insertCount += writeResult.InsertedCount;
            }

            return insertCount > 0;
        }

        public IReadOnlyList<TDocument> GetAll()
        {
            return DbSet.Find().ToList();
        }

        public (IReadOnlyList<TDocument> entities, long count) GetAll(int skip, int take)
        {
            var count = DbSet.EstimatedDocumentCount();
            var entities = DbSet.Find().Skip(skip).Limit(take).ToList();

            return (entities, count);
        }

        public IReadOnlyList<TDocument> Get(Expression<Func<TDocument, bool>> predicate)
        {
            return DbSet.Find(predicate).ToList();
        }

        public (IReadOnlyList<TDocument> entities, long count) Get(Expression<Func<TDocument, bool>> predicate, int skip, int take)
        {
            var filter = Builders<TDocument>.Filter.Where(predicate);
            var count = DbSet.CountDocuments(predicate);
            var entities = DbSet.Find(filter).Skip(skip).Limit(take).ToList();

            return (entities, count);
        }

        public TDocument Find(object key)
        {
            var data = DbSet.Find(Builders<TDocument>.Filter.Eq(DbSet.GetDocumentIdFieldName(), key));
            return data.FirstOrDefault();
        }

        public TDocument FirstOrDefault()
        {
            return DbSet.Find().FirstOrDefault();
        }

        public TDocument FirstOrDefault(Expression<Func<TDocument, bool>> predicate)
        {
            return DbSet.Find(predicate).FirstOrDefault();
        }

        public bool Upsert(TDocument entity)
        {
            return DbSet.ReplaceOne(Builders<TDocument>.Filter.Eq(DbSet.GetDocumentIdFieldName(), DbSet.GetIdValue(entity)), entity, new ReplaceOptions { IsUpsert = true }).ModifiedCount > 0;
        }

        public bool Upsert(ICollection<TDocument> entities)
        {
            var entityBatches = entities.BatchesOf(2000).ToList();

            var upsertCount = 0;

            foreach (var batch in entityBatches)
            {
                var upserts = GetBulkUpsertWriteModels(batch);
                var writeResult = DbSet.BulkWrite(upserts, new BulkWriteOptions { IsOrdered = false });

                upsertCount += writeResult.Upserts.Count;
            }

            return upsertCount > 0;
        }

        public bool Update(TDocument entity)
        {
            return DbSet.ReplaceOne(Builders<TDocument>.Filter.Eq(DbSet.GetDocumentIdFieldName(), DbSet.GetIdValue(entity)), entity).ModifiedCount > 0;
        }

        public bool Update(ICollection<TDocument> entities)
        {
            var updates = GetBulkUpdateWriteModels(entities);

            var writeResult = DbSet.BulkWrite(updates);

            return writeResult.ModifiedCount > 0;
        }

        public bool UpdateField<TField>(Expression<Func<TDocument, TField>> fieldSelector, TField value)
        {
            var (filterDefinition, updateDefinition) = GetUpdateFieldDefinition((fieldSelector, value));

            return DbSet.UpdateMany(filterDefinition, updateDefinition).ModifiedCount > 0;
        }

        public bool UpdateField<TField>(Expression<Func<TDocument, bool>> predicate, Expression<Func<TDocument, TField>> fieldSelector, TField value)
        {
            var (filterDefinition, updateDefinition) = GetUpdateFieldDefinition(predicate, (fieldSelector, value));

            return DbSet.UpdateMany(filterDefinition, updateDefinition).ModifiedCount > 0;
        }

        public bool UpdateFields<TField>(params (Expression<Func<TDocument, TField>> fieldSelector, TField value)[] fieldsToUpdate)
        {
            var (filterDefinition, updateDefinition) = GetUpdateFieldDefinition(fieldsToUpdate);

            return DbSet.UpdateMany(filterDefinition, updateDefinition).ModifiedCount > 0;
        }

        public bool UpdateFields<TField>(Expression<Func<TDocument, bool>> predicate, params (Expression<Func<TDocument, TField>> fieldSelector, TField value)[] fieldsToUpdate)
        {
            var (filterDefinition, updateDefinition) = GetUpdateFieldDefinition(predicate, fieldsToUpdate);

            return DbSet.UpdateMany(filterDefinition, updateDefinition).ModifiedCount > 0;
        }

        public bool Delete(object key)
        {
            return DbSet.DeleteOne(Builders<TDocument>.Filter.Eq(DbSet.GetDocumentIdFieldName(), key)).DeletedCount > 0;
        }

        public bool Delete(TDocument entity)
        {
            return DbSet.DeleteOne(Builders<TDocument>.Filter.Eq(DbSet.GetDocumentIdFieldName(), DbSet.GetIdValue(entity))).DeletedCount > 0;
        }

        public bool Delete(Expression<Func<TDocument, bool>> predicate)
        {
            return DbSet.DeleteMany(predicate).DeletedCount > 0;
        }

        public Task<bool> AnyAsync()
        {
            return DbSet.Find().AnyAsync();
        }

        public Task<bool> AnyAsync(Expression<Func<TDocument, bool>> predicate)
        {
            return DbSet.Find(predicate).AnyAsync();
        }

        public Task<long> CountAsync()
        {
            return DbSet.EstimatedDocumentCountAsync();
        }

        public Task<long> CountAsync(Expression<Func<TDocument, bool>> predicate)
        {
            return DbSet.CountDocumentsAsync(predicate);
        }

        public async Task<TDocument> InsertAsync(TDocument entity)
        {
            await DbSet.InsertOneAsync(entity);

            return entity;
        }

        public async Task<bool> InsertAsync(ICollection<TDocument> entities)
        {
            var entityBatches = entities.BatchesOf(2000).ToList();

            var insertCount = 0L;

            foreach (var batch in entityBatches)
            {
                var upserts = GetBulkInsertWriteModels(batch);
                var writeResult = await DbSet.BulkWriteAsync(upserts, new BulkWriteOptions { IsOrdered = false });

                insertCount += writeResult.InsertedCount;
            }

            return insertCount > 0;
        }

        public async Task<IReadOnlyList<TDocument>> GetAllAsync()
        {
            return await DbSet.Find().ToListAsync();
        }

        public async Task<(IReadOnlyList<TDocument> entities, long count)> GetAllAsync(int skip, int take)
        {
            var count = await DbSet.EstimatedDocumentCountAsync();
            var entities = await DbSet.Find().Skip(skip).Limit(take).ToListAsync();

            return (entities, count);
        }

        public async Task<IReadOnlyList<TDocument>> GetAsync(Expression<Func<TDocument, bool>> predicate)
        {
            return await DbSet.Find(predicate).ToListAsync();
        }

        public async Task<(IReadOnlyList<TDocument> entities, long count)> GetAsync(Expression<Func<TDocument, bool>> predicate, int skip, int take)
        {
            var count = await DbSet.CountDocumentsAsync(predicate);
            var entities = await DbSet.Find(predicate).Skip(skip).Limit(take).ToListAsync();

            return (entities, count);
        }

        public async Task<TDocument> FindAsync(object key)
        {
            var data = DbSet.Find(Builders<TDocument>.Filter.Eq(DbSet.GetDocumentIdFieldName(), key));
            return await data.FirstOrDefaultAsync();
        }

        public Task<TDocument> FirstOrDefaultAsync()
        {
            return DbSet.Find().FirstOrDefaultAsync();
        }

        public Task<TDocument> FirstOrDefaultAsync(Expression<Func<TDocument, bool>> predicate)
        {
            return DbSet.Find(predicate).FirstOrDefaultAsync();
        }

        public async Task<bool> UpsertAsync(TDocument entity)
        {
            return (await DbSet.ReplaceOneAsync(Builders<TDocument>.Filter.Eq(DbSet.GetDocumentIdFieldName(), DbSet.GetIdValue(entity)), entity, new ReplaceOptions { IsUpsert = true })).ModifiedCount > 0;
        }

        public async Task<bool> UpsertAsync(ICollection<TDocument> entities)
        {
            var entityBatches = entities.BatchesOf(2000).ToList();

            var upsertCount = 0;

            foreach (var batch in entityBatches)
            {
                var upserts = GetBulkUpsertWriteModels(batch);
                var writeResult = await DbSet.BulkWriteAsync(upserts, new BulkWriteOptions { IsOrdered = false });

                upsertCount += writeResult.Upserts.Count;
            }

            return upsertCount > 0;
        }

        public async Task<bool> UpdateAsync(TDocument entity)
        {
            return (await DbSet.ReplaceOneAsync(Builders<TDocument>.Filter.Eq(DbSet.GetDocumentIdFieldName(), DbSet.GetIdValue(entity)), entity)).ModifiedCount > 0;
        }

        public async Task<bool> UpdateAsync(ICollection<TDocument> entities)
        {
            var updates = GetBulkUpdateWriteModels(entities);

            return (await DbSet.BulkWriteAsync(updates)).ModifiedCount > 0;
        }

        public async Task<bool> UpdateFieldAsync<TField>(Expression<Func<TDocument, TField>> fieldSelector, TField value)
        {
            var (filterDefinition, updateDefinition) = GetUpdateFieldDefinition((fieldSelector, value));

            return (await DbSet.UpdateManyAsync(filterDefinition, updateDefinition)).ModifiedCount > 0;
        }

        public async Task<bool> UpdateFieldAsync<TField>(Expression<Func<TDocument, bool>> predicate, Expression<Func<TDocument, TField>> fieldSelector, TField value)
        {
            var (filterDefinition, updateDefinition) = GetUpdateFieldDefinition(predicate, (fieldSelector, value));

            return (await DbSet.UpdateManyAsync(filterDefinition, updateDefinition)).ModifiedCount > 0;
        }

        public async Task<bool> UpdateFieldsAsync<TField>(params (Expression<Func<TDocument, TField>> fieldSelector, TField value)[] fieldsToUpdate)
        {
            var (filterDefinition, updateDefinition) = GetUpdateFieldDefinition(fieldsToUpdate);

            return (await DbSet.UpdateManyAsync(filterDefinition, updateDefinition)).ModifiedCount > 0;
        }

        public async Task<bool> UpdateFieldsAsync<TField>(Expression<Func<TDocument, bool>> predicate, params (Expression<Func<TDocument, TField>> fieldSelector, TField value)[] fieldsToUpdate)
        {
            var (filterDefinition, updateDefinition) = GetUpdateFieldDefinition(predicate, fieldsToUpdate);

            return (await DbSet.UpdateManyAsync(filterDefinition, updateDefinition)).ModifiedCount > 0;
        }

        public async Task<bool> DeleteAsync(object key)
        {
            return (await DbSet.DeleteOneAsync(Builders<TDocument>.Filter.Eq(DbSet.GetDocumentIdFieldName(), key))).DeletedCount > 0;
        }

        public async Task<bool> DeleteAsync(TDocument entity)
        {
            return (await DbSet.DeleteOneAsync(Builders<TDocument>.Filter.Eq(DbSet.GetDocumentIdFieldName(), DbSet.GetIdValue(entity)))).DeletedCount > 0;
        }

        public async Task<bool> DeleteAsync(Expression<Func<TDocument, bool>> predicate)
        {
            return (await DbSet.DeleteManyAsync(predicate)).DeletedCount > 0;
        }

        private static (FilterDefinition<TDocument> filterDefinition, UpdateDefinition<TDocument> updateDefinition) GetUpdateFieldDefinition<TField>(params (Expression<Func<TDocument, TField>> fieldSelector, TField value)[] fieldsToUpdate)
        {
            var filter = Builders<TDocument>.Filter.Empty;

            UpdateDefinition<TDocument> updateDefinition = null;

            foreach (var (fieldSelector, value) in fieldsToUpdate)
            {
                updateDefinition = updateDefinition?.Set(fieldSelector, value) ?? Builders<TDocument>.Update.Set(fieldSelector, value);
            }

            return (filter, updateDefinition);
        }

        private static (FilterDefinition<TDocument> filterDefinition, UpdateDefinition<TDocument> updateDefinition) GetUpdateFieldDefinition<TField>(Expression<Func<TDocument, bool>> predicate, params (Expression<Func<TDocument, TField>> fieldSelector, TField value)[] fieldsToUpdate)
        {
            var filter = Builders<TDocument>.Filter.Where(predicate);

            UpdateDefinition<TDocument> updateDefinition = null;

            foreach (var (fieldSelector, value) in fieldsToUpdate)
            {
                updateDefinition = updateDefinition?.Set(fieldSelector, value) ?? Builders<TDocument>.Update.Set(fieldSelector, value);
            }

            return (filter, updateDefinition);
        }

        private IEnumerable<InsertOneModel<TDocument>> GetBulkInsertWriteModels(IEnumerable<TDocument> entities)
        {
            return entities.Select(entity => new InsertOneModel<TDocument>(entity));
        }

        private IEnumerable<ReplaceOneModel<TDocument>> GetBulkUpdateWriteModels(IEnumerable<TDocument> entities)
        {
            return from entity in entities
                   let filter = Builders<TDocument>.Filter.Eq(DbSet.GetDocumentIdFieldName(), DbSet.GetIdValue(entity))
                   select new ReplaceOneModel<TDocument>(filter, entity);
        }

        private IEnumerable<ReplaceOneModel<TDocument>> GetBulkUpsertWriteModels(IEnumerable<TDocument> entities)
        {
            return GetBulkUpdateWriteModels(entities).Select(t =>
            {
                t.IsUpsert = true;
                return t;
            });
        }
    }
}
