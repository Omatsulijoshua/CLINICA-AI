"use client";

import React, { useState, useEffect, useRef } from "react";
import { Send, Paperclip, Camera, Mic, Volume2, Sparkles, Loader2, FileText, CheckCircle } from "lucide-react";
import { UserInfo } from "@/app/page";

interface ChatInterfaceProps {
  token: string;
  currentChatId: string | null;
  setRightPanelData: (data: any) => void;
  fetchConversations: () => void;
  user: UserInfo;
}

export default function ChatInterface({
  token,
  currentChatId,
  setRightPanelData,
  fetchConversations,
  user
}: ChatInterfaceProps) {
  const [messages, setMessages] = useState<any[]>([]);
  const [input, setInput] = useState("");
  const [loading, setLoading] = useState(false);
  const [uploading, setUploading] = useState(false);
  const [uploadSuccess, setUploadSuccess] = useState("");
  const [isRecording, setIsRecording] = useState(false);
  const messagesEndRef = useRef<HTMLDivElement>(null);
  
  const fileInputRef = useRef<HTMLInputElement>(null);
  const imageInputRef = useRef<HTMLInputElement>(null);

  const API_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5000";

  // Load Messages on Active Chat Change
  useEffect(() => {
    if (currentChatId) {
      fetchMessages();
    } else {
      setMessages([]);
      setRightPanelData({
        sources: [],
        videos: [],
        confidenceScore: 0,
        relatedConditions: [],
        recommendedNextSteps: [],
        isEmergency: false,
        emergencyGuidance: ""
      });
    }
  }, [currentChatId]);

  // Scroll to bottom on messages update
  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: "smooth" });
  }, [messages]);

  const fetchMessages = async () => {
    try {
      const res = await fetch(`${API_URL}/api/chat/conversations/${currentChatId}/messages`, {
        headers: { Authorization: `Bearer ${token}` }
      });
      if (res.ok) {
        const data = await res.json();
        setMessages(data);
        
        // Feed the last AI message data to the right panel
        const lastAiMessage = [...data].reverse().find(m => m.sender === "AI");
        if (lastAiMessage && lastAiMessage.structuredResponseJson) {
          const struct = JSON.parse(lastAiMessage.structuredResponseJson);
          setRightPanelData(struct);
        } else {
          setRightPanelData({
            sources: [],
            videos: [],
            confidenceScore: 0,
            relatedConditions: [],
            recommendedNextSteps: [],
            isEmergency: false,
            emergencyGuidance: ""
          });
        }
      }
    } catch (err) {
      console.error(err);
    }
  };

  const handleSendMessage = async (e?: React.FormEvent, customInput?: string) => {
    e?.preventDefault();
    const prompt = customInput || input;
    if (!prompt.trim() || !currentChatId) return;

    setInput("");
    setLoading(true);

    // Append user message instantly to UI
    const tempUserMsg = { sender: "User", content: prompt, createdAt: new Date() };
    setMessages((prev) => [...prev, tempUserMsg]);

    try {
      const res = await fetch(`${API_URL}/api/chat/conversations/${currentChatId}/message`, {
        method: "POST",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`
        },
        body: JSON.stringify({ userInput: prompt })
      });

      if (res.ok) {
        const aiMsg = await res.json();
        setMessages((prev) => {
          // Remove temp message and append actual from backend
          const filtered = prev.filter(m => m.createdAt !== tempUserMsg.createdAt);
          return [...filtered, tempUserMsg, aiMsg];
        });

        if (aiMsg.structuredResponseJson) {
          const struct = JSON.parse(aiMsg.structuredResponseJson);
          setRightPanelData(struct);
        }
        fetchConversations();
      }
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const handleFileUpload = async (e: React.ChangeEvent<HTMLInputElement>, isImage = false) => {
    const files = e.target.files;
    if (!files || files.length === 0 || !currentChatId) return;

    setUploading(true);
    setUploadSuccess("");

    const formData = new FormData();
    formData.append("file", files[0]);

    try {
      const res = await fetch(`${API_URL}/api/chat/conversations/${currentChatId}/upload`, {
        method: "POST",
        headers: { Authorization: `Bearer ${token}` },
        body: formData
      });

      if (res.ok) {
        const data = await res.json();
        setUploadSuccess(`Uploaded: ${data.file.fileName}`);
        
        // System message in chat stream
        setMessages((prev) => [
          ...prev,
          {
            sender: "System",
            content: `📄 Uploaded file "${data.file.fileName}" (${Math.round(data.file.fileSize / 1024)} KB) successfully parsed for clinical diagnostics.`,
            createdAt: new Date()
          }
        ]);

        // Prompt helper message
        if (isImage) {
          handleSendMessage(undefined, `Analyze uploaded image: ${data.file.fileName}`);
        } else {
          handleSendMessage(undefined, `Interpret my blood test report: ${data.file.fileName}`);
        }
      }
    } catch (err) {
      console.error(err);
    } finally {
      setUploading(false);
    }
  };

  const startVoiceInput = () => {
    setIsRecording(true);
    // Emulate a 3 second recording, then transcribe
    setTimeout(() => {
      setIsRecording(false);
      setInput("I have a severe fever and joint pain, what could it suggest?");
    }, 3000);
  };

  const promptSuggestions = [
    { text: "Analyze symptoms: high fever, severe joint pain", type: "symptom" },
    { text: "Explain side effects of Lisinopril medication", type: "medication" },
    { text: "Interpret low hemoglobin blood test report", type: "lab" },
    { text: "Dietary tips for severe Hypertension", type: "nutrition" }
  ];

  return (
    <div className="flex-1 flex flex-col h-full bg-slate-950/20 overflow-hidden relative">
      {/* Top Session Header */}
      <div className="px-6 py-4 flex items-center justify-between border-b border-white/5 bg-slate-900/60 backdrop-blur-md">
        <div>
          <h2 className="text-sm font-bold text-gray-200">Clinica Medical Engine</h2>
          <p className="text-[10px] text-teal-400 font-semibold flex items-center gap-1.5 mt-0.5">
            <span className="h-1.5 w-1.5 rounded-full bg-teal-400 animate-ping"></span>
            Active Model: Local Clinical Brain
          </p>
        </div>
      </div>

      {/* Messages Scroll Area */}
      <div className="flex-1 overflow-y-auto px-6 py-8 space-y-6 scrollbar-thin">
        {messages.length === 0 ? (
          <div className="h-full flex flex-col justify-center items-center max-w-xl mx-auto text-center space-y-6 animate-fade-in">
            <div className="p-3 bg-teal-500/10 border border-teal-500/20 rounded-2xl flex items-center justify-center">
              <Sparkles className="h-10 w-10 text-teal-400 animate-pulse" />
            </div>
            <div>
              <h1 className="text-xl font-extrabold text-white">How can I assist your health inquiry today?</h1>
              <p className="text-xs text-gray-400 mt-2">
                Upload reports (PDF/DOCX), skin photos (JPG/PNG), or describe symptoms. Every output is referenced by official medical publications.
              </p>
            </div>

            {/* suggestions */}
            <div className="grid grid-cols-2 gap-3.5 w-full pt-4">
              {promptSuggestions.map((s, idx) => (
                <button
                  key={idx}
                  onClick={(e) => {
                    if (!currentChatId) return;
                    handleSendMessage(e, s.text);
                  }}
                  disabled={!currentChatId}
                  className="p-3 rounded-xl border border-white/5 bg-slate-900/50 hover:bg-slate-900/80 hover:border-teal-500/30 text-left text-xs font-semibold text-gray-300 transition duration-200 disabled:opacity-40 disabled:cursor-not-allowed"
                >
                  <span className="text-[9px] uppercase tracking-wider text-teal-400 font-bold block mb-1">{s.type}</span>
                  {s.text}
                </button>
              ))}
            </div>

            {!currentChatId && (
              <p className="text-[10px] text-teal-400 font-bold bg-teal-950/40 border border-teal-500/20 px-3 py-1.5 rounded-full">
                ⚠️ Click &quot;Start New Consultation&quot; in the left sidebar to begin.
              </p>
            )}
          </div>
        ) : (
          messages.map((m, idx) => (
            <div
              key={idx}
              className={`flex gap-4 max-w-3xl ${m.sender === "User" ? "ml-auto flex-row-reverse" : ""}`}
            >
              {/* Avatar */}
              <div className={`h-8 w-8 rounded-full flex items-center justify-center shrink-0 text-xs font-bold ${
                m.sender === "User" 
                  ? "bg-slate-800 text-white" 
                  : m.sender === "System"
                    ? "bg-teal-950 border border-teal-500/30 text-teal-400"
                    : "bg-teal-500 text-white"
              }`}>
                {m.sender === "User" ? user.fullName.charAt(0) : m.sender === "System" ? "S" : "AI"}
              </div>

              {/* Bubble */}
              <div className="space-y-1.5 max-w-[85%]">
                <div className={`px-4 py-3 rounded-2xl text-xs leading-relaxed ${
                  m.sender === "User"
                    ? "bg-gradient-to-br from-teal-500/20 to-emerald-500/20 border border-teal-500/10 text-gray-100 font-semibold"
                    : m.sender === "System"
                      ? "bg-slate-900/50 border border-teal-950 text-teal-300"
                      : "bg-slate-900/85 border border-white/5 text-gray-200"
                }`}>
                  <p className="whitespace-pre-line">{m.content}</p>
                </div>

                <div className="flex items-center gap-2 text-[9px] text-gray-500 px-1">
                  <span>{new Date(m.createdAt).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })}</span>
                  {m.sender === "AI" && (
                    <button
                      onClick={() => {
                        if (m.structuredResponseJson) {
                          setRightPanelData(JSON.parse(m.structuredResponseJson));
                        }
                      }}
                      className="text-teal-500 hover:text-teal-400 transition font-bold"
                    >
                      • View Evidence Sources
                    </button>
                  )}
                </div>
              </div>
            </div>
          ))
        )}

        {/* Loading Spinner */}
        {loading && (
          <div className="flex gap-4 max-w-3xl">
            <div className="h-8 w-8 rounded-full bg-teal-500 flex items-center justify-center text-white text-xs font-bold animate-pulse">AI</div>
            <div className="px-4 py-3 rounded-2xl bg-slate-900/50 border border-white/5 flex items-center gap-2">
              <Loader2 className="h-4 w-4 text-teal-400 animate-spin" />
              <span className="text-xs text-gray-400">Coordinator Agent running differential diagnostics...</span>
            </div>
          </div>
        )}

        <div ref={messagesEndRef} />
      </div>

      {/* Upload files notification */}
      {uploadSuccess && (
        <div className="mx-6 mb-2 p-2 bg-teal-950/40 border border-teal-500/20 rounded-lg text-[10px] text-teal-300 flex items-center gap-1.5">
          <CheckCircle className="h-3.5 w-3.5 text-teal-400" />
          {uploadSuccess}
        </div>
      )}

      {/* Input Action Panel */}
      <div className="p-6 border-t border-white/5 bg-slate-900/40 backdrop-blur-md relative z-10">
        {isRecording && (
          <div className="absolute inset-0 bg-teal-950/90 backdrop-blur-md rounded-b-2xl flex items-center justify-center gap-4 text-teal-400 text-xs font-bold animate-pulse">
            <Mic className="h-5 w-5 text-red-500 animate-bounce" />
            <span>Listening to clinical symptoms... Recording audio transcript...</span>
          </div>
        )}

        <form onSubmit={(e) => handleSendMessage(e)} className="flex items-center gap-3">
          {/* File Inputs (hidden) */}
          <input
            type="file"
            ref={fileInputRef}
            className="hidden"
            accept=".pdf,.docx,.txt"
            onChange={(e) => handleFileUpload(e, false)}
          />
          <input
            type="file"
            ref={imageInputRef}
            className="hidden"
            accept="image/*"
            onChange={(e) => handleFileUpload(e, true)}
          />

          <div className="flex gap-1">
            <button
              type="button"
              onClick={() => fileInputRef.current?.click()}
              disabled={!currentChatId || uploading}
              className="p-2 rounded-lg bg-slate-950 border border-white/5 text-gray-400 hover:text-teal-400 hover:bg-slate-900 transition disabled:opacity-40 disabled:cursor-not-allowed"
              title="Attach Medical Report (PDF/DOCX)"
            >
              {uploading ? <Loader2 className="h-4.5 w-4.5 animate-spin" /> : <Paperclip className="h-4.5 w-4.5" />}
            </button>
            <button
              type="button"
              onClick={() => imageInputRef.current?.click()}
              disabled={!currentChatId || uploading}
              className="p-2 rounded-lg bg-slate-950 border border-white/5 text-gray-400 hover:text-teal-400 hover:bg-slate-900 transition disabled:opacity-40 disabled:cursor-not-allowed"
              title="Upload Diagnostic Image"
            >
              <Camera className="h-4.5 w-4.5" />
            </button>
            <button
              type="button"
              onClick={startVoiceInput}
              disabled={!currentChatId}
              className="p-2 rounded-lg bg-slate-950 border border-white/5 text-gray-400 hover:text-teal-400 hover:bg-slate-900 transition disabled:opacity-40 disabled:cursor-not-allowed"
              title="Voice Input Transcription"
            >
              <Mic className="h-4.5 w-4.5" />
            </button>
          </div>

          <input
            type="text"
            className="flex-1 px-4 py-2 rounded-lg bg-slate-950 border border-white/5 focus:outline-none focus:border-teal-500 focus:ring-1 focus:ring-teal-500 text-xs text-gray-200 placeholder-gray-500 disabled:opacity-45 disabled:cursor-not-allowed"
            placeholder={
              !currentChatId 
                ? "Click 'Start New Consultation' to start..." 
                : "Ask about symptoms, drugs interactions, abnormal lipid counts..."
            }
            value={input}
            onChange={(e) => setInput(e.target.value)}
            disabled={!currentChatId || loading}
          />

          <button
            type="submit"
            disabled={!input.trim() || loading || !currentChatId}
            className="p-2 rounded-lg bg-teal-500 hover:bg-teal-600 text-white transition disabled:opacity-40 disabled:cursor-not-allowed shadow-md shadow-teal-500/15"
          >
            <Send className="h-4.5 w-4.5" />
          </button>
        </form>
      </div>
    </div>
  );
}
