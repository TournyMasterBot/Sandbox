using Core.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Helpers
{
    public static class WebRequestHelper
    {
        private static BlockingCollection<GetWebRequestModel> getQueue = new BlockingCollection<GetWebRequestModel>();
        private static BlockingCollection<PostWebRequestModel> postQueue = new BlockingCollection<PostWebRequestModel>();

        static WebRequestHelper()
        {
            Debug.Print("Starting processing queue");
            var getWorker = new BackgroundWorker();
            getWorker.DoWork += ProcessGetQueue;
            getWorker.RunWorkerAsync();

            var postWorker = new BackgroundWorker();
            postWorker.DoWork += ProcessPostQueue;
            postWorker.RunWorkerAsync();
        }

        public static bool Get(string url, Action<string> callback, Dictionary<string, string> headers = null, int priority = 0, CancellationTokenSource cancelTokenSource = null)
        {
            try
            {
                var request = new GetWebRequestModel()
                {
                    Url = url,
                    Callback = callback,
                    Priority = priority,
                    Headers = headers,
                    CancellationToken = cancelTokenSource == null
                        ? new CancellationTokenSource()
                        : cancelTokenSource
                };
                getQueue.TryAdd(request, 5000, request.CancellationToken.Token);
                return true;
            }
            catch (Exception ex)
            {
                Debug.Print(ex.ToString());
                return false;
            }
        }

        public static bool Post<T>(string url, T data, Action<string> callback, Dictionary<string, string> headers = null, int priority = 0, CancellationTokenSource cancelTokenSource = null)
        {
            try
            {
                var request = new PostWebRequestModel()
                {
                    Url = url,
                    Callback = callback,
                    Priority = priority,
                    Headers = headers,
                    Data = data,
                    CancellationToken = cancelTokenSource == null 
                        ? new CancellationTokenSource()
                        : cancelTokenSource
                };
                postQueue.TryAdd(request, 5000, request.CancellationToken.Token);
                return true;
            }
            catch (Exception ex)
            {
                Debug.Print(ex.ToString());
                return false;
            }
        }

        private static void ProcessGetQueue(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                try
                {
                    getQueue.TryTake(out GetWebRequestModel request, 3000);
                    // Todo: Create a queue list with max number of requests
                    if(request != null && request != default(GetWebRequestModel))
                    {
                        ProcessGetQueueRequest(request).Wait();
                    }
                }
                catch (TimeoutException) { }
                catch (OperationCanceledException) { }
                catch(Exception ex)
                {
                    Debug.Print(ex.ToString());
                }
            }
        }

        private static async Task ProcessGetQueueRequest(GetWebRequestModel request)
        {
            using (var handler = new HttpClientHandler())
            {
                // Disable SSL checking
                handler.ClientCertificateOptions = ClientCertificateOption.Manual;
                handler.ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, cetChain, policyErrors) =>
                {
                    return true;
                };
                handler.AllowAutoRedirect = true;
                handler.MaxAutomaticRedirections = 3;
                handler.MaxConnectionsPerServer = 100;

                try
                {
                    using (var client = new HttpClient(handler))
                    {
                        if (request.Headers != null && request.Headers.Count > 0)
                        {
                            foreach (var header in request.Headers)
                            {
                                client.DefaultRequestHeaders.Add(header.Key, header.Value);
                            }
                        }
                        var response = await client.GetAsync(request.Url, request.CancellationToken.Token);
                        using (var stream = await response.Content.ReadAsStreamAsync())
                        {
                            using (var reader = new StreamReader(stream, Encoding.UTF8))
                            {
                                var data = reader.ReadToEnd();
                                request.Callback(data);
                            }
                        }
                    }
                }
                catch (WebException ex)
                {
                    using (var stream = ex.Response.GetResponseStream())
                    {
                        using (var reader = new StreamReader(stream))
                        {
                            Console.WriteLine(reader.ReadToEnd());
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.Print(ex.ToString());
                }
            }
        }

        private static void ProcessPostQueue(object sender, DoWorkEventArgs e)
        {
            while (true)
            {
                try
                {
                    postQueue.TryTake(out PostWebRequestModel request, 3000);
                    // Todo: Create a queue list with max number of requests
                    if(request != null && request != default(PostWebRequestModel))
                    {
                        ProcessPostQueueRequest(request).Wait();
                    }
                }
                catch (TimeoutException) { }
                catch (OperationCanceledException) { }
                catch (Exception ex)
                {
                    Debug.Print(ex.ToString());
                }
            }
        }

        private static async Task ProcessPostQueueRequest(PostWebRequestModel request)
        {
            using (var handler = new HttpClientHandler())
            {
                // Disable SSL checking
                handler.ClientCertificateOptions = ClientCertificateOption.Manual;
                handler.ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, cetChain, policyErrors) =>
                {
                    return true;
                };
                handler.AllowAutoRedirect = true;
                handler.MaxAutomaticRedirections = 3;
                handler.MaxConnectionsPerServer = 100;
                var content = new StringContent(JsonConvert.SerializeObject(request.Data), Encoding.UTF8, "application/json");
                try
                {
                    using (var client = new HttpClient(handler))
                    {
                        if (request.Headers != null && request.Headers.Count > 0)
                        {
                            foreach (var header in request.Headers)
                            {
                                client.DefaultRequestHeaders.Add(header.Key, header.Value);
                            }
                        }
                        var response = await client.PostAsync(request.Url, content, request.CancellationToken.Token);
                        
                        using (var stream = await response.Content.ReadAsStreamAsync())
                        {
                            using (var reader = new StreamReader(stream, Encoding.UTF8))
                            {
                                var data = reader.ReadToEnd();
                                request.Callback(data);
                            }
                        }
                    }
                }
                catch (WebException ex)
                {
                    using (var stream = ex.Response.GetResponseStream())
                    {
                        using (var reader = new StreamReader(stream))
                        {
                            Console.WriteLine(reader.ReadToEnd());
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.Print(ex.ToString());
                }
                finally
                {
                    content.Dispose();
                }
            }
        }
    }
}
