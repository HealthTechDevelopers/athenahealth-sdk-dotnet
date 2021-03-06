﻿// ReSharper disable IdentifierTypo
// ReSharper disable StringLiteralTypo
// ReSharper disable CommentTypo

using AthenaHealth.Sdk.Models.Converters;
using AthenaHealth.Sdk.Models.Enums;
using AthenaHealth.Sdk.Models.Request.Interfaces;
using Newtonsoft.Json;

namespace AthenaHealth.Sdk.Models.Request
{
    public class GetPrescriptionsFilter : IPagingFilter
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
        public bool ShowDeleted { get; set; } = false;

        /// <summary>
        /// The status of the task to filter document results.
        /// </summary>
        [JsonProperty("status")]
        [JsonConverter(typeof(EnumConverter))]
        public PrescriptionStatusEnum? Status { get; set; }

        /// <summary>
        /// Number of entries to return (default 1500, max 5000)
        /// </summary>
        public int? Limit { get; set; }

        /// <summary>
        /// Starting point of entries; 0-indexed
        /// </summary>
        public int? Offset { get; set; }

        public GetPrescriptionsFilter(int departmentId)
        {
            DepartmentId = departmentId;
        }
    }
}
