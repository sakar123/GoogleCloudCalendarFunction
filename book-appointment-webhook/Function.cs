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
using System.Threading;
using System.Threading.Tasks;

namespace book_appointment_webhook
{
    public class Function : IHttpFunction
    {
        private int Check_Username = 0;
        private const string GoogleAuthUrl = "https://oauth2.googleapis.com/token";
        private const string CalendarApiUrl = "https://www.googleapis.com/calendar/v3/calendars/{0}/events";

        public async Task HandleAsync(HttpContext context)
        {
            try
            {
                // Read headers for username and password
                var username = context.Request.Headers["Username"].FirstOrDefault();
                var password = context.Request.Headers["Password"].FirstOrDefault();

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
                var accessToken = await GetAccessTokenAsync();
                if (string.IsNullOrEmpty(accessToken))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    await context.Response.WriteAsync("Failed to authenticate with Google.");
                    return;
                }
                context.Response.StatusCode = StatusCodes.Status200OK;
                await context.Response.WriteAsync($"Access_Token is: {accessToken}");

                // Parse the request body
                var requestBody = await new StreamReader(context.Request.Body).ReadToEndAsync();
                dynamic requestData = JsonConvert.DeserializeObject(requestBody);
                var dateTime = (string)requestData?.dateTime;
                var calendarId = (string)requestData?.calendarId;

                if (string.IsNullOrEmpty(dateTime) || string.IsNullOrEmpty(calendarId))
                {
                    context.Response.StatusCode = StatusCodes.Status400BadRequest;
                    await context.Response.WriteAsync("Invalid request body.");
                    return;
                }

                // Retrieve events and book an appointment
                var bookedEvent = await BookEventIfAvailableAsync(accessToken, calendarId, dateTime);
                if (bookedEvent != null)
                {
                    context.Response.StatusCode = StatusCodes.Status200OK;
                    await context.Response.WriteAsync($"Event booked: {bookedEvent.HtmlLink}");
                }
                else
                {
                    context.Response.StatusCode = StatusCodes.Status204NoContent;
                    await context.Response.WriteAsync("No available slot found for the specified time.");
                }
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                await context.Response.WriteAsync($"An error occurred: {ex.Message}");
            }
        }

        private async Task<string> GetAccessTokenAsync()
        {
            // Set the OAuth 2.0 client ID and secret
            string[] Scopes = { CalendarService.Scope.CalendarReadonly };
            string ApplicationName = "Google Calendar API .NET Quickstart";

            // Path to the client secrets file
            string clientSecretsFile = "client_secret_787167726286-125t0h69jrf2ijd7b77f12umj28fjq1q.apps.googleusercontent.com.json"; // Place your credentials.json file here

            UserCredential credential;

            //var codeReceiver = new ICodeReceiver

            // Load the client secrets file
            using (var stream =
                new FileStream(clientSecretsFile, FileMode.Open, FileAccess.Read))
            {
                credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.FromStream(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore("token.json", true));
            }

            // Create Google Calendar API service
            var service = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = ApplicationName,
            });

            // Define request parameters
            EventsResource.ListRequest request = service.Events.List("primary");
            //request.TimeMin = DateTime.Now;
            request.ShowDeleted = false;
            request.SingleEvents = true;
            request.MaxResults = 10;
            request.OrderBy = EventsResource.ListRequest.OrderByEnum.StartTime;

            // Fetch the events
            var events = await request.ExecuteAsync();
            Console.WriteLine("Upcoming events:");
            if (events.Items != null && events.Items.Count > 0)
            {
                foreach (var eventItem in events.Items)
                {
                    string when = eventItem.Start.DateTime.ToString();
                    if (string.IsNullOrEmpty(when))
                    {
                        when = eventItem.Start.Date;
                    }
                    Console.WriteLine($"{eventItem.Summary} ({when})");
                }
            }
            else
            {
                Console.WriteLine("No upcoming events found.");
            }
            return "Hello";
            
        }

        private async Task<Event> BookEventIfAvailableAsync(string accessToken, string calendarId, string dateTime)
        {
            var service = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = new UserCredential(new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
                {
                    ClientSecrets = new ClientSecrets
                    {
                        ClientId = "YOUR_CLIENT_ID",
                        ClientSecret = "YOUR_CLIENT_SECRET"
                    },
                    Scopes = new[] { CalendarService.Scope.Calendar }
                }), "user", new TokenResponse { AccessToken = accessToken }),
                ApplicationName = "Your Application Name",
            });

            var startDateTime = DateTime.Parse(dateTime);
            var endDateTime = startDateTime.AddMinutes(45);

            // Check for conflicting events
            var request = service.Events.List(calendarId);
            request.TimeMin = startDateTime;
            request.TimeMax = endDateTime;
            request.SingleEvents = true;
            var events = await request.ExecuteAsync();

            if (!events.Items.Any())
            {
                // No conflicts, create a new event
                var newEvent = new Event
                {
                    Summary = "Booked Appointment",
                    //Start = new EventDateTime { DateTime = startDateTime.ToString("o"), TimeZone = "UTC" },
                    //End = new EventDateTime { DateTime = endDateTime.ToString("o"), TimeZone = "UTC" }
                };

                var insertRequest = service.Events.Insert(newEvent, calendarId);
                return await insertRequest.ExecuteAsync();
            }

            return null;
        }
    }
}
