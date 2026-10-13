using System.Text.Json.Serialization;

namespace SipPortal.Models;

public sealed class ApiExceptionDetail
{
    [JsonPropertyName("message")] public string? Message { get; set; }
    [JsonPropertyName("code")] public int Code { get; set; }
}

public sealed class ApiListResponse<T>
{
    [JsonPropertyName("success")] public bool Success { get; set; }
    [JsonPropertyName("data")] public ApiListData<T>? Data { get; set; }
    [JsonPropertyName("exception")] public ApiExceptionDetail? Exception { get; set; }
}

public sealed class ApiListData<T>
{
    [JsonPropertyName("totalCount")] public int TotalCount { get; set; }
    [JsonPropertyName("count")] public int Count { get; set; }
    [JsonPropertyName("models")] public List<T> Models { get; set; } = [];
    [JsonPropertyName("page")] public int Page { get; set; }
    [JsonPropertyName("pageCount")] public int PageCount { get; set; }
}

public sealed class ApiSingleResponse<T>
{
    [JsonPropertyName("success")] public bool Success { get; set; }
    [JsonPropertyName("data")] public T? Data { get; set; }
    [JsonPropertyName("exception")] public ApiExceptionDetail? Exception { get; set; }
}

public sealed class Incident
{
    public int Id { get; set; }
    public string? Code { get; set; }
    [JsonPropertyName("sender_id")] public int SenderId { get; set; }
    [JsonPropertyName("writer_id")] public int WriterId { get; set; }
    [JsonPropertyName("created_at")] [JsonConverter(typeof(SipPortal.Services.FlexibleInt64Converter))] public long CreatedAt { get; set; }
    public int Status { get; set; }
    [JsonPropertyName("reviewed_at")] [JsonConverter(typeof(SipPortal.Services.FlexibleInt64Converter))] public long ReviewedAt { get; set; }
    public string? Type { get; set; }
    [JsonPropertyName("affected_systems")] public string? AffectedSystems { get; set; }
    [JsonPropertyName("detection_method")] public string? DetectionMethod { get; set; }
    [JsonPropertyName("notification_method")] public string? NotificationMethod { get; set; }
    [JsonPropertyName("system_down")] public int SystemDown { get; set; }
    public string? Description { get; set; }
    [JsonPropertyName("start_date")] [JsonConverter(typeof(SipPortal.Services.FlexibleInt64Converter))] public long StartDate { get; set; }
    [JsonPropertyName("detection_date")] [JsonConverter(typeof(SipPortal.Services.FlexibleInt64Converter))] public long DetectionDate { get; set; }
    [JsonPropertyName("track_record_detected")] public int TrackRecordDetected { get; set; }
    [JsonPropertyName("track_source_ip")] public string? TrackSourceIp { get; set; }
    [JsonPropertyName("track_target_ip")] public string? TrackTargetIp { get; set; }
    [JsonPropertyName("track_port")] public string? TrackPort { get; set; }
    [JsonPropertyName("track_other")] public string? TrackOther { get; set; }
    public string? Prevention { get; set; }
    public string? Addition { get; set; }
    public List<ApiFile> Files { get; set; } = [];
    public List<IncidentMessage> Messages { get; set; } = [];
}

public sealed class IncidentMessage
{
    public int Id { get; set; }
    [JsonPropertyName("writer_id")] public int WriterId { get; set; }
    [JsonPropertyName("created_at")] [JsonConverter(typeof(SipPortal.Services.FlexibleInt64Converter))] public long CreatedAt { get; set; }
    public string? Message { get; set; }
}

public sealed class Vulnerability
{
    public int Id { get; set; }
    public string? Code { get; set; }
    [JsonPropertyName("type_id")] public int TypeId { get; set; }
    [JsonPropertyName("type_other")] public string? TypeOther { get; set; }
    public int Status { get; set; }
    [JsonPropertyName("some_id")] public int SomeId { get; set; }
    public string? Description { get; set; }
    public string? Explanation { get; set; }
    public string? Action { get; set; }
    public string? Address { get; set; }
    [JsonPropertyName("created_at")] [JsonConverter(typeof(SipPortal.Services.FlexibleInt64Converter))] public long CreatedAt { get; set; }
    public List<ApiFile> Files { get; set; } = [];
    [JsonPropertyName("vulnerabilityAddresses")] public List<VulnerabilityAddress> VulnerabilityAddresses { get; set; } = [];
    [JsonPropertyName("vulnerabilityMessages")] public List<VulnerabilityMessage> VulnerabilityMessages { get; set; } = [];
}

public sealed class VulnerabilityMessage
{
    public int Id { get; set; }
    [JsonPropertyName("writer_id")] public int WriterId { get; set; }
    [JsonPropertyName("created_at")] [JsonConverter(typeof(SipPortal.Services.FlexibleInt64Converter))] public long CreatedAt { get; set; }
    public string? Message { get; set; }
}

