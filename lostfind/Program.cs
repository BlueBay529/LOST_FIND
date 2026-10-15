using System.Collections.Concurrent;
using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using LostFind.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.FileProviders;

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    WebRootPath = "dist"
});

// Add CORS to allow local development / frontend calls
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 서울 열린데이터광장 호출용 클라이언트입니다.
// 인증키가 요청 경로에 포함되므로 기본 HttpClient 로깅에 노출되지 않도록
// 애플리케이션 수명 동안 하나의 클라이언트를 재사용합니다.
builder.Services.AddSingleton(new HttpClient
{
    Timeout = TimeSpan.FromSeconds(20)
});
builder.Services.AddMemoryCache();

var app = builder.Build();

app.UseCors("AllowAll");

// Serve static files from 'dist' directory
var distPath = Path.Combine(builder.Environment.ContentRootPath, "dist");
if (Directory.Exists(distPath))
{
    app.UseDefaultFiles(new DefaultFilesOptions
    {
        FileProvider = new PhysicalFileProvider(distPath),
        RequestPath = ""
    });
    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(distPath),
        RequestPath = ""
    });
}
else
{
    app.UseDefaultFiles();
    app.UseStaticFiles();
}

app.MapGet("/items", () => Results.File(
    Path.Combine(distPath, "items.html"),
    "text/html; charset=utf-8"));

// In-Memory Thread-safe Database
var items = new ConcurrentDictionary<string, LostFoundItem>();
var missingPets = new ConcurrentDictionary<string, MissingPet>();
var inquiries = new ConcurrentDictionary<string, List<Inquiry>>();
var supportTickets = new ConcurrentBag<SupportTicket>();

// Initialize Seed Data
void SeedData()
{
    var seeds = new List<LostFoundItem>
    {
        new()
        {
            Id = "item-01",
            Type = "found",
            Category = "wallet",
            CategoryName = "지갑",
            Dong = "면목동",
            Title = "진한 초록색 반지갑",
            Location = "면목역 2번 출구 앞 벤치",
            OccurredAt = "오늘 오후 2:10",
            TimeAgo = "12분 전",
            Description = "초록색 가죽 반지갑입니다. 내부에 학생증 및 교통카드가 들어있어 지구대에 전달하기 전 임시 보관 중입니다. 신분 확인 가능한 분께 전달드립니다.",
            Status = "보관중",
            MatchCount = 2,
            ViewCount = 42,
            BookmarkCount = 3,
            ReporterName = "친절한이웃",
            ContactInfo = "면목역 인근 직거래 또는 안심메시지"
        },
        new()
        {
            Id = "item-02",
            Type = "lost",
            Category = "phone",
            CategoryName = "휴대폰",
            Dong = "상봉동",
            Title = "투명 케이스 아이폰 15",
            Location = "상봉 코스트코 인근 버스정류장",
            OccurredAt = "어제 오후 7:30",
            TimeAgo = "1시간 전",
            Description = "투명 맥세이프 케이스를 씌운 아이폰입니다. 배경화면에 강아지 사진이 설정되어 있습니다. 소중한 추억이 담겨있으니 습득하신 분 꼭 연락 부탁드립니다.",
            Status = "제보대기",
            MatchCount = 0,
            ViewCount = 89,
            BookmarkCount = 8,
            ReporterName = "상봉주민",
            ContactInfo = "안심 메시지 또는 상봉파출소"
        },
        new()
        {
            Id = "item-03",
            Type = "found",
            Category = "bag",
            CategoryName = "가방",
            Dong = "망우동",
            Title = "남색 에코백",
            Location = "망우역사문화공원 산책로 쉼터",
            OccurredAt = "오늘 오전 11:20",
            TimeAgo = "2시간 전",
            Description = "남색 캔버스 에코백입니다. 안에는 책 한 권과 텀블러가 들어있습니다. 훼손 없이 안전하게 보관하고 있으니 특징 확인 후 찾아가세요.",
            Status = "보관중",
            MatchCount = 1,
            ViewCount = 31,
            BookmarkCount = 1,
            ReporterName = "망우동라이더",
            ContactInfo = "망우역 인근 약속 가능"
        },
        new()
        {
            Id = "item-04",
            Type = "lost",
            Category = "keys",
            CategoryName = "열쇠",
            Dong = "신내동",
            Title = "곰돌이 키링 자동차 스마트키",
            Location = "신내동 봉화산역 3번 출구 방향",
            OccurredAt = "지난 월요일 저녁",
            TimeAgo = "어제",
            Description = "현대 스마트키에 갈색 뜨개 곰돌이 인형 키링이 달려 있습니다. 차 운행을 못하고 있어 애타게 찾고 있습니다. 사례금 준비되어 있습니다.",
            Status = "제보대기",
            MatchCount = 0,
            ViewCount = 65,
            BookmarkCount = 5,
            ReporterName = "봉화산토박이",
            ContactInfo = "신내동 직거래 가능"
        },
        new()
        {
            Id = "item-05",
            Type = "found",
            Category = "other",
            CategoryName = "기타",
            Dong = "중화동",
            Title = "무선 블루투스 이어폰 케이스",
            Location = "중화역 1번 출구 앞 편의점 파라솔",
            OccurredAt = "오늘 오전 9:40",
            TimeAgo = "4시간 전",
            Description = "흰색 무선 이어폰 충전 케이스(본체 포함)입니다. 스티커가 붙어있어 주인이 쉽게 알아보실 수 있습니다.",
            Status = "보관중",
            MatchCount = 1,
            ViewCount = 53,
            BookmarkCount = 2,
            ReporterName = "중화역지킴이",
            ContactInfo = "중화역 인근"
        },
        new()
        {
            Id = "item-06",
            Type = "lost",
            Category = "wallet",
            CategoryName = "지갑",
            Dong = "묵동",
            Title = "검정색 카드지갑",
            Location = "먹골역 7번 출구 다이소 근처",
            OccurredAt = "어제 밤 10:15",
            TimeAgo = "어제",
            Description = "검정색 사피아노 가죽 카드지갑이며 신분증과 신용카드 2장 들어있습니다. 습득하신 분 제보 부탁드립니다!",
            Status = "제보대기",
            MatchCount = 0,
            ViewCount = 38,
            BookmarkCount = 4,
            ReporterName = "묵동새댁",
            ContactInfo = "안심 메시지"
        }
    };

    foreach (var item in seeds)
    {
        items[item.Id] = item;
    }
}

