﻿using AthenaHealth.Sdk.Models.Enums;
using AthenaHealth.Sdk.Models.Request.Interfaces;
using Newtonsoft.Json;

// ReSharper disable StringLiteralTypo
// ReSharper disable CommentTypo
namespace AthenaHealth.Sdk.Models.Request
{
    public class GetDocumentsFilter : IPagingFilter
    {
        /// <summary>
        /// The athenaNet department id.
        /// </summary>
        [JsonProperty("departmentid")]
        public int DepartmentId { get; set; }

        /// <summary>
        /// The class(es) of document(s) comma separated.
        /// </summary>
        [JsonProperty("documentclass")]
        public string DocumentClass { get; set; }

        /// <summary>
        /// The document subclass to filter document results.
        /// </summary>
        [JsonProperty("documentsubclass")]
        public string DocumentSubclass { get; set; }

        /// <summary>
        /// Show only documents attached to this encounter.
        /// </summary>
        [JsonProperty("encounterid")]
        public int? EncounterId { get; set; }

        /// <summary>
        /// If set, include orders that were declined
        /// </summary>
        [JsonProperty("showdeclinedorders")]
        public bool? ShowDeclinedOrders { get; set; }

        /// <summary>
        /// By default, deleted documents are not listed. Set to list these.
        /// </summary>
        [JsonProperty("showdeleted")]
        public bool? ShowDeleted { get; set; } = false;

        /// <summary>
        /// The status of the task to filter document results.
        /// </summary>
        [JsonProperty("status")]
        public DocumentStatusEnum? Status { get; set; }

        /// <summary>
        /// Number of entries to return (default 1000, max 10000). Please note that this endpoint has
        /// a different default and max than normal.
        /// </summary>
        public int? Limit { get; set; }

        /// <summary>
        /// Starting point of entries; 0-indexed
        /// </summary>
        public int? Offset { get; set; }

        public GetDocumentsFilter(int departmentId)
        {
            DepartmentId = departmentId;
        }
    }
}
