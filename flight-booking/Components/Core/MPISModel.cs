using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity.Data;

namespace flight_booking.Core
{
    public enum MpisState
    {
        Initial, //Start screen
        FlightEnquiry, //Choose a flight
        SeatEnquiry, //Choose a seat
        Reservation, //Review details
        Confirmation //Booking confirmed
    }

    public sealed class Flight
    {
        public string Id { get; init; } = Guid.NewGuid().ToString("N");
        public string Number { get; init; } = "";
        public string From { get; init; } = "";
        public string To { get; init; } = "";
        public DateTime DepartureUtc { get; set; }
        public int SeatsAvailable { get; set; }
    }

    public sealed class MPISModel
    {
        //Changed is raised whenever there's an important update
        public event Action? Changed;

        //Stack of previous states is kept to allow for a 'Back' button
        private readonly Stack<MpisState> _history = new();
        public MpisState State { get; private set; } = MpisState.Initial;

        //Readonly so that UI cannot edit
        public IReadOnlyList<Flight> Flights => _flights;
        public IReadOnlyList<string> Seats => _seats;

        public Flight? SelectedFlight { get; private set; }
        public string? SelectedSeat { get; private set; }
        public string? ConfirmationCode { get; private set; }

        private readonly List<Flight> _flights = new();
        private readonly List<string> _seats = new();

        //Move from Initial to FlightEnquiry and load flights
        public void startFlow()
        {
            if (State != MpisState.Initial) return;
            Push(); State = MpisState.FlightEnquiry;
            LoadFlights();
            Notify();
        }

        //User chooses flight, then move to SeatEnquiry and load seats.
        //If choice is invalid or in the wrong state, return false
        public bool PickFlight(string flightId)
        {
            if (State != MpisState.FlightEnquiry) return false;

            var f = _flights.FirstOrDefault(x => x.Id == flightId);
            if (f is null) return false;

            SelectedFlight = f;
            LoadSeats();
            Push(); State = MpisState.SeatEnquiry;
            Notify();
            return true;
        }

        //User chooses seat, move to Reservation
        //If choice is invalid or in the wrong state, return false
        public bool PickSeat(string seat)
        {
            if (State != MpisState.SeatEnquiry) return false;
            if (!_seats.Contains(seat)) return false;

            SelectedSeat = seat;
            Push(); State = MpisState.Reservation;
            Notify();
            return true;
        }

        //Create simple confirmation code
        //Return false if we cannot confirm yet
        public bool ConfirmReservation()
        {
            if (State != MpisState.Reservation) return false;
            if (SelectedFlight is null || SelectedSeat is null) return false;

            //Simple code for demo
            ConfirmationCode = $"{SelectedFlight.Number}-{SelectedSeat}-{Random.Shared.Next(1000, 9999)}";
            Push(); State = MpisState.Confirmation;
            Notify();
            return true;
        }

        //After confirmation, 'Finish' resets everything to the start
        public void Finish()
        {
            if (State != MpisState.Confirmation) return;
            Reset();
            Notify();
        }

        //Go back one step
        public void Back()
        {
            if (_history.Count == 0) return;
            State = _history.Pop();
            Notify();
        }

        //------Helpers------
        //Clear all data and return to Initial
        public void Reset()
        {
            _history.Clear();
            State = MpisState.Initial;
            _flights.Clear();
            _seats.Clear();
            SelectedFlight = null;
            SelectedSeat = null;
            ConfirmationCode = null;
        }

        private void Push() => _history.Push(State);
        private void Notify() => Changed?.Invoke();

        //Small set of demo flights
        private void LoadFlights()
        {
            _flights.Clear();
            var baseTime = DateTime.UtcNow.AddDays(1);

            _flights.AddRange(new[]
             {
                new Flight { Number="MP101", From="LHR", To="JFK", DepartureUtc=baseTime.AddHours(3), SeatsAvailable=9 },
                new Flight { Number="MP202", From="LHR", To="DXB", DepartureUtc=baseTime.AddHours(5), SeatsAvailable=6 },
                new Flight { Number="MP303", From="LHR", To="SIN", DepartureUtc=baseTime.AddHours(7), SeatsAvailable=4 },
            });
        }

        //Small seat map (rows 1-3, seats A-D)
        private void LoadSeats()
        {
            _seats.Clear();
            for (int row = 1; row <= 3; row++)
            {
                foreach (var col in new[] { 'A', 'B', 'C', 'D' })
                    _seats.Add($"{row}{col}");
            }
        }
    }
}