SeedData();

// 실종 반려동물 모듈 화면 확인용 초안 데이터
missingPets["pet-sample-01"] = new MissingPet
{
    Id = "pet-sample-01",
    Name = "몽이",
    AnimalType = "강아지",
    Breed = "말티즈",
    Color = "흰색",
    Dong = "면목동",
    LostLocation = "용마폭포공원 입구 산책로",
    LostAt = "오늘 오후 3시경",
    Description = "빨간색 목줄을 착용한 작은 체구의 강아지입니다. 사람을 잘 따르지만 큰 소리에 놀랄 수 있습니다.",
    PhotoUrl = "/assets/mongi-sample.png",
    ContactInfo = "찾아드림 안심 메시지",
    ReporterName = "면목동 주민",
    Status = "찾는중",
    CreatedAt = DateTime.UtcNow
};

// Helper to map category English code to Korean
string GetCategoryName(string cat) => cat switch
{
    "wallet" => "지갑",
    "phone" => "휴대폰",
    "bag" => "가방",
    "keys" => "열쇠",
    "electronics" => "전자기기",
    "cloth" => "의류/잡화",
    _ => "기타"
};

string NormalizePublicDescription(string value)
{
    if (string.IsNullOrWhiteSpace(value))
    {
        return string.Empty;
    }

    var withLineBreaks = Regex.Replace(value, @"<br\s*/?>", "\n", RegexOptions.IgnoreCase);
    var withoutTags = Regex.Replace(withLineBreaks, @"<[^>]+>", string.Empty);
    return WebUtility.HtmlDecode(withoutTags).Trim();
}

bool IsPublicItemFrom2025OrLater(SeoulLostArticleRow row)
{
    static bool IsAllowedYear(string value) =>
        value.Length >= 4 &&
        int.TryParse(value[..4], out var year) &&
        year >= 2025;

    return IsAllowedYear(row.RegisteredDate) || IsAllowedYear(row.ReceivedDate);
}

