"use client";

import { getPhotoUrl } from "@/lib/api";

interface CheckInResultProps {
  isMatch: boolean;
  employeeName?: string | null;
  photoUrl?: string | null;
  distance?: number;
  timestamp?: Date;
}

export default function CheckInResult({
  isMatch,
  employeeName,
  photoUrl,
  distance,
  timestamp,
}: CheckInResultProps) {
  if (!isMatch) {
    return (
      <div className="bg-gray-800/80 backdrop-blur-sm text-gray-300 rounded-xl p-4 text-center transition-opacity duration-300">
        <p className="text-lg">No reconocido</p>
      </div>
    );
  }

  const photo = getPhotoUrl(photoUrl);
  const confidence = distance != null ? Math.max(0, (1 - distance) * 100) : 0;

  return (
    <div className="bg-green-600/90 backdrop-blur-sm text-white rounded-xl p-6 text-center shadow-2xl shadow-green-500/30 animate-bounce-in">
      <p className="text-xs font-semibold tracking-widest mb-2 opacity-80">CHECK-IN</p>
      {photo && (
        <img
          src={photo}
          alt={employeeName || "Employee"}
          className="w-20 h-20 rounded-full mx-auto mb-3 object-cover border-4 border-white shadow-lg"
        />
      )}
      <p className="text-2xl font-bold mb-1">
        Presente! {employeeName}
      </p>
      <p className="text-sm opacity-80">
        {timestamp?.toLocaleTimeString()} &middot; {confidence.toFixed(0)}% confidence
      </p>
    </div>
  );
}
