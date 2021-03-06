﻿using AthenaHealth.Sdk.Exceptions;
using AthenaHealth.Sdk.Models.Enums;
using AthenaHealth.Sdk.Models.Request;
using AthenaHealth.Sdk.Models.Response;
using AthenaHealth.Sdk.Tests.EndToEnd.Data.Patient;
using AthenaHealth.Sdk.Tests.EndToEnd.Fixtures;
using Bogus;
using Bogus.DataSets;
using Shouldly;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Xunit;

// ReSharper disable StringLiteralTypo
namespace AthenaHealth.Sdk.Tests.EndToEnd
{
    public class PatientClientTests : IClassFixture<AthenaHealthClientFixture>
    {
        private readonly IAthenaHealthClient _client;

        private readonly Faker _faker;

        private readonly Faker<CreatePatient> _createPatientFaker;

        public PatientClientTests(AthenaHealthClientFixture athenaHealthClientFixture)
        {
            _client = athenaHealthClientFixture.Client;

            _faker = new Faker();
            _createPatientFaker = new Faker<CreatePatient>()
                .CustomInstantiator(f =>
                {
                    return new CreatePatient(
                        1,
                        f.Date.Past(80).Date,
                        CultureInfo.CurrentCulture.TextInfo.ToTitleCase(f.Name.FirstName(Name.Gender.Male).ToLower()),
                        CultureInfo.CurrentCulture.TextInfo.ToTitleCase(f.Name.LastName(Name.Gender.Male).ToLower()));
                }).RuleFor(x => x.Email, (f, u) => f.Internet.Email(u.FirstName, u.LastName).ToLower());
        }

        [Theory]
        [ClassData(typeof(GetDocumentsData))]
        public async Task GetDocuments_DocumentsExists_ShouldNotThrowJsonSerializationException(int patientId)
        {
            // Arrange
            // Act
            var result = await _client.Patients.GetDocuments(patientId, new GetDocumentsFilter(1)
            {
                ShowDeclinedOrders = true,
                ShowDeleted = true
            });

            // Assert
            result.ShouldNotBeNull();
            result.Items.Count().ShouldBeGreaterThan(0);
        }

        [Theory]
        [ClassData(typeof(GetPatientCustomFieldsData))]
        public async Task GetPatientCustomFields_FieldsExist_ShouldNotThrowJsonSerializationException(int patientId)
        {
            // Arrange
            // Act
            var result = await _client.Patients.GetPatientCustomFields(patientId, 1);

            // Assert
            result.ShouldNotBeNull();
            result.Length.ShouldBeGreaterThan(0);

            Debug.WriteLine($"yield return new object[] {{ {patientId} }};");
        }

        [Fact]
        public void GetDefaultPharmacy_NotExistingPharmacy_ThrowsException()
        {
            // Arrange
            // Act
            // Assert
            Should.Throw<ApiValidationException>(async () => await _client.Patients.GetDefaultPharmacy(300, 2));
        }

        [Theory]
        [ClassData(typeof(GetAllergiesData))]
        public async Task GetAllergies_ResultsExists_ShouldNotThrowJsonSerializationException(int patientId)
        {
            // Arrange
            // Act
            var result = await _client.Patients.GetAllergies(patientId, 1, true);

            // Assert
            result.ShouldNotBeNull();
            result.Allergies.Count().ShouldBeGreaterThan(0);
        }

        [Theory]
        [ClassData(typeof(GetSocialHistoryData))]
        public async Task GetSocialHistory_HistoryExists_ShouldNotThrowJsonSerializationException(int patientId)
        {
            // Arrange
            // Act
            var result = await _client.Patients.GetSocialHistory(patientId, new GetSocialHistoryFilter(1)
            {
                ShowNotPerformedQuestions = true,
                ShowUnansweredQuestions = true
            });

            // Assert
            result.ShouldNotBeNull();
            result.Questions.Length.ShouldBeGreaterThan(0);
        }

        [Fact]
        public async Task UpdateSocialHistory_AddingAnswers_HistoryUpdated()
        {
            // Arrange
            CreatePatient request = _createPatientFaker.Generate();

            var createResult = await _client.Patients.CreatePatient(request);

            var socialHistoryTemplates = await _client.Dictionaries.GetSocialHistoryTemplates();

            var distinctQuestions = socialHistoryTemplates
                .SelectMany(x => x.Questions)
                .GroupBy(x => x.Key)
                .Select(x => x.First())
                .ToList();

            var templateIds = socialHistoryTemplates
                .Select(x => x.Id)
                .ToArray();

            await _client.Patients.UpdateSocialHistoryTemplates(createResult.PatientId.Value, new UpdateSocialHistoryTemplates(1, templateIds));

            var answers = distinctQuestions.Select(x =>
            {
                var question = new UpdateSocialHistory.Question(x.Key);

                if (x.InputType == InputTypeEnum.YesNo)
                    question.Answer = _faker.PickRandom(new string[] { "y", "n" });
                if (x.InputType == InputTypeEnum.FreeText)
                    question.Answer = _faker.Lorem.Sentence(10, 20);
                if (x.InputType == InputTypeEnum.Numeric)
                    question.Answer = _faker.Random.Int(0, 20).ToString();
                if (x.InputType == InputTypeEnum.DropDown)
                    question.Answer = _faker.PickRandom(x.Options.Keys);

                return question;
            }).ToArray();

            // Act
            await _client.Patients.UpdateSocialHistory(createResult.PatientId.Value, new UpdateSocialHistory(1)
            {
                Questions = answers
            });

            // Assert
            var socialHistory = await _client.Patients.GetSocialHistory(createResult.PatientId.Value, new GetSocialHistoryFilter(1));

            socialHistory.Questions.Length.ShouldBe(answers.Length);
            socialHistory.Questions.ShouldAllBe(x => answers.Any(y => y.Key == x.Key && y.Answer == x.Answer));
        }

