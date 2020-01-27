﻿using Dispatching.Broker.Commands;
using Dispatching.Broker.Commands.Mappers;
using Dispatching.Broker.Events.Mappers.DomainModel;
using Dispatching.Rides;
using Dispatching.Rides.Processes.PrimaryPorts;
using Rebus.Handlers;
using System;
using System.Threading.Tasks;

namespace Dispatching.Broker.Handlers
{
    public class DriveCustomerToTrainStationHandler : IHandleMessages<DriveCustomerToTrainStation>
    {
        private readonly IQueue _messageBus;
        private readonly ICabRideService _cabRideService;
        private readonly IDriveCustomerToTrainStationMapper _driveCustomerToTrainStationMapper;
        private readonly ICabRideMapper _cabRideMapper;

        public DriveCustomerToTrainStationHandler(IQueue messageBus,
                     ICabRideService cabRideService,
                     IDriveCustomerToTrainStationMapper driveCustomerToTrainStationMapper,
                     ICabRideMapper cabRideMapper)
        {
            _messageBus = messageBus;
            _cabRideService = cabRideService;
            _driveCustomerToTrainStationMapper = driveCustomerToTrainStationMapper;
            _cabRideMapper = cabRideMapper;
        }

        public async Task Handle(DriveCustomerToTrainStation message)
        {
            Ride ride;
            try
            {
                var customerId = _driveCustomerToTrainStationMapper.MapToCustomerId(message);
                var customerLocation = _driveCustomerToTrainStationMapper.MapToCustomerLocation(message);

                ride = await _cabRideService.BringCustomerToTheTrainStation(customerId, customerLocation);
            }
            catch (Exception e)
            {
                var failed = _cabRideMapper.MapFailedEvent(message, e);
                await _messageBus.Enqueue(failed);
                return;
            }

            var success = _cabRideMapper.MapSuccessEvent(ride);
            await _messageBus.Enqueue(success);
        }
    }
}
