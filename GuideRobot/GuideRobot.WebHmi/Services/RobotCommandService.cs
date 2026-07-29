using System.Net;
using System.Net.Http.Json;

namespace GuideRobot.WebHmi.Services;

public sealed class RobotCommandService(HttpClient httpClient)
{
    public async Task<CommandResult> SendDestinationAsync(string destinationId, CancellationToken cancellationToken = default)
    {
        try
        {
            // 현재 서버 담당자가 제공한 규격: { "destination": "목적지 ID" }
            using var response = await httpClient.PostAsJsonAsync(
                "api/command",
                new { destination = destinationId },
                cancellationToken);

            return new CommandResult(
                response.IsSuccessStatusCode,
                response.StatusCode,
                response.IsSuccessStatusCode
                    ? $"서버가 {destinationId} 안내 명령을 받았습니다. (HTTP {(int)response.StatusCode})"
                    : CreateFailureMessage(response.StatusCode));
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
        {
            return new CommandResult(false, null, "서버와의 접속을 확인해 주세요.");
        }
    }

    public Task<CommandResult> PauseGuidanceAsync(CancellationToken cancellationToken = default) =>
        SendControlCommandAsync("pause", "일시정지", cancellationToken);

    public Task<CommandResult> ResumeGuidanceAsync(CancellationToken cancellationToken = default) =>
        SendControlCommandAsync("resume", "안내 재개", cancellationToken);

    public Task<CommandResult> CancelGuidanceAsync(CancellationToken cancellationToken = default) =>
        SendControlCommandAsync("cancel", "안내 취소", cancellationToken);

    private async Task<CommandResult> SendControlCommandAsync(
        string command,
        string commandName,
        CancellationToken cancellationToken)
    {
        try
        {
            // 서버 추가 규격: { "command": "pause" | "resume" | "cancel" }
            using var response = await httpClient.PostAsJsonAsync(
                "api/command",
                new { command },
                cancellationToken);

            return new CommandResult(
                response.IsSuccessStatusCode,
                response.StatusCode,
                response.IsSuccessStatusCode
                    ? $"서버가 {commandName} 명령을 받았습니다. (HTTP {(int)response.StatusCode})"
                    : CreateFailureMessage(response.StatusCode));
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
        {
            return new CommandResult(false, null, "서버와의 접속을 확인해 주세요.");
        }
    }

    private static string CreateFailureMessage(HttpStatusCode statusCode) =>
        (int)statusCode >= 500
            ? "서버와의 접속을 확인해 주세요."
            : "서버가 요청을 처리하지 못했습니다. 잠시 후 다시 시도해 주세요.";
}

public sealed record CommandResult(bool IsSuccess, HttpStatusCode? StatusCode, string Message);
