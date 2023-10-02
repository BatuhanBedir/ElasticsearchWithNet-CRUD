﻿using Elastic.Clients.Elasticsearch;
using Elastic.Transport;

namespace Elasticsearch.API.Extensions;

public static class ElasticsearchExtension
{
    public static void AddElastic(this IServiceCollection services, IConfiguration configuration)
    {
        var userName = configuration.GetSection("Elastic")["Usename"];
        var password = configuration.GetSection("Elastic")["Password"];
        
        var settings = new ElasticsearchClientSettings(new Uri(configuration.GetSection("Elastic")["Url"]!)).Authentication(new BasicAuthentication(userName!,password!));

        var client = new ElasticsearchClient(settings);
        
        services.AddSingleton(client);
    }
}
