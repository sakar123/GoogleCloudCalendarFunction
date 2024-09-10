using System;

namespace book_appointment_webhook{
    public class IncomingRequestBody{
        public DateTime bookingStartTime;
        public DateTime bookingEndTime;
        public string bookingType;
        public string bookingWith;

        public string bookingSummary;
        public IncomingRequestBody(){

        }
    }
}