public sealed class VulnerabilityAddress
{
    public int Id { get; set; }
    [JsonPropertyName("time_stamp")] public string? TimeStamp { get; set; }
    public string? Status { get; set; }
    public string? Address { get; set; }
    public string? Port { get; set; }
    [JsonPropertyName("protocol_id")] public string? ProtocolId { get; set; }
    public List<VulnerabilityProtocol> Protocol { get; set; } = [];
    [JsonPropertyName("owner_type_id")] public int OwnerTypeId { get; set; }
    [JsonPropertyName("owner_name")] public string? OwnerName { get; set; }
}

public sealed class VulnerabilityProtocol
{
    public int Id { get; set; }
    public string? Name { get; set; }
}

public sealed class VulnerabilityOwnerUpdate
{
    public int Id { get; set; }
    [JsonPropertyName("owner_type_id")] public int OwnerTypeId { get; set; }
    [JsonPropertyName("owner_name")] public string? OwnerName { get; set; }
}

public sealed class VulnerabilityType
{
    public int Id { get; set; }
    public string? Name { get; set; }
}

public sealed class ApiFile
{
    public int Id { get; set; }
    public string? Name { get; set; }
}

public sealed class SafetyNotice
{
    public int Id { get; set; }
    public string? Code { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }
    public int Level { get; set; }
    [JsonPropertyName("created_at")] [JsonConverter(typeof(SipPortal.Services.FlexibleInt64Converter))] public long CreatedAt { get; set; }
}

public sealed class Announcement
{
    public int Id { get; set; }
    public string? Code { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }
    [JsonPropertyName("type_id")] public int TypeId { get; set; }
    [JsonPropertyName("created_at")] [JsonConverter(typeof(SipPortal.Services.FlexibleInt64Converter))] public long CreatedAt { get; set; }
    public List<ApiFile> Files { get; set; } = [];
}

public sealed class AnnouncementType
{
    public int Id { get; set; }
    public string? Name { get; set; }
}

public sealed class Message
{
    public int Id { get; set; }
    public string? Code { get; set; }
    public string? Title { get; set; }
    public string? Content { get; set; }
    [JsonPropertyName("created_at")] [JsonConverter(typeof(SipPortal.Services.FlexibleInt64Converter))] public long CreatedAt { get; set; }
    public List<ApiFile> Files { get; set; } = [];
}

public sealed class UserInfo
{
    public int Id { get; set; }
    public string? Firstname { get; set; }
    public string? Lastname { get; set; }
    public string? Email { get; set; }
    public int Status { get; set; }
    [JsonPropertyName("user_title")] public string? UserTitle { get; set; }
    [JsonPropertyName("created_at")] [JsonConverter(typeof(SipPortal.Services.FlexibleInt64Converter))] public long CreatedAt { get; set; }
    [JsonPropertyName("work_phone")] public string? WorkPhone { get; set; }
    [JsonPropertyName("mobile_phone")] public string? MobilePhone { get; set; }
    public string? Role { get; set; }
    [JsonPropertyName("some_id")] public int SomeId { get; set; }
}

public sealed class SomeInfo
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public int Status { get; set; }
    public string? Email { get; set; }
    public string? Address { get; set; }
    [JsonPropertyName("created_at")] [JsonConverter(typeof(SipPortal.Services.FlexibleInt64Converter))] public long CreatedAt { get; set; }
    [JsonPropertyName("land_phone")] public string? LandPhone { get; set; }
    [JsonPropertyName("mobile_phone")] public string? MobilePhone { get; set; }
    public string? Fax { get; set; }
}

public sealed class IncidentCreateRequest
{
    public string? Type { get; set; }
    [JsonPropertyName("type_more")] public string? TypeMore { get; set; }
    [JsonPropertyName("affected_systems")] public string? AffectedSystems { get; set; }
    [JsonPropertyName("affected_systems_more")] public string? AffectedSystemsMore { get; set; }
    [JsonPropertyName("detection_method")] public string? DetectionMethod { get; set; }
    [JsonPropertyName("notification_method")] public string? NotificationMethod { get; set; }
    [JsonPropertyName("notification_method_more")] public string? NotificationMethodMore { get; set; }
    [JsonPropertyName("system_down")] public int SystemDown { get; set; }
    public string? Description { get; set; }
    [JsonPropertyName("start_date")] [JsonConverter(typeof(SipPortal.Services.FlexibleInt64Converter))] public long StartDate { get; set; }
    [JsonPropertyName("detection_date")] [JsonConverter(typeof(SipPortal.Services.FlexibleInt64Converter))] public long DetectionDate { get; set; }
    [JsonPropertyName("track_record_detected")] public int TrackRecordDetected { get; set; }
    [JsonPropertyName("track_source_ip")] public string? TrackSourceIp { get; set; }
    [JsonPropertyName("track_target_ip")] public string? TrackTargetIp { get; set; }
    [JsonPropertyName("track_port")] public string? TrackPort { get; set; }
    [JsonPropertyName("track_other")] public string? TrackOther { get; set; }
    public string? Prevention { get; set; }
    public string? Addition { get; set; }
}

public sealed class StatusUpdateRequest
{
    public int Status { get; set; }
}

public sealed class MessageRequest
{
    public string? Message { get; set; }
}
