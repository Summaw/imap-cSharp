using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace EmailChecker
{
    public class Startup
    {
        private static readonly List<string> AllowedIPs = new List<string>
        {
            "::1",
            // Add more IP addresses as needed
        };

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddRouting();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapPost("/check", async context =>
                {
                    var clientIP = context.Connection.RemoteIpAddress.ToString();

                    if (!IsIPAllowed(clientIP))
                    {
                        context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                        await context.Response.WriteAsync("Access Forbidden: Your IP address is not allowed to access this resource.");
                        return;
                    }

                    using (var reader = new StreamReader(context.Request.Body))
                    {
                        var json = await reader.ReadToEndAsync();

                        var requestData = JsonConvert.DeserializeObject<RequestData>(json);

                        var email = requestData.email;
                        var password = requestData.password;
                        var proxy = requestData.proxy;

                        var responseData = new ResponseData();

                        try
                        {
                            bool isValid = await Task.Run(async () =>
                            {
                                if (email.Contains("@outlook.com"))
                                {
                                    return await ImapHandlerOutlook.ReturnEmailAsync(email, password, proxy);
                                }
                                else if (email.Contains("@hotmail.com"))
                                {
                                    return await ImapHandlerOutlook.ReturnEmailAsync(email, password, proxy);
                                }

                                return false; // Handle other cases accordingly
                            });


                            if (isValid)
                            {
                                responseData.Status = "Success";
                                responseData.Message = email + " checked successfully.";
                                context.Response.StatusCode = 200;
                            }
                            else
                            {
                                responseData.Status = "Error";
                                responseData.Message = email + "Invalid account.";
                                context.Response.StatusCode = 400;
                            }
                        }
                        catch (Exception ex)
                        {
                            responseData.Status = "Error";
                            responseData.Message = ex.Message;
                            context.Response.StatusCode = 500;
                        }

                        var jsonResponse = JsonConvert.SerializeObject(responseData);
                        context.Response.ContentType = "application/json";
                        await context.Response.WriteAsync(jsonResponse);
                    }
                });
            });
        }

        private bool IsIPAllowed(string ipAddress)
        {
            return AllowedIPs.Contains(ipAddress);
        }

        // Define a class to represent the JSON request data
        public class RequestData
        {
            public string email { get; set; }
            public string password { get; set; }
            public string proxy { get; set; }
        }

        public class ResponseData
        {
            public string Status { get; set; }
            public string Message { get; set; }
            // Add other properties as needed
        }

        public class Program
        {
            public static void Main(string[] args)
            {
                CreateWebHostBuilder(args).Build().Run();
            }

            public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
                WebHost.CreateDefaultBuilder(args)
                    .UseStartup<Startup>()
                    .UseUrls("http://localhost:5000"); // Specify the address and port
        }
    }
}
