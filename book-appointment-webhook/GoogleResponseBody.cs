using System;
using System.Collections.Generic;

namespace book_appointment_webhook
{
    public class GoogleResponseBody{
        public string Summary { get; set; }
        public List<Item> Items { get; set; }
    }
        // Root myDeserializedClass = JsonConvert.DeserializeObject<Root>(myJsonResponse);
    public class Creator
    {
        public object DisplayName { get; set; }
        public string Email { get; set; }
        public object Id { get; set; }
        public bool Self { get; set; }
    }

    public class End
    {
        public object Date { get; set; }
        public DateTime DateTimeRaw { get; set; }
        public DateTime DateTimeDateTimeOffset { get; set; }
        public DateTime DateTime { get; set; }
        public string TimeZone { get; set; }
        public object ETag { get; set; }
    }

    public class Item
    {
        public object AnyoneCanAddSelf { get; set; }
        public object Attachments { get; set; }
        public object Attendees { get; set; }
        public object AttendeesOmitted { get; set; }
        public object ColorId { get; set; }
        public object ConferenceData { get; set; }
        public DateTime CreatedRaw { get; set; }
        public DateTime CreatedDateTimeOffset { get; set; }
        public DateTime Created { get; set; }
        public Creator Creator { get; set; }
        public string Description { get; set; }
        public End End { get; set; }
        public object EndTimeUnspecified { get; set; }
        public string ETag { get; set; }
        public string EventType { get; set; }
        public object ExtendedProperties { get; set; }
        public object FocusTimeProperties { get; set; }
        public object Gadget { get; set; }
        public object GuestsCanInviteOthers { get; set; }
        public object GuestsCanModify { get; set; }
        public object GuestsCanSeeOtherGuests { get; set; }
        public object HangoutLink { get; set; }
        public string HtmlLink { get; set; }
        public string ICalUID { get; set; }
        public string Id { get; set; }
        public string Kind { get; set; }
        public object Location { get; set; }
        public object Locked { get; set; }
        public Organizer Organizer { get; set; }
        public object OriginalStartTime { get; set; }
        public object OutOfOfficeProperties { get; set; }
        public object PrivateCopy { get; set; }
        public object Recurrence { get; set; }
        public object RecurringEventId { get; set; }
        public Reminders Reminders { get; set; }
        public int Sequence { get; set; }
        public object Source { get; set; }
        public Start Start { get; set; }
        public string Status { get; set; }
        public string Summary { get; set; }
        public object Transparency { get; set; }
        public DateTime UpdatedRaw { get; set; }
        public DateTime UpdatedDateTimeOffset { get; set; }
        public DateTime Updated { get; set; }
        public object Visibility { get; set; }
        public object WorkingLocationProperties { get; set; }
    }

    public class Organizer
    {
        public object DisplayName { get; set; }
        public string Email { get; set; }
        public object Id { get; set; }
        public bool Self { get; set; }
    }

    public class Reminders
    {
        public object Overrides { get; set; }
        public bool UseDefault { get; set; }
    }



    public class Start
    {
        public object Date { get; set; }
        public DateTime DateTimeRaw { get; set; }
        public DateTime DateTimeDateTimeOffset { get; set; }
        public DateTime DateTime { get; set; }
        public string TimeZone { get; set; }
        public object ETag { get; set; }
    }


    
}