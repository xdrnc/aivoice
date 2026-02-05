import React, { useEffect, useState, useRef } from "react";
import { connection } from "./signalrConnection";

function App() {
  const [messages, setMessages] = useState([]);
  const [isRecording, setIsRecording] = useState(false);
  const mediaRecorderRef = useRef(null);
  const audioChunksRef = useRef([]);

  const [lastRecording, setLastRecording] = useState(null);


  useEffect(() => {
    connection.start()
      .then(() => console.log("SignalR Connected"))
      .catch(err => console.error("Connection failed: ", err));

    connection.on("ReceiveText", (text) => {
      setMessages(prev => [...prev, text]);
    });
  }, []);

  const startRecording = async () => {
  try {
    const stream = await navigator.mediaDevices.getUserMedia({ audio: true });

    const mediaRecorder = new MediaRecorder(stream);
    mediaRecorderRef.current = mediaRecorder;
    audioChunksRef.current = [];

    mediaRecorder.ondataavailable = (event) => {
      audioChunksRef.current.push(event.data);
    };

    mediaRecorder.onstop = () => {
      const audioBlob = new Blob(audioChunksRef.current, { type: "audio/webm" });
      console.log("Recorded audio blob:", audioBlob);

      setLastRecording(audioBlob);
      // Later: send audioBlob to your backend STT endpoint
    };

    mediaRecorder.start();
    setIsRecording(true);
  } catch (err) {
    console.error("Microphone access denied:", err);
  }
};

const stopRecording = () => {
  if (mediaRecorderRef.current) {
    mediaRecorderRef.current.stop();
    setIsRecording(false);
  }
};

const playLastRecording = () => {
  if (!lastRecording) return;

  const audioUrl = URL.createObjectURL(lastRecording);
  const audio = new Audio(audioUrl);
  audio.play();
};

return (
  <div>
  <h1>Voice Agent UI</h1>

  <button onClick={isRecording ? stopRecording : startRecording}>
    {isRecording ? "Stop Recording" : "Start Recording"}
  </button>

  <button
  onClick={playLastRecording}
  disabled={!lastRecording}
  style={{ marginLeft: "10px" }}
  >
  Play Last Recording
  </button>

  <ul>
    {messages.map((m, i) => <li key={i}>{m}</li>)}
  </ul>
</div>
);
}




export default App;