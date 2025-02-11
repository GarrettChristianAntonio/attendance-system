import * as faceapi from "@vladmandic/face-api";

let modelsLoaded = false;
let loadingPromise: Promise<void> | null = null;

export async function loadModels(retries = 3): Promise<void> {
  if (modelsLoaded) return;
  if (loadingPromise) return loadingPromise;

  loadingPromise = (async () => {
    const MODEL_URL = "/models";
    for (let attempt = 1; attempt <= retries; attempt++) {
      try {
        await Promise.all([
          faceapi.nets.ssdMobilenetv1.loadFromUri(MODEL_URL),
          faceapi.nets.faceLandmark68Net.loadFromUri(MODEL_URL),
          faceapi.nets.faceRecognitionNet.loadFromUri(MODEL_URL),
        ]);
        modelsLoaded = true;
        return;
      } catch (err) {
        console.error(`Model load attempt ${attempt}/${retries} failed:`, err);
        if (attempt < retries) {
          await new Promise((r) => setTimeout(r, 1000 * attempt));
        } else {
          loadingPromise = null;
          throw new Error("Failed to load face detection models after retries");
        }
      }
    }
  })();

  return loadingPromise;
}

export interface DetectionResult {
  descriptor: Float32Array;
  faceCount: number;
}

export async function detectFaces(
  input: HTMLVideoElement | HTMLCanvasElement | HTMLImageElement
): Promise<DetectionResult | null> {
  const detections = await faceapi
    .detectAllFaces(input)
    .withFaceLandmarks()
    .withFaceDescriptors();

  if (detections.length === 0) return null;

  return {
    descriptor: detections[0].descriptor,
    faceCount: detections.length,
  };
}

export async function detectSingleFace(
  input: HTMLVideoElement | HTMLCanvasElement | HTMLImageElement
): Promise<Float32Array | null> {
  const result = await faceapi
    .detectSingleFace(input)
    .withFaceLandmarks()
    .withFaceDescriptor();

  return result?.descriptor ?? null;
}

export { faceapi };
