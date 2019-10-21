﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AthenaHealth.Sdk.Clients.Interfaces;
using AthenaHealth.Sdk.Extensions;
using AthenaHealth.Sdk.Http;
using AthenaHealth.Sdk.Models.Request;
using AthenaHealth.Sdk.Models.Response;
// ReSharper disable StringLiteralTypo

namespace AthenaHealth.Sdk.Clients
{
    public class AppointmentClient : IAppointmentClient
    {
        private readonly IConnection _connection;

        public AppointmentClient(IConnection connection)
        {
            _connection = connection;
        }

        public async Task<AppointmentTypeResponse> GetAppointmentTypes(GetAppointmentTypeFilter filter = null)
        {
            return await _connection.Get<AppointmentTypeResponse>($"{_connection.PracticeId}/appointmenttypes", filter);
        }

        public async Task<AppointmentType> GetAppointmentType(int appointmentTypeId)
        {
            AppointmentType[] result = await _connection.Get<AppointmentType[]>($"{_connection.PracticeId}/appointmenttypes/{appointmentTypeId}");
            return result.FirstOrThrowException();
        }

        public async Task<AppointmentResponse> GetBookedAppointments(GetBookedAppointmentsFilter filter)
        {
            if (filter.DepartmentIds != null && filter.DepartmentIds.Length > 1)
                return await _connection.Get<AppointmentResponse>($"{_connection.PracticeId}/appointments/booked/multipledepartment", filter);
            return await _connection.Get<AppointmentResponse>($"{_connection.PracticeId}/appointments/booked", filter);
        }
    }
}
