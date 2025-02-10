"use client";

import { useRef, useState, useEffect, useCallback } from "react";
import { loadModels, detectSingleFace } from "@/lib/face-api-setup";
import { apiPost } from "@/lib/api";
import CheckInResult from "./CheckInResult";

interface MatchResponse {
  isMatch: boolean;
  employeeId: string | null;
  employeeName: string | null;
  photoUrl: string | null;
  distance: number;
}

interface MatchResult {
  isMatch: boolean;
  employeeName: string | null;
  employeeId: string | null;
  photoUrl: string | null;
  distance: number;
  timestamp: Date;
}

export default function CameraFeed() {
  const videoRef = useRef<HTMLVideoElement>(null);
  const [modelsReady, setModelsReady] = useState(false);
  const [cameraReady, setCameraReady] = useState(false);
  const [lastResult, setLastResult] = useState<MatchResult | null>(null);
  const [detecting, setDetecting] = useState(false);
  const detectingRef = useRef(false);
  const intervalRef = useRef<ReturnType<typeof setInterval> | null>(null);
  const streamRef = useRef<MediaStream | null>(null);

  useEffect(() => {
    loadModels().then(() => setModelsReady(true));
  }, []);

  useEffect(() => {
    if (!modelsReady) return;

    const startCamera = async () => {
      try {
        const stream = await navigator.mediaDevices.getUserMedia({
          video: { facingMode: "user", width: 640, height: 480 },
        });
        streamRef.current = stream;
        if (videoRef.current) {
          videoRef.current.srcObject = stream;
        }
        setCameraReady(true);
      } catch {
        console.error("Camera access denied");
      }
    };

    startCamera();

    return () => {
      if (streamRef.current) {
        streamRef.current.getTracks().forEach((t) => t.stop());
      }
    };
  }, [modelsReady]);

  const runDetection = useCallback(async () => {
    if (!videoRef.current || !cameraReady || detectingRef.current) return;

    detectingRef.current = true;
    setDetecting(true);

    try {
      const descriptor = await detectSingleFace(videoRef.current);

      if (descriptor) {
        const response = await apiPost<MatchResponse>("/api/face/match", {
          descriptor: Array.from(descriptor),
        });

        setLastResult({
          isMatch: response.isMatch,
          employeeName: response.employeeName,
          employeeId: response.employeeId,
          photoUrl: response.photoUrl,
          distance: response.distance,
          timestamp: new Date(),
        });
      } else {
        setLastResult(null);
      }
    } catch (err) {
      console.error("Detection error:", err);
    } finally {
      detectingRef.current = false;
      setDetecting(false);
    }
  }, [cameraReady]);

  useEffect(() => {
    if (!cameraReady || !modelsReady) return;

    intervalRef.current = setInterval(runDetection, 500);

    return () => {
      if (intervalRef.current) clearInterval(intervalRef.current);
    };
  }, [cameraReady, modelsReady, runDetection]);

  return (
    <div className="relative w-full h-full">
      {!modelsReady && (
        <div className="absolute inset-0 flex items-center justify-center bg-gray-900 text-white z-10">
          <div className="text-center">
            <div className="animate-spin w-12 h-12 border-4 border-blue-400 border-t-transparent rounded-full mx-auto mb-4" />
            <p className="text-xl">Loading face detection models...</p>
          </div>
        </div>
      )}

      <video
        ref={videoRef}
        autoPlay
        playsInline
        muted
        className="w-full h-full object-cover"
      />

      {detecting && (
        <div className="absolute top-4 right-4 w-3 h-3 bg-yellow-400 rounded-full animate-pulse" />
      )}

      {lastResult && (
        <div className="absolute bottom-8 left-1/2 -translate-x-1/2 w-80">
          <CheckInResult
            isMatch={lastResult.isMatch}
            employeeName={lastResult.employeeName}
            photoUrl={lastResult.photoUrl}
            distance={lastResult.distance}
            timestamp={lastResult.timestamp}
          />
        </div>
      )}
    </div>
  );
}
