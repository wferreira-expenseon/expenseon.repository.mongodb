namespace ExpenseOn.Repository.MongoDB
{
    using System;
    using System.Linq;
    using System.Reflection;
    using global::MongoDB.Bson.Serialization;
    using global::MongoDB.Driver;

    /// <summary>
    ///     Provides extension methods for <see cref="IMongoCollection{TDocument}"/> instances.
    /// </summary>
    internal static class MongoCollectionExtensions
    {
        /// <summary>
        ///     Returns the name of the document field mapped as the Id for BSON documents of type <typeparamref name="TDocument"/>.
        /// </summary>
        /// <typeparam name="TDocument">The document type.</typeparam>
        /// <param name="_">The MongoDB typed collection.</param>
        /// <returns>The name of the Id field for documents of type <typeparamref name="TDocument"/>.</returns>
        internal static string GetDocumentIdFieldName<TDocument>(this IMongoCollection<TDocument> _)
        {
            return GetIdMemberMap(typeof(TDocument)).ElementName;
        }

        /// <summary>
        ///     Returns the value of the Id property of the specified <typeparamref name="TDocument"/> BSON object.
        /// </summary>
        /// <typeparam name="TDocument">The document type.</typeparam>
        /// <param name="_">The MongoDB typed collection.</param>
        /// <param name="entity">The object whose Id property value will be returned.</param>
        /// <returns>The value of the Id property of the specified <typeparamref name="TDocument"/> object.</returns>
        internal static object GetIdValue<TDocument>(this IMongoCollection<TDocument> _, TDocument entity)
        {
            var idPropName = GetIdMemberMap(typeof(TDocument)).MemberName;

            return typeof(TDocument).GetProperty(idPropName, BindingFlags.Public | BindingFlags.Instance)?.GetValue(entity);
        }

        /// <summary>Begins a fluent find interface.</summary>
        /// <typeparam name="TDocument">The type of the document.</typeparam>
        /// <param name="collection">The collection.</param>
        /// <param name="options">The options.</param>
        /// <returns>A fluent find interface.</returns>
        internal static IFindFluent<TDocument, TDocument> Find<TDocument>(this IMongoCollection<TDocument> collection, FindOptions options = null)
        {
            return collection.Find(FilterDefinition<TDocument>.Empty, options);
        }

        /// <summary>
        ///     Returns the class map for BSON documents of the specified type.
        /// </summary>
        /// <param name="classType">The type of the mapped class.</param>
        /// <returns>The Id member map.</returns>
        private static BsonMemberMap GetIdMemberMap(Type classType)
        {
            return BsonClassMap.GetRegisteredClassMaps().FirstOrDefault(t => t.ClassType == classType)?.IdMemberMap ??
                   throw new InvalidOperationException($"No Id property mapped for type '{classType.Name}'.");
        }
    }
}