async Task<List<SeoulPublicLostItem>> FetchLost112ItemsAsync(
    HttpClient httpClient,
    IConfiguration configuration,
    CancellationToken cancellationToken)
{
    var apiKey = configuration["DATA_GO_KR_API_KEY"] ?? configuration["LOST112_API_KEY"];
    if (string.IsNullOrWhiteSpace(apiKey))
    {
        return [];
    }

    var endpoint = configuration["LOST112_API_BASE_URL"]
        ?? "https://apis.data.go.kr/1320000/LosfundInfoInqireService/getLosfundInfoAccToClAreaPd";
    var serviceKey = apiKey.Contains('%') ? apiKey : Uri.EscapeDataString(apiKey);
    var requestUrl = $"{endpoint}?serviceKey={serviceKey}&START_YMD=20250101&END_YMD={DateTime.Today:yyyyMMdd}&pageNo=1&numOfRows=500";

    try
    {
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(5));
        using var response = await httpClient.GetAsync(requestUrl, timeoutCts.Token);
        if (!response.IsSuccessStatusCode)
        {
            return [];
        }

        var xml = await response.Content.ReadAsStringAsync(timeoutCts.Token);
        var document = XDocument.Parse(xml);

        static string Value(XElement item, string name) =>
            item.Elements().FirstOrDefault(element =>
                element.Name.LocalName.Equals(name, StringComparison.OrdinalIgnoreCase))?.Value.Trim() ?? string.Empty;

        static string FormatDate(string value) =>
            value.Length == 8 && DateOnly.TryParseExact(value, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)
                ? date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)
                : value;

        return document.Descendants()
            .Where(element => element.Name.LocalName.Equals("item", StringComparison.OrdinalIgnoreCase))
            .Select(item =>
            {
                var place = Value(item, "fdPlace");
                var storage = Value(item, "cstdyPlace");
                var organization = Value(item, "depPlace");
                if (string.IsNullOrWhiteSpace(organization)) organization = Value(item, "orgNm");

                return new SeoulPublicLostItem
                {
                    Id = Value(item, "atcId"),
                    Status = "보관 중",
                    RegisteredDate = FormatDate(Value(item, "fdYmd")),
                    ReceivedDate = FormatDate(Value(item, "fdYmd")),
                    Description = NormalizePublicDescription(Value(item, "uniq")),
                    StoragePlace = string.IsNullOrWhiteSpace(storage)
                        ? place
                        : string.IsNullOrWhiteSpace(place) || storage.Contains(place, StringComparison.OrdinalIgnoreCase)
                            ? storage
                            : $"{storage} (습득: {place})",
                    ItemName = Value(item, "fdPrdtNm"),
                    ItemCategory = Value(item, "prdtClNm"),
                    ReceivingOrganization = organization,
                    Source = "경찰청 LOST112"
                };
            })
            .Where(item =>
                item.RegisteredDate.Length >= 4 &&
                int.TryParse(item.RegisteredDate[..4], out var year) && year >= 2025 &&
                $"{item.StoragePlace} {item.ReceivingOrganization} {item.Description}".Contains("중랑", StringComparison.OrdinalIgnoreCase))
            .ToList();
    }
    catch
    {
        return [];
    }
}

List<SeoulPublicLostItem> MergePublicItems(
    IEnumerable<SeoulPublicLostItem> seoulItems,
    IEnumerable<SeoulPublicLostItem> lost112Items)
{
    static string Normalize(string value) =>
        Regex.Replace(value ?? string.Empty, "[^0-9A-Za-z가-힣]", string.Empty).ToLowerInvariant();

    var merged = new List<SeoulPublicLostItem>();
    foreach (var incoming in seoulItems.Concat(lost112Items))
    {
        var idKey = Normalize(incoming.Id);
        var fingerprint = $"{Normalize(incoming.ItemName)}|{incoming.RegisteredDate.Replace("-", string.Empty)}|{Normalize(incoming.StoragePlace)}";
        var existing = merged.FirstOrDefault(item =>
            (!string.IsNullOrWhiteSpace(idKey) && Normalize(item.Id) == idKey) ||
            $"{Normalize(item.ItemName)}|{item.RegisteredDate.Replace("-", string.Empty)}|{Normalize(item.StoragePlace)}" == fingerprint);

        if (existing is null)
        {
            merged.Add(incoming);
            continue;
        }

        if (string.IsNullOrWhiteSpace(existing.Description)) existing.Description = incoming.Description;
        if (string.IsNullOrWhiteSpace(existing.StoragePlace)) existing.StoragePlace = incoming.StoragePlace;
        if (string.IsNullOrWhiteSpace(existing.ReceivingOrganization)) existing.ReceivingOrganization = incoming.ReceivingOrganization;
        if (string.IsNullOrWhiteSpace(existing.ItemCategory)) existing.ItemCategory = incoming.ItemCategory;
        if (string.IsNullOrWhiteSpace(existing.ReceivedDate)) existing.ReceivedDate = incoming.ReceivedDate;
        if (!existing.Source.Contains(incoming.Source, StringComparison.OrdinalIgnoreCase))
        {
            existing.Source = $"{existing.Source} · {incoming.Source}";
        }
    }

    return merged;
}

