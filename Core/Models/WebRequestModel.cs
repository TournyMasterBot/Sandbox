using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Core.Models
{
    public class GetWebRequestModel
    {
        public int Priority { get; set; } = 0;
        public string Url { get; set; }
        public Action<string> Callback { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public CancellationTokenSource CancellationToken { get; set; }
    }

    public class PostWebRequestModel
    {
        public int Priority { get; set; } = 0;
        public string Url { get; set; }
        public Action<string> Callback { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public dynamic Data { get; set; }
        public CancellationTokenSource CancellationToken { get; set; }
    }
}
