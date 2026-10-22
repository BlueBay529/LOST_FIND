using LostFind.Models;

namespace LostFind.Services;

// 외부 조회는 이 작업에서만 실행하고, HTTP 요청은 완성된 스냅샷만 읽습니다.
public sealed class PublicItemsRefreshService(ILogger<PublicItemsRefreshService> logger) : BackgroundService
{
    private Func<CancellationToken, Task<PublicItemsCacheEntry>>? _loader;
    private PublicItemsCacheEntry? _snapshot;
    private volatile bool _refreshing = true;
    private volatile string? _lastError;

    public PublicItemsCacheEntry? Snapshot => Volatile.Read(ref _snapshot);
    public bool IsRefreshing => _refreshing;
    public string? LastError => _lastError;

    public static DateTimeOffset NextRefreshAt
    {
        get
        {
            // UTC 정시는 한국 시간의 정시와 일치합니다.
            var now = DateTimeOffset.UtcNow;
            return new DateTimeOffset(now.Year, now.Month, now.Day, now.Hour, 0, 0, TimeSpan.Zero).AddHours(1);
        }
    }

    public void SetLoader(Func<CancellationToken, Task<PublicItemsCacheEntry>> loader) => _loader = loader;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();
        while (!stoppingToken.IsCancellationRequested)
        {
            _refreshing = true;
            try
            {
                var snapshot = await (_loader ?? throw new InvalidOperationException("공공데이터 조회 작업이 설정되지 않았습니다."))(stoppingToken);
                // 일부 제공기관만 성공했다면 이미 저장된 전체 정보를 유지합니다.
                if (!snapshot.Partial || Snapshot is null)
                {
                    Volatile.Write(ref _snapshot, snapshot);
                }
                _lastError = snapshot.Partial ? "일부 제공기관의 갱신에 실패했습니다. 저장된 정보를 표시합니다." : null;
                logger.LogInformation("공공데이터 메모리 갱신 처리 완료: {Count}건, 부분 응답: {Partial}", snapshot.Data.Count, snapshot.Partial);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
            catch (Exception exception)
            {
                _lastError = Snapshot is null
                    ? "공공데이터 연결이 지연되고 있습니다. 30초 후 다시 확인합니다."
                    : "공공데이터 조회에 실패했습니다. 저장된 정보를 유지하며 다음 정시에 다시 확인합니다.";
                // 인증키가 포함될 수 있는 요청 주소나 예외 메시지는 기록하지 않습니다.
                logger.LogWarning("공공데이터 갱신 실패. 기존 메모리 정보 유지. 오류 종류: {ErrorType}", exception.GetType().Name);
            }
            finally
            {
                _refreshing = false;
            }

            try
            {
                var now = DateTimeOffset.UtcNow;
                // 첫 성공 전에는 재시도하고, 저장 정보가 생기면 매 정시 갱신합니다.
                var delay = Snapshot is null ? TimeSpan.FromSeconds(30) : NextRefreshAt - now;
                await Task.Delay(delay, stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                return;
            }
        }
    }
}