// --- REST API Endpoints ---

// 서울 열린데이터광장 대중교통 습득물 조회
// Cloudtype 환경변수: SEOUL_OPEN_API_KEY
app.MapGet("/api/public-items", async (
    int? start,
    int? end,
    string? managementNumber,
    string? registeredDate,
    string? q,
    bool? refresh,
    HttpClient httpClient,
    IConfiguration configuration,
    IMemoryCache cache,
    CancellationToken cancellationToken) =>
{
    var apiKey = configuration["SEOUL_OPEN_API_KEY"];
    if (string.IsNullOrWhiteSpace(apiKey) && app.Environment.IsDevelopment())
    {
        apiKey = "6175714668746c6431323246744c5a41";
    }
    if (string.IsNullOrWhiteSpace(apiKey))
    {
        return Results.Json(new
        {
            success = false,
            message = "서울시 Open API 인증키가 설정되지 않았습니다."
        }, statusCode: StatusCodes.Status503ServiceUnavailable);
    }

    var startIndex = start ?? 1;
    var endIndex = end ?? 100;
    if (apiKey.Equals("sample", StringComparison.OrdinalIgnoreCase))
    {
        endIndex = Math.Min(endIndex, 5);
    }
    if (startIndex < 1 || endIndex < startIndex || endIndex - startIndex + 1 > 1000)
    {
        return Results.BadRequest(new
        {
            success = false,
            message = "조회 범위는 1 이상이며 한 번에 최대 1,000건까지 가능합니다."
        });
    }

    var cacheKey = $"public-items:{startIndex}:{endIndex}:{managementNumber}:{registeredDate}:{q}";
    if (refresh is not true && cache.TryGetValue<PublicItemsCacheEntry>(cacheKey, out var cached) && cached is not null)
    {
        return Results.Ok(new
        {
            success = true,
            sources = new[] { "서울 열린데이터광장", "경찰청 LOST112" },
            total = cached.Total,
            count = cached.Data.Count,
            cutoffYear = 2025,
            adaptive = true,
            cached = true,
            updatedAt = cached.UpdatedAt,
            start = startIndex,
            end = endIndex,
            data = cached.Data
        });
    }

    if (!string.IsNullOrWhiteSpace(registeredDate) &&
        !DateOnly.TryParseExact(
            registeredDate,
            "yyyy-MM-dd",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out _))
    {
        return Results.BadRequest(new
        {
            success = false,
            message = "등록일자는 yyyy-MM-dd 형식으로 입력해주세요."
        });
    }

    var baseUrl = configuration["SEOUL_OPEN_API_BASE_URL"];
    if (string.IsNullOrWhiteSpace(baseUrl))
    {
        baseUrl = "http://openapi.seoul.go.kr:8088";
    }

    var requestUrl = $"{baseUrl.TrimEnd('/')}/{Uri.EscapeDataString(apiKey)}/json/lostArticleInfo/{startIndex}/{endIndex}";

    if (!string.IsNullOrWhiteSpace(managementNumber) || !string.IsNullOrWhiteSpace(registeredDate))
    {
        var managementSegment = string.IsNullOrWhiteSpace(managementNumber)
            ? "%20"
            : Uri.EscapeDataString(managementNumber.Trim());
        requestUrl += $"/{managementSegment}";

        if (!string.IsNullOrWhiteSpace(registeredDate))
        {
            requestUrl += $"/{Uri.EscapeDataString(registeredDate)}";
        }
    }

    requestUrl += "/";

    var requestedCount = endIndex - startIndex + 1;
    var lost112Task = FetchLost112ItemsAsync(httpClient, configuration, cancellationToken);
    using var seoulTimeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
    seoulTimeoutCts.CancelAfter(TimeSpan.FromSeconds(5));
    var seoulToken = seoulTimeoutCts.Token;

    try
    {
        using var response = await httpClient.GetAsync(requestUrl, seoulToken);
        if (!response.IsSuccessStatusCode)
        {
            return Results.Json(new
            {
                success = false,
                message = "서울시 공공데이터 서버 응답을 받지 못했습니다."
            }, statusCode: StatusCodes.Status502BadGateway);
        }

        var envelope = await response.Content.ReadFromJsonAsync<SeoulLostArticleEnvelope>(seoulToken);
        var payload = envelope?.LostArticleInfo;
        var apiResult = payload?.Result ?? envelope?.Result;

        if (payload is null)
        {
            return Results.Json(new
            {
                success = false,
                code = apiResult?.Code,
                message = apiResult?.Message ?? "서울시 공공데이터 응답 형식을 확인할 수 없습니다."
            }, statusCode: StatusCodes.Status502BadGateway);
        }

        if (apiResult is not null && apiResult.Code is not ("INFO-000" or "INFO-200"))
        {
            return Results.Json(new
            {
                success = false,
                code = apiResult.Code,
                message = apiResult.Message
            }, statusCode: StatusCodes.Status502BadGateway);
        }

        var recentRows = payload.Rows
            .Where(IsPublicItemFrom2025OrLater)
            .ToList();

        // 전체 건수와 실제 응답 날짜를 기준으로 최신 구간부터 필요한 만큼 역방향 조회한다.
        if (string.IsNullOrWhiteSpace(managementNumber) &&
            string.IsNullOrWhiteSpace(registeredDate) &&
            recentRows.Count < requestedCount &&
            payload.TotalCount > endIndex)
        {
            var batchSize = Math.Min(500, Math.Max(100, requestedCount * 2));
            var cursorEnd = payload.TotalCount;
            var requestCount = 0;

            while (recentRows.Count < requestedCount && cursorEnd >= 1 && requestCount < 2)
            {
                var batchStart = Math.Max(1, cursorEnd - batchSize + 1);
                var latestUrl = $"{baseUrl.TrimEnd('/')}/{Uri.EscapeDataString(apiKey)}/json/lostArticleInfo/{batchStart}/{cursorEnd}/";

                using var latestResponse = await httpClient.GetAsync(latestUrl, seoulToken);
                if (!latestResponse.IsSuccessStatusCode)
                {
                    break;
                }

                var latestEnvelope = await latestResponse.Content.ReadFromJsonAsync<SeoulLostArticleEnvelope>(seoulToken);
                var batchRows = latestEnvelope?.LostArticleInfo?.Rows ?? [];
                var batchRecentRows = batchRows.Where(IsPublicItemFrom2025OrLater).ToList();

                recentRows.AddRange(batchRecentRows);
                requestCount++;
                cursorEnd = batchStart - 1;

                // 최신순으로 정렬된 데이터에서 기준 연도 이전 구간에 도달하면 조회를 끝낸다.
                if (batchRecentRows.Count == 0)
                {
                    break;
                }
            }
        }

        IEnumerable<SeoulLostArticleRow> filteredRows = recentRows
            .DistinctBy(row => string.IsNullOrWhiteSpace(row.ManagementNumber)
                ? $"{row.ItemName}|{row.RegisteredDate}|{row.StoragePlace}"
                : row.ManagementNumber)
            .OrderByDescending(row => string.CompareOrdinal(row.RegisteredDate, row.ReceivedDate) >= 0
                ? row.RegisteredDate
                : row.ReceivedDate)
            .Take(requestedCount);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            filteredRows = filteredRows.Where(row =>
                row.ItemName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                row.ItemCategory.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                row.Description.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                row.StoragePlace.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                row.ReceivingOrganization.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        var seoulData = filteredRows.Select(row => new SeoulPublicLostItem
        {
            Id = row.ManagementNumber,
            Status = row.Status,
            RegisteredDate = row.RegisteredDate,
            ReceivedDate = row.ReceivedDate,
            Description = NormalizePublicDescription(row.Description),
            StoragePlace = row.StoragePlace,
            RegistrantId = row.RegistrantId,
            ItemName = row.ItemName,
            ItemCategory = row.ItemCategory,
            ReceivingOrganization = row.ReceivingOrganization,
            Source = "서울 열린데이터광장"
        }).ToList();

        var lost112Data = await lost112Task;
        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim();
            lost112Data = lost112Data.Where(item =>
                item.ItemName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                item.ItemCategory.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                item.Description.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                item.StoragePlace.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                item.ReceivingOrganization.Contains(term, StringComparison.OrdinalIgnoreCase)).ToList();
        }

        var data = MergePublicItems(seoulData, lost112Data)
            .OrderByDescending(item => string.CompareOrdinal(item.RegisteredDate, item.ReceivedDate) >= 0
                ? item.RegisteredDate
                : item.ReceivedDate)
            .Take(requestedCount)
            .ToList();

        var updatedAt = DateTime.UtcNow;
        cache.Set(cacheKey, new PublicItemsCacheEntry
        {
            Data = data,
            Total = payload.TotalCount,
            UpdatedAt = updatedAt
        }, TimeSpan.FromMinutes(10));

        return Results.Ok(new
        {
            success = true,
            sources = new[] { "서울 열린데이터광장", "경찰청 LOST112" },
            total = payload.TotalCount,
            count = data.Count,
            cutoffYear = 2025,
            adaptive = true,
            cached = false,
            updatedAt,
            start = startIndex,
            end = endIndex,
            data
        });
    }
    catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
    {
        var fallback = (await lost112Task).Take(requestedCount).ToList();
        if (fallback.Count > 0)
        {
            var updatedAt = DateTime.UtcNow;
            cache.Set(cacheKey, new PublicItemsCacheEntry { Data = fallback, Total = fallback.Count, UpdatedAt = updatedAt }, TimeSpan.FromMinutes(10));
            return Results.Ok(new { success = true, sources = new[] { "경찰청 LOST112" }, total = fallback.Count, count = fallback.Count, cutoffYear = 2025, adaptive = true, cached = false, partial = true, updatedAt, start = startIndex, end = endIndex, data = fallback });
        }
        return Results.Json(new
        {
            success = false,
            message = "서울시 공공데이터 서버 응답 시간이 초과되었습니다."
        }, statusCode: StatusCodes.Status504GatewayTimeout);
    }
    catch (HttpRequestException)
    {
        var fallback = (await lost112Task).Take(requestedCount).ToList();
        if (fallback.Count > 0)
        {
            var updatedAt = DateTime.UtcNow;
            cache.Set(cacheKey, new PublicItemsCacheEntry { Data = fallback, Total = fallback.Count, UpdatedAt = updatedAt }, TimeSpan.FromMinutes(10));
            return Results.Ok(new { success = true, sources = new[] { "경찰청 LOST112" }, total = fallback.Count, count = fallback.Count, cutoffYear = 2025, adaptive = true, cached = false, partial = true, updatedAt, start = startIndex, end = endIndex, data = fallback });
        }
        return Results.Json(new
        {
            success = false,
            message = "서울시 공공데이터 서버에 연결할 수 없습니다."
        }, statusCode: StatusCodes.Status502BadGateway);
    }
    catch (System.Text.Json.JsonException)
    {
        var fallback = (await lost112Task).Take(requestedCount).ToList();
        if (fallback.Count > 0)
        {
            var updatedAt = DateTime.UtcNow;
            cache.Set(cacheKey, new PublicItemsCacheEntry { Data = fallback, Total = fallback.Count, UpdatedAt = updatedAt }, TimeSpan.FromMinutes(10));
            return Results.Ok(new { success = true, sources = new[] { "경찰청 LOST112" }, total = fallback.Count, count = fallback.Count, cutoffYear = 2025, adaptive = true, cached = false, partial = true, updatedAt, start = startIndex, end = endIndex, data = fallback });
        }
        return Results.Json(new
        {
            success = false,
            message = "서울시 공공데이터 응답을 처리할 수 없습니다."
        }, statusCode: StatusCodes.Status502BadGateway);
    }
});

