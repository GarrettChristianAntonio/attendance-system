"use client";

import { useRef, useState, useCallback, useEffect } from "react";
import { loadModels, detectFaces } from "@/lib/face-api-setup";
import { apiPostForm } from "@/lib/api";
import { useRouter } from "next/navigation";

interface EmployeeDto {
  id: string;
  name: string;
  email: string | null;
  photoUrl: string | null;
  isActive: boolean;
  createdAt: string;
}

export default function EmployeeForm() {
  const router = useRouter();
  const videoRef = useRef<HTMLVideoElement>(null);
  const canvasRef = useRef<HTMLCanvasElement>(null);
  const [name, setName] = useState("");
  const [email, setEmail] = useState("");
  const [capturedPhoto, setCapturedPhoto] = useState<Blob | null>(null);
  const [capturedPhotoUrl, setCapturedPhotoUrl] = useState<string | null>(null);
  const [descriptor, setDescriptor] = useState<Float32Array | null>(null);
  const [cameraActive, setCameraActive] = useState(false);
  const [modelsReady, setModelsReady] = useState(false);
  const [status, setStatus] = useState("");
  const [submitting, setSubmitting] = useState(false);
  const streamRef = useRef<MediaStream | null>(null);

  useEffect(() => {
    loadModels().then(() => setModelsReady(true));
  }, []);

  const startCamera = useCallback(async () => {
    try {
      const stream = await navigator.mediaDevices.getUserMedia({
        video: { facingMode: "user", width: 640, height: 480 },
      });
      streamRef.current = stream;
      if (videoRef.current) {
        videoRef.current.srcObject = stream;
      }
      setCameraActive(true);
      setStatus("");
    } catch {
      setStatus("Camera access denied. Please allow camera permissions.");
    }
  }, []);

  const stopCamera = useCallback(() => {
    if (streamRef.current) {
      streamRef.current.getTracks().forEach((t) => t.stop());
      streamRef.current = null;
    }
    setCameraActive(false);
  }, []);

  const capturePhoto = useCallback(async () => {
    if (!videoRef.current || !canvasRef.current || !modelsReady) return;

    setStatus("Detecting face...");
    const result = await detectFaces(videoRef.current);

    if (!result) {
      setStatus("No face detected. Please face the camera and try again.");
      return;
    }

    if (result.faceCount > 1) {
      setStatus("Multiple faces detected. Please ensure only one face is visible.");
      return;
    }

    const desc = result.descriptor;

    const canvas = canvasRef.current;
    canvas.width = videoRef.current.videoWidth;
    canvas.height = videoRef.current.videoHeight;
    const ctx = canvas.getContext("2d")!;
    ctx.drawImage(videoRef.current, 0, 0);

    canvas.toBlob((blob) => {
      if (blob) {
        setCapturedPhoto(blob);
        setCapturedPhotoUrl(URL.createObjectURL(blob));
        setDescriptor(desc);
        setStatus("Face detected successfully!");
        stopCamera();
      }
    }, "image/jpeg", 0.9);
  }, [modelsReady, stopCamera]);

  const retakePhoto = useCallback(() => {
    setCapturedPhoto(null);
    if (capturedPhotoUrl) URL.revokeObjectURL(capturedPhotoUrl);
    setCapturedPhotoUrl(null);
    setDescriptor(null);
    setStatus("");
    startCamera();
  }, [capturedPhotoUrl, startCamera]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!capturedPhoto || !descriptor) {
      setStatus("Please capture a photo with a detected face first.");
      return;
    }

    setSubmitting(true);
    setStatus("Registering employee...");

    try {
      const formData = new FormData();
      formData.append("name", name);
      if (email) formData.append("email", email);
      formData.append("faceDescriptor", JSON.stringify(Array.from(descriptor)));
      formData.append("photo", capturedPhoto, "photo.jpg");

      await apiPostForm<EmployeeDto>("/api/employees", formData);
      setStatus("Employee registered successfully!");
      router.push("/employees");
    } catch (err) {
      setStatus(`Error: ${err instanceof Error ? err.message : "Unknown error"}`);
    } finally {
      setSubmitting(false);
    }
  };

  useEffect(() => {
    return () => {
      stopCamera();
      if (capturedPhotoUrl) URL.revokeObjectURL(capturedPhotoUrl);
    };
  }, []);

  return (
    <form onSubmit={handleSubmit} className="max-w-2xl mx-auto space-y-6">
      <div>
        <label htmlFor="name" className="block text-sm font-medium mb-1">
          Name *
        </label>
        <input
          id="name"
          type="text"
          required
          value={name}
          onChange={(e) => setName(e.target.value)}
          className="w-full rounded-lg border border-gray-300 px-4 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
          placeholder="John Doe"
        />
      </div>

      <div>
        <label htmlFor="email" className="block text-sm font-medium mb-1">
          Email
        </label>
        <input
          id="email"
          type="email"
          value={email}
          onChange={(e) => setEmail(e.target.value)}
          className="w-full rounded-lg border border-gray-300 px-4 py-2 focus:outline-none focus:ring-2 focus:ring-blue-500"
          placeholder="john@example.com"
        />
      </div>

      <div>
        <label className="block text-sm font-medium mb-2">Photo *</label>
        <div className="relative bg-black rounded-lg overflow-hidden aspect-video">
          {!capturedPhotoUrl && (
            <video
              ref={videoRef}
              autoPlay
              playsInline
              muted
              className={`w-full h-full object-cover ${
                cameraActive ? "block" : "hidden"
              }`}
            />
          )}
          {capturedPhotoUrl && (
            <img
              src={capturedPhotoUrl}
              alt="Captured"
              className="w-full h-full object-cover"
            />
          )}
          {!cameraActive && !capturedPhotoUrl && (
            <div className="absolute inset-0 flex items-center justify-center text-white">
              <button
                type="button"
                onClick={startCamera}
                disabled={!modelsReady}
                className="bg-blue-600 hover:bg-blue-700 disabled:bg-gray-500 text-white px-6 py-3 rounded-lg font-medium"
              >
                {modelsReady ? "Start Camera" : "Loading models..."}
              </button>
            </div>
          )}
        </div>
        <canvas ref={canvasRef} className="hidden" />

        <div className="mt-3 flex gap-3">
          {cameraActive && (
            <button
              type="button"
              onClick={capturePhoto}
              className="bg-green-600 hover:bg-green-700 text-white px-6 py-2 rounded-lg font-medium"
            >
              Capture Photo
            </button>
          )}
          {capturedPhotoUrl && (
            <button
              type="button"
              onClick={retakePhoto}
              className="bg-gray-600 hover:bg-gray-700 text-white px-6 py-2 rounded-lg font-medium"
            >
              Retake
            </button>
          )}
        </div>
      </div>

      {status && (
        <p
          className={`text-sm ${
            status.includes("Error") || status.includes("No face") || status.includes("denied")
              ? "text-red-600"
              : status.includes("success")
              ? "text-green-600"
              : "text-blue-600"
          }`}
        >
          {status}
        </p>
      )}

      <button
        type="submit"
        disabled={!capturedPhoto || !descriptor || !name || submitting}
        className="w-full bg-blue-600 hover:bg-blue-700 disabled:bg-gray-400 text-white py-3 rounded-lg font-medium text-lg"
      >
        {submitting ? "Registering..." : "Register Employee"}
      </button>
    </form>
  );
}
