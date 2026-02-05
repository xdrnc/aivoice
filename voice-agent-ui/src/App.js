import React, { useEffect, useState, useRef } from "react";
import { connection } from "./signalrConnection";

function App() {
  // STT and AI text boxes
  const [sttText, setSttText] = useState("");
  const [aiText, setAiText] = useState("");

  // Recording state
  const [isRecording, setIsRecording] = useState(false);
  const [lastRecording, setLastRecording] = useState(null);

  const mediaRecorderRef = useRef(null);
  const audioChunksRef = useRef([]);

  // -----------------------------
  // SignalR Setup
  // -----------------------------
  useEffect(() => {
    connection
      .start()
      .then(() => console.log("SignalR Connected"))
      .catch((err) => console.error("Connection failed: ", err));

    // When backend sends AI text
    connection.on("ReceiveText", (text) => {
      setAiText(text);
    });

    // When backend sends STT text (we'll wire this up later)
    connection.on("ReceiveSTT", (text) => {
      setSttText(text);
    });
  }, []);

  // -----------------------------
  // Microphone Recording
  // -----------------------------
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
        const audioBlob = new Blob(audioChunksRef.current, {
          type: "audio/webm",
        });

        console.log("Recorded audio blob:", audioBlob);
        setLastRecording(audioBlob);

        // Later: send audioBlob to backend STT endpoint
        // After STT returns transcription:
        // setSttText(transcription);
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

  // -----------------------------
  // Playback
  // -----------------------------
  const playLastRecording = () => {
    if (!lastRecording) return;

    const audioUrl = URL.createObjectURL(lastRecording);
    const audio = new Audio(audioUrl);
    audio.play();
  };

  // -----------------------------
  // UI
  // -----------------------------
  return (
    <div style={{ padding: "20px", fontFamily: "sans-serif", maxWidth: "600px", margin: "0 auto" }}>
      <h1>Voice Agent UI</h1>

      <div style={{ marginBottom: "20px" }}>
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
      </div>

      {/* STT Textbox */}
      <div style={{ marginBottom: "20px" }}>
        <h3>You said (STT):</h3>
        <textarea
          value={sttText}
          readOnly
          style={{
            width: "100%",
            height: "80px",
            padding: "10px",
            fontSize: "14px",
            borderRadius: "6px",
            border: "1px solid #ccc",
            background: "#f7faff",
          }}
        />
      </div>

      {/* AI Response Textbox */}
      <div>
        <h3>AI Response (LLM):</h3>
        <textarea
          value={aiText}
          readOnly
          style={{
            width: "100%",
            height: "120px",
            padding: "10px",
            fontSize: "14px",
            borderRadius: "6px",
            border: "1px solid #ccc",
            background: "#fafff7",
          }}
        />
      </div>
    </div>
  );
}

export default App;