// 1. Get Items with Filters
app.MapGet("/api/items", (string? q, string? dong, string? type, string? cat) =>
{
    var query = items.Values.AsEnumerable();

    if (!string.IsNullOrWhiteSpace(cat) && cat != "all")
    {
        query = query.Where(i => i.Category.Equals(cat, StringComparison.OrdinalIgnoreCase));
    }

    if (!string.IsNullOrWhiteSpace(dong) && dong != "all")
    {
        query = query.Where(i => i.Dong.Contains(dong, StringComparison.OrdinalIgnoreCase));
    }

    if (!string.IsNullOrWhiteSpace(type) && type != "all")
    {
        query = query.Where(i => i.Type.Equals(type, StringComparison.OrdinalIgnoreCase));
    }

    if (!string.IsNullOrWhiteSpace(q))
    {
        var term = q.Trim().ToLowerInvariant();
        query = query.Where(i =>
            i.Title.ToLowerInvariant().Contains(term) ||
            i.Location.ToLowerInvariant().Contains(term) ||
            i.Description.ToLowerInvariant().Contains(term) ||
            i.Dong.ToLowerInvariant().Contains(term) ||
            i.CategoryName.ToLowerInvariant().Contains(term));
    }

    var result = query.OrderByDescending(i => i.CreatedAt).ToList();
    return Results.Ok(new
    {
        success = true,
        total = result.Count,
        data = result
    });
});