        [Theory]
        [ClassData(typeof(GetDefaultLaboratoryData))]
        public async Task GetDefaultLaboratory_LaboratoryExists_ShouldNotThrowJsonSerializationException(int patientId)
        {
            // Arrange
            // Act
            var result = await _client.Patients.GetDefaultLaboratory(patientId, 1);

            // Assert
            result.ShouldNotBeNull();
        }

        [Fact]
        public void GetDefaultLaboratory_NotExistingLaboratory_ThrowsException()
        {
            // Arrange
            // Act
            // Assert
            var exception = Should.Throw<ApiValidationException>(async () => await _client.Patients.GetDefaultLaboratory(10054, 1));

            exception.Message.ShouldContain("Default lab not found.");
            exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        }

        [Theory]
        [ClassData(typeof(GetPatientInsurancesData))]
        public async Task GetPatientInsurances_InsurancesExists_ShouldNotThrowJsonSerializationException(int patientId)
        {
            // Arrange
            // Act
            var result = await _client.Patients.GetPatientInsurances(patientId, new GetPatientInsurancesFilter()
            {
                ShowCancelled = true,
                ShowFullSSN = true
            });

            // Assert
            result.ShouldNotBeNull();
            result.Items.Length.ShouldBeGreaterThan(0);
        }

        [Theory]
        [ClassData(typeof(GetPatientEncountersData))]
        public async Task GetPatientEncounters_EncountersExists_ShouldNotThrowJsonSerializationException(int patientId)
        {
            // Arrange
            // Act
            var result = await _client.Patients.GetPatientEncounters(patientId, new GetPatientEncountersFilter(1)
            {
                ShowAllStatuses = true,
                ShowAllTypes = true,
                ShowDiagnoses = true
            });

            // Assert
            result.ShouldNotBeNull();
            result.Items.Length.ShouldBeGreaterThan(0);
        }

        [Theory]
        [ClassData(typeof(GetSocialHistoryTemplatesData))]
        public async Task GetSocialHistoryTemplates_TemplateExists_ShouldNotThrowJsonSerializationException(int patientId)
        {
            // Arrange
            // Act
            var result = await _client.Patients.GetSocialHistoryTemplates(patientId, 1);

            // Assert
            result.ShouldNotBeNull();
            result.Length.ShouldBeGreaterThan(0);
        }

        [Fact]
        public async Task UpdateSocialHistoryTemplates_TemplateExists_TemplatesAdded()
        {
            // Arrange
            CreatePatient request = _createPatientFaker.Generate();

            var createResult = await _client.Patients.CreatePatient(request);

            var socialHistoryTemplates = await _client.Dictionaries.GetSocialHistoryTemplates();

            var templateIds = socialHistoryTemplates
                .Where(x => _faker.Random.Bool())
                .Select(x => x.Id)
                .ToArray();

            // Act
            await _client.Patients.UpdateSocialHistoryTemplates(createResult.PatientId.Value, new UpdateSocialHistoryTemplates(1, templateIds));

            // Assert
            var socialHistoryTemplatesAfter = await _client.Patients.GetSocialHistoryTemplates(createResult.PatientId.Value, 1);

            socialHistoryTemplatesAfter.ShouldNotBeNull();
            socialHistoryTemplatesAfter.Length.ShouldBe(templateIds.Length);
            socialHistoryTemplatesAfter.ShouldAllBe(x => templateIds.Any(y => y == x.Id));
        }

        [Fact]
        public async Task UpdateSocialHistoryTemplates_TemplateExists_TemplatesRemoved()
        {
            // Arrange
            CreatePatient request = _createPatientFaker.Generate();

            var createResult = await _client.Patients.CreatePatient(request);

            var socialHistoryTemplates = await _client.Dictionaries.GetSocialHistoryTemplates();

            var templateIds = socialHistoryTemplates
                .Where(x => _faker.Random.Bool())
                .Select(x => x.Id)
                .ToArray();

            await _client.Patients.UpdateSocialHistoryTemplates(createResult.PatientId.Value, new UpdateSocialHistoryTemplates(1, templateIds));

            // Act
            await _client.Patients.UpdateSocialHistoryTemplates(createResult.PatientId.Value, new UpdateSocialHistoryTemplates(1, Array.Empty<int>()));

            // Assert
            var socialHistoryTemplatesAfter = await _client.Patients.GetSocialHistoryTemplates(createResult.PatientId.Value, 1);

            socialHistoryTemplatesAfter.ShouldNotBeNull();
            socialHistoryTemplatesAfter.Length.ShouldBe(0);
        }

        [Fact]
        public async Task UpdateSocialHistoryTemplates_TemplateExists_TemplatesChanged()
        {
            // Arrange
            CreatePatient request = _createPatientFaker.Generate();

            var createResult = await _client.Patients.CreatePatient(request);

            var socialHistoryTemplates = await _client.Dictionaries.GetSocialHistoryTemplates();

            var templateIds = socialHistoryTemplates
                .Where(x => _faker.Random.Bool())
                .Select(x => x.Id)
                .ToArray();

            await _client.Patients.UpdateSocialHistoryTemplates(createResult.PatientId.Value, new UpdateSocialHistoryTemplates(1, templateIds));

            templateIds = socialHistoryTemplates
                .Where(x => _faker.Random.Bool())
                .Select(x => x.Id)
                .ToArray();

            // Act
            await _client.Patients.UpdateSocialHistoryTemplates(createResult.PatientId.Value, new UpdateSocialHistoryTemplates(1, templateIds));

            // Assert
            var socialHistoryTemplatesAfter = await _client.Patients.GetSocialHistoryTemplates(createResult.PatientId.Value, 1);

            socialHistoryTemplatesAfter.ShouldNotBeNull();
            socialHistoryTemplatesAfter.Length.ShouldBe(templateIds.Length);
            socialHistoryTemplatesAfter.ShouldAllBe(x => templateIds.Any(y => y == x.Id));
        }

