using System;
using System.Collections.Generic;
using System.Linq;
using Nest;
using OnlineStore.BuisnessLogic.ElasticRepository.Contracts;
using OnlineStore.BuisnessLogic.Models.Dto;

namespace OnlineStore.BuisnessLogic.ElasticRepository
{
    public class ElasticRepository : IElasticRepository
    {
        private const string EsUri = "http://localhost:9200";
        private const string EsIndex = "database";
        private const string EsType = "products";

        public void CheckConnection()
        {
            try
            {
                GetElasticClient().Ping();
            }
            catch (Exception)
            {
                throw new Exception("Elastic database connection lost");
            }
        }

        public void AddOrUpdate(ProductElasticDto product)
        {
            var client = GetElasticClient();
            client.Index(product, i => i.Index(EsIndex).Type(EsType).Id(product.Id));
        }

        public void RemoveById(int id)
        {
            var client = GetElasticClient();
            client.Delete<ProductElasticDto>(id, i => i.Type(EsType).Index(EsIndex));
        }

        public int[] SearchByNameAndCategory(string name, string category, int from, int size)
        {
            var fromIndex = from*size;

            var client = GetElasticClient();
            IEnumerable<IHit<ProductElasticDto>> hits;

            if (string.IsNullOrEmpty(name) && string.IsNullOrEmpty(category))
                return null;

            if (string.IsNullOrEmpty(name))
                hits =
                    client.Search<ProductElasticDto>(
                        s =>
                            s.From(fromIndex)
                                .Size(size)
                                .Query(q => q.Match(m => m.OnField(p => p.Category).Query(category)))).Hits;
            else if (string.IsNullOrEmpty(category))
                hits =
                    client.Search<ProductElasticDto>(
                        s =>
                            s.From(fromIndex)
                                .Size(size)
                                .Query(q => q.QueryString(qs => qs.OnFields(p => p.Name).Query("*" + name + "*")))).Hits;
            else
                hits =
                    client.Search<ProductElasticDto>(
                        s =>
                            s.From(fromIndex)
                                .Size(size)
                                .Query(q => q.QueryString(qs => qs.OnFields(p => p.Name).Query("*" + name + "*")) &&
                                            q.Match(m => m.OnField(p => p.Category).Query(category)))).Hits;

            return hits.Select(hit => hit.Source.Id).OrderBy(id => id).ToArray();
        }

        public long GetCount(string name = null, string category = null)
        {
            var client = GetElasticClient();

            if (string.IsNullOrEmpty(name) && string.IsNullOrEmpty(category))
                return client.Count<ProductElasticDto>().Count;

            if (string.IsNullOrEmpty(name))
                return
                    client.Count<ProductElasticDto>(
                        s => s.Query(q => q.Match(m => m.OnField(p => p.Category).Query(category)))).Count;

            if (string.IsNullOrEmpty(category))
                return
                    client.Count<ProductElasticDto>(
                        s => s.Query(q => q.QueryString(qs => qs.OnFields(p => p.Name).Query("*" + name + "*")))).Count;

            return
                client.Count<ProductElasticDto>(
                    s =>
                        s.Query(
                            q =>
                                q.QueryString(qs => qs.OnFields(p => p.Name).Query("*" + name + "*")) &&
                                q.Match(m => m.OnField(p => p.Category).Query(category)))).Count;
        }


        private static ElasticClient GetElasticClient()
        {
            var uri = new Uri(EsUri);
            var settings = new ConnectionSettings(uri);
            settings.SetTimeout(3000);
            settings.SetDefaultIndex(EsIndex);
            settings.MapDefaultTypeNames(t => t.Add(typeof (ProductElasticDto), EsType));
            settings.ThrowOnElasticsearchServerExceptions();
            return new ElasticClient(settings);
        }
    }
}