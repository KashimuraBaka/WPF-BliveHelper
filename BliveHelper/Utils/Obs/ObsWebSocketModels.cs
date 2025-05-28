using System.Text.Json.Serialization;

namespace BliveHelper.Utils.Obs
{
    public class ObsData<T>
    {
        [JsonPropertyName("op")]
        public ObsMessageTypes OperationCode { get; set; }
        [JsonPropertyName("d")]
        public T? Data { get; set; }
    }

    public class ObsRequestStatus
    {
        [JsonPropertyName("code")]
        public int Code { get; set; }
        [JsonPropertyName("result")]
        public bool Result { get; set; }
    }

    public class ObsVersion
    {
        [JsonPropertyName("obsWebSocketVersion")]
        public string ObsPluginVersion { get; set; } = string.Empty;
        [JsonPropertyName("obsVersion")]
        public string OBSStudioVersion { get; set; } = string.Empty;
        [JsonPropertyName("rpcVersion")]
        public int RpcVersion { set; get; }
        [JsonPropertyName("availableRequests")]
        public List<string> AvailableRequests { get; set; } = [];
        [JsonPropertyName("supportedImageFormats")]
        public List<string> SupportedImageFormats { get; set; } = [];
        [JsonPropertyName("platform")]
        public string Platform { get; set; } = string.Empty;
        [JsonPropertyName("platformDescription")]
        public string PlatformDescription { get; set; } = string.Empty;
    }

    public class ObsResponse<T>
    {
        [JsonPropertyName("requestId")]
        public string RequestId { get; set; } = string.Empty;
        [JsonPropertyName("requestStaus")]
        public ObsRequestStatus RequestStatus { get; set; } = new();
        [JsonPropertyName("requestType")]
        public string RequestType { get; set; } = string.Empty;
        [JsonPropertyName("responseData")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public T? ResponseData { get; set; }
    }

    public class ObsAuthentication
    {
        [JsonPropertyName("challenge")]
        public string Challenge { get; set; } = string.Empty;
        [JsonPropertyName("salt")]
        public string Salt { get; set; } = string.Empty;
    }

    public class ObsHelloResponse
    {
        [JsonPropertyName("authentication")]
        public required ObsAuthentication Authentication { get; set; }
        [JsonPropertyName("obsWebSocketVersion")]
        public string ObsWebSocketVersion { get; set; } = string.Empty;
        [JsonPropertyName("rpcVersion")]
        public int RpcVersion { get; set; }
    }

    public class ObsAuthenticationRequest
    {
        [JsonPropertyName("rpcVersion")]
        public int RpcVersion { get; set; } = 1;
        [JsonPropertyName("authentication")]
        public string Authentication { get; set; } = string.Empty;
    }

    public class ObsRequestMessage<T>
    {
        [JsonPropertyName("requestId")]
        public string RequestId { get; set; } = Guid.NewGuid().ToString();
        [JsonPropertyName("requestType")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? RequestType { get; set; } = string.Empty;
        [JsonPropertyName("requestData")]
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public T? RequestData { get; set; }
    }

    public class RequestBatchMessage
    {
        [JsonPropertyName("requestId")]
        public string RequestId { get; set; } = Guid.NewGuid().ToString();
        [JsonPropertyName("requests")]
        public List<ObsRequestMessage<object>> Requests { get; set; } = [];
    }
}