        [Theory]
        [ClassData(typeof(GetMedicalHistoryData))]
        public async Task GetMedicalHistory_ResultsExists_ShouldNotThrowJsonSerializationException(int patientId)
        {
            // Arrange
            // Act
            var result = await _client.Patients.GetMedicalHistory(patientId, 1);

            // Assert
            result.ShouldNotBeNull();
            result.Questions.Length.ShouldBeGreaterThan(0);
        }

        [Theory]
        [ClassData(typeof(GetMedicationsData))]
        public async Task GetMedications_MedicationsExists_ShouldNotThrowJsonSerializationException(int patientId)
        {
            // Arrange
            // Act
            var result = await _client.Patients.GetMedications(patientId, new GetMedicationsFilter(1));

            // Assert
            result.ShouldNotBeNull();
            result.Medications.Length.ShouldBeGreaterThan(0);
        }

        [Theory]
        [ClassData(typeof(GetPrescriptionsData))]
        public async Task GetPrescriptions_PrescriptionsExists_ShouldNotThrowJsonSerializationException(int patientId)
        {
            // Arrange
            // Act
            var result = await _client.Patients.GetPrescriptions(patientId, new GetPrescriptionsFilter(1)
            {
                ShowDeclinedOrders = true,
                ShowDeleted = true
            });

            // Assert
            result.ShouldNotBeNull();
            result.Items.Length.ShouldBeGreaterThan(0);
        }

        [Theory]
        [ClassData(typeof(GetLabResultsData))]
        public async Task GetLabResults_ResultsExists_ShouldNotThrowJsonSerializationException(int patientId)
        {
            // Arrange
            // Act
            var result = await _client.Patients.GetLabResults(patientId, new GetLabResultsFilter(1)
            {
                ShowAbnormalDetails = true,
                ShowHidden = true,
                ShowTemplate = true
            });

            // Assert
            result.ShouldNotBeNull();
            result.Items.Length.ShouldBeGreaterThan(0);
        }

        [Theory]
        [ClassData(typeof(GetLabResultDetailsData))]
        public async Task GetLabResultDetails_ResultsExists_ShouldNotThrowJsonSerializationException(int patientId, int labResultId)
        {
            // Arrange
            // Act
            var result = await _client.Patients.GetLabResultDetails(patientId, labResultId, true);

            // Assert
            result.ShouldNotBeNull();
        }

        [Theory]
        [ClassData(typeof(GetOrdersData))]
        public async Task GetAnalytes_AnalytesExists_ShouldNotThrowJsonSerializationException(int patientId)
        {
            // Arrange
            // Act
            var result = await _client.Patients.GetAnalytes(patientId, new GetAnalytesFilter(1)
            {
                ShowAbnormalDetails = true,
                ShowHidden = true,
                ShowTemplate = true
            });

            // Assert
            result.ShouldNotBeNull();
            result.Items.Length.ShouldBeGreaterThan(0);
        }

        [Theory]
        [ClassData(typeof(EnhancedBestmatchData))]
        public async Task EnhancedBestmatch_SearchByFirstNameLastNameAndDob_ShouldNotThrowJsonSerializationException(string firstName, string lastName, string dob)
        {
            // Arrange
            DateTime dateOfBirth = DateTime.ParseExact(dob, "MM/dd/yyyy", CultureInfo.InvariantCulture);

            // Act
            var result = await _client.Patients.EnhancedBestmatch(new EnhancedBestmatchFilter(dateOfBirth, firstName, lastName)
            {
                ShowAllClaims = true,
                ShowAllPatientDepartmentStatus = true,
                ShowBalanceDetails = true,
                Show2015EdCehrtValues = true,
                ShowCustomFields = true,
                ShowFullSsn = true,
                ShowInsurance = true,
                ShowLocalPatientId = true,
                ShowPortalStatus = true,
                MinScore = 1
            });

            // Assert
            result.ShouldNotBeNull();
            result.Count().ShouldBeGreaterThan(0);
        }

        [Theory]
        [ClassData(typeof(GetPatientsData))]
        public async Task GetPatients_SearchByFirstName_ShouldNotThrowJsonSerializationException(string firstName, int offset)
        {
            // Arrange
            // Act
            var result = await _client.Patients.GetPatients(new GetPatientsFilter()
            {
                FirstName = firstName,
                DepartmentId = 1,
                Offset = offset,
                OmitBalances = false,
                OmitDefaultPharmacy = false,
                OmitPhotoInformation = false,
                Show2015EdCehrtValues = true
            });

            // Assert
            result.ShouldNotBeNull();
        }

        [Fact(Skip = "This test is slow (about 23 seconds) and creates user every time is run")]
        public async Task UpdatePatient_AddressChanged_PatientUpdated()
        {
            // Arrange
            CreatePatient request = _createPatientFaker.Generate();

            var createResult = await _client.Patients.CreatePatient(request);

            var getPatientResultBeforeUpdate = await _client.Patients.GetPatientById(createResult.PatientId.Value);

            // Act
            var updateResult = await _client.Patients.UpdatePatient(createResult.PatientId.Value, new UpdatePatient()
            {
                Address1 = "Sample Address"
            });

            // Assert
            updateResult.ShouldNotBeNull();

            var getPatientResultAfterUpdate = await _client.Patients.GetPatientById(createResult.PatientId.Value);

            getPatientResultBeforeUpdate.Address1.ShouldBeNullOrEmpty();
            getPatientResultAfterUpdate.Address1.ShouldBe("Sample Address");
        }

