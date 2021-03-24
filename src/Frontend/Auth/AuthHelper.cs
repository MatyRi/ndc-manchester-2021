﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace Frontend.Auth
{
    public class AuthHelper
    {
        private readonly HttpClient _client;
        private string _token;
        private DateTimeOffset _tokenTime;

        public AuthHelper(HttpClient client)
        {
            _client = client;
        }

        public ValueTask<string> GetTokenAsync()
        {
            if (_token is {Length: > 0 } && DateTimeOffset.UtcNow - _tokenTime < TimeSpan.FromMinutes(59))
            {
                return new ValueTask<string>(_token);
            }

            return new ValueTask<string>(GetTokenImpl());
        }

        private async Task<string> GetTokenImpl()
        {
            _tokenTime = DateTimeOffset.UtcNow; // NOTE: Get it form the token actually
            _token = await _client.GetStringAsync("/generateJwtToken?name=fronetned");
            return _token;
        }

    }
}