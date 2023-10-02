﻿using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Elasticsearch.API.Models.ECommerceModel;
using System.Collections.Immutable;

namespace Elasticsearch.API.Repositories;

public class ECommerceRepository
{
    private readonly ElasticsearchClient _client;
    private string indexName = "kibana_sample_data_ecommerce";
    public ECommerceRepository(ElasticsearchClient client)
    {
        _client = client;
    }
    public async Task<ImmutableList<ECommerce>> TermQueryAsync(string customerFirstName)
    {

        //1.way
        //var result = await _client.SearchAsync<ECommerce>(
        //    s => s.Index(indexName)
        //    .Query(q => q
        //    .Term(t => t
        //    .Field("customer_first_name.keyword").Value(customerFirstName))));

        //2.way
        //var result = await _client.SearchAsync<ECommerce>(
        //    s => s.Index(indexName)
        //    .Query(q => q
        //    .Term(t => t.CustomerFirstName
        //    .Suffix("keyword"), customerFirstName)));

        //3.way
        var termQuery = new TermQuery("customer_first_name.keyword") {Value=customerFirstName,CaseInsensitive=true };

        var result = await _client.SearchAsync<ECommerce>(
            s => s.Index(indexName).Query(termQuery));

        foreach (var hit in result.Hits) hit.Source.Id = hit.Id;

        return result.Documents.ToImmutableList();
    }

    public async Task<ImmutableList<ECommerce>> TermsQueryAsync(List<string> customerFirstName)
    {
        List<FieldValue> terms = new List<FieldValue>();
        customerFirstName.ForEach(x =>
        {
            terms.Add(x);
        });

        //1.way
        //var termsQuery = new TermsQuery()
        //{
        //    Field = "customer_first_name.keyword",
        //    Terms = new TermsQueryField(terms.AsReadOnly())
        //};
        //var result =await _client.SearchAsync<ECommerce>(s => s.Index(indexName).Query(termsQuery));

        //2.way
        var result = await _client
        .SearchAsync<ECommerce>(s => s.Index(indexName)
        .Size(100)
        .Query(q => q
        .Terms(t => t
        .Field(f => f.CustomerFirstName
        .Suffix("keyword"))
        .Terms(new TermsQueryField(terms.AsReadOnly())))));

        foreach (var hit in result.Hits) hit.Source.Id = hit.Id;

        return result.Documents.ToImmutableList();
    }

    public async Task<ImmutableList<ECommerce>> PrefixQueryAsync(string customerFullName)
    {
        var result = await _client.SearchAsync<ECommerce>(
            s => s.Index(indexName)
            .Size(100)
            .Query(q=>q
            .Prefix(p=>p
            .Field(f=>f.CustomerFullName
            .Suffix("keyword")).Value(customerFullName))));

        return result.Documents.ToImmutableList();
    }

    public async Task<ImmutableList<ECommerce>> RangeQueryAsync(double fromPrice, double toPrice)
    {
        var result = await _client.SearchAsync<ECommerce>(
            s => s.Index(indexName)
            .Size(100)
            .Query(q => q
            .Range(r=>r
            .NumberRange(nr=>nr
            .Field(f=>f.TaxfulTotalPrice)
            .Gte(fromPrice)
            .Lte(toPrice)))));

        return result.Documents.ToImmutableList();
    } 

    public async Task<ImmutableList<ECommerce>> MatchAllQueryAsync()
    {
        var result = await _client.SearchAsync<ECommerce>(s => s
        .Index(indexName)
        .Query(q => q.MatchAll()));

        foreach (var hit in result.Hits) hit.Source!.Id = hit.Id;

        return result.Documents.ToImmutableList();
    }

    public async Task<ImmutableList<ECommerce>> PaginationQueryAsync(int page, int pageSize)
    {
        //page=1, pageSize=10 => 1-10
        //page=2, pageSize=10 => 11-20

        var pageFrom = (page - 1) * pageSize;

        var result = await _client.SearchAsync<ECommerce>(s => s
        .Index(indexName)
        .Size(pageSize)
        .From(pageFrom)
        .Query(q => q
        .MatchAll()));

        foreach (var hit in result.Hits) hit.Source!.Id = hit.Id;

        return result.Documents.ToImmutableList();
    }

    public async Task<ImmutableList<ECommerce>> WildCardQueryAsync(string customerFullName)
    {
        var result = await _client.SearchAsync<ECommerce>(s => s
        .Index(indexName)
        .Query(q => q
        .Wildcard(w=>w
        .Field(f=>f.CustomerFullName
        .Suffix("keyword"))
        .Wildcard(customerFullName))));

        foreach (var hit in result.Hits) hit.Source!.Id = hit.Id;

        return result.Documents.ToImmutableList();
    }

    public async Task<ImmutableList<ECommerce>> FuzzyQueryAsync(string customerName)
    {
        var result = await _client.SearchAsync<ECommerce>(s=>s
        .Index(indexName)
        .Query(q=>q
        .Fuzzy(fu=>fu
        .Field(f=>f.CustomerFirstName
        .Suffix("keyword"))
        .Value(customerName)
        .Fuzziness(new Fuzziness(2))))
        .Sort(sort=>sort
        .Field(f=>f.TaxfulTotalPrice,new FieldSort() { Order = SortOrder.Desc})));

        foreach (var hit in result.Hits) hit.Source!.Id = hit.Id;

        return result.Documents.ToImmutableList();
    }
}
