namespace flight_booking.Core
{
    //Controller that forwards user actions to the model
    public sealed class MPISController
    {
        private readonly MPISModel _model;
        public MPISController(MPISModel model) => _model = model;

        public void Start() => _model.startFlow();
        public bool PickFlight(string flightId) => _model.PickFlight(flightId);
        public bool PickSeat(string seat) => _model.PickSeat(seat);
        public bool Confirm() => _model.ConfirmReservation();
        public void Finish() => _model.Finish();
        public void Back() => _model.Back();
    }
}