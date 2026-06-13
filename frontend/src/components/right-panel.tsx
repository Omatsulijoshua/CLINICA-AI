"use client";

import React from "react";
import { BookOpen, Video, Activity, AlertTriangle, ExternalLink, ShieldCheck, HeartHandshake } from "lucide-react";

interface RightPanelProps {
  data: {
    sources: any[];
    videos: any[];
    confidenceScore: number;
    relatedConditions: string[];
    recommendedNextSteps: string[];
    isEmergency: boolean;
    emergencyGuidance: string;
  };
}

export default function RightPanel({ data }: RightPanelProps) {
  const scorePercent = Math.round(data.confidenceScore * 100);
  
  // Gauge styling calculations
  const strokeDashoffset = 157 - (157 * (scorePercent || 0)) / 100;

  return (
    <div className="w-96 flex flex-col h-full bg-slate-900 border-l border-white/5 relative z-20 overflow-y-auto scrollbar-thin">
      {/* Title */}
      <div className="p-4 border-b border-white/5 bg-slate-950/20">
        <h2 className="text-sm font-bold text-gray-200 tracking-wide">Sources & Learnings</h2>
        <p className="text-[10px] text-gray-500 font-semibold mt-0.5">Academic Citations & Explanations</p>
      </div>

      {/* EMERGENCY MODE ALERT */}
      {data.isEmergency && (
        <div className="p-4 bg-red-950/35 border-b border-red-500/30 text-red-300 space-y-2.5 animate-pulse">
          <div className="flex items-center gap-2 text-red-400 font-extrabold text-xs uppercase tracking-wider">
            <AlertTriangle className="h-5 w-5 animate-bounce shrink-0" />
            <span>Emergency Mode Active</span>
          </div>
          <p className="text-[11px] font-semibold leading-relaxed whitespace-pre-line bg-red-950/60 p-2.5 rounded-lg border border-red-500/20">
            {data.emergencyGuidance}
          </p>
        </div>
      )}

      {/* CONFIDENCE SCORE GAUGE */}
      <div className="p-4 border-b border-white/5 flex items-center gap-4 bg-slate-950/10">
        <div className="relative h-16 w-16 shrink-0 flex items-center justify-center">
          <svg className="w-full h-full transform -rotate-90">
            <circle cx="32" cy="32" r="25" stroke="rgba(255,255,255,0.04)" strokeWidth="4" fill="transparent" />
            <circle
              cx="32"
              cy="32"
              r="25"
              stroke={data.isEmergency ? "#ef4444" : "#14b8a6"}
              strokeWidth="4"
              fill="transparent"
              strokeDasharray="157"
              strokeDashoffset={strokeDashoffset}
              className="transition-all duration-1000 ease-out"
            />
          </svg>
          <span className="absolute text-xs font-black text-white">{scorePercent}%</span>
        </div>
        <div>
          <span className="text-[9px] uppercase font-bold tracking-widest text-gray-500">Confidence Index</span>
          <h3 className="text-xs font-bold text-gray-200 mt-0.5">
            {scorePercent > 80 
              ? "High Medical Consensus" 
              : scorePercent > 50 
                ? "Moderate Evidence Base"
                : scorePercent > 0 
                  ? "Diagnostic Verification Req."
                  : "Awaiting Input Data"}
          </h3>
          <p className="text-[10px] text-gray-500 font-semibold leading-tight">Calculated across vector reference weights</p>
        </div>
      </div>

      {/* RELATED CONDITIONS */}
      {data.relatedConditions && data.relatedConditions.length > 0 && (
        <div className="p-4 border-b border-white/5">
          <span className="text-[9px] uppercase font-bold tracking-widest text-gray-500 block mb-2.5">Related Conditions</span>
          <div className="flex flex-wrap gap-1.5">
            {data.relatedConditions.map((cond, idx) => (
              <span
                key={idx}
                className="text-[10px] font-bold px-2 py-0.5 rounded bg-slate-800 text-gray-300 border border-white/5"
              >
                {cond}
              </span>
            ))}
          </div>
        </div>
      )}

      {/* ACADEMIC MEDICAL SOURCES */}
      <div className="p-4 border-b border-white/5 space-y-3">
        <div className="flex items-center gap-1.5 text-gray-400">
          <BookOpen className="h-4 w-4 text-teal-400" />
          <span className="text-[10px] uppercase font-extrabold tracking-widest">Medical Citations</span>
        </div>

        {data.sources && data.sources.length > 0 ? (
          <div className="space-y-3">
            {data.sources.map((src, idx) => (
              <div key={idx} className="p-2.5 rounded-lg bg-slate-950/45 border border-white/5 hover:border-teal-500/20 transition">
                <div className="flex items-start justify-between gap-2">
                  <span className="text-[9px] font-extrabold uppercase px-1.5 py-0.5 rounded bg-teal-950/60 border border-teal-500/20 text-teal-400 shrink-0">
                    {src.name}
                  </span>
                  <a
                    href={src.url}
                    target="_blank"
                    rel="noreferrer"
                    className="text-gray-500 hover:text-teal-400 transition"
                    title="View Publication"
                  >
                    <ExternalLink className="h-3.5 w-3.5" />
                  </a>
                </div>
                <h4 className="text-xs font-bold text-gray-200 mt-2 leading-tight">{src.title}</h4>
                <p className="text-[10px] text-gray-400 mt-1.5 leading-relaxed">{src.snippet}</p>
              </div>
            ))}
          </div>
        ) : (
          <p className="text-[10px] text-gray-600 font-semibold text-center py-4">No reference publications loaded for this query.</p>
        )}
      </div>

      {/* YOUTUBE EXPLAINERS */}
      <div className="p-4 border-b border-white/5 space-y-3">
        <div className="flex items-center gap-1.5 text-gray-400">
          <Video className="h-4 w-4 text-teal-400" />
          <span className="text-[10px] uppercase font-extrabold tracking-widest">Educational Videos</span>
        </div>

        {data.videos && data.videos.length > 0 ? (
          <div className="space-y-3">
            {data.videos.map((vid, idx) => (
              <a
                key={idx}
                href={vid.link}
                target="_blank"
                rel="noreferrer"
                className="block p-2 rounded-lg bg-slate-950/45 border border-white/5 hover:border-teal-500/20 hover:bg-slate-950 transition group overflow-hidden"
              >
                <div className="relative w-full aspect-video rounded bg-slate-900 overflow-hidden mb-2">
                  <img src={vid.thumbnailUrl} alt={vid.title} className="w-full h-full object-cover group-hover:scale-105 transition" />
                  <span className="absolute bottom-1 right-1 bg-black/75 px-1.5 py-0.5 rounded text-[9px] text-white font-bold leading-none">
                    {vid.duration}
                  </span>
                </div>
                <h4 className="text-xs font-bold text-gray-300 leading-tight group-hover:text-teal-400 transition truncate">{vid.title}</h4>
                <span className="text-[10px] text-gray-500 font-semibold block mt-1">{vid.channel}</span>
              </a>
            ))}
          </div>
        ) : (
          <p className="text-[10px] text-gray-600 font-semibold text-center py-4">No video explainers retrieved.</p>
        )}
      </div>

      {/* RECOMMENDED NEXT STEPS */}
      {data.recommendedNextSteps && data.recommendedNextSteps.length > 0 && (
        <div className="p-4 space-y-3 bg-slate-950/10">
          <div className="flex items-center gap-1.5 text-gray-400">
            <HeartHandshake className="h-4 w-4 text-teal-400" />
            <span className="text-[10px] uppercase font-extrabold tracking-widest">Recommended Actions</span>
          </div>
          <ul className="space-y-2">
            {data.recommendedNextSteps.map((step, idx) => (
              <li
                key={idx}
                className="text-[11px] text-gray-400 leading-normal flex items-start gap-2 bg-slate-950/40 p-2 rounded border border-white/5"
              >
                <span className="h-4 w-4 rounded-full bg-teal-950 text-teal-400 text-[9px] font-extrabold flex items-center justify-center shrink-0 mt-0.5">
                  {idx + 1}
                </span>
                <span>{step}</span>
              </li>
            ))}
          </ul>
        </div>
      )}
    </div>
  );
}
