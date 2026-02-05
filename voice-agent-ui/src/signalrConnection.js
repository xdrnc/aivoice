import * as signalR from "@microsoft/signalr";

export const connection = new signalR.HubConnectionBuilder()
  .withUrl("https://localhost:7260/voiceHub") // alextest backend hub route
  .withAutomaticReconnect()
  .configureLogging(signalR.LogLevel.Information)
  .build();