        [Fact(Skip = "This test is slow (about 23 seconds) and creates user every time is run")]
        public async Task DeletePatient_StatusChanged_PatientDeleted()
        {
            // Arrange
            _createPatientFaker.RuleFor(x => x.Status, (f, u) => StatusEnum.Active);

            CreatePatient request = _createPatientFaker.Generate();

            var createResult = await _client.Patients.CreatePatient(request);

            var getPatientResultBeforeDelete = await _client.Patients.GetPatientById(createResult.PatientId.Value);

            // Act
            var deleteResult = await _client.Patients.DeletePatient(createResult.PatientId.Value);

            // Assert
            deleteResult.ShouldNotBeNull();

            var getPatientResultAfterDelete = await _client.Patients.GetPatientById(createResult.PatientId.Value);

            getPatientResultBeforeDelete.Status.ShouldBe(StatusEnum.Active);
            getPatientResultAfterDelete.Status.ShouldBe(StatusEnum.Inactive);
        }

        [Fact(Skip = "This test is slow (about 12 seconds) and creates user every time is run")]
        public async Task CreatePatient_RequiredFieldsProvidedPatientDoesNotExist_PatientCreated()
        {
            // Arrange
            CreatePatient request = _createPatientFaker.Generate();

            // Act
            var result = await _client.Patients.CreatePatient(request);

            // Assert
            result.ShouldNotBeNull();
            result.PatientId.HasValue.ShouldBeTrue();

            var getPatientResult = await _client.Patients.GetPatientById(result.PatientId.Value);

            getPatientResult.FirstName.ShouldBe(request.FirstName);
            getPatientResult.LastName.ShouldBe(request.LastName);
            getPatientResult.DateOfBirth.ShouldBe(request.DateOfBirth);
            getPatientResult.DepartmentId.ShouldBe(request.DepartmentId);
            getPatientResult.Email.ShouldBe(request.Email);
        }

        [Fact]
        public async Task CreatePatient_RequiredFieldsProvidedPatientExists_PatientNotCreated()
        {
            // Arrange
            CreatePatient request = new CreatePatient(1, new DateTime(1992, 01, 02), "Jan", "Kowalski")
            {
                Email = "kowal234@gmail.com"
            };

            // Act
            var result = await _client.Patients.CreatePatient(request);

            // Assert
            result.ShouldNotBeNull();
            result.PatientId.HasValue.ShouldBeTrue();
            result.PatientId.Value.ShouldBe(42000);
            result.ErrorMessage.ShouldBeNullOrEmpty();

            var getPatientResult = await _client.Patients.GetPatientById(result.PatientId.Value);

            getPatientResult.FirstName.ShouldBe(request.FirstName);
            getPatientResult.LastName.ShouldBe(request.LastName);
            getPatientResult.DateOfBirth.ShouldBe(request.DateOfBirth);
            getPatientResult.DepartmentId.ShouldBe(request.DepartmentId);
            getPatientResult.Email.ShouldBe(request.Email);
        }

        [Fact]
        public async Task CreatePatient_RequiredFieldsProvidedPatientExistsFlagToShowErrors_PatientNotCreated()
        {
            // Arrange
            CreatePatient request = new CreatePatient(1, new DateTime(1992, 01, 02), "Jan", "Kowalski")
            {
                Email = "kowal234@gmail.com",
                ShowErrorMessage = true
            };

            // Act
            var result = await _client.Patients.CreatePatient(request);

            // Assert
            result.ShouldNotBeNull();
            result.PatientId.HasValue.ShouldBeFalse();
            result.ErrorMessage.ShouldNotBeNullOrEmpty();
        }

        [Theory]
        [ClassData(typeof(GetPatientProblemsData))]
        public async Task GetPatientProblems_PatientExists_ShouldNotThrowJsonSerializationException(int patientId)
        {
            // Arrange
            // Act
            var result = await _client.Patients.GetPatientProblems(patientId, new GetPatientProblemsFilter(1)
            {
                ShowDiagnosisInfo = true,
                ShowInactive = true
            });

            // Assert
            result.ShouldNotBeNull();
            result.Problems.Length.ShouldBeGreaterThan(0);
        }

        [Theory]
        [ClassData(typeof(GetPatientByIdData))]
        public async Task GetPatientById_PatientExists_ShouldNotThrowJsonSerializationException(int patientId)
        {
            // Arrange
            // Act
            var result = await _client.Patients.GetPatientById(patientId, new GetPatientByIdFilter()
            {
                ShowAllClaims = true,
                ShowAllPatientDepartmentStatus = true,
                ShowBalanceDetails = true,
                Show2015EdCehrtValues = true,
                ShowCustomFields = true,
                ShowFullSsn = true,
                ShowInsurance = true,
                ShowLocalPatientId = true,
                ShowPortalStatus = true
            });

            // Assert
            result.ShouldNotBeNull();
        }

        [Fact]
        public void GetPatientById_PatientDoesNotExists_ShouldThrowException()
        {
            // Arrange
            var queryParameters = new GetPatientByIdFilter()
            {
                ShowAllClaims = true,
                ShowAllPatientDepartmentStatus = true,
                ShowBalanceDetails = true,
                Show2015EdCehrtValues = true,
                ShowCustomFields = true,
                ShowFullSsn = true,
                ShowInsurance = true,
                ShowLocalPatientId = true,
                ShowPortalStatus = true
            };

            // Act
            ApiValidationException exception = Should.Throw<ApiValidationException>(async () => await _client.Patients.GetPatientById(0, queryParameters));

            // Assert
            exception.ShouldNotBeNull();
        }

        [Fact]
        public async Task EnhancedBestmatch_SearchByFirstNameLastNameAndDob_NoMatch()
        {
            // Arrange
            DateTime dateOfBirth = DateTime.ParseExact("01/01/1982", "MM/dd/yyyy", CultureInfo.InvariantCulture);

            // Act
            var result = await _client.Patients.EnhancedBestmatch(new EnhancedBestmatchFilter(dateOfBirth, "InvalidName", "InvalidName")
            {
                ShowAllClaims = true,
                ShowAllPatientDepartmentStatus = true,
                ShowBalanceDetails = true,
                Show2015EdCehrtValues = true,
                ShowCustomFields = true,
                ShowFullSsn = true,
                ShowInsurance = true,
                ShowLocalPatientId = true,
                ShowPortalStatus = true
            });

            // Assert
            result.ShouldNotBeNull();
            result.Count().ShouldBe(0);
        }

