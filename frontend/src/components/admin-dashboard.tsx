"use client";

import React, { useState, useEffect } from "react";
import { ShieldAlert, Users, Server, Database, TrendingUp, RefreshCw, FileCheck } from "lucide-react";

interface AdminDashboardProps {
  token: string;
}

export default function AdminDashboard({ token }: AdminDashboardProps) {
  const [analytics, setAnalytics] = useState<any>(null);
  const [users, setUsers] = useState<any[]>([]);
  const [health, setHealth] = useState<any>(null);
  const [auditLogs, setAuditLogs] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [roleUpdateMsg, setRoleUpdateMsg] = useState("");

  const API_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5000";

  useEffect(() => {
    fetchAdminData();
  }, []);

  const fetchAdminData = async () => {
    setLoading(true);
    try {
      const headers = { Authorization: `Bearer ${token}` };

      const [analyticsRes, usersRes, healthRes, logsRes] = await Promise.all([
        fetch(`${API_URL}/api/admin/analytics`, { headers }),
        fetch(`${API_URL}/api/admin/users`, { headers }),
        fetch(`${API_URL}/api/admin/system-health`, { headers }),
        fetch(`${API_URL}/api/admin/audit-logs`, { headers })
      ]);

      if (analyticsRes.ok) setAnalytics(await analyticsRes.json());
      if (usersRes.ok) setUsers(await usersRes.json());
      if (healthRes.ok) setHealth(await healthRes.json());
      if (logsRes.ok) setAuditLogs(await logsRes.json());
    } catch (err) {
      console.error("Failed to retrieve admin details", err);
    } finally {
      setLoading(false);
    }
  };

  const handleRoleChange = async (email: string, newRoleVal: number) => {
    setRoleUpdateMsg("");
    try {
      const res = await fetch(`${API_URL}/api/admin/users/role`, {
        method: "PUT",
        headers: {
          "Content-Type": "application/json",
          Authorization: `Bearer ${token}`
        },
        body: JSON.stringify({ email, newRole: newRoleVal })
      });
      if (res.ok) {
        setRoleUpdateMsg(`Role updated for ${email}`);
        fetchAdminData();
      }
    } catch (err) {
      console.error(err);
    }
  };

  const roleNameMap = (roleVal: number | string) => {
    if (roleVal === 0 || roleVal === "Free") return "Free";
    if (roleVal === 1 || roleVal === "Premium") return "Premium";
    if (roleVal === 2 || roleVal === "Professional") return "Professional";
    if (roleVal === 3 || roleVal === "Admin") return "Admin";
    return "Unknown";
  };

  if (loading) {
    return (
      <div className="flex-1 flex flex-col justify-center items-center h-full text-xs text-gray-400">
        <RefreshCw className="h-6 w-6 text-teal-400 animate-spin mb-2" />
        <span>Loading clinical SaaS logs & analytics...</span>
      </div>
    );
  }

  return (
    <div className="flex-1 flex flex-col h-full overflow-y-auto px-6 py-6 space-y-6 scrollbar-thin">
      {/* Header */}
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-xl font-bold text-white flex items-center gap-2">
            <ShieldAlert className="h-5 w-5 text-teal-400" />
            Clinica AI Admin Console
          </h1>
          <p className="text-[10px] text-gray-400 font-semibold mt-0.5">HIPAA Security Audit Trail & Subscription Analytics</p>
        </div>
        <button
          onClick={fetchAdminData}
          className="p-2 rounded bg-slate-900 border border-white/5 hover:border-teal-500/20 text-gray-400 hover:text-white transition text-xs font-semibold flex items-center gap-1.5"
        >
          <RefreshCw className="h-3.5 w-3.5" /> Refresh Records
        </button>
      </div>

      {roleUpdateMsg && (
        <div className="p-3 bg-emerald-950/40 border border-emerald-500/30 text-emerald-400 text-xs rounded-lg">
          {roleUpdateMsg}
        </div>
      )}

      {/* Analytics Cards */}
      {analytics && (
        <div className="grid grid-cols-4 gap-4">
          <div className="p-4 rounded-xl bg-slate-900/60 border border-white/5 shadow-lg">
            <span className="text-[9px] uppercase font-bold tracking-widest text-gray-500 flex items-center gap-1.5">
              <Users className="h-4 w-4 text-teal-400" /> Total Accounts
            </span>
            <h3 className="text-2xl font-black text-white mt-1">{analytics.totalUsers}</h3>
            <p className="text-[10px] text-gray-500 font-medium mt-0.5">Free: {analytics.totalUsers - analytics.premiumCount} | Premium: {analytics.premiumCount}</p>
          </div>

          <div className="p-4 rounded-xl bg-slate-900/60 border border-white/5 shadow-lg">
            <span className="text-[9px] uppercase font-bold tracking-widest text-gray-500 flex items-center gap-1.5">
              <TrendingUp className="h-4 w-4 text-emerald-400" /> Projected MRR
            </span>
            <h3 className="text-2xl font-black text-emerald-400 mt-1">${analytics.estimatedRevenue}</h3>
            <p className="text-[10px] text-gray-500 font-medium mt-0.5">Estimated monthly recurring revenue</p>
          </div>

          <div className="p-4 rounded-xl bg-slate-900/60 border border-white/5 shadow-lg">
            <span className="text-[9px] uppercase font-bold tracking-widest text-gray-500 flex items-center gap-1.5">
              <Server className="h-4 w-4 text-cyan-400" /> Conversations
            </span>
            <h3 className="text-2xl font-black text-white mt-1">{analytics.conversationsCount}</h3>
            <p className="text-[10px] text-gray-500 font-medium mt-0.5">Total AI messages processed: {analytics.messagesCount}</p>
          </div>

          <div className="p-4 rounded-xl bg-slate-900/60 border border-white/5 shadow-lg">
            <span className="text-[9px] uppercase font-bold tracking-widest text-gray-500 flex items-center gap-1.5">
              <FileCheck className="h-4 w-4 text-teal-400" /> Reports Uploaded
            </span>
            <h3 className="text-2xl font-black text-white mt-1">{analytics.interpretedReportsCount}</h3>
            <p className="text-[10px] text-gray-500 font-medium mt-0.5">Total file attachments parsed: {analytics.uploadedFilesCount}</p>
          </div>
        </div>
      )}

      {/* System Health Block */}
      {health && (
        <div className="p-4 rounded-xl bg-slate-900/60 border border-white/5 shadow-lg space-y-3">
          <span className="text-[9px] uppercase font-bold tracking-widest text-gray-500 flex items-center gap-1.5">
            <Database className="h-4 w-4 text-teal-400" /> EHR Database & Server Health Check
          </span>
          <div className="grid grid-cols-4 gap-4 pt-1">
            <div className="p-2.5 rounded bg-slate-950/45 border border-white/5 text-xs">
              <p className="text-gray-500">PostgreSQL Status</p>
              <strong className="text-emerald-400 block mt-1">{health.postgreSQL}</strong>
            </div>
            <div className="p-2.5 rounded bg-slate-950/45 border border-white/5 text-xs">
              <p className="text-gray-500">Redis Cache</p>
              <strong className="text-teal-400 block mt-1">{health.redisCache}</strong>
            </div>
            <div className="p-2.5 rounded bg-slate-950/45 border border-white/5 text-xs">
              <p className="text-gray-500">Object Storage</p>
              <strong className="text-cyan-400 block mt-1">{health.minIOStorage}</strong>
            </div>
            <div className="p-2.5 rounded bg-slate-950/45 border border-white/5 text-xs">
              <p className="text-gray-500">Reasoning Engine</p>
              <strong className="text-teal-400 block mt-1">{health.memoryModel}</strong>
            </div>
          </div>
        </div>
      )}

      {/* User Manager Section */}
      <div className="p-4 rounded-xl bg-slate-900/60 border border-white/5 shadow-lg space-y-3">
        <span className="text-[9px] uppercase font-bold tracking-widest text-gray-500 block">User Access Control</span>
        <div className="overflow-x-auto">
          <table className="w-full text-xs text-left text-gray-400">
            <thead className="text-[10px] uppercase text-gray-500 border-b border-white/5">
              <tr>
                <th className="py-2">FullName</th>
                <th className="py-2">Email</th>
                <th className="py-2">Plan</th>
                <th className="py-2">Verify</th>
                <th className="py-2">Joined</th>
                <th className="py-2 text-right">Moderator actions</th>
              </tr>
            </thead>
            <tbody className="divide-y divide-white/5">
              {users.map((u) => (
                <tr key={u.id}>
                  <td className="py-2.5 text-gray-200 font-bold">{u.fullName}</td>
                  <td className="py-2.5">{u.email}</td>
                  <td className="py-2.5">
                    <span className="px-1.5 py-0.5 rounded text-[9px] bg-slate-800 border border-white/5 text-gray-300 uppercase tracking-wider font-extrabold">
                      {roleNameMap(u.role)}
                    </span>
                  </td>
                  <td className="py-2.5">
                    {u.isEmailVerified ? (
                      <span className="text-emerald-400 font-bold">Verified</span>
                    ) : (
                      <span className="text-yellow-500">Pending</span>
                    )}
                  </td>
                  <td className="py-2.5">{new Date(u.createdAt).toLocaleDateString()}</td>
                  <td className="py-2.5 text-right">
                    <select
                      className="bg-slate-950 border border-white/5 rounded px-2 py-0.5 focus:outline-none focus:border-teal-500"
                      value={u.role === "Free" ? 0 : u.role === "Premium" ? 1 : u.role === "Professional" ? 2 : u.role === "Admin" ? 3 : u.role}
                      onChange={(e) => handleRoleChange(u.email, parseInt(e.target.value))}
                    >
                      <option value={0}>Set Free</option>
                      <option value={1}>Set Premium</option>
                      <option value={2}>Set Professional</option>
                      <option value={3}>Set Admin</option>
                    </select>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      </div>

      {/* Audit Logs Trail */}
      <div className="p-4 rounded-xl bg-slate-900/60 border border-white/5 shadow-lg space-y-3">
        <span className="text-[9px] uppercase font-bold tracking-widest text-gray-500 block">HIPAA Audit Logs (Recent Actions)</span>
        <div className="max-h-72 overflow-y-auto space-y-2 scrollbar-thin">
          {auditLogs.map((l) => (
            <div key={l.id} className="p-2.5 rounded bg-slate-950/45 border border-white/5 text-[10px] text-gray-400 flex justify-between items-start gap-4">
              <div>
                <span className="text-teal-400 font-bold">{l.action}</span>
                <p className="text-gray-500 mt-1">{l.details}</p>
                <span className="text-slate-600 block mt-0.5">IP Address: {l.ipAddress}</span>
              </div>
              <span className="text-gray-600 font-semibold">{new Date(l.timestamp).toLocaleString()}</span>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}
