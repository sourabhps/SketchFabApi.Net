﻿using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace DEM.Net.Extension.SketchFab
{
    public partial class SketchFabApi
    {
        public static string Source = "elevationapi";
        public async Task<string> UploadModelAsync(UploadModelRequest request, string sketchFabToken)
        {
            try
            {
                string uuid = await UploadFileAsync(request, sketchFabToken);

                return request.ModelId;
            }
            catch (Exception ex)
            {
                _logger.LogError($"SketchFab upload error: {ex.Message}");
                throw;
            }

        }
        private async Task<string> UploadFileAsync(UploadModelRequest request, string sketchFabToken)
        {
            try
            {
                _logger.LogInformation($"Uploading model [{request.FilePath}].");
                if (string.IsNullOrWhiteSpace(request.FilePath))
                {
                    throw new ArgumentNullException(nameof(request.FilePath));
                }

                if (!File.Exists(request.FilePath))
                {
                    throw new FileNotFoundException($"File [{request.FilePath}] not found.");
                }
                HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Post, $"{SketchFabApiUrl}/models");
                httpRequestMessage.Headers.Add("Authorization", $"Token {sketchFabToken}");
                using var form = new MultipartFormDataContent();
                using var fileContent = new ByteArrayContent(await File.ReadAllBytesAsync(request.FilePath));
                fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("multipart/form-data");
                form.Add(fileContent, "modelFile", Path.GetFileName(request.FilePath));
                form.Add(new StringContent(SketchFabApi.Source), "source");

                AddCommonModelFields(form, request);

                httpRequestMessage.Content = form;


                var response = await _httpClient.SendAsync(httpRequestMessage, HttpCompletionOption.ResponseContentRead);
                _logger.LogInformation($"{nameof(UploadFileAsync)} responded {response.StatusCode}");
                response.EnsureSuccessStatusCode();
                string uuid = response.Headers.GetValues("Location").FirstOrDefault();
                request.ModelId = uuid;
                _logger.LogInformation("Uploading is complete. Model uuid is " + uuid);
                return uuid;
            }
            catch (Exception ex)
            {
                _logger.LogError($"SketchFab upload error: {ex.Message}");
                throw;
            }

        }
        public async Task UpdateModelAsync(string modelUuid, UploadModelRequest request, string sketchFabToken)
        {
            try
            {
                _logger.LogInformation($"Updating model [{request.Name}].");
                if (string.IsNullOrWhiteSpace(modelUuid))
                {
                    throw new ArgumentNullException(nameof(modelUuid));
                }

                HttpRequestMessage httpRequestMessage = new HttpRequestMessage(HttpMethod.Patch, $"{SketchFabApiUrl}/models/{modelUuid}");
                httpRequestMessage.Headers.Add("Authorization", $"Token {sketchFabToken}");

                using var form = new MultipartFormDataContent();
                form.Headers.ContentType.MediaType = "multipart/form-data";

                AddCommonModelFields(form, request);

                httpRequestMessage.Content = form;

                var response = await _httpClient.SendAsync(httpRequestMessage);

                _logger.LogInformation($"{nameof(UpdateModelAsync)} responded {response.StatusCode}");
                response.EnsureSuccessStatusCode();

            }
            catch (Exception ex)
            {
                _logger.LogError($"SketchFab update error: {ex.Message}");
                throw;
            }

        }
    }
}