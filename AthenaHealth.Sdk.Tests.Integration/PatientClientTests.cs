﻿using System;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AthenaHealth.Sdk.Exceptions;
using AthenaHealth.Sdk.Models.Request;
using AthenaHealth.Sdk.Tests.Integration.TestingHelpers;
using Shouldly;
using Xunit;

// ReSharper disable StringLiteralTypo
namespace AthenaHealth.Sdk.Tests.Integration
{
    public class PatientClientTests
    {
        [Fact]
        public async Task GetPatientById_ValidId_ReturnsPatient()
        {
            // Arrange
            var patientClient = new Sdk.Clients.PatientClient(ConnectionFactory.CreateFromFile(@"Data\Patient\GetPatient.json"));

            // Act
            var result = await patientClient.GetPatientById(1);

            // Assert
            result.ShouldNotBeNull();
            result.Email.ShouldBe("test@test.com");
            result.DepartmentId.ShouldBe(1);
            result.Balances.ShouldNotBeEmpty();
        }

        [Fact]
        public void GetPatientById_InvalidId_ThrowsApiException()
        {
            // Arrange
            var patientClient = new Sdk.Clients.PatientClient(ConnectionFactory.Create("{ \"missingfields\": [ \"patientid\" ], \"error\": \"Additional fields are required.\" }", HttpStatusCode.BadRequest));

            // Act
            ApiException exception = Should.Throw<ApiException>(async () => await patientClient.GetPatientById(0));

            // Assert
            exception.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetMedicalHistory_ValidData_ReturnsMedicalHistory()
        {
            // Arrange
            var patientClient = new Sdk.Clients.PatientClient(ConnectionFactory.CreateFromFile(@"Data\Patient\GetMedicalHistory.json"));

            // Act
            var result = await patientClient.GetMedicalHistory(1, 1);

            // Assert
            result.ShouldNotBeNull();
            result.Questions.Count().ShouldBeGreaterThan(0);
        }

        [Fact]
        public void GetMedicalHistory_PatientInDifferentDepartment_ThrowsException()
        {
            // Arrange
            var patientClient = new Sdk.Clients.PatientClient(ConnectionFactory.Create("{\"detailedmessage\":\"The specified patient does not exist in that department.\",\"error\":\"The specified patient does not exist in that department.\"}", HttpStatusCode.NotFound));

            // Act
            ApiException exception = Should.Throw<ApiException>(async () => await patientClient.GetMedicalHistory(1, 2));

            // Assert
            exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetLabResults_ValidData_ReturnsLabResultCollection()
        {
            // Arrange
            var patientClient = new Sdk.Clients.PatientClient(ConnectionFactory.CreateFromFile(@"Data\Patient\GetLabResults.json"));
            var queryParameters = new GetLabResultsFilter()
            {
                DepartmentId = 1,
                ShowAbnormalDetails = true,
                ShowHidden = true,
                ShowTemplate = true
            };

            // Act
            var result = await patientClient.GetLabResults(1, queryParameters);

            // Assert
            result.ShouldNotBeNull();
            result.Items.Count().ShouldBeGreaterThan(0);
        }

        [Fact]
        public void GetLabResults_PatientInDifferentDepartment_ThrowsException()
        {
            // Arrange
            var patientClient = new Sdk.Clients.PatientClient(ConnectionFactory.Create("{\"error\":\"The Patient ID or Department ID is invalid.\"}", HttpStatusCode.NotFound));
            var queryParameters = new GetLabResultsFilter()
            {
                DepartmentId = 2,
                ShowAbnormalDetails = true,
                ShowHidden = true,
                ShowTemplate = true
            };

            // Act
            ApiException exception = Should.Throw<ApiException>(async () => await patientClient.GetLabResults(1, queryParameters));

            // Assert
            exception.StatusCode.ShouldBe(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task GetPatientProblems_ValidData_ReturnsProblemsCollection()
        {
            // Arrange
            var patientClient = new Sdk.Clients.PatientClient(ConnectionFactory.CreateFromFile(@"Data\Patient\GetPatientProblems.json"));
            var queryParameters = new GetPatientProblemsFilter()
            {
                DepartmentId = 1,
                ShowDiagnosisInfo = true,
                ShowInactive = true
            };

            // Act
            var result = await patientClient.GetPatientProblems(1, queryParameters);

            // Assert
            result.ShouldNotBeNull();
            result.Problems.Count().ShouldBeGreaterThan(0);
        }

        [Fact]
        public void GetPatientProblems_PatientInDifferentDepartment_ThrowsException()
        {
            // Arrange
            var patientClient = new Sdk.Clients.PatientClient(ConnectionFactory.Create("{\"error\":\"Invalid departmentid or departmentid / patientid combination.\"}", HttpStatusCode.BadRequest));
            var queryParameters = new GetPatientProblemsFilter()
            {
                DepartmentId = 2,
                ShowDiagnosisInfo = true,
                ShowInactive = true
            };

            // Act
            ApiException exception = Should.Throw<ApiException>(async () => await patientClient.GetPatientProblems(1, queryParameters));

            // Assert
            exception.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetPatients_ValidData_ReturnsPatientsCollection()
        {
            // Arrange
            var patientClient = new Sdk.Clients.PatientClient(ConnectionFactory.CreateFromFile(@"Data\Patient\GetPatients.json"));
            var queryParameters = new GetPatientsFilter()
            {
                FirstName = "Michael",
                DepartmentId = 1,
                OmitBalances = false,
                OmitDefaultPharmacy = false,
                OmitPhotoInformation = false,
                Show2015EdCehrtValues = true
            };

            // Act
            var result = await patientClient.GetPatients(queryParameters);

            // Assert
            result.ShouldNotBeNull();
            result.Items.Count().ShouldBeGreaterThan(0);
        }

        [Fact]
        public void GetPatients_TooMuchDataFound_ThrowsException()
        {
            // Arrange
            var patientClient = new Sdk.Clients.PatientClient(ConnectionFactory.Create("{\"error\":\"The given search parameters would produce a total data set larger than 1000 records.Please refine your search and try again.\"}", HttpStatusCode.BadRequest));
            var queryParameters = new GetPatientsFilter()
            {
                DepartmentId = 1,
                OmitBalances = false,
                OmitDefaultPharmacy = false,
                OmitPhotoInformation = false,
                Show2015EdCehrtValues = true
            };

            // Act
            ApiException exception = Should.Throw<ApiException>(async () => await patientClient.GetPatients(queryParameters));

            // Assert
            exception.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        }

        [Fact]
        public void GetPatients_InvalidFilter_ThrowsException()
        {
            // Arrange
            var patientClient = new Sdk.Clients.PatientClient(ConnectionFactory.Create("{\"fields\":[\"guarantorfirstname\",\"dob\",\"firstname\",\"workphone\",\"departmentid\",\"guarantorsuffix\",\"guarantorlastname\",\"mobilephone\",\"middlename\",\"suffix\",\"guarantormiddlename\",\"lastname\",\"homephone\",\"anyphone\"],\"error\":\"Data for one or more of the fields listed above are required to successfully find a patient record.Note: invalid phone numbers are ignored.\"}", HttpStatusCode.BadRequest));
            var queryParameters = new GetPatientsFilter()
            {
                OmitBalances = false,
                OmitDefaultPharmacy = false,
                OmitPhotoInformation = false,
                Show2015EdCehrtValues = true
            };

            // Act
            ApiException exception = Should.Throw<ApiException>(async () => await patientClient.GetPatients(queryParameters));

            // Assert
            exception.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task EnhancedBestmatch_ValidData_ReturnsPatientsCollection()
        {
            // Arrange
            var patientClient = new Sdk.Clients.PatientClient(ConnectionFactory.CreateFromFile(@"Data\Patient\EnhancedBestmatch.json"));
            var queryParameters = new EnhancedBestmatchFilter()
            {
                DateOfBirth = new DateTime(1989, 09, 07),
                FirstName = "Peter",
                LastName = "Tots",
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
            var result = await patientClient.EnhancedBestmatch(queryParameters);

            // Assert
            result.ShouldNotBeNull();
            result.Count().ShouldBeGreaterThan(0);
        }

        [Fact]
        public async Task EnhancedBestmatch_ValidData_ReturnsNoPatient()
        {
            // Arrange
            var patientClient = new Sdk.Clients.PatientClient(ConnectionFactory.Create("[]"));
            var queryParameters = new EnhancedBestmatchFilter()
            {
                DateOfBirth = new DateTime(1989, 09, 07),
                FirstName = "Peter",
                LastName = "Tots",
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
            var result = await patientClient.EnhancedBestmatch(queryParameters);

            // Assert
            result.ShouldNotBeNull();
            result.Count().ShouldBe(0);
        }

        [Fact]
        public void EnhancedBestmatch_InvalidDateOfBirthFormat_ThrowsException()
        {
            // Arrange
            var patientClient = new Sdk.Clients.PatientClient(ConnectionFactory.CreateFromFile(@"Data\Patient\EnhancedBestmatch_InvalidDateOfBirthFormat.json", HttpStatusCode.BadRequest));
            var queryParameters = new EnhancedBestmatchFilter()
            {
                DateOfBirth = new DateTime(01, 09, 07),
                FirstName = "Peter",
                LastName = "Tots",
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
            ApiException exception = Should.Throw<ApiException>(async () => await patientClient.EnhancedBestmatch(queryParameters));

            // Assert
            exception.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        }

        [Fact]
        public void EnhancedBestmatch_MissingFields_ThrowsException()
        {
            // Arrange
            var patientClient = new Sdk.Clients.PatientClient(ConnectionFactory.CreateFromFile(@"Data\Patient\EnhancedBestmatch_MissingFields.json", HttpStatusCode.BadRequest));
            var queryParameters = new EnhancedBestmatchFilter()
            {
                DateOfBirth = new DateTime(1989, 09, 07),
                LastName = "Tots",
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
            ApiException exception = Should.Throw<ApiException>(async () => await patientClient.EnhancedBestmatch(queryParameters));

            // Assert
            exception.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task GetDefaultPharmacy_ValidId_ReturnsDefaultPharmacy()
        {
            // Arrange
            var patientClient = new Sdk.Clients.PatientClient(ConnectionFactory.CreateFromFile(@"Data\Patient\GetDefaultPharmacy.json", HttpStatusCode.OK));

            // Act
            var result = await patientClient.GetDefaultPharmacy(300, 1);

            // Assert
            result.ShouldNotBeNull();
            result.State.ShouldBe("NY");
            result.ClinicalProviderName.ShouldBe("Himani Shishodia");
        }

        [Fact]
        public async Task GetPreferredPharmacies_ReturnsPreferredPharmacies()
        {
            // Arrange
            var patientClient = new Sdk.Clients.PatientClient(ConnectionFactory.CreateFromFile(@"Data\Patient\GetPreferredPharmacies.json", HttpStatusCode.OK));
            var queryParameters = new GetPreferredPharmacyFilter
            {
                DepartmentId = 1
            };

            // Act
            var result = await patientClient.GetPreferredPharmacies(300, queryParameters);

            // Assert
            result.ShouldNotBeNull();
            result.Total.ShouldBe(1);
            result.Items.ShouldNotBeNull();
            result.Items.Length.ShouldBe(1);
            result.Items[0].ClinicalProviderId.ShouldBe(11242674);
        }

        [Fact]
        public void SetDefaultPharmacy_ValidData_NotThrow()
        {
            var patientClient = new Sdk.Clients.PatientClient(ConnectionFactory.Create("{\"success\": true}", HttpStatusCode.OK));

            Should.NotThrow(async () => await patientClient.SetDefaultPharmacy(5000, new SetPharmacyRequest{DepartmentId = 164, ClinicalProviderId = 11242674}));
        }
        
        [Fact]
        public void AddPreferredPharmacy_ValidData_NotThrow()
        {
            var patientClient = new Sdk.Clients.PatientClient(ConnectionFactory.Create("{\"success\": true}", HttpStatusCode.OK));

            Should.NotThrow(async () => await patientClient.AddPreferredPharmacy(5000, new SetPharmacyRequest{DepartmentId = 164, ClinicalProviderId = 11242674}));
        }
    }
}
