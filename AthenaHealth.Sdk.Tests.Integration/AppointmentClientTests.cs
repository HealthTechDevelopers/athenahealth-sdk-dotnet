﻿using AthenaHealth.Sdk.Clients.Interfaces;
using AthenaHealth.Sdk.Exceptions;
using AthenaHealth.Sdk.Models;
using AthenaHealth.Sdk.Models.Enums;
using AthenaHealth.Sdk.Models.Request;
using AthenaHealth.Sdk.Models.Response;
using AthenaHealth.Sdk.Tests.Integration.TestingHelpers;
using Castle.Core.Internal;
using Shouldly;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

// ReSharper disable StringLiteralTypo
namespace AthenaHealth.Sdk.Tests.Integration
{
    public class AppointmentClientTests
    {
        [Fact]
        public async Task GetAppointmentTypes_ReturnsRecords()
        {
            IAppointmentClient client = new Clients.AppointmentClient(ConnectionFactory.CreateFromFile(@"Data\Appointment\GetAppointmentTypes.json"));

            AppointmentTypeResponse response = await client.GetAppointmentTypes();

            response.Total.ShouldBe(466);
            response.Items.ShouldContain(a => a.Name == "Office Visit");
        }

        [Fact]
        public async Task GetAppointmentType_ValidId_ReturnsAppointmentType()
        {
            IAppointmentClient client = new Clients.AppointmentClient(ConnectionFactory.CreateFromFile(@"Data\Appointment\GetAppointmentType.json"));

            AppointmentType appointmentType = await client.GetAppointmentType(622);

            appointmentType.ShouldNotBeNull();
            appointmentType.Name.ShouldBe("Sick Visit");
        }

        [Fact]
        public async Task GetAppointmentType_InvalidId_ThrowException()
        {
            IAppointmentClient client = new Clients.AppointmentClient(ConnectionFactory.Create("[]"));

            await Should.ThrowAsync<ApiValidationException>(async () => await client.GetAppointmentType(5000000));
        }

        [Fact]
        public async Task GetBookedAppointments_ReturnsRecords()
        {
            IAppointmentClient client = new Clients.AppointmentClient(ConnectionFactory.CreateFromFile(@"Data\Appointment\GetBookedAppointments.json"));

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

            AppointmentResponse response = await client.GetBookedAppointments(filter);

            response.Total.ShouldBe(2031);
            response.Items.ShouldNotBeNull();
            response.Items.Length.ShouldBe(1000);
            response.Items.ShouldContain(a => a.DepartmentId.HasValue);
            response.Items.First().Date.ShouldNotBeNull();
            response.Items.First().AppointmentStatus.ShouldNotBeNull();
        }

        [Fact]
        public async Task GetById_ValidId_ReturnsAppointment()
        {
            IAppointmentClient client = new Clients.AppointmentClient(ConnectionFactory.CreateFromFile(@"Data\Appointment\GetAppointment.json"));

            GetAppointmentFilter filter = new GetAppointmentFilter
            {
                ShowClaimDetail = true,
                ShowExpectedProcedureCodes = true,
                ShowCopay = true,
                ShowPatientDetail = true,
                ShowInsurance = true,
            };

            Appointment appointment = await client.GetById(997681, filter);

            appointment.ShouldNotBeNull();
            appointment.DepartmentId.ShouldNotBeNull();
        }

        [Fact]
        public async Task GetNotes_ValidId_ReturnsEmptyResult()
        {
            IAppointmentClient client = new Clients.AppointmentClient(ConnectionFactory.CreateFromFile(@"Data\Appointment\GetNotes.json"));

            AppointmentNotesResponse response = await client.GetNotes(2, true);

            response.Items.All(x => !string.IsNullOrWhiteSpace(x.Text)).ShouldBeTrue();
            response.Items.All(x => int.Parse(x.Id) > 0).ShouldBeTrue();
        }

        [Fact]
        public void CreateNote_ValidInput_NotThrowsException()
        {
            IAppointmentClient client = new Clients.AppointmentClient(ConnectionFactory.CreateFromFile(@"Data\Appointment\GetNotes.json"));

            Should.NotThrow(async () => await client.CreateNote(100, "testing", true));
        }

