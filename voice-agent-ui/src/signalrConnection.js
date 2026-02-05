import * as signalR from "@microsoft/signalr";

export const connection = new signalR.HubConnectionBuilder()
  .withUrl("http://localhost:5000/voiceHub") // alextest backend hub route
  .withAutomaticReconnect()
  .build();
