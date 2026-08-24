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

const AppModal = {
    _modal: null,

    _getInstance() {
        const el = document.getElementById('page-modal');
        this._modal ??= new bootstrap.Modal(el);
        return { modal: this._modal, el };
    },

    _setContent(title, message, footerHtml) {
        document.getElementById('modal-content').innerHTML = `
            <div class="modal-header">
                <h5 class="modal-title">${title}</h5>
                <button type="button" class="btn-close" data-bs-dismiss="modal"></button>
            </div>
            <div class="modal-body">
                <p>${message}</p>
            </div>
            <div class="modal-footer">
                ${footerHtml}
            </div>
        `;
    },

    notify(title, message) {
        return new Promise(resolve => {
            const { modal, el } = this._getInstance();

            this._setContent(title, message, `
                <button type="button" class="btn btn-primary" data-bs-dismiss="modal">OK</button>
            `);

            el.addEventListener('hidden.bs.modal', resolve, { once: true });
            modal.show();
        });
    },

    confirm(title, message, confirmLabel = 'Confirm', confirmClass = 'btn-primary') {
        return new Promise(resolve => {
            const { modal, el } = this._getInstance();

            this._setContent(title, message, `
                <button type="button" class="btn btn-secondary" data-bs-dismiss="modal">Cancel</button>
                <button type="button" class="btn ${confirmClass}" id="appModalConfirm">${confirmLabel}</button>
            `);

            el.querySelector('#appModalConfirm').addEventListener('click', () => {
                modal.hide();
                resolve(true);
            }, { once: true });

            el.addEventListener('hidden.bs.modal', () => resolve(false), { once: true });

            modal.show();
        });
    }
};