using Google.Apis.Auth;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Google.Cloud.Functions.Framework;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;

namespace book_appointment_webhook
{
    public class Function : IHttpFunction
    {
        private int Check_Username = 0;
        private int IsProd = 0;

        private static readonly string[] Scopes = { CalendarService.Scope.CalendarReadonly,
                                                    CalendarService.Scope.CalendarEvents
                                                     };
        private static readonly string ApplicationName = "Daai Service Account";

        public async Task HandleAsync(HttpContext context)
        {
            try
            {
                
                /*
                    Variables needed :
                        1. DateTime
                        2. Phone Number /Email of the user calling
                        3. Description? Location?
                */
                // Read headers for username and password
                DateTime Test_Start_Time = DateTime.Parse("2024-09-11T09:15:00-05:00");
                DateTime Test_End_Time = DateTime.Parse("2024-09-11T09:45:00-05:00");
                string Test_Summary = "Hair Cut Apppointment with Henry";

                var username = context.Request.Headers["Username"].FirstOrDefault();
                var password = context.Request.Headers["Password"].FirstOrDefault();
                //var incomingRequestBody = await context.Request.ReadFromJsonAsync<IncomingRequestBody>();
                var incomingRequestBody = new IncomingRequestBody();

                var incomingRequestStartTime = Test_Start_Time ;
                var incomingRequestEndTime = Test_End_Time ;
                var summary = Test_Summary ;

                if(IsProd == 1){
                    incomingRequestStartTime = incomingRequestBody.bookingStartTime ;
                    incomingRequestEndTime = incomingRequestBody.bookingEndTime  ;
                    summary = incomingRequestBody.bookingSummary ;
                }
                if(Check_Username == 1)
                {

                    
                    // Validate credentials 
                    if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                    {
                        
                        context.Response.StatusCode = StatusCodes.Status400BadRequest;
                        await context.Response.WriteAsync("Missing username or password.");
                        return;
                    }

                    if(!username.Equals("admin") && !username.Equals("password")){
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        await context.Response.WriteAsync("Incorrect username or password.");
                        return;
                    }
                }

                // Obtain an access token from Google Auth

                //Get credential from the secrets file
                var credential = GoogleCredential.FromFile("axial-matter-426114-c3-700649e909b7.json")
                                                 .CreateScoped(Scopes);

                // Create the Calendar API service.
                var service = new CalendarService(new BaseClientService.Initializer()
                {
                    HttpClientInitializer = credential,
                    ApplicationName = ApplicationName,
                });

                // Define the calendar ID and the date range.
                //var calendarId = "primary"; // or a specific calendar ID
                var calendarId = "daai.phoneai@gmail.com"; // or a specific calendar ID
                var request = service.Events.List(calendarId);
                request.TimeMin = DateTime.UtcNow;
                request.TimeMax = DateTime.UtcNow.AddMonths(1);
                request.ShowDeleted = false;
                request.SingleEvents = true;
                request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

                // Fetch the events.
                var events = await request.ExecuteAsync();

                // Prepare response.
                var response = new
                {
                    BookingStatus = "Unconfirmed",
                    Message = "Sorry, your booking could not be made"
                };

                foreach (var an_event in events.Items){
                    var event_start_time = DateTime.Parse(an_event.Start.DateTimeRaw);
                    var event_end_time = DateTime.Parse(an_event.End.DateTimeRaw);

                    if (incomingRequestStartTime < event_end_time && incomingRequestEndTime > event_start_time)
                    {
                        //time slot overlaps
                        var not_confirmed_response = new
                        {
                            BookingStatus = "Not Confirmed",
                            Message = "Sorry, your booking could not be made"
                        };
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(not_confirmed_response));
                    }
                    else{
                        var eventBody = new Event
                        {
                            Summary = summary,
                            Start = new EventDateTime
                            {
                                DateTime = incomingRequestStartTime.ToUniversalTime(),
                                TimeZone = "UTC"
                            },
                            End = new EventDateTime
                            {
                                DateTime = incomingRequestEndTime.ToUniversalTime(),
                                TimeZone = "UTC"
                            },
                            //Location = location
                        };

                        var result = await service.Events.Insert(eventBody, "daai.phoneai@gmail.com").ExecuteAsync();
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(result));
                    }
                }

                // Write the response.
                // context.Response.ContentType = "application/json";
                // await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(response));

                    
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsync($"An error occurred: {ex.Message}");
            }
        }
    }
}