// 2. Get Single Item
app.MapGet("/api/items/{id}", (string id) =>
{
    if (items.TryGetValue(id, out var item))
    {
        item.ViewCount++;
        return Results.Ok(new { success = true, data = item });
    }
    return Results.NotFound(new { success = false, message = "물품을 찾을 수 없습니다." });
});

// 3. Create Item
app.MapPost("/api/items", (CreateItemRequest req) =>
{
    if (string.IsNullOrWhiteSpace(req.Title))
    {
        return Results.BadRequest(new { success = false, message = "물건 이름을 입력해주세요." });
    }

    var newItem = new LostFoundItem
    {
        Id = "item-" + Guid.NewGuid().ToString("N")[..6],
        Type = string.IsNullOrWhiteSpace(req.Type) ? "lost" : req.Type,
        Category = string.IsNullOrWhiteSpace(req.Category) ? "other" : req.Category,
        CategoryName = GetCategoryName(req.Category ?? "other"),
        Dong = string.IsNullOrWhiteSpace(req.Dong) ? "중랑구" : req.Dong,
        Title = req.Title.Trim(),
        Location = req.Location?.Trim() ?? "중랑구 관내",
        OccurredAt = string.IsNullOrWhiteSpace(req.OccurredAt) ? "방금 전" : req.OccurredAt,
        TimeAgo = "방금 전",
        Description = req.Description?.Trim() ?? "등록된 상세 설명이 없습니다.",
        Status = req.Type == "found" ? "보관중" : "제보대기",
        MatchCount = 0,
        ViewCount = 1,
        BookmarkCount = 0,
        ReporterName = string.IsNullOrWhiteSpace(req.ReporterName) ? "이웃 주민" : req.ReporterName,
        ContactInfo = string.IsNullOrWhiteSpace(req.ContactInfo) ? "안심 메시지 전용" : req.ContactInfo,
        CreatedAt = DateTime.UtcNow
    };

    items[newItem.Id] = newItem;

    return Results.Created($"/api/items/{newItem.Id}", new
    {
        success = true,
        message = "신고가 정상 등록되었습니다.",
        data = newItem
    });
});

