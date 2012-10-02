declare var webkitAudioContext: {
    new (): AudioContext;
}

interface AudioContextConstructor {
    new(): AudioContext;
}

interface Window {
    AudioContext: AudioContextConstructor;
}