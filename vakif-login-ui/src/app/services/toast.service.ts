import { Injectable, signal } from '@angular/core';

export interface ToastMessage {
    text: string;
    type: 'success' | 'error' | 'info';
    id: number;
}

@Injectable({
    providedIn: 'root'
})
export class ToastService {
    messages = signal<ToastMessage[]>([]);
    private idCounter = 0;

    show(text: string, type: 'success' | 'error' | 'info' = 'info') {
        const id = this.idCounter++;
        this.messages.update(msgs => [...msgs, { text, type, id }]);

        // 4 saniye sonra mesajı otomatik sil
        setTimeout(() => {
            this.remove(id);
        }, 4000);
    }

    remove(id: number) {
        this.messages.update(msgs => msgs.filter(m => m.id !== id));
    }
}
