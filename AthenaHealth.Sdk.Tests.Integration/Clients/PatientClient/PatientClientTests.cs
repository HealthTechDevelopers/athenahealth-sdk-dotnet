﻿using System;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using AthenaHealth.Sdk.Exceptions;
using AthenaHealth.Sdk.Models.Response;
using AthenaHealth.Sdk.Tests.Integration.TestingHelpers;
using Shouldly;
using Xunit;

namespace AthenaHealth.Sdk.Tests.Integration.Clients.PatientClient
{
    public class PatientClientTests
    {
        [Fact]
        public async Task GetPatientById_ValidId_ReturnsPatient ()
        {
            var patientClient = new Sdk.Clients.PatientClient(ConnectionFactory.CreateFromFile(@"Clients\PatientClient\Patient.json"));

            Patient patient = await patientClient.GetPatientById(1);

            patient.ShouldNotBeNull();
            patient.Email.ShouldBe("monroe86@hotmail.com");
            patient.DepartmentId.ShouldBe(162);
            patient.Balances.ShouldNotBeEmpty();
            patient.Balances.First().Value.ShouldBe(10);
        }
    }
}
