﻿using AthenaHealth.Sdk.Exceptions;
using AthenaHealth.Sdk.Models;
using AthenaHealth.Sdk.Models.Enums;
using AthenaHealth.Sdk.Models.Request;
using AthenaHealth.Sdk.Models.Response;
using AthenaHealth.Sdk.Tests.EndToEnd.Data.Appointments;
using AthenaHealth.Sdk.Tests.EndToEnd.Fixtures;
using Shouldly;
using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;

// ReSharper disable StringLiteralTypo
namespace AthenaHealth.Sdk.Tests.EndToEnd
{
    public class AppointmentClientTests : IClassFixture<AthenaHealthClientFixture>
    {
        private readonly IAthenaHealthClient _client;

        public AppointmentClientTests(AthenaHealthClientFixture athenaHealthClientFixture)
        {
            _client = athenaHealthClientFixture.Client;
        }

        [Fact]
        public async Task GetAppointmentTypes_ReturnsRecords()
        {
            AppointmentTypeResponse response = await _client.Appointments.GetAppointmentTypes();

            response.Total.ShouldBeGreaterThan(0);
            response.Items.ShouldContain(a => a.Name == "Office Visit");
        }

        [Fact]
        public async Task GetAppointmentTypes_FilterApplied_ReturnsFilteredRecords()
        {

            GetAppointmentTypeFilter filter = new GetAppointmentTypeFilter
            {
                DepartmentIds = new int[] { 1, 2 },
                HideNonPatient = false,
                ProviderIds = new int[] { 1 }
            };

            AppointmentTypeResponse response = await _client.Appointments.GetAppointmentTypes(filter);

            response.Total.ShouldBeGreaterThan(0);
            response.Items.ShouldContain(a => a.Name == "Office Visit");
        }

        [Fact]
        public async Task GetAppointmentType_ValidId_ReturnsAppointmentType()
        {
            AppointmentType appointmentType = await _client.Appointments.GetAppointmentType(622);

            appointmentType.ShouldNotBeNull();
            appointmentType.Name.ShouldNotBeNullOrEmpty();
        }

        [Fact]
        public async Task GetAppointmentType_InvalidId_ThrowException()
        {
            await Should.ThrowAsync<ApiValidationException>(async () => await _client.Appointments.GetAppointmentType(5000000));
        }

        [Fact]
        public async Task GetBookedAppointments_SingleDepartment_ReturnsRecords()
        {
            GetAppointmentsBookedFilter filter = new GetAppointmentsBookedFilter(
                new DateTime(2019, 01, 01),
                new DateTime(2019, 02, 01))
            {
                DepartmentIds = new[] { 1 },
                ShowClaimDetail = true,
                ShowExpectedProcedureCodes = true,
                ShowCopay = true,
                ShowPatientDetail = true,
                ShowInsurance = true,
                ShowReminderCallDetail = true
            };

            AppointmentResponse response = await _client.Appointments.GetBookedAppointments(filter);

            response.Items.ShouldNotBeNull();
            response.Items.ShouldContain(a => a.DepartmentId.HasValue);
            response.Items.ShouldContain(a => a.Date != null);
            response.Items.First().AppointmentStatus.ShouldNotBeNull();
        }

        [Fact]
        public async Task GetBookedAppointments_MultipleDepartments_ReturnsRecords()
        {
            GetAppointmentsBookedFilter filter = new GetAppointmentsBookedFilter(
                new DateTime(2019, 01, 01),
                new DateTime(2019, 02, 01))
            {
                DepartmentIds = new[] { 1, 21 },
                ShowClaimDetail = true,
                ShowExpectedProcedureCodes = true,
                ShowCopay = true,
                ShowPatientDetail = true,
                ShowInsurance = true,
                ShowReminderCallDetail = true
            };

            AppointmentResponse response = await _client.Appointments.GetBookedAppointments(filter);

            response.Items.ShouldNotBeNull();
            response.Items.ShouldContain(a => a.DepartmentId == 1);
            response.Items.ShouldContain(a => a.DepartmentId == 21);
        }