        [Fact]
        public async Task GetDefaultPharmacy_ReturnsPharmacy()
        {
            Pharmacy pharmacy = await _client.Patients.GetDefaultPharmacy(300, 1);

            // Assert
            pharmacy.State.ShouldNotBeEmpty();
            pharmacy.ClinicalProviderId.ShouldBeGreaterThan(0);
        }

        [Fact]
        public async Task GetPreferredPharmacies_ReturnsPharmacy()
        {
            PharmacyResponse pharmacies = await _client.Patients.GetPreferredPharmacies(300, new GetPreferredPharmacyFilter(1));

            pharmacies.ShouldNotBeNull();
            pharmacies.Total.ShouldBeGreaterThan(0);
            pharmacies.Items.ShouldNotBeNull();
            pharmacies.Items.Length.ShouldBeGreaterThan(0);
            pharmacies.Items[0].ClinicalProviderId.ShouldBeGreaterThan(0);
        }

        [Fact]
        public async Task GetPreferredPharmacies_NotExistingPharmacy_ThrowsException()
        {
            await Should.ThrowAsync<ApiValidationException>(async () => await _client.Patients.GetPreferredPharmacies(300, new GetPreferredPharmacyFilter(2)));
        }

        [Fact]
        public void SetDefaultPharmacy_ValidData_NotThrow()
        {
            Should.NotThrow(async () => await _client.Patients.SetDefaultPharmacy(5000, new SetPharmacyRequest(164) { ClinicalProviderId = 11242674 }));
        }

        [Fact]
        public void AddPreferredPharmacy_ValidData_NotThrow()
        {
            Should.NotThrow(async () => await _client.Patients.AddPreferredPharmacy(5000, new SetPharmacyRequest(164) { ClinicalProviderId = 11242674 }));
        }

        [Fact]
        public async Task AddMedication_ValidData_ReturnsCreatedId()
        {
            MedicationAddedResponse response = await _client.Patients.AddMedication(100, new AddMedication(1, 296232));

            response.MedicationEntryId.ShouldNotBeNullOrWhiteSpace();
            response.IsSuccess.ShouldBeTrue();
        }

        [Fact]
        public async Task AddMedication_InvalidMedicationId_ReturnsErrorMessage()
        {
            int medicationId = 1;
            MedicationAddedResponse response = await _client.Patients.AddMedication(100, new AddMedication(1, medicationId));

            response.IsSuccess.ShouldBeFalse();
            response.ErrorMessage.ShouldContain($"MedicationID {medicationId} does not match any known medications.");
        }

        [Fact]
        public void SetMedicationSettings_ValidModel_NotThrowsException()
        {
            Should.NotThrow(async () => await _client.Patients.SetMedicationSettings(100, new MedicationSetting(1, "Test 123")));
        }

        [Fact]
        public async Task GetPatientAppointments_WithFilter_ReturnsAppointments()
        {
            GetPatientAppointmentFilter filter = new GetPatientAppointmentFilter
            {
                ShowCancelled = true,
                ShowExpectedProcedureCodes = true,
                ShowPast = true,
                Limit = 500
            };
            AppointmentResponse response = await _client.Patients.GetPatientAppointments(1, filter);

            response.Total.ShouldBeGreaterThan(0);
            response.Items.ShouldNotBeNull();
            response.Items.Length.ShouldBeGreaterThan(0);
            response.Items.ShouldContain(a => a.Date != null);
        }

        [Fact]
        public async Task GetPatientAppointments_NoFilter_ReturnsAppointments()
        {
            AppointmentResponse response = await _client.Patients.GetPatientAppointments(1);

            response.Total.ShouldBeGreaterThan(0);
            response.Items.ShouldNotBeNull();
            response.Items.Length.ShouldBeGreaterThan(0);
            response.Items.ShouldContain(a => a.Date != null);
        }

        [Fact]
        public void SetPatientDefaultLaboratory_NotThrowsException()
        {
            Should.NotThrow(async () => await _client.Patients.SetPatientDefaultLaboratory(1, 1, 10943173));
        }

        [Fact]
        public void SetAllergies_NotThrowsException()
        {
            SetPatientAllergies request = new SetPatientAllergies(1)
            {
                SectionNote = "test2",
                Allergies = new[]
                {
                    new SetPatientAllergies.Allergy
                    {
                        Id = 13549,
                        AllergenName = "Allantoin",
                        Note = "allergy test 3",
                        Reactions = new[]
                        {
                            new SetPatientAllergies.Allergy.Reaction()
                            {
                                ReactionName = "chest pain",
                                Severity = "moderate to severe",
                                SeveritySnomedCode = "371924009",
                                SnomedCode = "29857009"
                            }
                        }
                    }
                }
            };

            Should.NotThrow(async () => await _client.Patients.SetAllergies(111, request));
        }

        [Fact]
        public async Task CreateInsurance_ExistingInsurance_ThrowsApiValidationException()
        {
            // Arrange.
            try
            {
                await _client.Patients.CreateInsurance(100,
                    new CreateInsurance(31724, SequenceEnum.Primary, "1842", "Test1", "Test2", SexEnum.Male));
            }
            catch
            {
                // ignored
            }

            var exception = Should.Throw<ApiValidationException>(async () => await _client.Patients.CreateInsurance(100,
                new CreateInsurance(31724, SequenceEnum.Primary, "1842", "Test1", "Test2", SexEnum.Male)));

            exception.StatusCode.ShouldBe(HttpStatusCode.Conflict);
            exception.Message.ShouldContain("An existing insurance package exists. Use PUT to update or DELETE to deactivate.");
        }