// 4. Bookmark Item
app.MapPost("/api/items/{id}/bookmark", (string id) =>
{
    if (items.TryGetValue(id, out var item))
    {
        item.BookmarkCount++;
        return Results.Ok(new { success = true, bookmarks = item.BookmarkCount });
    }
    return Results.NotFound(new { success = false, message = "물품을 찾을 수 없습니다." });
});

// 5. Update Status
app.MapPut("/api/items/{id}/status", (string id, UpdateStatusRequest req) =>
{
    if (items.TryGetValue(id, out var item))
    {
        item.Status = req.Status;
        return Results.Ok(new { success = true, data = item });
    }
    return Results.NotFound(new { success = false, message = "물품을 찾을 수 없습니다." });
});

// 6. Inquiries (Contact / Tip Message)
app.MapPost("/api/items/{id}/inquiries", (string id, InquiryRequest req) =>
{
    if (!items.ContainsKey(id))
    {
        return Results.NotFound(new { success = false, message = "해당 물품이 존재하지 않습니다." });
    }

    var list = inquiries.GetOrAdd(id, _ => new List<Inquiry>());
    var inq = new Inquiry
    {
        ItemId = id,
        SenderName = string.IsNullOrWhiteSpace(req.SenderName) ? "익명 이웃" : req.SenderName,
        SenderContact = string.IsNullOrWhiteSpace(req.SenderContact) ? "안심 채팅" : req.SenderContact,
        Message = req.Message,
        SentAt = DateTime.UtcNow
    };

    lock (list)
    {
        list.Add(inq);
    }

    return Results.Ok(new
    {
        success = true,
        message = "소중한 제보 메시지가 등록자에게 전달되었습니다.",
        data = inq
    });
});

app.MapGet("/api/items/{id}/inquiries", (string id) =>
{
    if (inquiries.TryGetValue(id, out var list))
    {
        lock (list)
        {
            return Results.Ok(new { success = true, data = list.ToList() });
        }
    }
    return Results.Ok(new { success = true, data = new List<Inquiry>() });
});

// 반려동물 실종 신고는 일반 분실물과 분리된 전용 데이터/API를 사용합니다.
app.MapGet("/api/pets", () =>
{
    var result = missingPets.Values.OrderByDescending(p => p.CreatedAt).ToList();
    return Results.Ok(new { success = true, total = result.Count, data = result });
});

