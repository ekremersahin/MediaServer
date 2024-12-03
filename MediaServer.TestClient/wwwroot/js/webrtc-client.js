// wwwroot/js/webrtc-client.js
class WebRTCClient {
    constructor() {
        this.initializeElements();
        this.setupEventListeners();
        this.localStream = null;
        this.peerConnection = null;
        this.ws = null;
    }

    initializeElements() {
        this.localVideo = document.getElementById('localVideo');
        this.remoteVideo = document.getElementById('remoteVideo');
        this.startButton = document.getElementById('startButton');
        this.callButton = document.getElementById('callButton');
        this.hangupButton = document.getElementById('hangupButton');
        this.logArea = document.getElementById('logArea');
    }

    setupEventListeners() {
        this.startButton.onclick = () => this.startLocalMedia();
        this.callButton.onclick = () => this.initiateCall();
        this.hangupButton.onclick = () => this.endCall();
    }

    log(message) {
        const timestamp = new Date().toISOString();
        this.logArea.innerHTML += `[${timestamp}] ${message}\n`;
        this.logArea.scrollTop = this.logArea.scrollHeight;
    }

    async startLocalMedia() {
        try {
            this.localStream = await navigator.mediaDevices.getUserMedia({
                video: true,
                audio: true
            });

            this.localVideo.srcObject = this.localStream;

            this.startButton.disabled = true;
            this.callButton.disabled = false;

            this.log('Local media stream started successfully');
            this.initWebSocket();
        } catch (error) {
            this.log(`Error accessing media devices: ${error.message}`);
        }
    }

    initWebSocket() {
        try {
            this.ws = new WebSocket(`wss://${window.location.host}/ws`);
            
            this.ws.onopen = () => {
                this.log('WebSocket connection established');
            };

            this.ws.onmessage = (event) => {
                const message = JSON.parse(event.data);
                this.handleSignalingMessage(message);
            };

            this.ws.onerror = (error) => {
                this.log(`WebSocket error: ${error.message}`);
            };

            this.ws.onclose = () => {
                this.log('WebSocket connection closed');
            };
        } catch (error) {
            this.log(`WebSocket initialization error: ${error.message}`);
        }
    }

    initiateCall() {
      


        this.createPeerConnection();
        this.callButton.disabled = true;
        this.hangupButton.disabled = false;
        this.log('Initiating WebRTC call');
    }

    createPeerConnection() {
        const configuration = {
            iceServers: [
                { urls: 'stun:stun.l.google.com:19302' }
            ]
        };

        this.peerConnection = new RTCPeerConnection(configuration);

        this.peerConnection.onicecandidate = (event) => {
            if (event.candidate) {
                this.ws.send(JSON.stringify({
                    type: 'ice-candidate',
                    candidate: event.candidate
                }));
            }
        };

        this.peerConnection.ontrack = (event) => {
            this.remoteVideo.srcObject = event.streams[0];
            this.log('Remote stream received');
        };

        this.localStream.getTracks().forEach(track => {
            this.peerConnection.addTrack(track, this.localStream);
        });

        this.peerConnection.createOffer().then(o => {
            this.ws.send(JSON.stringify(o));
        });
    }

    async handleSignalingMessage(message) {
        try {
            switch (message.type) {
                case 'offer':
                    await this.handleOffer(message);
                    break;
                case 'answer':
                    await this.handleAnswer(message);
                    break;
                case 'ice-candidate':
                    await this.handleIceCandidate(message);
                    break;
            }
        } catch (error) {
            this.log(`Signaling error: ${error.message}`);
        }
    }

    async handleOffer(message) {
        if (!this.peerConnection) {
            this.createPeerConnection();
        }

        await this.peerConnection.setRemoteDescription(
            new RTCSessionDescription(message)
        );

        const answer = await this.peerConnection.createAnswer();
        await this.peerConnection.setLocalDescription(answer);

        this.ws.send(JSON.stringify(answer));
        this.log('Offer received and answered');
    }

    async handleAnswer(message) {
        await this.peerConnection.setRemoteDescription(
            new RTCSessionDescription(message)
        );
        this.log('Answer received');
    }

    async handleIceCandidate(message) {
        await this.peerConnection.addIceCandidate(
            new RTCIceCandidate(message.candidate)
        );
        this.log('ICE candidate added');
    }

    endCall() {
        if (this.peerConnection) {
            this.peerConnection.close();
            this.peerConnection = null;
        }

        if (this.localStream) {
            this.localStream.getTracks().forEach(track => track.stop());
            this.localVideo.srcObject = null;
        }

        this.startButton.disabled = false;
        this.callButton.disabled = true;
        this.hangupButton.disabled = true;
        this.ws.close();
        this.log('Call ended');
    }
}

// Initialize WebRTC Client when page loads
window.addEventListener('load', () => {
    window.webrtcClient = new WebRTCClient();
});