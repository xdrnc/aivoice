using Microsoft.AspNetCore.SignalR;

public class VoiceHub : Hub
{
    public async Task SendText(string text)
    {
        // This sends the text to ALL connected clients
        await Clients.All.SendAsync("ReceiveText", text);
    }
}