        [Fact(Skip = "It takes ~22 seconds to complete.")]
        public async Task CreateInsurance_NotExistingInsurance_NotThrowsException()
        {
            // Arrange.
            try
            {
                await _client.Patients.DeleteInsurance(100, SequenceEnum.Primary);
            }
            catch
            {
                // ignored
            }

            // Act.
            Insurance response = await _client.Patients.CreateInsurance(100,
                 new CreateInsurance(
                     31724,
                     SequenceEnum.Primary,
                     "1842",
                     "Test1",
                     "Test2",
                     SexEnum.Male)
                 );

            response.InsuranceId.HasValue.ShouldBeTrue();
            response.InsuranceIdNumber.Length.ShouldBeGreaterThan(0);
        }

        [Fact(Skip = "It takes ~21 seconds to complete.")]
        public async Task DeleteInsurance_ExistingInsurance_NotThrowsException()
        {
            // Arrange.
            try
            {
                await _client.Patients.CreateInsurance(100,
                        new CreateInsurance(31724, SequenceEnum.Primary, "1842", "Test1", "Test2", SexEnum.Male));
            }
            catch
            {
                // ignored
            }

            // Act.
            await _client.Patients.DeleteInsurance(100, SequenceEnum.Primary);
        }

        [Fact]
        public async Task DeleteInsurance_NotExistingInsurance_NotThrowsException()
        {
            // Arrange.
            try
            {
                await _client.Patients.DeleteInsurance(100, SequenceEnum.Primary);
            }
            catch
            {
                // ignored
            }

            // Act.
            ApiValidationException exception = Should.Throw<ApiValidationException>(async () =>
                await _client.Patients.DeleteInsurance(100, SequenceEnum.Primary));

            // Assert.
            exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
            exception.Message.ShouldContain("No insurance package is active at the sequence number given.");
        }

        [Fact(Skip = "This test is slow (about 12 seconds) and creates user every time is run")]
        public async Task AddPatientProblem_RequiredFieldsProvided_ProblemAdded()
        {
            // Arrange
            var createPatientResult = await _client.Patients.CreatePatient(_createPatientFaker.Generate());

            // Act
            var result = await _client.Patients.AddProblem(createPatientResult.PatientId.Value, new AddProblem(1, "52967002")
            {
                Laterality = LateralityEnum.Bilateral,
                Note = "note",
                StartDate = DateTime.Today,
                Status = ProblemStatusEnum.Chronic
            });

            // Assert
            result.ShouldNotBeNull();
            result.ProblemId.HasValue.ShouldBeTrue();

            var getPatientProblemsResponse = await _client.Patients.GetPatientProblems(createPatientResult.PatientId.Value, new GetPatientProblemsFilter(1));

            getPatientProblemsResponse.Problems.Length.ShouldBe(1);
            getPatientProblemsResponse.Problems.First().Code.ShouldBe("52967002");
            getPatientProblemsResponse.Problems.First().Events.First().Note.ShouldBe("note");
            getPatientProblemsResponse.Problems.First().Events.First().Laterality.ShouldBe(LateralityEnum.Bilateral);
            getPatientProblemsResponse.Problems.First().Events.First().Status.ShouldBe(ProblemStatusEnum.Chronic);
        }

        [Fact]
        public async Task UpdateInsurance_ExistingInsurance_NotThrowsException()
        {
            // Arrange.
            var insurance = new CreateInsurance(31724, SequenceEnum.Primary, "1842", "Test1", "Test2", SexEnum.Male);

            try
            {
                await _client.Patients.CreateInsurance(100, insurance);
            }
            catch
            {
                // ignored
            }

            await _client.Patients.UpdateInsurance(100, insurance);
        }

        [Fact(Skip = "This test is slow (about 14 seconds) and creates user every time is run")]
        public async Task AddDocument_DocumentWithFile_DocumentCreated()
        {
            // Arrange
            CreatePatient request = _createPatientFaker.Generate();

            var createResult = await _client.Patients.CreatePatient(request);

            // Act
            var result = await _client.Patients.AddDocument(createResult.PatientId.Value, new AddDocument(1, new FileInfo(@"Data\Patient\document.pdf"), DocumentSubclassEnum.Consent));

            // Assert
            result.ShouldNotBeNull();
            result.DocumentId.HasValue.ShouldBeTrue();
        }

        [Fact]
        public async Task GetPrivacyInformation_ValidRequest_ReturnsRecord()
        {
            var result = await _client.Patients.GetPrivacyInformation(34772, 1);

            result.ShouldNotBeNull();
        }

        [Fact(Skip = "It takes approximately ~11 seconds for the test to complete.")]
        public async Task SetPrivacyInformation_ValidRequest_ReturnsPatientId()
        {
            // Arrange
            SetPrivacyInformation request = new SetPrivacyInformation(
                1,
                new DateTime(2019, 11, 5, 14, 53, 0),
                "TestSignature"
            );

            int patientId = 34772;


            // Act
            var result = await _client.Patients.SetPrivacyInformation(patientId, request);

            // Assert
            result.ShouldNotBeNull();
            result.PatientId.ShouldBe(patientId);
        }

        [Fact(Skip = "This test is slow (about 12 seconds) and creates user every time is run")]
        public async Task UpdateMedicalHistory_AddingAnswers_HistoryUpdated()
        {
            // Arrange
            CreatePatient request = _createPatientFaker.Generate();

            var createResult = await _client.Patients.CreatePatient(request);

            var medicalHistoryQuestions = await _client.Dictionaries.GetMedicalHistoryQuestions();

            var medicalHistoryBefore = await _client.Patients.GetMedicalHistory(createResult.PatientId.Value, 1);

            var answers = medicalHistoryQuestions.Items.Select(x => new UpdateMedicalHistory.Question(x.QuestionId)
            {
                Answer = _faker.PickRandom(new AnswerEnum?[] { AnswerEnum.Yes, AnswerEnum.No, AnswerEnum.Unspecified, null })
            }).ToArray();

            // Act
            await _client.Patients.UpdateMedicalHistory(createResult.PatientId.Value, new UpdateMedicalHistory(1)
            {
                Questions = answers
            });

            // Assert
            var medicalHistoryAfter = await _client.Patients.GetMedicalHistory(createResult.PatientId.Value, 1);

            medicalHistoryAfter.Questions.Length.ShouldBe(answers.Where(x => x.Answer.HasValue).Count(x => x.Answer.Value != AnswerEnum.Unspecified));
            medicalHistoryAfter.Questions.ShouldAllBe(x => answers.Any(y => y.Id == x.Id && y.Answer == x.Answer));
        }

