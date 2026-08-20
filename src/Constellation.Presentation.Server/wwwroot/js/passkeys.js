const PasskeyUtils = {
    bufferToBase64Url(buffer) {
        const bytes = new Uint8Array(buffer);
        let str = '';
        for (const byte of bytes) str += String.fromCharCode(byte);
        return btoa(str).replace(/\+/g, '-').replace(/\//g, '_').replace(/=/g, '');
    },

    base64UrlToBuffer(base64Url) {
        const base64 = base64Url.replace(/-/g, '+').replace(/_/g, '/');
        const binary = atob(base64);
        const bytes = new Uint8Array(binary.length);
        for (let i = 0; i < binary.length; i++) bytes[i] = binary.charCodeAt(i);
        return bytes.buffer;
    },

    // Serialize the credential the browser returns into something JSON-serializable
    serializeRegistrationCredential(credential) {
        return {
            id: credential.id,
            rawId: this.bufferToBase64Url(credential.rawId),
            type: credential.type,
            response: {
                attestationObject: this.bufferToBase64Url(credential.response.attestationObject),
                clientDataJSON: this.bufferToBase64Url(credential.response.clientDataJSON)
            }
        };
    },

    serializeAssertionCredential(credential) {
        return {
            id: credential.id,
            rawId: this.bufferToBase64Url(credential.rawId),
            type: credential.type,
            response: {
                authenticatorData: this.bufferToBase64Url(credential.response.authenticatorData),
                clientDataJSON: this.bufferToBase64Url(credential.response.clientDataJSON),
                signature: this.bufferToBase64Url(credential.response.signature),
                userHandle: credential.response.userHandle
                    ? this.bufferToBase64Url(credential.response.userHandle)
                    : null
            }
        };
    }
};