app.MapPost("/api/pets", (CreateMissingPetRequest req) =>
{
    if (string.IsNullOrWhiteSpace(req.Name) || string.IsNullOrWhiteSpace(req.AnimalType) || string.IsNullOrWhiteSpace(req.LostLocation))
    {
        return Results.BadRequest(new { success = false, message = "반려동물 이름, 종류, 실종 장소를 입력해주세요." });
    }

    var pet = new MissingPet
    {
        Name = req.Name.Trim(),
        AnimalType = req.AnimalType.Trim(),
        Breed = req.Breed.Trim(),
        Color = req.Color.Trim(),
        Dong = string.IsNullOrWhiteSpace(req.Dong) ? "중랑구" : req.Dong,
        LostLocation = req.LostLocation.Trim(),
        LostAt = string.IsNullOrWhiteSpace(req.LostAt) ? "최근" : req.LostAt.Trim(),
        Description = req.Description.Trim(),
        ContactInfo = string.IsNullOrWhiteSpace(req.ContactInfo) ? "안심 메시지 전용" : req.ContactInfo.Trim(),
        ReporterName = string.IsNullOrWhiteSpace(req.ReporterName) ? "이웃 주민" : req.ReporterName.Trim()
    };

    missingPets[pet.Id] = pet;
    return Results.Created($"/api/pets/{pet.Id}", new { success = true, message = "반려동물 실종 신고가 등록되었습니다.", data = pet });
});

// 7. Auth (Login & Register)
app.MapPost("/api/auth/login", (LoginRequest req) =>
{
    if (string.IsNullOrWhiteSpace(req.Username))
    {
        return Results.BadRequest(new { success = false, message = "아이디를 입력해주세요." });
    }

    var displayName = req.Username switch
    {
        "hong" or "홍길동" => "중랑구민 홍길동",
        "admin" => "중랑구 관리자",
        _ => req.Username + " 님"
    };

    return Results.Ok(new AuthResponse
    {
        Success = true,
        Username = req.Username,
        DisplayName = displayName,
        Message = $"{displayName}으로 로그인되었습니다.",
        Token = "token_" + Guid.NewGuid().ToString("N")
    });
});

app.MapPost("/api/auth/register", (LoginRequest req) =>
{
    if (string.IsNullOrWhiteSpace(req.Username))
    {
        return Results.BadRequest(new { success = false, message = "가입할 아이디를 입력해주세요." });
    }

    return Results.Ok(new AuthResponse
    {
        Success = true,
        Username = req.Username,
        DisplayName = req.Username + " 님",
        Message = "회원가입이 완료되었습니다! 자동 로그인되었습니다.",
        Token = "token_" + Guid.NewGuid().ToString("N")
    });
});

// 8. Support & FAQ
app.MapPost("/api/support", (SupportTicketRequest req) =>
{
    if (string.IsNullOrWhiteSpace(req.Content))
    {
        return Results.BadRequest(new { success = false, message = "문의 내용을 입력해주세요." });
    }

    var ticket = new SupportTicket
    {
        Category = req.Category,
        Title = req.Title,
        Content = req.Content,
        Contact = req.Contact,
        SubmittedAt = DateTime.UtcNow
    };

    supportTickets.Add(ticket);

    return Results.Ok(new
    {
        success = true,
        ticketId = ticket.Id,
        message = "고객센터 문의가 안전하게 접수되었습니다. 빠른 시일 내 안내드리겠습니다."
    });
});

app.MapGet("/api/support/faq", () =>
{
    var faqs = new[]
    {
        new { Q = "물건을 주웠을 때 어떻게 해야 하나요?", A = "습득물 등록 후 안전한 지구대나 파출소에 인계하시거나, 본인 확인 질문을 통해 주인에게 직접 인계하실 수 있습니다." },
        new { Q = "개인 연락처가 외부에 노출되나요?", A = "아닙니다. 개인 전화번호는 절대 직접 공개되지 않으며, 안심 메시지 시스템을 통해서만 안전하게 소통합니다." },
        new { Q = "분실물을 찾았을 때 어떻게 하나요?", A = "상세 보기에서 [해결 완료 처리] 버튼을 누르면 다른 이웃들이 더 이상 제보하지 않도록 완료 처리됩니다." },
        new { Q = "보상금(사례금) 지급 기준은 어떻게 되나요?", A = "유실물법에 따라 물건 가액의 5%~20% 범위에서 습득자와 분실자 간 상호 합의로 결정할 수 있습니다." }
    };
    return Results.Ok(new { success = true, data = faqs });
});

Console.WriteLine("==================================================");
Console.WriteLine(" 찾아드림 - ASP.NET Core C# 서버 준비 완료 ");
Console.WriteLine(" http://localhost:5000 또는 브라우저에서 확인 가능 ");
Console.WriteLine("==================================================");

app.Run();
