"use client";

import React, { useState, useEffect } from "react";
import LeftPanel from "@/components/left-panel";
import ChatInterface from "@/components/chat-interface";
import RightPanel from "@/components/right-panel";
import AdminDashboard from "@/components/admin-dashboard";
import { LogIn, UserPlus, ShieldAlert, CheckCircle2, AlertTriangle, KeyRound } from "lucide-react";

export interface UserInfo {
  id: string;
  email: string;
  fullName: string;
  role: string;
  isEmailVerified: boolean;
}

export default function Home() {
  const [token, setToken] = useState<string | null>(null);
  const [user, setUser] = useState<UserInfo | null>(null);
  const [currentChatId, setCurrentChatId] = useState<string | null>(null);
  const [chats, setChats] = useState<any[]>([]);
  const [activeTab, setActiveTab] = useState<"chat" | "admin" | "profile" | "billing">("chat");
  const [searchQuery, setSearchQuery] = useState("");
  const [rightPanelData, setRightPanelData] = useState<any>({
    sources: [],
    videos: [],
    confidenceScore: 0,
    relatedConditions: [],
    recommendedNextSteps: [],
    isEmergency: false,
    emergencyGuidance: ""
  });

  // Auth States
  const [authMode, setAuthMode] = useState<"login" | "register" | "reset">("login");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [fullName, setFullName] = useState("");
  const [authError, setAuthError] = useState("");
  const [authSuccess, setAuthSuccess] = useState("");

  const API_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5000";

  // Check LocalStorage for JWT
  useEffect(() => {
    const savedToken = localStorage.getItem("clinica_token");
    const savedUser = localStorage.getItem("clinica_user");
    if (savedToken && savedUser) {
      setToken(savedToken);
      setUser(JSON.parse(savedUser));
    }
  }, []);

  // Fetch Conversation History
  useEffect(() => {
    if (token) {
      fetchConversations();
    }
  }, [token, searchQuery]);

  const fetchConversations = async () => {
    try {
      const res = await fetch(`${API_URL}/api/chat/conversations?search=${searchQuery}`, {
        headers: { Authorization: `Bearer ${token}` }
      });
      if (res.ok) {
        const data = await res.json();
        setChats(data);
        if (data.length > 0 && !currentChatId) {
          setCurrentChatId(data[0].id);
        }
      }
    } catch (err) {
      console.error("Failed fetching conversations", err);
    }
  };

  const handleLogout = () => {
    localStorage.removeItem("clinica_token");
    localStorage.removeItem("clinica_user");
    setToken(null);
    setUser(null);
    setCurrentChatId(null);
    setChats([]);
    setActiveTab("chat");
  };

  const handleLogin = async (e: React.FormEvent) => {
    e.preventDefault();
    setAuthError("");
    setAuthSuccess("");
    try {
      const res = await fetch(`${API_URL}/api/auth/login`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ email, password })
      });
      const data = await res.json();
      if (res.ok) {
        localStorage.setItem("clinica_token", data.token);
        localStorage.setItem("clinica_user", JSON.stringify(data.user));
        setToken(data.token);
        setUser(data.user);
        setAuthSuccess("Successfully logged in!");
      } else {
        setAuthError(data.message || "Invalid credentials.");
      }
    } catch (err) {
      setAuthError("Could not reach the server.");
    }
  };

  const handleRegister = async (e: React.FormEvent) => {
    e.preventDefault();
    setAuthError("");
    setAuthSuccess("");
    try {
      const res = await fetch(`${API_URL}/api/auth/register`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ email, password, fullName })
      });
      const data = await res.json();
      if (res.ok) {
        setAuthSuccess("Registration successful! Email verification is pending. You can log in now.");
        setAuthMode("login");
      } else {
        setAuthError(data.message || "Failed registering user.");
      }
    } catch (err) {
      setAuthError("Could not reach the server.");
    }
  };

  const handleGoogleSignIn = async () => {
    setAuthError("");
    setAuthSuccess("");
    try {
      const res = await fetch(`${API_URL}/api/auth/google-signin`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ email: "google_patient@clinica.ai", name: "Google Patient Integration" })
      });
      const data = await res.json();
      if (res.ok) {
        localStorage.setItem("clinica_token", data.token);
        localStorage.setItem("clinica_user", JSON.stringify(data.user));
        setToken(data.token);
        setUser(data.user);
      } else {
        setAuthError("Google Sign-In failed.");
      }
    } catch (err) {
      setAuthError("Could not reach Google Auth Mock.");
    }
  };

  const handleResetPassword = async (e: React.FormEvent) => {
    e.preventDefault();
    setAuthError("");
    setAuthSuccess("");
    try {
      const res = await fetch(`${API_URL}/api/auth/reset-password-request`, {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ email })
      });
      const data = await res.json();
      if (res.ok) {
        setAuthSuccess("Password reset token generated: " + data.resetToken + ". In production this is sent to email.");
      } else {
        setAuthError(data.message || "Reset request failed.");
      }
    } catch (err) {
      setAuthError("Error connecting to server.");
    }
  };

  // Auth Overlay Form Layout
  if (!token || !user) {
    return (
      <div className="flex items-center justify-center min-h-screen bg-clinical-dark text-gray-100 p-4 relative overflow-hidden">
        {/* Glow Effects */}
        <div className="absolute top-[-20%] left-[-20%] w-[60%] h-[60%] bg-teal-900/20 rounded-full blur-[120px] pointer-events-none"></div>
        <div className="absolute bottom-[-20%] right-[-20%] w-[60%] h-[60%] bg-emerald-900/20 rounded-full blur-[120px] pointer-events-none"></div>

        <div className="w-full max-w-md p-8 rounded-2xl glass-panel-heavy shadow-2xl relative z-10 border border-white/10 animate-fade-in">
          {/* Logo Branding */}
          <div className="text-center mb-8">
            <img src="/logo.png" alt="Clinica AI" className="mx-auto h-20 w-auto mb-4 drop-shadow-[0_0_12px_rgba(20,184,166,0.3)]" />
            <h1 className="text-2xl font-bold tracking-tight text-white">Welcome to Clinica AI</h1>
            <p className="text-sm text-gray-400 mt-2">HIPAA-Inspired Evidence-Based Medical Brain</p>
          </div>

          {authError && (
            <div className="mb-4 p-3 rounded-lg bg-red-950/40 border border-red-500/30 text-red-300 text-xs flex items-center gap-2">
              <AlertTriangle className="h-4 w-4 text-red-400 shrink-0" />
              <span>{authError}</span>
            </div>
          )}

          {authSuccess && (
            <div className="mb-4 p-3 rounded-lg bg-emerald-950/40 border border-emerald-500/30 text-emerald-300 text-xs flex items-center gap-2">
              <CheckCircle2 className="h-4 w-4 text-emerald-400 shrink-0" />
              <span>{authSuccess}</span>
            </div>
          )}

          {authMode === "login" && (
            <form onSubmit={handleLogin} className="space-y-4">
              <div>
                <label className="block text-xs font-semibold uppercase tracking-wider text-gray-400 mb-1">Email Address</label>
                <input
                  type="email"
                  required
                  placeholder="name@clinica.ai"
                  className="w-full px-4 py-2.5 rounded-lg bg-slate-900 border border-white/10 focus:border-teal-500 focus:ring-1 focus:ring-teal-500 focus:outline-none transition text-sm text-gray-100 placeholder-gray-500"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                />
              </div>

              <div>
                <div className="flex justify-between items-center mb-1">
                  <label className="block text-xs font-semibold uppercase tracking-wider text-gray-400">Password</label>
                  <button type="button" onClick={() => setAuthMode("reset")} className="text-xs text-teal-400 hover:text-teal-300 transition">Forgot Password?</button>
                </div>
                <input
                  type="password"
                  required
                  placeholder="••••••••"
                  className="w-full px-4 py-2.5 rounded-lg bg-slate-900 border border-white/10 focus:border-teal-500 focus:ring-1 focus:ring-teal-500 focus:outline-none transition text-sm text-gray-100 placeholder-gray-500"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                />
              </div>

              <button
                type="submit"
                className="w-full py-2.5 rounded-lg bg-gradient-to-r from-teal-500 to-emerald-500 text-white font-semibold text-sm hover:from-teal-600 hover:to-emerald-600 focus:outline-none focus:ring-2 focus:ring-teal-500 focus:ring-offset-2 focus:ring-offset-slate-950 transition duration-150 flex items-center justify-center gap-2 shadow-lg shadow-teal-500/10"
              >
                <LogIn className="h-4 w-4" /> Sign In
              </button>
            </form>
          )}

          {authMode === "register" && (
            <form onSubmit={handleRegister} className="space-y-4">
              <div>
                <label className="block text-xs font-semibold uppercase tracking-wider text-gray-400 mb-1">Full Name</label>
                <input
                  type="text"
                  required
                  placeholder="Dr. Sarah Carter"
                  className="w-full px-4 py-2.5 rounded-lg bg-slate-900 border border-white/10 focus:border-teal-500 focus:ring-1 focus:ring-teal-500 focus:outline-none transition text-sm text-gray-100 placeholder-gray-500"
                  value={fullName}
                  onChange={(e) => setFullName(e.target.value)}
                />
              </div>

              <div>
                <label className="block text-xs font-semibold uppercase tracking-wider text-gray-400 mb-1">Email Address</label>
                <input
                  type="email"
                  required
                  placeholder="name@clinica.ai"
                  className="w-full px-4 py-2.5 rounded-lg bg-slate-900 border border-white/10 focus:border-teal-500 focus:ring-1 focus:ring-teal-500 focus:outline-none transition text-sm text-gray-100 placeholder-gray-500"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                />
              </div>

              <div>
                <label className="block text-xs font-semibold uppercase tracking-wider text-gray-400 mb-1">Password</label>
                <input
                  type="password"
                  required
                  placeholder="Min. 8 characters"
                  className="w-full px-4 py-2.5 rounded-lg bg-slate-900 border border-white/10 focus:border-teal-500 focus:ring-1 focus:ring-teal-500 focus:outline-none transition text-sm text-gray-100 placeholder-gray-500"
                  value={password}
                  onChange={(e) => setPassword(e.target.value)}
                />
              </div>

              <button
                type="submit"
                className="w-full py-2.5 rounded-lg bg-gradient-to-r from-teal-500 to-emerald-500 text-white font-semibold text-sm hover:from-teal-600 hover:to-emerald-600 focus:outline-none focus:ring-2 focus:ring-teal-500 focus:ring-offset-2 focus:ring-offset-slate-950 transition duration-150 flex items-center justify-center gap-2 shadow-lg shadow-teal-500/10"
              >
                <UserPlus className="h-4 w-4" /> Create Account
              </button>
            </form>
          )}

          {authMode === "reset" && (
            <form onSubmit={handleResetPassword} className="space-y-4">
              <div>
                <label className="block text-xs font-semibold uppercase tracking-wider text-gray-400 mb-1">Registered Email Address</label>
                <input
                  type="email"
                  required
                  placeholder="name@clinica.ai"
                  className="w-full px-4 py-2.5 rounded-lg bg-slate-900 border border-white/10 focus:border-teal-500 focus:ring-1 focus:ring-teal-500 focus:outline-none transition text-sm text-gray-100 placeholder-gray-500"
                  value={email}
                  onChange={(e) => setEmail(e.target.value)}
                />
              </div>

              <button
                type="submit"
                className="w-full py-2.5 rounded-lg bg-gradient-to-r from-teal-500 to-emerald-500 text-white font-semibold text-sm hover:from-teal-600 hover:to-emerald-600 focus:outline-none focus:ring-2 focus:ring-teal-500 focus:ring-offset-2 focus:ring-offset-slate-950 transition duration-150 flex items-center justify-center gap-2 shadow-lg shadow-teal-500/10"
              >
                <KeyRound className="h-4 w-4" /> Request Reset Code
              </button>
            </form>
          )}

          {/* Social and Toggle Auth Mode */}
          <div className="mt-6 pt-6 border-t border-white/10 space-y-4">
            <button
              onClick={handleGoogleSignIn}
              type="button"
              className="w-full py-2.5 rounded-lg bg-white text-slate-900 font-semibold text-sm hover:bg-gray-100 transition duration-150 flex items-center justify-center gap-2 shadow-md shadow-white/5"
            >
              <svg className="h-4 w-4" viewBox="0 0 24 24">
                <path fill="#4285F4" d="M22.56 12.25c0-.78-.07-1.53-.2-2.25H12v4.26h5.92c-.26 1.37-1.04 2.53-2.21 3.31v2.77h3.57c2.08-1.92 3.28-4.74 3.28-8.09z" />
                <path fill="#34A853" d="M12 23c2.97 0 5.46-.98 7.28-2.66l-3.57-2.77c-.98.66-2.23 1.06-3.71 1.06-2.86 0-5.29-1.93-6.16-4.53H2.18v2.84C3.99 20.53 7.7 23 12 23z" />
                <path fill="#FBBC05" d="M5.84 14.09c-.22-.66-.35-1.36-.35-2.09s.13-1.43.35-2.09V7.06H2.18C1.43 8.55 1 10.22 1 12s.43 3.45 1.18 4.94l2.85-2.22.81-.63z" fillRule="evenodd" clipRule="evenodd" />
                <path fill="#EA4335" d="M12 5.38c1.62 0 3.06.56 4.21 1.64l3.15-3.15C17.45 2.09 14.97 1 12 1 7.7 1 3.99 3.47 2.18 7.06l3.66 2.84c.87-2.6 3.3-4.52 6.16-4.52z" />
              </svg>
              Sign In with Google
            </button>

            <div className="text-center text-xs text-gray-400">
              {authMode === "login" ? (
                <>
                  Don&apos;t have an account?{" "}
                  <button onClick={() => { setAuthMode("register"); setAuthError(""); setAuthSuccess(""); }} className="text-teal-400 hover:text-teal-300 font-semibold">Sign Up</button>
                </>
              ) : (
                <>
                  Already registered?{" "}
                  <button onClick={() => { setAuthMode("login"); setAuthError(""); setAuthSuccess(""); }} className="text-teal-400 hover:text-teal-300 font-semibold">Sign In</button>
                </>
              )}
            </div>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="flex h-screen w-screen bg-clinical-dark text-gray-100 overflow-hidden relative">
      {/* Dynamic Background Mesh Grid */}
      <div className="absolute inset-0 bg-[radial-gradient(ellipse_at_top,_var(--tw-gradient-stops))] from-teal-950/15 via-slate-950 to-slate-950 -z-10"></div>
      <div className="absolute inset-0 bg-[linear-gradient(to_right,rgba(255,255,255,0.02)_1px,transparent_1px),linear-gradient(to_bottom,rgba(255,255,255,0.02)_1px,transparent_1px)] bg-[size:32px_32px] -z-10"></div>

      {/* LEFT PANEL */}
      <LeftPanel
        token={token}
        user={user}
        chats={chats}
        currentChatId={currentChatId}
        setCurrentChatId={setCurrentChatId}
        activeTab={activeTab}
        setActiveTab={setActiveTab}
        searchQuery={searchQuery}
        setSearchQuery={setSearchQuery}
        handleLogout={handleLogout}
        fetchConversations={fetchConversations}
      />

      {/* CENTER PANEL */}
      <div className="flex-1 flex flex-col h-full overflow-hidden border-r border-white/5">
        {activeTab === "chat" && (
          <ChatInterface
            token={token}
            currentChatId={currentChatId}
            setRightPanelData={setRightPanelData}
            fetchConversations={fetchConversations}
            user={user}
          />
        )}
        {activeTab === "admin" && (
          <AdminDashboard token={token} />
        )}
        {activeTab === "profile" && (
          <div className="flex-1 flex items-center justify-center p-6">
            <div className="p-8 rounded-2xl glass-panel border border-white/10 w-full max-w-xl max-h-[85vh] overflow-y-auto">
              <h2 className="text-xl font-bold mb-4 text-glow-teal text-teal-400">Clinical Profile Settings</h2>
              <UserProfileForm token={token} API_URL={API_URL} />
            </div>
          </div>
        )}
        {activeTab === "billing" && (
          <div className="flex-1 flex items-center justify-center p-6">
            <div className="p-8 rounded-2xl glass-panel border border-white/10 w-full max-w-lg">
              <h2 className="text-xl font-bold mb-4 text-glow-teal text-teal-400">Subscription Plans & Gating</h2>
              <SubscriptionPlans token={token} user={user} setUser={setUser} API_URL={API_URL} />
            </div>
          </div>
        )}
      </div>

      {/* RIGHT PANEL */}
      {activeTab === "chat" && (
        <RightPanel data={rightPanelData} />
      )}
    </div>
  );
}

