﻿using AthenaHealth.Sdk.Models.Converters;
using AthenaHealth.Sdk.Models.Enums;
using Newtonsoft.Json;
using System.IO;

namespace AthenaHealth.Sdk.Models.Request
{
    public class AddDocument
    {
        /// <summary>
        /// Any note to accompany the creation of this document.
        /// </summary>
        [JsonProperty("actionnote")]
        public string ActionNote { get; set; }

        /// <summary>
        /// The appointment ID associated with this document, for certain document classes. These can
        /// only be uploaded AFTER check-in. The department ID is looked up from the appointment.
        /// (Department ID takes precedence if both are supplied.)
        /// </summary>
        [JsonProperty("appointmentid")]
        public int? AppointmentId { get; set; }

        /// <summary>
        /// The file that will become the document. PDFs are recommended. Generally, this implies
        /// that this is a multipart/form-data content-type submission. This does NOT work correctly
        /// in I/O Docs. The filename itself is not used by athenaNet, but it is required to be sent.
        /// </summary>
        [JsonIgnore]
        [JsonProperty("attachmentcontents")]
        public FileInfo Attachment { get; set; }

        /// <summary>
        /// Documents will, normally, automatically appear in the clinical inbox for providers to
        /// review. In some cases, you might want to force the document to skip the clinical inbox,
        /// and go directly to the patient chart with a "closed" status. For that case, set this to true.
        /// </summary>
        [JsonProperty("autoclose")]
        public bool? AutoClose { get; set; }

        /// <summary>
        /// The department ID associated with the uploaded document. Except when appointmentid is
        /// supplied, this is required.
        /// </summary>
        [JsonProperty("departmentid")]
        public int DepartmentId { get; set; }

        /// <summary>
        /// The document subclass.
        /// </summary>
        [JsonProperty("documentsubclass")]
        [JsonConverter(typeof(EnumConverter))]
        public DocumentSubclassEnum DocumentSubclass { get; set; }

        /// <summary>
        /// The 'Internal Note' attached to this document
        /// </summary>
        [JsonProperty("internalnote")]
        public string InternalNote { get; set; }

        /// <summary>
        /// The provider ID attached to this document. This populates the provider name field.
        /// </summary>
        [JsonProperty("providerid")]
        public int? ProviderId { get; set; }

        public AddDocument(int departmentId, FileInfo attachment, DocumentSubclassEnum documentSubclass)
        {
            DepartmentId = departmentId;
            Attachment = attachment;
            DocumentSubclass = documentSubclass;
        }
    }
}
