"use client";

import React from "react";
import { MessageSquare, Plus, Search, User, CreditCard, LogOut, Settings, Shield } from "lucide-react";
import { UserInfo } from "@/app/page";

interface LeftPanelProps {
  token: string;
  user: UserInfo;
  chats: any[];
  currentChatId: string | null;
  setCurrentChatId: (id: string | null) => void;
  activeTab: "chat" | "admin" | "profile" | "billing";
  setActiveTab: (tab: "chat" | "admin" | "profile" | "billing") => void;
  searchQuery: string;
  setSearchQuery: (query: string) => void;
  handleLogout: () => void;
  fetchConversations: () => void;
}

export default function LeftPanel({
  token,
  user,
  chats,
  currentChatId,
  setCurrentChatId,
  activeTab,
  setActiveTab,
  searchQuery,
  setSearchQuery,
  handleLogout,
  fetchConversations
}: LeftPanelProps) {
  const API_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5000";

  const handleNewChat = async () => {
    try {
      const res = await fetch(`${API_URL}/api/chat/conversations`, {
        method: "POST",
        headers: { Authorization: `Bearer ${token}` }
      });
      if (res.ok) {
        const newChat = await res.json();
        setCurrentChatId(newChat.id);
        setActiveTab("chat");
        fetchConversations();
      }
    } catch (err) {
      console.error(err);
    }
  };

  const handleDeleteChat = async (e: React.MouseEvent, chatId: string) => {
    e.stopPropagation();
    try {
      const res = await fetch(`${API_URL}/api/chat/conversations/${chatId}`, {
        method: "DELETE",
        headers: { Authorization: `Bearer ${token}` }
      });
      if (res.ok) {
        if (currentChatId === chatId) {
          setCurrentChatId(null);
        }
        fetchConversations();
      }
    } catch (err) {
      console.error(err);
    }
  };

  return (
    <div className="w-80 flex flex-col h-full bg-slate-900 border-r border-white/5 relative z-20">
      {/* Clinica Brand Header */}
      <div className="p-4 flex items-center gap-3 border-b border-white/5">
        <img src="/logo.png" alt="Clinica Logo" className="h-8 w-auto filter drop-shadow-[0_0_8px_rgba(20,184,166,0.25)]" />
        <div>
          <span className="font-extrabold text-sm tracking-wider bg-gradient-to-r from-teal-400 to-emerald-400 bg-clip-text text-transparent uppercase">Clinica AI</span>
          <p className="text-[10px] text-gray-500 font-semibold leading-none">Medical Assistant</p>
        </div>
      </div>

      {/* New Chat Button */}
      <div className="p-4">
        <button
          onClick={handleNewChat}
          className="w-full flex items-center justify-center gap-2 py-2.5 rounded-lg bg-teal-500/10 hover:bg-teal-500/20 text-teal-400 border border-teal-500/20 text-xs font-bold transition duration-200"
        >
          <Plus className="h-4 w-4" /> Start New Consultation
        </button>
      </div>

      {/* Search Bar */}
      <div className="px-4 pb-2">
        <div className="relative">
          <Search className="absolute left-3 top-2.5 h-3.5 w-3.5 text-gray-500" />
          <input
            type="text"
            className="w-full pl-9 pr-4 py-2 rounded-lg bg-slate-950 border border-white/5 focus:outline-none focus:border-teal-500 focus:ring-1 focus:ring-teal-500 text-xs text-gray-300 placeholder-gray-500"
            placeholder="Search consultation logs..."
            value={searchQuery}
            onChange={(e) => setSearchQuery(e.target.value)}
          />
        </div>
      </div>

      {/* Conversations Stream */}
      <div className="flex-1 overflow-y-auto px-2 space-y-1 py-2 scrollbar-thin">
        <span className="px-3 text-[10px] uppercase font-bold tracking-widest text-gray-500 block mb-2">Saved Consultations</span>
        {chats.length === 0 ? (
          <p className="text-center text-xs text-gray-600 mt-8 font-medium">No prior records found.</p>
        ) : (
          chats.map((c) => (
            <div
              key={c.id}
              onClick={() => {
                setCurrentChatId(c.id);
                setActiveTab("chat");
              }}
              className={`group flex items-center justify-between p-2.5 rounded-lg text-xs font-semibold cursor-pointer transition ${
                currentChatId === c.id && activeTab === "chat"
                  ? "bg-teal-950/45 text-teal-400 border-l-2 border-teal-500"
                  : "text-gray-400 hover:bg-slate-800/50 hover:text-gray-200"
              }`}
            >
              <div className="flex items-center gap-2.5 overflow-hidden">
                <MessageSquare className="h-4 w-4 text-gray-500 group-hover:text-teal-400 shrink-0" />
                <span className="truncate">{c.title}</span>
              </div>
              <button
                onClick={(e) => handleDeleteChat(e, c.id)}
                className="opacity-0 group-hover:opacity-100 p-1 hover:text-red-400 transition"
                title="Delete Chat Log"
              >
                <svg className="h-3.5 w-3.5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                  <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-16v1a3 3 0 003 3h10M4 7h16" />
                </svg>
              </button>
            </div>
          ))
        )}
      </div>

      {/* Bottom Profile Settings Controls */}
      <div className="p-4 border-t border-white/5 space-y-2.5 bg-slate-950/45">
        {/* User Card */}
        <div className="flex items-center gap-3 mb-2 p-1">
          <div className="h-9 w-9 rounded-full bg-gradient-to-tr from-teal-500 to-emerald-500 flex items-center justify-center text-white font-bold text-sm shadow-inner shadow-black/25 uppercase">
            {user.fullName.charAt(0)}
          </div>
          <div className="overflow-hidden">
            <h4 className="text-xs font-bold text-gray-200 truncate leading-tight">{user.fullName}</h4>
            <span className="inline-block mt-0.5 text-[9px] font-extrabold uppercase tracking-widest text-teal-400 bg-teal-950/60 border border-teal-500/20 px-1.5 py-0.5 rounded leading-none">
              {user.role}
            </span>
          </div>
        </div>

        {/* Action Toggles */}
        <div className="grid grid-cols-4 gap-1">
          <button
            onClick={() => setActiveTab("profile")}
            className={`p-2 rounded-lg flex items-center justify-center hover:bg-slate-800 transition ${activeTab === "profile" ? "text-teal-400 bg-slate-800" : "text-gray-500 hover:text-gray-300"}`}
            title="EHR Clinical Profile"
          >
            <User className="h-4.5 w-4.5" />
          </button>
          <button
            onClick={() => setActiveTab("billing")}
            className={`p-2 rounded-lg flex items-center justify-center hover:bg-slate-800 transition ${activeTab === "billing" ? "text-teal-400 bg-slate-800" : "text-gray-500 hover:text-gray-300"}`}
            title="Billing & Subscription Plans"
          >
            <CreditCard className="h-4.5 w-4.5" />
          </button>
          
          {user.role === "Admin" ? (
            <button
              onClick={() => setActiveTab("admin")}
              className={`p-2 rounded-lg flex items-center justify-center hover:bg-slate-800 transition ${activeTab === "admin" ? "text-teal-400 bg-slate-800" : "text-gray-500 hover:text-gray-300"}`}
              title="Admin Dashboard"
            >
              <Shield className="h-4.5 w-4.5" />
            </button>
          ) : (
            <div className="p-2 text-gray-800 cursor-not-allowed flex items-center justify-center">
              <Shield className="h-4.5 w-4.5 opacity-25" />
            </div>
          )}
          
          <button
            onClick={handleLogout}
            className="p-2 rounded-lg flex items-center justify-center hover:bg-red-950/20 text-gray-500 hover:text-red-400 transition"
            title="Sign Out Session"
          >
            <LogOut className="h-4.5 w-4.5" />
          </button>
        </div>
      </div>
    </div>
  );
}
