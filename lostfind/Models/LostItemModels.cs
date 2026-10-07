namespace LostFind.Models;

public class LostFoundItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];
    public string Type { get; set; } = "lost"; // "lost" or "found"
    public string Category { get; set; } = "other"; // "wallet", "phone", "bag", "keys", "other"
    public string CategoryName { get; set; } = "기타";
    public string Dong { get; set; } = "면목동";
    public string Title { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string OccurredAt { get; set; } = "오늘";
    public string TimeAgo { get; set; } = "방금 전";
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = "접수완료"; // "접수완료", "보관중", "제보대기", "해결완료"
    public int MatchCount { get; set; } = 0;
    public int ViewCount { get; set; } = 1;
    public int BookmarkCount { get; set; } = 0;
    public string ReporterName { get; set; } = "익명 이웃";
    public string ContactInfo { get; set; } = "안심 메시지 전용";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class CreateItemRequest
{
    public string Type { get; set; } = "lost";
    public string Category { get; set; } = "other";
    public string Dong { get; set; } = "면목동";
    public string Title { get; set; } = string.Empty;
    public string Location { get; set; } = string.Empty;
    public string? OccurredAt { get; set; }
    public string? Description { get; set; }
    public string? ContactInfo { get; set; }
    public string? ReporterName { get; set; }
}

public class MissingPet
{
    public string Id { get; set; } = "pet-" + Guid.NewGuid().ToString("N")[..8];
    public string Name { get; set; } = string.Empty;
    public string AnimalType { get; set; } = string.Empty;
    public string Breed { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string Dong { get; set; } = string.Empty;
    public string LostLocation { get; set; } = string.Empty;
    public string LostAt { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PhotoUrl { get; set; } = string.Empty;
    public string ContactInfo { get; set; } = string.Empty;
    public string ReporterName { get; set; } = "이웃 주민";
    public string Status { get; set; } = "찾는중";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class CreateMissingPetRequest
{
    public string Name { get; set; } = string.Empty;
    public string AnimalType { get; set; } = string.Empty;
    public string Breed { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
    public string Dong { get; set; } = string.Empty;
    public string LostLocation { get; set; } = string.Empty;
    public string LostAt { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ContactInfo { get; set; } = string.Empty;
    public string ReporterName { get; set; } = string.Empty;
}

public class UpdateStatusRequest
{
    public string Status { get; set; } = "해결완료";
}

public class InquiryRequest
{
    public string ItemId { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string SenderContact { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

public class Inquiry
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];
    public string ItemId { get; set; } = string.Empty;
    public string SenderName { get; set; } = string.Empty;
    public string SenderContact { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime SentAt { get; set; } = DateTime.UtcNow;
}

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class AuthResponse
{
    public bool Success { get; set; }
    public string Username { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}

public class SupportTicketRequest
{
    public string Category { get; set; } = "일반문의";
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Contact { get; set; } = string.Empty;
}

public class SupportTicket
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N")[..8];
    public string Category { get; set; } = "일반문의";
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string Contact { get; set; } = string.Empty;
    public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
}