        [Fact]
        public async Task GetById_ValidId_ReturnsAppointment()
        {
            GetAppointmentFilter filter = new GetAppointmentFilter
            {
                ShowClaimDetail = true,
                ShowExpectedProcedureCodes = true,
                ShowCopay = true,
                ShowPatientDetail = true,
                ShowInsurance = true,
            };

            Appointment appointment = await _client.Appointments.GetById(997681, filter);

            appointment.ShouldNotBeNull();
            appointment.DepartmentId.ShouldNotBeNull();
        }

        [Fact]
        public async Task GetById_InvalidId_ThrowsException()
        {
            await Should.ThrowAsync<ApiException>(async () => await _client.Appointments.GetById(0));
        }

        [Theory]
        [ClassData(typeof(GetNotesAppointmentsData))]
        public async Task GetNotes_ValidId_ReturnsRecords(int appointmentId)
        {
            AppointmentNotesResponse response = await _client.Appointments.GetNotes(appointmentId, true);

            response.Items.All(x => !string.IsNullOrWhiteSpace(x.Text)).ShouldBeTrue();
            response.Items.All(x => int.Parse(x.Id) > 0).ShouldBeTrue();
        }

        [Fact]
        public async Task GetNotes_NotExistingId_ThrowsApiValidationException()
        {
            ApiException exception = await Should.ThrowAsync<ApiException>(async ()
                => await _client.Appointments.GetNotes(1));

            exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
            exception.Message.ShouldContain("The appointment is not available");
        }

        [Fact]
        public async Task CreateNote_ValidInput_CreatesNote()
        {
            int appointmentId = 100;
            string noteText = "AL testing " + DateTime.Now.ToLongDateString() + DateTime.Now.ToLongTimeString();
            bool displayOnSchedule = true;

            Should.NotThrow(async ()
                => await _client.Appointments.CreateNote(appointmentId, noteText, displayOnSchedule)
            );

            AppointmentNotesResponse notes = await _client.Appointments.GetNotes(appointmentId);
            AppointmentNote createdNote = notes.Items
                .FirstOrDefault(x => x.Text == noteText && x.DisplayOnSchedule == displayOnSchedule);

            createdNote.ShouldNotBeNull();
        }

