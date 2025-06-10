using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace BliveHelper.Utils.Obs
{
    public class ObsData<T>
    {
        [JsonProperty("op")]
        public ObsMessageTypes OperationCode { get; set; }
        [JsonProperty("d")]
        public T Data { get; set; }
    }

    public class ObsRequestStatus
    {
        [JsonProperty("code")]
        public int Code { get; set; }
        [JsonProperty("result")]
        public bool Result { get; set; }
    }

    public class ObsVersion
    {
        [JsonProperty("obsWebSocketVersion")]
        public string ObsPluginVersion { get; set; } = string.Empty;
        [JsonProperty("obsVersion")]
        public string OBSStudioVersion { get; set; } = string.Empty;
        [JsonProperty("rpcVersion")]
        public int RpcVersion { set; get; }
        [JsonProperty("availableRequests")]
        public List<string> AvailableRequests { get; set; } = new List<string>();
        [JsonProperty("supportedImageFormats")]
        public List<string> SupportedImageFormats { get; set; } = new List<string>();
        [JsonProperty("platform")]
        public string Platform { get; set; } = string.Empty;
        [JsonProperty("platformDescription")]
        public string PlatformDescription { get; set; } = string.Empty;
    }

    public class ObsResponse<T>
    {
        [JsonProperty("requestId")]
        public string RequestId { get; set; } = string.Empty;
        [JsonProperty("requestStaus")]
        public ObsRequestStatus RequestStatus { get; set; } = new ObsRequestStatus();
        [JsonProperty("requestType")]
        public string RequestType { get; set; } = string.Empty;
        [JsonProperty("responseData", NullValueHandling = NullValueHandling.Ignore)]
        public T ResponseData { get; set; }
    }

    public class ObsAuthentication
    {
        [JsonProperty("challenge")]
        public string Challenge { get; set; } = string.Empty;
        [JsonProperty("salt")]
        public string Salt { get; set; } = string.Empty;
    }

    public class ObsHelloResponse
    {
        [JsonProperty("authentication")]
        public ObsAuthentication Authentication { get; set; }
        [JsonProperty("obsWebSocketVersion")]
        public string ObsWebSocketVersion { get; set; } = string.Empty;
        [JsonProperty("rpcVersion")]
        public int RpcVersion { get; set; }
    }

    public class ObsAuthenticationRequest
    {
        [JsonProperty("rpcVersion")]
        public int RpcVersion { get; set; } = 1;
        [JsonProperty("authentication")]
        public string Authentication { get; set; } = string.Empty;
    }

    public class ObsRequestMessage<T>
    {
        [JsonProperty("requestId")]
        public string RequestId { get; set; } = Guid.NewGuid().ToString();
        [JsonProperty("requestType", NullValueHandling = NullValueHandling.Ignore)]
        public string RequestType { get; set; }
        [JsonProperty("requestData", NullValueHandling = NullValueHandling.Ignore)]
        public T RequestData { get; set; }
    }

    public class RequestBatchMessage
    {
        [JsonProperty("requestId")]
        public string RequestId { get; set; } = Guid.NewGuid().ToString();
        [JsonProperty("requests")]
        public List<ObsRequestMessage<object>> Requests { get; set; } = new List<ObsRequestMessage<object>>();
    }
}
