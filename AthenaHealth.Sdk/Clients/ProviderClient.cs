﻿using AthenaHealth.Sdk.Clients.Interfaces;
using AthenaHealth.Sdk.Extensions;
using AthenaHealth.Sdk.Http;
using AthenaHealth.Sdk.Models.Request;
using AthenaHealth.Sdk.Models.Response;
using System.Threading.Tasks;

namespace AthenaHealth.Sdk.Clients
{
    public class ProviderClient : IProviderClient
    {
        private readonly IConnection _connection;

        public ProviderClient(IConnection connection)
        {
            _connection = connection;
        }

        [Endpoint("GET /providers")]
        public async Task<ProviderResponse> GetAll(GetProviderFilter filter = null)
        {
            return await _connection.Get<ProviderResponse>($"{_connection.PracticeId}/providers", filter);
        }

        [Endpoint("GET /providers/{providerid}")]
        public async Task<ProviderExtended> GetById(int providerId, GetByIdProviderFilter filter = null)
        {
            var result = await _connection.Get<ProviderExtended[]>($"{_connection.PracticeId}/providers/{providerId}", filter);
            return result.FirstOrThrowException();
        }
    }
}