        [Fact]
        public async Task SearchReminders_ValidFilter_ReturnsRecords()
        {
            IAppointmentClient client = new Clients.AppointmentClient(ConnectionFactory.CreateFromFile(@"Data\Appointment\SearchReminders.json"));

            SearchAppointmentRemindersFilter filter = new SearchAppointmentRemindersFilter(
                1,
                new DateTime(2018, 1, 1),
                new DateTime(2019, 12, 31));

            AppointmentRemindersResponse response = await client.SearchReminders(filter);

            response.Total.ShouldBeGreaterThan(0);
            response.Items.All(x => x.ApproximateDate != DateTime.MinValue).ShouldBeTrue();
            response.Items.All(x => x.Id > 0).ShouldBeTrue();
            response.Items.All(x => x.DepartmentId > 0).ShouldBeTrue();
        }

        [Fact]
        public async Task GetReminderById_ExistingId_ReturnsRecord()
        {
            IAppointmentClient client = new Clients.AppointmentClient(ConnectionFactory.CreateFromFile(@"Data\Appointment\GetReminderById.json"));

            var response = await client.GetReminderById(15042);

            response.ShouldNotBeNull();
            response.Id.ShouldBe(15042);
            response.DepartmentId.ShouldBe(150);
            response.PatientId.ShouldBe(33339);
        }

