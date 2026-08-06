// Ecosistema de Voz Enterprise de LuxuryCo
// Soporta Web Speech API nativa + Fallback a Whisper Backend

class VoiceAssistant {
    constructor() {
        this.recognition = null;
        this.isListening = false;
        this.whisperEndpoint = "/Home/TranscribeAudio"; // Proxy to bypass CORS
        this.state = "IDLE"; // IDLE, LISTENING, PROCESSING, SPEAKING, ERROR
        this.mediaRecorder = null;
        this.audioChunks = [];
        this.onTranscriptionCallback = null;
        this.onStateChangeCallback = null;

        this.initWebSpeech();
    }

    initWebSpeech() {
        const SpeechRecognition = window.SpeechRecognition || window.webkitSpeechRecognition;
        if (SpeechRecognition) {
            this.recognition = new SpeechRecognition();
            this.recognition.continuous = false;
            this.recognition.lang = "es-CO";
            this.recognition.interimResults = false;

            this.recognition.onstart = () => {
                this.setState("LISTENING");
            };

            this.recognition.onresult = (event) => {
                const text = event.results[0][0].transcript;
                this.setState("PROCESSING");
                if (this.onTranscriptionCallback) {
                    this.onTranscriptionCallback(text);
                }
            };

            this.recognition.onerror = (event) => {
                console.warn("Web Speech Error, falling back to Whisper...", event.error);
                this.startWhisperRecording();
            };

            this.recognition.onend = () => {
                if (this.state === "LISTENING") {
                    this.setState("IDLE");
                }
            };
        } else {
            console.warn("Speech recognition not supported in this browser, default to Whisper fallback.");
        }
    }

    setState(newState) {
        this.state = newState;
        if (this.onStateChangeCallback) {
            this.onStateChangeCallback(newState);
        }
    }

    async startListening(onTranscription, onStateChange) {
        this.onTranscriptionCallback = onTranscription;
        this.onStateChangeCallback = onStateChange;

        if (this.recognition) {
            try {
                this.recognition.start();
                this.isListening = true;
            } catch (ex) {
                this.startWhisperRecording();
            }
        } else {
            this.startWhisperRecording();
        }
    }

    stopListening() {
        if (this.recognition && this.isListening) {
            this.recognition.stop();
            this.isListening = false;
        } else if (this.mediaRecorder && this.mediaRecorder.state !== "inactive") {
            this.mediaRecorder.stop();
        }
    }

    async startWhisperRecording() {
        this.setState("LISTENING");
        this.audioChunks = [];

        try {
            const stream = await navigator.mediaDevices.getUserMedia({ audio: true });
            this.mediaRecorder = new MediaRecorder(stream);
            
            this.mediaRecorder.ondataavailable = (event) => {
                this.audioChunks.push(event.data);
            };

            this.mediaRecorder.onstop = async () => {
                this.setState("PROCESSING");
                const audioBlob = new Blob(this.audioChunks, { type: 'audio/wav' });
                await this.sendToWhisperBackend(audioBlob);
            };

            this.mediaRecorder.start();
        } catch (err) {
            console.error("No se pudo acceder al micrófono:", err);
            this.setState("ERROR");
        }
    }

    async sendToWhisperBackend(blob) {
        const formData = new FormData();
        formData.append("file", blob, "voice.wav");

        try {
            const response = await fetch(this.whisperEndpoint, {
                method: "POST",
                body: formData
            });

            if (!response.ok) throw new Error("Whisper failed");

            const data = await response.json();
            if (this.onTranscriptionCallback) {
                this.onTranscriptionCallback(data.text);
            }
            this.setState("IDLE");
        } catch (err) {
            console.error("Whisper endpoint error:", err);
            this.setState("ERROR");
        }
    }

    speak(text) {
        if ('speechSynthesis' in window) {
            this.setState("SPEAKING");
            const utterance = new SpeechSynthesisUtterance(text);
            utterance.lang = "es-CO";
            utterance.onend = () => this.setState("IDLE");
            window.speechSynthesis.speak(utterance);
        }
    }
}
window.VoiceAssistant = new VoiceAssistant();
