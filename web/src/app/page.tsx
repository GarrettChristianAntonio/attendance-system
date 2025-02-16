"use client";

import dynamic from "next/dynamic";

const CameraFeed = dynamic(() => import("@/components/CameraFeed"), {
  ssr: false,
  loading: () => (
    <div className="flex items-center justify-center h-full bg-gray-900 text-white">
      <div className="text-center">
        <div className="animate-spin w-10 h-10 border-4 border-blue-400 border-t-transparent rounded-full mx-auto mb-3" />
        <p>Loading camera...</p>
      </div>
    </div>
  ),
});

export default function Home() {
  return (
    <div className="h-[calc(100vh-53px)] bg-black">
      <CameraFeed />
    </div>
  );
}
