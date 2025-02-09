import * as faceapi from "@vladmandic/face-api";

let modelsLoaded = false;
let loadingPromise: Promise<void> | null = null;

export async function loadModels(): Promise<void> {
  if (modelsLoaded) return;
  if (loadingPromise) return loadingPromise;

  loadingPromise = (async () => {
    const MODEL_URL = "/models";
    await Promise.all([
      faceapi.nets.ssdMobilenetv1.loadFromUri(MODEL_URL),
      faceapi.nets.faceLandmark68Net.loadFromUri(MODEL_URL),
      faceapi.nets.faceRecognitionNet.loadFromUri(MODEL_URL),
    ]);
    modelsLoaded = true;
  })();

  return loadingPromise;
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