        [Fact]
        public async Task CreateReminder_ValidModel_ReturnsCreatedReminder()
        {
            IAppointmentClient client = new Clients.AppointmentClient(ConnectionFactory.CreateFromFile(@"Data\Appointment\CreateReminder.json"));

            var model = new CreateAppointmentReminder(
                new DateTime(2019, 10, 28),
                82,
                31014,
                144);
            CreatedAppointmentReminder response = await client.CreateReminder(model);

            response.ShouldNotBeNull();
            response.Id.ShouldBe(15123);
            response.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public void DeleteReminderById_ValidAppointmentReminderId_NotThrowsException()
        {
            IAppointmentClient client = new Clients.AppointmentClient(ConnectionFactory.Create(@"{ ""success"": true }"));

            Should.NotThrow(async () => await client.DeleteReminderById(15128));
        }

        [Fact]
        public async Task GetOpenAppointments_ReturnsRecords()
        {
            IAppointmentClient client = new Clients.AppointmentClient(ConnectionFactory.CreateFromFile(@"Data\Appointment\GetAppointmentSlots.json"));

            GetAppointmentSlotsFilter filter = new GetAppointmentSlotsFilter(new int[] { 1 });

            AppointmentSlotResponse response = await client.GetAppointmentSlots(filter);
            response.Total.ShouldBe(131);
            response.Items.ShouldNotBeNull();
            response.Items.Length.ShouldBe(131);
            response.Items.ShouldContain(a => a.DepartmentId.HasValue);
            response.Items.First().Date.ShouldNotBeNull();
            response.Items.First(a => a.StartTime != null).StartTime.ToString().IsNullOrEmpty();
        }

        [Fact]
        public async Task CreateAppointmentSlot_ValidData_IdReturned()
        {
            IAppointmentClient client = new Clients.AppointmentClient(ConnectionFactory.Create("{\"appointmentids\": {\"1205956\": \"16:00\"}}"));

            CreateAppointmentSlot slot = new CreateAppointmentSlot(
                1,
                86,
                new DateTime(2020, 1, 1),
                new ClockTime[] { new ClockTime(16, 00) })
            {
                ReasonId = 962
            };

            AppointmentSlotCreationResponse response = await client.CreateAppointmentSlot(slot);

            response.AppointmentIds.First(a => a.Key == "1205956" && a.Value == "16:00").ShouldNotBeNull();
        }

        [Fact]
        public async Task BookAppointment_ValidData_ReturnsAppointment()
        {
            IAppointmentClient client = new Clients.AppointmentClient(ConnectionFactory.CreateFromFile(@"Data\Appointment\BookAppointment.json"));

            BookAppointment booking = new BookAppointment()
            {
                PatientId = 1,
                ReasonId = 962,
                IgnoreSchedulablePermission = true
            };

            Appointment appointment = await client.BookAppointment(1205967, booking);
            appointment.Id = 1205967;
            appointment.Date.ShouldNotBeNull();
        }

        [Fact]
        public void CancelAppointment_ValidData_NoExceptionIsThrown()
        {
            IAppointmentClient client = new Clients.AppointmentClient(ConnectionFactory.Create(""));

            CancelAppointment cancelRequest = new CancelAppointment(1, "test");

            Should.NotThrow(() => client.CancelAppointment(1, cancelRequest));
        }

        [Fact]
        public async Task GetCheckInRequirements_ExistingId_ReturnsRecords()
        {
            IAppointmentClient client = new Clients.AppointmentClient(ConnectionFactory.CreateFromFile(@"Data\Appointment\GetCheckInRequirements.json"));

            var response = await client.GetCheckInRequirements(1313);

            response.ShouldNotBeNull();
            response.All(x => x.Fields != null).ShouldBeTrue();
            response.All(x => !string.IsNullOrEmpty(x.Name)).ShouldBeTrue();
        }

        [Fact]
        public void CompleteCheckIn_ExistingId_NotThrowsException()
        {
            IAppointmentClient client = new Clients.AppointmentClient(ConnectionFactory.Create(@"{ ""success"": true }"));

            Should.NotThrow(async () => await client.CompleteCheckIn(2267));
        }

        [Fact]
        public void StartCheckIn_ExistingId_NotThrowsException()
        {
            IAppointmentClient client = new Clients.AppointmentClient(ConnectionFactory.Create(@"{ ""success"": true }"));

            Should.NotThrow(async () => await client.StartCheckIn(2267));
        }

        [Fact]
        public void CancelCheckIn_ExistingId_NotThrowsException()
        {
            IAppointmentClient client = new Clients.AppointmentClient(ConnectionFactory.Create(@"{ ""success"": true }"));

            Should.NotThrow(async () => await client.CancelCheckIn(2267));
        }

        [Fact]
        public async Task GetAppointmentInsurances_ReturnsRecords()
        {
            IAppointmentClient client = new Clients.AppointmentClient(ConnectionFactory.CreateFromFile(@"Data\Appointment\GetAppointmentInsurances.json"));
            GetAppointmentInsurancesFilter filter = new GetAppointmentInsurancesFilter(23133)
            {
                ShowCancelled = true,
                ShowFullSsn = true
            };

            InsuranceResponse response = await client.GetAppointmentInsurances(filter);

            response.Total.ShouldBe(1);
            response.Items.ShouldNotBeNull();
            response.Items.First().InsurancePolicyHolderCountryCode.ShouldBe("USA");
        }

        [Fact]
        public async Task GetAppointmentReasons_ReturnsRecords()
        {
            IAppointmentClient client = new Clients.AppointmentClient(ConnectionFactory.CreateFromFile(@"Data\Appointment\GetAppointmentReasons.json"));

            GetAppointmentReasonsFilter filter = new GetAppointmentReasonsFilter(1, 86);

            AppointmentReasonResponse response = await client.GetAppointmentReasons(filter);

            response.Total.ShouldBeGreaterThan(0);
            response.Items.ShouldNotBeNull();
            response.Items.ShouldContain(a => a.ReasonType == AppointmentReasonTypeEnum.All);
            response.Items.ShouldContain(a => a.ReasonType == AppointmentReasonTypeEnum.Existing);
            response.Items.ShouldContain(a => a.ReasonType == AppointmentReasonTypeEnum.New);
        }

        [Fact]
        public async Task GetAppointmentReasonsForNewPatient_ReturnsRecords()
        {
            IAppointmentClient client = new Clients.AppointmentClient(ConnectionFactory.CreateFromFile(@"Data\Appointment\GetAppointmentReasonsForNewPatient.json"));

            GetAppointmentReasonsFilter filter = new GetAppointmentReasonsFilter(1, 86);

            AppointmentReasonResponse response = await client.GetAppointmentReasonsForNewPatient(filter);

            response.Total.ShouldBeGreaterThan(0);
            response.Items.ShouldNotBeNull();
            response.Items.ShouldAllBe(a => a.ReasonType != AppointmentReasonTypeEnum.Existing);
        }

        [Fact]
        public async Task GetAppointmentReasonsForExistingPatient_ReturnsRecords()
        {
            IAppointmentClient client = new Clients.AppointmentClient(ConnectionFactory.CreateFromFile(@"Data\Appointment\GetAppointmentReasonsForExistingPatient.json"));

            GetAppointmentReasonsFilter filter = new GetAppointmentReasonsFilter(1, 86);

            AppointmentReasonResponse response = await client.GetAppointmentReasonsForExistingPatient(filter);

            response.Total.ShouldBeGreaterThan(0);
            response.Items.ShouldNotBeNull();
            response.Items.ShouldAllBe(a => a.ReasonType != AppointmentReasonTypeEnum.New);
        }

        [Fact]
        public async Task RescheduleAppointment()
        {
            IAppointmentClient client = new Clients.AppointmentClient(ConnectionFactory.CreateFromFile(@"Data\Appointment\RescheduleAppointment.json"));

            RescheduleAppointment rescheduleRequest = new RescheduleAppointment(1206017, 1, "test");

            Appointment appointmentRescheduled = await client.RescheduleAppointment(1, rescheduleRequest);

            appointmentRescheduled.ShouldNotBeNull();
            appointmentRescheduled.Id.ShouldBe(1206017);
            appointmentRescheduled.PatientId.ShouldBe(1);
        }

        [Fact]
        public async Task GetWaitList_ReturnsRecords()
        {
            IAppointmentClient client = new Clients.AppointmentClient(ConnectionFactory.CreateFromFile(@"Data\Appointment\GetWaitList.json"));

            var response = await client.GetWaitList();

            response.ShouldNotBeNull();
            response.Items.ShouldAllBe(x => x.Id > 0);
            response.Items.ShouldAllBe(x => x.PatientId > 0);
            response.Items.ShouldAllBe(x => !string.IsNullOrWhiteSpace(x.Created));
        }

        [Fact]
        public async Task AddToWaitList_ReturnsCreatedRecordId()
        {
            IAppointmentClient client = new Clients.AppointmentClient(ConnectionFactory.CreateFromFile(@"Data\Appointment\AddToWaitList.json"));

            AddToWaitListRequest request = new AddToWaitListRequest(100, 1)
            {
                Note = "Just testing",
                Priority = PriorityEnum.Low
            };
            AddToWaitListResponse response = await client.AddToWaitList(request);

            response.ShouldNotBeNull();
            response.WaitListId.ShouldBeGreaterThan(0);
        }

        [Fact]
        public async Task GetChangedAppointmentSlots_ReturnsRecords()
        {
            IAppointmentClient client = new Clients.AppointmentClient(ConnectionFactory.CreateFromFile(@"Data\Appointment\GetChangedAppointmentSlots.json"));

            var response = await client.GetChangedAppointmentSlots();

            response.ShouldNotBeNull();
            response.Items.ShouldAllBe(x => x.AppointmentId > 0);
            response.Items.ShouldAllBe(x => x.Date != DateTime.MinValue);
            response.Items.ShouldAllBe(x => x.StartTime.HasValue);
            response.Items.ShouldAllBe(x => x.DepartmentId.HasValue);
            response.Items.ShouldAllBe(x => x.Duration > 0);
        }

        [Fact]
        public async Task GetCustomFields_ReturnsRecords()
        {
            IAppointmentClient client = new Clients.AppointmentClient(ConnectionFactory.CreateFromFile(@"Data\Appointment\GetCustomFields.json"));

            var response = await client.GetCustomFields();

            response.Total.ShouldBeGreaterThan(0);
            response.Items.Length.ShouldBeGreaterThan(0);
        }
    }
}