        [Fact(Skip = "This test is slow (about 12 seconds) and creates user every time is run")]
        public async Task UpdateMedicalHistory_DeletingAnswers_HistoryUpdated()
        {
            // Arrange
            CreatePatient request = _createPatientFaker.Generate();

            var createResult = await _client.Patients.CreatePatient(request);

            var medicalHistoryQuestions = await _client.Dictionaries.GetMedicalHistoryQuestions();

            var medicalHistoryBefore = await _client.Patients.GetMedicalHistory(createResult.PatientId.Value, 1);

            var answers = medicalHistoryQuestions.Items.Select(x => new UpdateMedicalHistory.Question(x.QuestionId)
            {
                Answer = _faker.PickRandom(new AnswerEnum?[] { AnswerEnum.Yes, AnswerEnum.No })
            }).ToArray();

            await _client.Patients.UpdateMedicalHistory(createResult.PatientId.Value, new UpdateMedicalHistory(1)
            {
                Questions = answers
            });

            foreach (var answer in answers)
            {
                answer.Delete = true;
            }

            // Act
            await _client.Patients.UpdateMedicalHistory(createResult.PatientId.Value, new UpdateMedicalHistory(1)
            {
                Questions = answers
            });

            // Assert
            var medicalHistoryAfter = await _client.Patients.GetMedicalHistory(createResult.PatientId.Value, 1);

            medicalHistoryAfter.Questions.Length.ShouldBe(0);
        }

        [Fact]
        public async Task UpdatePhoto_ImageProvided_PhotoUpdated()
        {
            // Arrange
            CreatePatient request = _createPatientFaker.Generate();

            var createResult = await _client.Patients.CreatePatient(request);

            // Act
            await _client.Patients.UpdatePhoto(createResult.PatientId.Value, new UpdatePhoto(new FileInfo(@"Data\Patient\photo.png")));

            // Assert
            var photoBytes = await _client.Patients.GetPhoto(createResult.PatientId.Value, true);

            photoBytes.ShouldNotBeNull();
            photoBytes.Length.ShouldBeGreaterThan(0);
        }

        [Fact]
        public async Task GetPhoto_PhotoAvailable_PhotoBytesDownloaded()
        {
            // Arrange
            CreatePatient request = _createPatientFaker.Generate();

            var createResult = await _client.Patients.CreatePatient(request);

            await _client.Patients.UpdatePhoto(createResult.PatientId.Value, new UpdatePhoto(new FileInfo(@"Data\Patient\photo.png")));

            // Act
            var photoBytes = await _client.Patients.GetPhoto(createResult.PatientId.Value, true);

            // Assert
            photoBytes.ShouldNotBeNull();
            photoBytes.Length.ShouldBeGreaterThan(0);
        }

        [Fact]
        public async Task GetPhoto_PhotoNotAvailable_PhotoBytesDownloaded()
        {
            // Arrange
            CreatePatient request = _createPatientFaker.Generate();

            var createResult = await _client.Patients.CreatePatient(request);

            // Act
            ApiException exception = Should.Throw<ApiException>(async () => await _client.Patients.GetPhoto(createResult.PatientId.Value, true));

            // Assert
            exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
            exception.Message.ShouldContain("No valid image was found.");
        }

        [Fact]
        public async Task DeletePhoto_PhotoAvailable_PhotoDeleted()
        {
            // Arrange
            CreatePatient request = _createPatientFaker.Generate();

            var createResult = await _client.Patients.CreatePatient(request);

            await _client.Patients.UpdatePhoto(createResult.PatientId.Value, new UpdatePhoto(new FileInfo(@"Data\Patient\photo.png")));

            // Act
            await _client.Patients.DeletePhoto(createResult.PatientId.Value);

            // Assert
            ApiException exception = Should.Throw<ApiException>(async () => await _client.Patients.GetPhoto(createResult.PatientId.Value, true));

            exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
            exception.Message.ShouldContain("No valid image was found.");
        }

        [Fact]
        public async Task DeletePhoto_PhotoNotAvailable_PhotoDeleted()
        {
            // Arrange
            CreatePatient request = _createPatientFaker.Generate();

            var createResult = await _client.Patients.CreatePatient(request);

            // Act
            await _client.Patients.DeletePhoto(createResult.PatientId.Value);

            // Assert
            ApiException exception = Should.Throw<ApiException>(async () => await _client.Patients.GetPhoto(createResult.PatientId.Value, true));

            exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
            exception.Message.ShouldContain("No valid image was found.");
        }

        [Fact]
        public async Task UpdateInsuranceCardPhoto_ImageProvided_PhotoUpdated()
        {
            // Arrange
            CreatePatient request = _createPatientFaker.Generate();

            var createResult = await _client.Patients.CreatePatient(request);

            var insurance = await _client.Patients.CreateInsurance(createResult.PatientId.Value, new CreateInsurance(31724, SequenceEnum.Primary, "1842", "Test1", "Test2", SexEnum.Male));

            // Act
            await _client.Patients.UpdateInsuranceCardPhoto(createResult.PatientId.Value, insurance.InsuranceId.Value, new UpdateInsuranceCardPhoto(new FileInfo(@"Data\Patient\photo.png")));

            // Assert
            var photoBytes = await _client.Patients.GetInsuranceCardPhoto(createResult.PatientId.Value, insurance.InsuranceId.Value, true);

            photoBytes.ShouldNotBeNull();
            photoBytes.Length.ShouldBeGreaterThan(0);
        }

