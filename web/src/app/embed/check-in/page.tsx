"use client";

import { useEffect, useRef, useState } from "react";

export default function EmbedCheckInPage() {
  const videoRef = useRef<HTMLVideoElement>(null);
  const [status, setStatus] = useState<string>("Initializing camera...");
  const [lastMatch, setLastMatch] = useState<string | null>(null);

  useEffect(() => {
    const startCamera = async () => {
      try {
        const stream = await navigator.mediaDevices.getUserMedia({
          video: { facingMode: "user", width: 640, height: 480 },
        });
        if (videoRef.current) {
          videoRef.current.srcObject = stream;
          setStatus("Camera ready. Face the camera to check in.");
        }
      } catch {
        setStatus("Camera access denied");
      }
    };
    startCamera();
  }, []);

  useEffect(() => {
    const handleMessage = (event: MessageEvent) => {
      if (event.data?.type === "check-in-result") {
        setLastMatch(event.data.employeeName);
        setTimeout(() => setLastMatch(null), 5000);
      }
    };
    window.addEventListener("message", handleMessage);
    return () => window.removeEventListener("message", handleMessage);
  }, []);

  return (
    <div className="flex flex-col items-center justify-center min-h-screen p-4 bg-gray-50">
      <div className="relative w-full max-w-md">
        <video
          ref={videoRef}
          autoPlay
          playsInline
          muted
          className="w-full rounded-xl shadow-lg"
        />
        {lastMatch && (
          <div className="absolute bottom-4 left-4 right-4 bg-green-500 text-white rounded-lg p-3 text-center font-semibold animate-pulse">
            Welcome, {lastMatch}!
          </div>
        )}
      </div>
      <p className="mt-4 text-sm text-gray-500">{status}</p>
    </div>
  );
}
