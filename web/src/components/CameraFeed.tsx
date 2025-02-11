"use client";

import { useRef, useState, useEffect, useCallback } from "react";
import { loadModels, detectFaces } from "@/lib/face-api-setup";
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
  const [modelError, setModelError] = useState(false);
  const [cameraReady, setCameraReady] = useState(false);
  const [cameraError, setCameraError] = useState(false);
  const [lastResult, setLastResult] = useState<MatchResult | null>(null);
  const [warning, setWarning] = useState<string | null>(null);
  const [detecting, setDetecting] = useState(false);
  const detectingRef = useRef(false);
  const intervalRef = useRef<ReturnType<typeof setInterval> | null>(null);
  const streamRef = useRef<MediaStream | null>(null);

  useEffect(() => {
    loadModels()
      .then(() => setModelsReady(true))
      .catch(() => setModelError(true));
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
        setCameraError(true);
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
      const result = await detectFaces(videoRef.current);

      if (!result) {
        setLastResult(null);
        setWarning(null);
        return;
      }

      if (result.faceCount > 1) {
        setWarning("Multiple faces detected. Please show one face at a time.");
        setLastResult(null);
        return;
      }

      setWarning(null);
      const response = await apiPost<MatchResponse>("/api/face/match", {
        descriptor: Array.from(result.descriptor),
      });

      setLastResult({
        isMatch: response.isMatch,
        employeeName: response.employeeName,
        employeeId: response.employeeId,
        photoUrl: response.photoUrl,
        distance: response.distance,
        timestamp: new Date(),
      });
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

  if (modelError) {
    return (
      <div className="flex items-center justify-center h-full bg-gray-900 text-white">
        <div className="text-center p-8">
          <p className="text-xl text-red-400 mb-4">Failed to load face detection models</p>
          <button
            onClick={() => window.location.reload()}
            className="bg-blue-600 hover:bg-blue-700 text-white px-6 py-2 rounded-lg"
          >
            Retry
          </button>
        </div>
      </div>
    );
  }

  if (cameraError) {
    return (
      <div className="flex items-center justify-center h-full bg-gray-900 text-white">
        <div className="text-center p-8 max-w-md">
          <p className="text-xl text-red-400 mb-4">Camera access denied</p>
          <p className="text-gray-400 mb-4">
            Please allow camera permissions in your browser settings and refresh
            the page.
          </p>
          <button
            onClick={() => window.location.reload()}
            className="bg-blue-600 hover:bg-blue-700 text-white px-6 py-2 rounded-lg"
          >
            Retry
          </button>
        </div>
      </div>
    );
  }

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

      {warning && (
        <div className="absolute top-4 left-1/2 -translate-x-1/2 bg-yellow-500/90 text-black px-4 py-2 rounded-lg font-medium">
          {warning}
        </div>
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