        [Fact]
        public async Task GetInsuranceCardPhoto_PhotoAvailable_PhotoBytesDownloaded()
        {
            // Arrange
            CreatePatient request = _createPatientFaker.Generate();

            var createResult = await _client.Patients.CreatePatient(request);

            var insurance = await _client.Patients.CreateInsurance(createResult.PatientId.Value, new CreateInsurance(31724, SequenceEnum.Primary, "1842", "Test1", "Test2", SexEnum.Male));

            await _client.Patients.UpdateInsuranceCardPhoto(createResult.PatientId.Value, insurance.InsuranceId.Value, new UpdateInsuranceCardPhoto(new FileInfo(@"Data\Patient\photo.png")));

            // Act
            var photoBytes = await _client.Patients.GetInsuranceCardPhoto(createResult.PatientId.Value, insurance.InsuranceId.Value, true);

            // Assert
            photoBytes.ShouldNotBeNull();
            photoBytes.Length.ShouldBeGreaterThan(0);
        }

        [Fact]
        public async Task GetInsuranceCardPhoto_PhotoNotAvailable_PhotoBytesDownloaded()
        {
            // Arrange
            CreatePatient request = _createPatientFaker.Generate();

            var createResult = await _client.Patients.CreatePatient(request);

            var insurance = await _client.Patients.CreateInsurance(createResult.PatientId.Value, new CreateInsurance(31724, SequenceEnum.Primary, "1842", "Test1", "Test2", SexEnum.Male));

            // Act
            ApiException exception = Should.Throw<ApiException>(async () => await _client.Patients.GetInsuranceCardPhoto(createResult.PatientId.Value, insurance.InsuranceId.Value, true));

            // Assert
            exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
            exception.Message.ShouldContain("No valid image was found.");
        }

        [Fact]
        public async Task DeleteInsuranceCardPhoto_PhotoAvailable_PhotoDeleted()
        {
            // Arrange
            CreatePatient request = _createPatientFaker.Generate();

            var createResult = await _client.Patients.CreatePatient(request);

            var insurance = await _client.Patients.CreateInsurance(createResult.PatientId.Value, new CreateInsurance(31724, SequenceEnum.Primary, "1842", "Test1", "Test2", SexEnum.Male));

            await _client.Patients.UpdateInsuranceCardPhoto(createResult.PatientId.Value, insurance.InsuranceId.Value, new UpdateInsuranceCardPhoto(new FileInfo(@"Data\Patient\photo.png")));

            // Act
            await _client.Patients.DeleteInsuranceCardPhoto(createResult.PatientId.Value, insurance.InsuranceId.Value);

            // Assert
            ApiException exception = Should.Throw<ApiException>(async () => await _client.Patients.GetInsuranceCardPhoto(createResult.PatientId.Value, insurance.InsuranceId.Value, true));

            exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
            exception.Message.ShouldContain("No valid image was found.");
        }

        [Fact]
        public async Task DeleteInsuranceCardPhoto_PhotoNotAvailable_PhotoDeleted()
        {
            // Arrange
            CreatePatient request = _createPatientFaker.Generate();

            var createResult = await _client.Patients.CreatePatient(request);

            var insurance = await _client.Patients.CreateInsurance(createResult.PatientId.Value, new CreateInsurance(31724, SequenceEnum.Primary, "1842", "Test1", "Test2", SexEnum.Male));

            // Act
            await _client.Patients.DeleteInsuranceCardPhoto(createResult.PatientId.Value, insurance.InsuranceId.Value);

            // Assert
            ApiException exception = Should.Throw<ApiException>(async () => await _client.Patients.GetInsuranceCardPhoto(createResult.PatientId.Value, insurance.InsuranceId.Value, true));

            exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
            exception.Message.ShouldContain("No valid image was found.");
        }

        [Fact]
        public async Task CreateOrderGroup_ValidRequest_ReturnsEncounterId()
        {
            // Act
            var result = await _client.Patients.CreateOrderGroup(34772, new CreateOrderGroup(1));

            // Assert
            result.ShouldNotBeNull();
            result.EncounterId.ShouldBeGreaterThan(0);
        }

        [Theory]
        [ClassData(typeof(GetPatientChartListData))]
        public async Task GetPatientChartList_ChartListExists_ShouldNotThrowJsonSerializationException(int patientId)
        {
            // Arrange
            // Act
            var result = await _client.Patients.GetPatientChartList(patientId);

            // Assert
            result.ShouldNotBeNull();
            result.Items.Count().ShouldBeGreaterThan(0);
        }

        [Theory]
        [ClassData(typeof(GetPatientCasesData))]
        public async Task GetPatientCases_ShouldNotThrowJsonSerializationException(int patientId)
        {
            // Arrange
            // Act
            var result = await _client.Patients.GetPatientCases(patientId, new GetPatientCasesFilter(1));

            // Assert
            result.ShouldNotBeNull();
            result.Items.Length.ShouldBeGreaterThan(0);
        }

        [Theory]
        [ClassData(typeof(GetPatientVitalsData))]
        public async Task GetPatientVitals_ShouldNotThrowJsonSerializationException(int patientId)
        {
            // Arrange
            // Act
            var result = await _client.Patients.GetPatientVitals(patientId, new GetPatientVitalsFilter(1));

            // Assert
            result.ShouldNotBeNull();
            result.Items.Length.ShouldBeGreaterThan(0);
        }

        [Fact]
        public async Task GetPatientReceipts_ReturnsRecords()
        {
            Receipt[] response = await _client.Patients.GetPatientReceipts(1, 1);

            response.ShouldNotBeNull();
            response.Length.ShouldBeGreaterThan(0);
        }
    }
}
