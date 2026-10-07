using System.Collections.Concurrent;
using LostFind.Models;
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

// --- REST API Endpoints ---

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
