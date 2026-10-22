using System.Text.Json.Serialization;

namespace LostFind.Models;

public sealed class SeoulLostArticleEnvelope
{
    [JsonPropertyName("lostArticleInfo")]
    public SeoulLostArticlePayload? LostArticleInfo { get; set; }

    [JsonPropertyName("RESULT")]
    public SeoulOpenApiResult? Result { get; set; }
}

public sealed class SeoulLostArticlePayload
{
    [JsonPropertyName("list_total_count")]
    public int TotalCount { get; set; }

    [JsonPropertyName("RESULT")]
    public SeoulOpenApiResult? Result { get; set; }

    [JsonPropertyName("row")]
    public List<SeoulLostArticleRow> Rows { get; set; } = [];
}

public sealed class SeoulOpenApiResult
{
    [JsonPropertyName("CODE")]
    public string Code { get; set; } = string.Empty;

    [JsonPropertyName("MESSAGE")]
    public string Message { get; set; } = string.Empty;
}

public sealed class SeoulLostArticleRow
{
    [JsonPropertyName("LOST_MNG_NO")]
    public string ManagementNumber { get; set; } = string.Empty;

    [JsonPropertyName("LOST_STTS")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("REG_YMD")]
    public string RegisteredDate { get; set; } = string.Empty;

    [JsonPropertyName("RCV_YMD")]
    public string ReceivedDate { get; set; } = string.Empty;

    [JsonPropertyName("LGS_DTL_CN")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("CSTD_PLC")]
    public string StoragePlace { get; set; } = string.Empty;

    [JsonPropertyName("LOST_RGTR_ID")]
    public string RegistrantId { get; set; } = string.Empty;

    [JsonPropertyName("LOST_NM")]
    public string ItemName { get; set; } = string.Empty;

    [JsonPropertyName("LOST_KND")]
    public string ItemCategory { get; set; } = string.Empty;

    [JsonPropertyName("RCPL")]
    public string ReceivingOrganization { get; set; } = string.Empty;

}

public sealed class SeoulPublicLostItem
{
    public string Id { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string RegisteredDate { get; set; } = string.Empty;
    public string ReceivedDate { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string StoragePlace { get; set; } = string.Empty;
    public string RegistrantId { get; set; } = string.Empty;
    public string ItemName { get; set; } = string.Empty;
    public string ItemCategory { get; set; } = string.Empty;
    public string ReceivingOrganization { get; set; } = string.Empty;
    public string Source { get; set; } = "서울 열린데이터광장";
}

public sealed class PublicItemsCacheEntry
{
    public bool Partial { get; set; }
    public List<SeoulPublicLostItem> Data { get; set; } = [];
    public int Total { get; set; }
    public DateTime UpdatedAt { get; set; }
}
