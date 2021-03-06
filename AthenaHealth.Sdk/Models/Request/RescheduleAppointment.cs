﻿using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

// ReSharper disable StringLiteralTypo
// ReSharper disable CommentTypo
namespace AthenaHealth.Sdk.Models.Request
{
    public class RescheduleAppointment
    {
        /// <summary>
        /// By default, we allow booking of appointments marked as schedulable via the web.
        /// This flag allows you to bypass that restriction for booking.
        /// </summary>
        [JsonProperty("ignoreschedulablepermission")]
        // ReSharper disable once IdentifierTypo
        public bool? IgnoreSchedulablePermission { get; set; }

        /// <summary>
        /// The appointment ID of the new appointment.
        /// (The appointment ID in the URL is the ID of the currently scheduled appointment.)
        /// </summary>
        [JsonProperty("newappointmentid")]
        [Required]
        public int NewAppointmentId { get; set; }

        /// <summary>
        /// By default, we create a patient case upon booking an appointment for new patients.
        /// Setting this to true bypasses that patient case.
        /// </summary>
        [JsonProperty("nopatientcase")]
        public bool? NoPatientCase { get; set; }

        /// <summary>
        /// The athenaNet patient ID.
        /// </summary>
        [JsonProperty("patientid")]
        [Required]
        public int PatientId { get; set; }

        /// <summary>
        /// The appointment reason ID to be booked. If not provided, the same reason used in the original appointment will be used.
        /// Note: when getting open appointment slots, a special reason of -1 will return appointment slots for any reason.
        /// This is not recommended, however, because actual availability does depend on a real reason.
        /// In addition, appointment availability when using -1 does not account for the ability to not allow appointments to be scheduled
        /// too close to the current time (because that limit is set on a per appointment reason basis).
        /// </summary>
        [JsonProperty("reasonid")]
        public int? ReasonId { get; set; }

        /// <summary>
        /// A text explanation why the appointment is being rescheduled
        /// </summary>
        [JsonProperty("reschedulereason")]
        public string RescheduleReason { get; set; }

        public RescheduleAppointment(int newAppointmentId, int patientId, string rescheduleReason)
        {
            NewAppointmentId = newAppointmentId;
            PatientId = patientId;
            RescheduleReason = rescheduleReason;
        }
    }
}
