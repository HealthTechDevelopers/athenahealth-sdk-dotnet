﻿using AthenaHealth.Sdk.Models.Converters;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net;

namespace AthenaHealth.Sdk.Http
{
    public class Response
    {
        private static readonly EnumConverter EnumConverter = new EnumConverter();

        /// <summary>
        /// Raw response body. Typically a string, but when requesting images, it will be a byte array.
        /// </summary>
        public object Body { get; }

        /// <summary>
        /// Information about the API.
        /// </summary>
        public IReadOnlyDictionary<string, string> Headers { get; }

        /// <summary>
        /// The response status code.
        /// </summary>
        public HttpStatusCode StatusCode { get; set; }

        /// <summary>
        /// The content type of the response.
        /// </summary>
        public string ContentType { get; }

        public bool IsSuccessStatusCode { get; set; }

        public DateTime Time { get; } = DateTime.Now;

        public Response() : this(new Dictionary<string, string>())
        {
        }

        public Response(IDictionary<string, string> headers)
        {
            Headers = new ReadOnlyDictionary<string, string>(headers);
        }

        public Response(HttpStatusCode statusCode, object body, IDictionary<string, string> headers, string contentType, bool isSuccessStatusCode)
        {
            StatusCode = statusCode;
            Body = body;
            Headers = new ReadOnlyDictionary<string, string>(headers);
            ContentType = contentType;
            IsSuccessStatusCode = isSuccessStatusCode;
        }

        /// <summary>
        /// Deserializes response content to object of specified type.
        /// </summary>
        /// <typeparam name="T">Object type</typeparam>
        /// <param name="shouldThrowErrors">True: Throws exception if invalid response object.</param>
        /// <returns>Object of specified type</returns>
        public T GetObjectContent<T>(bool shouldThrowErrors = false)
        {
            JsonSerializerSettings settings = new JsonSerializerSettings()
            {
                MissingMemberHandling = MissingMemberHandling.Error,
                TraceWriter = new MemoryTraceWriter(),
                Error = (object sender, ErrorEventArgs args) =>
                {
                    Debug.WriteLine($"ERROR: {args.ErrorContext.Error.Message}");
                    args.ErrorContext.Handled = !shouldThrowErrors;
                },
                Converters = new List<JsonConverter>()
                {
                    EnumConverter
                }
            };

            return Body == null ? default : JsonConvert.DeserializeObject<T>(Body.ToString(), settings);
        }
    }
}