        [Fact]
        public async Task CreateNote_NullNoteText_ThrowsApiValidationException()
        {
            ApiException exception = await Should.ThrowAsync<ApiException>(async ()
                => await _client.Appointments.CreateNote(100, null));

            exception.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
            exception.Message.ShouldContain(@"""missingfields"":[""notetext""]");
        }

        [Theory]
        [ClassData(typeof(RemindersSearchData))]
        public async Task SearchReminders_ValidFilter_ReturnsRecords(int departmentId)
        {
            SearchAppointmentRemindersFilter filter = new SearchAppointmentRemindersFilter(
                departmentId,
                new DateTime(2018, 1, 1),
                new DateTime(2019, 12, 31));

            AppointmentRemindersResponse response = await _client.Appointments.SearchReminders(filter);

            response.Total.ShouldBeGreaterThan(0);
            response.Items.All(x => x.ApproximateDate != DateTime.MinValue).ShouldBeTrue();
            response.Items.All(x => x.Id > 0).ShouldBeTrue();
            response.Items.All(x => x.DepartmentId > 0).ShouldBeTrue();
        }

        [Theory]
        [ClassData(typeof(GetReminderByIdData))]
        public async Task GetReminderById_ValidId_ReturnsRecord(int appointmentReminderId)
        {
            AppointmentReminder response = await _client.Appointments.GetReminderById(appointmentReminderId);

            response.ShouldNotBeNull();
            response.Id.ShouldBeGreaterThan(0);
            response.DepartmentId.ShouldBeGreaterThan(0);
            response.ApproximateDate.ShouldNotBe(DateTime.MinValue);
        }

        [Fact]
        public async Task CreateReminder_ValidModel_ReturnsCreatedReminderId()
        {
            var model = new CreateAppointmentReminder(
                new DateTime(2019, 10, 28),
                82,
                31014,
                144);
            CreatedAppointmentReminder response = await _client.Appointments.CreateReminder(model);

            response.ShouldNotBeNull();
            response.Id.ShouldBeGreaterThan(0);
            response.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task CreateReminder_InvalidPatientId_ThrowsApiValidationException()
        {
            var model = new CreateAppointmentReminder(
                new DateTime(2019, 10, 28),
                82,
                0,
                144);
            ApiException exception = await Should.ThrowAsync<ApiException>(async ()
                => await _client.Appointments.CreateReminder(model));

            exception.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
            exception.Message.ShouldContain("A patient ID must be provided");
        }

        [Fact]
        public async Task DeleteReminderById_ValidAppointmentReminderId_NotThrowsException()
        {
            var model = new CreateAppointmentReminder(
                new DateTime(2019, 10, 28),
                82,
                31014,
                144);
            CreatedAppointmentReminder creationResponse = await _client.Appointments.CreateReminder(model);

            Should.NotThrow(async ()
                => await _client.Appointments.DeleteReminderById(creationResponse.Id)
            );
        }

        [Fact]
        public async Task DeleteReminderById_AlreadyDeletedReminder_ThrowsBadRequest()
        {
            // Arrange. Make sure such diagnosis not exists.
            int appointmentReminderId = 15128;

            try
            {
                await _client.Appointments.DeleteReminderById(appointmentReminderId);
            }
            catch (Exception)
            {
                // ignored
            }

            ApiValidationException exception = await Assert.ThrowsAsync<ApiValidationException>(() =>
                    _client.Appointments.DeleteReminderById(appointmentReminderId)
            );

            exception.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
            exception.Message.ShouldContain("That appointment reminder has already been deleted");
        }

        [Fact]
        public async Task GetAppointmentSlots_ReturnsRecords()
        {
            GetAppointmentSlotsFilter filter = new GetAppointmentSlotsFilter(Enumerable.Range(1, 999).ToArray())
            {
                StartDate = new DateTime(2019, 01, 01),
                EndDate = new DateTime(2019, 04, 01),
                IgnoreSchedulablePermission = true,
                AppointmentTypeId = 82,
                ProviderId = new[] { 71 }
            };

            AppointmentSlotResponse response = await _client.Appointments.GetAppointmentSlots(filter);
            response.Total.ShouldBeGreaterThan(0);
            response.Items.ShouldNotBeNull();
            response.Items.ShouldContain(a => a.DepartmentId.HasValue);
            response.Items.First().Date.ShouldNotBeNull();
        }

        [Fact]
        public async Task CreateAppointmentSlot_ValidData_IdReturned()
        {
            CreateAppointmentSlot slot = new CreateAppointmentSlot(
                1,
                86,
                new DateTime(2020, 1, 1),
                new ClockTime[] { new ClockTime(16, 00), new ClockTime(17, 00) })
            {
                ReasonId = 962
            };

            AppointmentSlotCreationResponse response = await _client.Appointments.CreateAppointmentSlot(slot);

            response.AppointmentIds.First(a => a.Value == "16:00").Key.ShouldNotBeNullOrEmpty();
        }

        /// <summary>
        /// This method tests both: booking and cancellation, but before it creates new appointment slot
        /// </summary>
        /// <returns></returns>
        [Fact(Skip = "Below test is slow - takes more than 10 seconds to run")]
        public async Task BookAndCancelAppointment_ValidData_ReturnsAppointment()
        {
            int patientId = 1;
            //Create new appointment slot
            CreateAppointmentSlot slot = new CreateAppointmentSlot(
                1,
                86,
                new DateTime(2020, 1, 1),
                new ClockTime[] { new ClockTime(16, 00) })

            {
                ReasonId = 962

            };
            AppointmentSlotCreationResponse response = await _client.Appointments.CreateAppointmentSlot(slot);
            int appointmentId = int.Parse(response.AppointmentIds.First().Key);

            //Book appointment
            BookAppointment booking = new BookAppointment()
            {
                PatientId = patientId,
                ReasonId = 962,
                IgnoreSchedulablePermission = true
            };
            Appointment appointment = await _client.Appointments.BookAppointment(appointmentId, booking);

            //Assert booking
            appointment.Id.ShouldBe(appointmentId);
            appointment.Date.ShouldNotBeNull();

            //Cancel appointment
            CancelAppointment cancelRequest = new CancelAppointment(patientId, "test");

            Should.NotThrow(() => _client.Appointments.CancelAppointment(appointmentId, cancelRequest));
        }

        [Theory]
        [ClassData(typeof(GetCheckInRequirementsData))]
        public async Task GetCheckInRequirements_ValidId_ReturnsRecord(int appointmentId)
        {
            CheckInRequirement[] response =
                await _client.Appointments.GetCheckInRequirements(appointmentId);

            response.ShouldNotBeNull();
            response.All(x => x.Fields != null).ShouldBeTrue();
            response.All(x => !string.IsNullOrEmpty(x.Name)).ShouldBeTrue();
        }

        [Theory(Skip = "Skipped. Reason: Test execution takes ~32.55 seconds. " +
                       "Once checkedin appointment can't be re checkedin.")]
        [ClassData(typeof(GetCheckInRequirementsData))]
        public void CompleteCheckIn_ValidId_NotThrowsException(int appointmentId)
        {
            Should.NotThrow(async ()
                => await _client.Appointments.GetCheckInRequirements(appointmentId)
            );
        }

        [Fact]
        public async Task CompleteCheckIn_AlreadyCheckedIn_ThrowsException()
        {
            ApiValidationException exception = await Assert.ThrowsAsync<ApiValidationException>(() =>
                _client.Appointments.CompleteCheckIn(2267)
            );

            exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
            exception.Message.ShouldContain("This appointment has already been checked in");
        }

        [Fact]
        public async Task StartCheckIn_AlreadyCheckedIn_ThrowsException()
        {
            ApiValidationException exception = await Assert.ThrowsAsync<ApiValidationException>(() =>
                _client.Appointments.StartCheckIn(2267)
            );

            exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
            exception.Message.ShouldContain("This appointment has already been checked in");
        }

        [Fact]
        public async Task CancelCheckIn_AlreadyCheckedIn_ThrowsException()
        {
            ApiValidationException exception = await Assert.ThrowsAsync<ApiValidationException>(() =>
                _client.Appointments.CancelCheckIn(2267)
            );

            exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
            exception.Message.ShouldContain("This appointment has already been checked in");
        }

        [Theory]
        [ClassData(typeof(GetAppointmentInsurancesData))]
        public async Task GetAppointmentInsurances_ReturnsRecords(int appointmentId)
        {
            GetAppointmentInsurancesFilter filter = new GetAppointmentInsurancesFilter(appointmentId)
            {
                ShowCancelled = true,
                ShowFullSsn = true
            };

            InsuranceResponse response = await _client.Appointments.GetAppointmentInsurances(filter);

            response.Total.ShouldBeGreaterThan(0);
            response.Items.ShouldNotBeNull();
            response.Items.ShouldContain(a => a.InsurancePolicyHolder != null);
        }

        [Fact]
        public async Task GetAppointmentReasons_ReturnsRecords()
        {
            GetAppointmentReasonsFilter filter = new GetAppointmentReasonsFilter(1, 86);

            AppointmentReasonResponse response = await _client.Appointments.GetAppointmentReasons(filter);

            response.Total.ShouldBeGreaterThan(0);
            response.Items.ShouldNotBeNull();
            response.Items.ShouldNotContain(a => a.Id == 0);
            response.Items.ShouldContain(a => !string.IsNullOrEmpty(a.Reason));
        }

        [Fact]
        public async Task GetAppointmentReasonsForExistingPatient_ReturnsRecords()
        {
            GetAppointmentReasonsFilter filter = new GetAppointmentReasonsFilter(1, 86);

            AppointmentReasonResponse response = await _client.Appointments.GetAppointmentReasonsForExistingPatient(filter);

            response.Total.ShouldBeGreaterThan(0);
            response.Items.ShouldNotBeNull();
            response.Items.ShouldNotContain(a => a.Id == 0);
            response.Items.ShouldContain(a => !string.IsNullOrEmpty(a.Reason));
        }

        [Fact]
        public async Task GetAppointmentReasonsForNewPatient_ReturnsRecords()
        {
            GetAppointmentReasonsFilter filter = new GetAppointmentReasonsFilter(1, 86);

            AppointmentReasonResponse response = await _client.Appointments.GetAppointmentReasonsForNewPatient(filter);

            response.Total.ShouldBeGreaterThan(0);
            response.Items.ShouldNotBeNull();
            response.Items.ShouldNotContain(a => a.Id == 0);
            response.Items.ShouldContain(a => !string.IsNullOrEmpty(a.Reason));
        }

        [Fact(Skip = "Below test is slow - takes around 45 seconds to run")]
        public async Task RescheduleAppointment()
        {
            int patientId = 1;
            //Create new appointment slot 1
            CreateAppointmentSlot slot1 = new CreateAppointmentSlot(
                1,
                86,
                new DateTime(2020, 1, 1),
                new ClockTime[] { new ClockTime(16, 00) })
            {
                ReasonId = 962

            };
            AppointmentSlotCreationResponse response = await _client.Appointments.CreateAppointmentSlot(slot1);
            int slot1Id = int.Parse(response.AppointmentIds.First().Key);

            //Create new appointment slot 2
            CreateAppointmentSlot slot2 = new CreateAppointmentSlot(
                1,
                86,
                new DateTime(2020, 1, 2),
                new ClockTime[] { new ClockTime(17, 00) })
            {
                ReasonId = 962

            };
            response = await _client.Appointments.CreateAppointmentSlot(slot2);
            int slot2Id = int.Parse(response.AppointmentIds.First().Key);


            //Book appointment on slot 1
            BookAppointment booking = new BookAppointment()
            {
                PatientId = patientId,
                ReasonId = 962,
                IgnoreSchedulablePermission = true
            };
            Appointment appointment = await _client.Appointments.BookAppointment(slot1Id, booking);

            //Reschedule appointment
            RescheduleAppointment rescheduleRequest = new RescheduleAppointment(slot2Id, patientId, "test");

            Appointment appointmentRescheduled = await _client.Appointments.RescheduleAppointment(slot1Id, rescheduleRequest);

            appointmentRescheduled.ShouldNotBeNull();
            appointmentRescheduled.Id.ShouldBe(slot2Id);
            appointmentRescheduled.PatientId.ShouldBe(1);
        }

        [Fact]
        public async Task GetWaitList_ReturnsRecords()
        {
            var response = await _client.Appointments.GetWaitList();

            response.ShouldNotBeNull();
            response.Items.ShouldAllBe(x => x.Id > 0);
            response.Items.ShouldAllBe(x => x.PatientId > 0);
            response.Items.ShouldAllBe(x => !string.IsNullOrWhiteSpace(x.Created));
        }

        [Fact]
        public async Task AddToWaitList_ReturnsCreatedRecordId()
        {
            AddToWaitListRequest request = new AddToWaitListRequest(100, 1)
            {
                Note = "Just testing",
                Priority = PriorityEnum.Low
            };
            AddToWaitListResponse response = await _client.Appointments.AddToWaitList(request);

            response.ShouldNotBeNull();
            response.WaitListId.ShouldBeGreaterThan(0);
        }

        [Fact]
        public async Task GetAppointmentSubscriptionEvents_ReturnsEvents()
        {
            var response = await _client.Appointments.GetAppointmentSubscriptionEvents();

            response.ShouldNotBeNull();
            response.Subscriptions.Length.ShouldBeGreaterThan(0);
        }

        [Fact]
        public async Task GetChangedAppointmentSlots_ReturnsRecords()
        {
            ChangedAppointmentsSlotResponse response = await _client.Appointments.GetChangedAppointmentSlots();

            response.ShouldNotBeNull();
            response.Items.ShouldAllBe(x => x.AppointmentId > 0);
            response.Items.ShouldAllBe(x => x.Date != DateTime.MinValue);
            response.Items.ShouldAllBe(x => x.StartTime.HasValue);
            response.Items.ShouldAllBe(x => x.DepartmentId.HasValue);
            response.Items.ShouldAllBe(x => x.Duration > 0);
        }

        [Fact]
        public async Task GetAccidentData_ShouldNotThrowSerializationException()
        {
            await _client.Appointments.GetAccidentInfo(1457);
        }

        [Fact]
        public async Task GetCustomFields_ReturnsRecords()
        {
            var response = await _client.Appointments.GetCustomFields();

            response.Total.ShouldBeGreaterThan(0);
            response.Items.Length.ShouldBeGreaterThan(0);
        }
    }
}
