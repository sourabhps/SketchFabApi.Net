﻿//
// SketchFabApi.Collections.cs
//
// Author:
//       Xavier Fischer 2020-4
//
// Copyright (c) 2020 Xavier Fischer
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace SketchFab
{


    public partial class SketchFabApi
    {

        public async Task<List<Collection>> GetMyCollectionsAsync(string sketchFabToken, TokenType tokenType)
        {
            try
            {
                _logger.LogInformation($"Get collections");

                HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, $"{SketchFabApiUrl}/me/collections ");
                httpRequestMessage.AddAuthorizationHeader(sketchFabToken, tokenType);

                var response = await _httpClient.SendAsync(httpRequestMessage, HttpCompletionOption.ResponseContentRead);
                _logger.LogInformation($"{nameof(GetMyCollectionsAsync)} responded {response.StatusCode}");
                response.EnsureSuccessStatusCode();

                var collectionsJson = await response.Content.ReadAsStringAsync();

                var collections = JsonConvert.DeserializeObject<PagedResult<Collection>>(collectionsJson);

                _logger.LogInformation($"GetMyCollectionsAsync got {collections.results.Count} collection(s).");

                return collections.results;
            }
            catch (Exception ex)
            {
                _logger.LogError($"SketchFab upload error: {ex.Message}");
                throw;
            }

        }

    }
}