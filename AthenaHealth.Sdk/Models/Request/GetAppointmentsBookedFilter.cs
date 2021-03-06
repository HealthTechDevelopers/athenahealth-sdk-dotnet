﻿using AthenaHealth.Sdk.Models.Converters;
using AthenaHealth.Sdk.Models.Enums;
using AthenaHealth.Sdk.Models.Request.Interfaces;
using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

// ReSharper disable CommentTypo
// ReSharper disable StringLiteralTypo
namespace AthenaHealth.Sdk.Models.Request
{
    public class GetAppointmentsBookedFilter : GetAppointmentFilter, IPagingFilter
    {
        /// <summary>
        /// Filter appointments by status.
        /// </summary>
        [JsonProperty("appointmentstatus")]
        public AppointmentStatusEnum? AppointmentStatus { get; set; }

        /// <summary>
        /// Filter by appointment type ID.
        /// </summary>
        [JsonProperty("appointmenttypeid")]
        public int? AppointmentTypeId { get; set; }

        /// <summary>
        /// The athenaNet department ID. Multiple IDs (either as a comma delimited list or multiple
        /// POSTed values) are allowed.
        /// </summary>
        [Required]
        [JsonProperty("departmentid")]
        [JsonConverter(typeof(DelimitedStringConverter), ",")]
        public int[] DepartmentIds { get; set; }

        /// <summary>
        /// Start of the appointment search date range (mm/dd/yyyy). Inclusive.
        /// </summary>
        [Required]
        [JsonConverter(typeof(DateConverter))]
        [JsonProperty("startdate")]
        public DateTime StartDate { get; set; }

        /// <summary>
        /// End of the appointment search date range (mm/dd/yyyy). Inclusive.
        /// </summary>
        [Required]
        [JsonConverter(typeof(DateConverter))]
        [JsonProperty("enddate")]
        public DateTime EndDate { get; set; }

        /// <summary>
        /// Identify appointments modified after this date/time (mm/dd/yyyy hh:mi:ss). Inclusive.
        /// </summary>
        [JsonConverter(typeof(DateConverter), "MM/dd/yyyy HH:mm:ss")]
        [JsonProperty("startlastmodified")]
        public DateTime? StartLastModified { get; set; }

        /// <summary>
        /// Identify appointments modified prior to this date/time (mm/dd/yyyy hh:mi:ss). Inclusive.
        /// Note: This can only be used if a startlastmodified value is supplied as well.
        /// </summary>
        [JsonConverter(typeof(DateConverter), "MM/dd/yyyy HH:mm:ss")]
        [JsonProperty("endlastmodified")]
        public DateTime? EndLastModified { get; set; }

        /// <summary>
        /// The athenaNet patient ID. If operating in a Provider Group Enterprise practice, this
        /// should be the enterprise patient ID.
        /// </summary>
        [JsonProperty("patientid")]
        public int? PatientId { get; set; }

        /// <summary>
        /// The athenaNet provider ID. Multiple IDs (either as a comma delimited list or multiple
        /// POSTed values) are allowed.
        /// </summary>
        [JsonProperty("providerid")]
        [JsonConverter(typeof(DelimitedStringConverter), ",")]
        public int[] ProviderId { get; set; }

        /// <summary>
        /// Start of the appointment scheduled search date range (mm/dd/yyyy). Inclusive.
        /// </summary>
        [JsonConverter(typeof(DateConverter))]
        [JsonProperty("scheduledstartdate")]
        public DateTime? ScheduledStartDate { get; set; }

        /// <summary>
        /// End of the appointment scheduled search date range (mm/dd/yyyy). Inclusive.
        /// </summary>
        [JsonConverter(typeof(DateConverter))]
        [JsonProperty("scheduledenddate")]
        public DateTime? ScheduledEndDate { get; set; }

        /// <summary>
        /// Include appointments that have been cancelled.
        /// </summary>
        [JsonProperty("showcancelled")]
        public bool ShowCancelled { get; set; } = false;

        /// <summary>
        /// Include all reminder call related results, if available, associated with an appointment.
        /// </summary>
        [JsonProperty("showremindercalldetail")]
        public bool ShowReminderCallDetail { get; set; }

        /// <summary>
        /// Number of entries to return (default 1000, max 10000). Please note that this endpoint
        /// has a different default and max than normal.
        /// </summary>
        public int? Limit { get; set; } = 1000;

        /// <summary>
        /// Starting point of entries; 0-indexed
        /// </summary>
        public int? Offset { get; set; }

        public GetAppointmentsBookedFilter(DateTime startDate, DateTime endDate)
        {
            StartDate = startDate;
            EndDate = endDate;
        }
    }
}