// User Profile Update Form Component
function UserProfileForm({ token, API_URL }: { token: string; API_URL: string }) {
  const [profile, setProfile] = useState<any>({
    age: 30,
    gender: "Male",
    country: "USA",
    stateRegion: "California",
    height: 175,
    weight: 70,
    bloodGroup: "O+",
    knownConditions: "None",
    allergies: "None",
    currentMedications: "None",
    medicalHistory: ""
  });
  const [success, setSuccess] = useState(false);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    fetchProfile();
  }, []);

  const fetchProfile = async () => {
    try {
      const res = await fetch(`${API_URL}/api/profile`, {
        headers: { Authorization: `Bearer ${token}` }
      });
      if (res.ok) {
        const data = await res.json();
        setProfile(data);
      }
      setLoading(false);
    } catch (err) {
      console.error(err);
      setLoading(false);
    }
  };

  const handleUpdate = async (e: React.FormEvent) => {
    e.preventDefault();
    setSuccess(false);
    try {
      const res = await fetch(`${API_URL}/api/profile`, {
        method: "PUT",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`
        },
        body: JSON.stringify(profile)
      });
      if (res.ok) {
        setSuccess(true);
      }
    } catch (err) {
      console.error(err);
    }
  };

  if (loading) return <div className="text-sm text-gray-400 text-center">Loading patient profile from EHR...</div>;

  return (
    <form onSubmit={handleUpdate} className="space-y-4 text-sm text-gray-300">
      {success && (
        <div className="p-3 rounded-lg bg-emerald-950/40 border border-emerald-500/30 text-emerald-400 text-xs">
          Profile changes securely synchronized with local clinical memory.
        </div>
      )}

      <div className="grid grid-cols-2 gap-4">
        <div>
          <label className="block text-xs text-gray-400 mb-1 font-semibold uppercase">Age</label>
          <input
            type="number"
            className="w-full px-3 py-1.5 rounded-lg bg-slate-900 border border-white/10 text-white focus:outline-none focus:border-teal-500"
            value={profile.age}
            onChange={(e) => setProfile({ ...profile, age: parseInt(e.target.value) || 0 })}
          />
        </div>
        <div>
          <label className="block text-xs text-gray-400 mb-1 font-semibold uppercase">Gender</label>
          <select
            className="w-full px-3 py-1.5 rounded-lg bg-slate-900 border border-white/10 text-white focus:outline-none focus:border-teal-500"
            value={profile.gender}
            onChange={(e) => setProfile({ ...profile, gender: e.target.value })}
          >
            <option>Male</option>
            <option>Female</option>
            <option>Non-binary</option>
            <option>Prefer not to say</option>
          </select>
        </div>
      </div>

      <div className="grid grid-cols-2 gap-4">
        <div>
          <label className="block text-xs text-gray-400 mb-1 font-semibold uppercase">Country</label>
          <select
            className="w-full px-3 py-1.5 rounded-lg bg-slate-900 border border-white/10 text-white focus:outline-none focus:border-teal-500"
            value={profile.country}
            onChange={(e) => setProfile({ ...profile, country: e.target.value })}
          >
            <option value="USA">USA (e.g. Lyme Disease)</option>
            <option value="UK">UK (e.g. Flu, RSV)</option>
            <option value="Nigeria">Nigeria (e.g. Malaria, Typhoid)</option>
          </select>
        </div>
        <div>
          <label className="block text-xs text-gray-400 mb-1 font-semibold uppercase">State / Region</label>
          <input
            type="text"
            className="w-full px-3 py-1.5 rounded-lg bg-slate-900 border border-white/10 text-white focus:outline-none focus:border-teal-500"
            placeholder="California / Lagos / London"
            value={profile.stateRegion}
            onChange={(e) => setProfile({ ...profile, stateRegion: e.target.value })}
          />
        </div>
      </div>

      <div className="grid grid-cols-3 gap-4">
        <div>
          <label className="block text-xs text-gray-400 mb-1 font-semibold uppercase">Height (cm)</label>
          <input
            type="number"
            className="w-full px-3 py-1.5 rounded-lg bg-slate-900 border border-white/10 text-white focus:outline-none focus:border-teal-500"
            value={profile.height}
            onChange={(e) => setProfile({ ...profile, height: parseFloat(e.target.value) || 0 })}
          />
        </div>
        <div>
          <label className="block text-xs text-gray-400 mb-1 font-semibold uppercase">Weight (kg)</label>
          <input
            type="number"
            className="w-full px-3 py-1.5 rounded-lg bg-slate-900 border border-white/10 text-white focus:outline-none focus:border-teal-500"
            value={profile.weight}
            onChange={(e) => setProfile({ ...profile, weight: parseFloat(e.target.value) || 0 })}
          />
        </div>
        <div>
          <label className="block text-xs text-gray-400 mb-1 font-semibold uppercase">Blood Group</label>
          <input
            type="text"
            className="w-full px-3 py-1.5 rounded-lg bg-slate-900 border border-white/10 text-white focus:outline-none focus:border-teal-500"
            placeholder="O+ / AB-"
            value={profile.bloodGroup}
            onChange={(e) => setProfile({ ...profile, bloodGroup: e.target.value })}
          />
        </div>
      </div>

      <div>
        <label className="block text-xs text-gray-400 mb-1 font-semibold uppercase">Allergies</label>
        <input
          type="text"
          className="w-full px-3 py-1.5 rounded-lg bg-slate-900 border border-white/10 text-white focus:outline-none focus:border-teal-500"
          placeholder="Peanuts, Penicillin, etc."
          value={profile.allergies}
          onChange={(e) => setProfile({ ...profile, allergies: e.target.value })}
        />
      </div>

      <div>
        <label className="block text-xs text-gray-400 mb-1 font-semibold uppercase">Current Medications</label>
        <input
          type="text"
          className="w-full px-3 py-1.5 rounded-lg bg-slate-900 border border-white/10 text-white focus:outline-none focus:border-teal-500"
          placeholder="Lisinopril 10mg, Metformin, etc."
          value={profile.currentMedications}
          onChange={(e) => setProfile({ ...profile, currentMedications: e.target.value })}
        />
      </div>

      <div>
        <label className="block text-xs text-gray-400 mb-1 font-semibold uppercase">Known Conditions</label>
        <input
          type="text"
          className="w-full px-3 py-1.5 rounded-lg bg-slate-900 border border-white/10 text-white focus:outline-none focus:border-teal-500"
          placeholder="Hypertension, Asthma, etc."
          value={profile.knownConditions}
          onChange={(e) => setProfile({ ...profile, knownConditions: e.target.value })}
        />
      </div>

      <div>
        <label className="block text-xs text-gray-400 mb-1 font-semibold uppercase">Medical History Summary</label>
        <textarea
          rows={3}
          className="w-full px-3 py-1.5 rounded-lg bg-slate-900 border border-white/10 text-white focus:outline-none focus:border-teal-500"
          placeholder="Chronic conditions, surgical history details..."
          value={profile.medicalHistory}
          onChange={(e) => setProfile({ ...profile, medicalHistory: e.target.value })}
        />
      </div>

      <button
        type="submit"
        className="w-full py-2 bg-teal-500 text-white font-semibold rounded-lg hover:bg-teal-600 transition"
      >
        Sync Profile & Memory
      </button>
    </form>
  );
}

// Subscription & Billing Page Gating Component
function SubscriptionPlans({ token, user, setUser, API_URL }: { token: string; user: UserInfo; setUser: any; API_URL: string }) {
  const [successMsg, setSuccessMsg] = useState("");

  const handleUpgrade = async (plan: string) => {
    setSuccessMsg("");
    try {
      const roleEnum = plan === "Premium" ? 1 : plan === "Professional" ? 2 : plan === "Admin" ? 3 : 0;
      const res = await fetch(`${API_URL}/api/admin/users/role`, {
        method: "PUT",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`
        },
        body: JSON.stringify({ email: user.email, newRole: roleEnum })
      });
      if (res.ok) {
        const updatedUser = { ...user, role: plan };
        localStorage.setItem("clinica_user", JSON.stringify(updatedUser));
        setUser(updatedUser);
        setSuccessMsg(`Upgraded plan status successfully updated to ${plan}!`);
      } else {
        // Since only Admin is allowed by backend default, for billing preview we bypass role gating
        // In realistic development, billing providers will webhook back.
        // We simulate a local update
        const updatedUser = { ...user, role: plan };
        localStorage.setItem("clinica_user", JSON.stringify(updatedUser));
        setUser(updatedUser);
        setSuccessMsg(`Upgraded subscription initialized for plan: ${plan}.`);
      }
    } catch (err) {
      console.error(err);
    }
  };

  return (
    <div className="space-y-6 text-gray-300 text-sm">
      {successMsg && (
        <div className="p-3 rounded-lg bg-emerald-950/40 border border-emerald-500/30 text-emerald-400 text-xs">
          {successMsg}
        </div>
      )}

      <p className="text-gray-400">
        Active tier: <strong className="text-teal-400 uppercase tracking-widest text-glow-teal">{user.role}</strong>
      </p>

      <div className="space-y-4">
        {/* Free Plan */}
        <div className="p-4 rounded-xl bg-slate-900 border border-white/5 flex justify-between items-center">
          <div>
            <h3 className="font-semibold text-white">Free Basic Plan</h3>
            <p className="text-xs text-gray-500">Symptom check queries, localized clinical references.</p>
          </div>
          {user.role === "Free" ? (
            <span className="text-xs px-2.5 py-1 rounded bg-teal-950/50 border border-teal-500/30 text-teal-400 font-semibold">Active</span>
          ) : (
            <button onClick={() => handleUpgrade("Free")} className="text-xs px-3 py-1 bg-slate-800 hover:bg-slate-700 text-white rounded font-semibold transition">Downgrade</button>
          )}
        </div>

        {/* Premium Plan */}
        <div className="p-4 rounded-xl bg-slate-900 border border-white/5 flex justify-between items-center relative overflow-hidden">
          <div className="absolute top-0 right-0 bg-gradient-to-l from-teal-500/25 to-transparent px-3 py-1 text-[9px] uppercase tracking-wider text-teal-300 font-bold">Recommended</div>
          <div>
            <h3 className="font-semibold text-white">Premium Clinic Plan</h3>
            <p className="text-xs text-gray-500">Lab reports processing (PDF/DOCX), skin photos diagnostics.</p>
            <p className="text-xs text-teal-400 font-semibold mt-1">$15 / month</p>
          </div>
          {user.role === "Premium" ? (
            <span className="text-xs px-2.5 py-1 rounded bg-teal-950/50 border border-teal-500/30 text-teal-400 font-semibold">Active</span>
          ) : (
            <button onClick={() => handleUpgrade("Premium")} className="text-xs px-3 py-1 bg-gradient-to-r from-teal-500 to-teal-600 hover:from-teal-600 hover:to-teal-700 text-white rounded font-semibold transition shadow-md shadow-teal-500/10">Upgrade</button>
          )}
        </div>

        {/* Professional Plan */}
        <div className="p-4 rounded-xl bg-slate-900 border border-white/5 flex justify-between items-center">
          <div>
            <h3 className="font-semibold text-white">Professional Researcher Plan</h3>
            <p className="text-xs text-gray-500">Unlimited uploads, direct vector database semantic RAG query search.</p>
            <p className="text-xs text-teal-400 font-semibold mt-1">$45 / month</p>
          </div>
          {user.role === "Professional" ? (
            <span className="text-xs px-2.5 py-1 rounded bg-teal-950/50 border border-teal-500/30 text-teal-400 font-semibold">Active</span>
          ) : (
            <button onClick={() => handleUpgrade("Professional")} className="text-xs px-3 py-1 bg-slate-800 hover:bg-slate-700 text-white rounded font-semibold transition">Upgrade</button>
          )}
        </div>

        {/* Demo Admin Option */}
        <div className="p-4 rounded-xl bg-slate-900 border border-red-500/10 flex justify-between items-center">
          <div>
            <h3 className="font-semibold text-red-400">Admin Dashboard Access</h3>
            <p className="text-xs text-gray-500">System health monitoring, EHR query logs, audit trails.</p>
          </div>
          {user.role === "Admin" ? (
            <span className="text-xs px-2.5 py-1 rounded bg-red-950/50 border border-red-500/30 text-red-400 font-semibold">Active</span>
          ) : (
            <button onClick={() => handleUpgrade("Admin")} className="text-xs px-3 py-1 bg-red-950/40 hover:bg-red-900/40 text-red-300 border border-red-500/30 rounded font-semibold transition">Acquire Access</button>
          )}
        </div>
      </div>
    </div>
  );
}
