import React, { useEffect, useState } from "react";
import { connection } from "./signalrConnection";

function App() {
  const [messages, setMessages] = useState([]);

  useEffect(() => {
    connection.start()
      .then(() => console.log("SignalR Connected"))
      .catch(err => console.error("Connection failed: ", err));

    connection.on("ReceiveText", (text) => {
      setMessages(prev => [...prev, text]);
    });
  }, []);

  return (
    <div>
      <h1>Voice Agent UI</h1>
      <ul>
        {messages.map((m, i) => <li key={i}>{m}</li>)}
      </ul>
    </div>
  );
}

export default App;
