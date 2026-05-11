import { Injectable, inject, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';

@Injectable({
  providedIn: 'root'
})
export class EncryptionService {
  private readonly platformId = inject(PLATFORM_ID);

  // YENI VE KESIN OLARAK GECERLI PUBLIC KEY (SPKI format)
  private readonly publicKeyBase64 = 'MIIBIjANBgkqhkiG9w0BAQEFAAOCAQ8AMIIBCgKCAQEAr25NvsjcvJV+qpMsPuT/XjUy0kO7m/GXrPZx/ALzB4fhD8GrZiApDGkArgY6qI6DQ71wU/W4/zkCHDJdBg9MmaMxBv5GfDrRlw2gi/DmGXaZet5aVtCy/ARv7p+TcExbEUp5Mm+FLsHIXvNRI9VLCQq0RfQ2tp+RMMmgLrT3ZKsHjH3nrAeuDjtrNdLBq3n3uL9GaR9w3WMwIm3JXLLabMbRFuxeSNKC/Bl0nQ5gYtfwCx9sPGGizusnsZJVXLddy7s5NOQZmqN3PZyqYHQ5mIXpBScSbuEmlR15rSVc60ytl0Diq6AqtyWr1NyLa4uKquYVYZ+nRAvo3i2htBlQfQIDAQAB';

  constructor() {}

  /**
   * RSA-OAEP (SHA-256) kullanarak metni şifreler.
   * Web Crypto API (SubtleCrypto) kullanır.
   */
  async encrypt(text: string): Promise<string> {
    if (!text) return text;

    // SSR ortamında window/crypto mevcut değil.
    // Login her zaman browser'da kullanıcı etkileşimiyle tetiklenir,
    // dolayısıyla bu dal pratikte hiç çalışmamalı.
    // Sessizce plain text döndürmek yerine açıkça hata fırlatıyoruz.
    if (!isPlatformBrowser(this.platformId)) {
      throw new Error('[EncryptionService] encrypt() sadece browser ortamında çağrılabilir.');
    }

    try {
      // 1. Base64 Public Key'i ArrayBuffer'a çevir
      // Not: Base64 string'i temizliyoruz (bazı durumlarda gizli karakterler olabilir)
      const cleanKey = this.publicKeyBase64.replace(/\s/g, '');
      const binaryDerString = window.atob(cleanKey);

      const binaryDer = new Uint8Array(binaryDerString.length);
      for (let i = 0; i < binaryDerString.length; i++) {
        binaryDer[i] = binaryDerString.charCodeAt(i);
      }

      // 2. Anahtarı içe aktar (SPKI formatında)
      const importedKey = await window.crypto.subtle.importKey(
        'spki',
        binaryDer.buffer,
        {
          name: 'RSA-OAEP',
          hash: 'SHA-256'
        },
        false,
        ['encrypt']
      );


      // 3. Veriyi şifrele
      const encoder = new TextEncoder();
      const encodedText = encoder.encode(text);
      const encryptedBuffer = await window.crypto.subtle.encrypt(
        { name: 'RSA-OAEP' },
        importedKey,
        encodedText
      );


      // 4. Sonucu Base64 olarak döndür
      return this.arrayBufferToBase64(encryptedBuffer);
    } catch (error) {
      console.error('[Encryption Service] Şifreleme hatası:', error);
      return text; // Hata durumunda (test için) orijinali döndür
    }
  }

  private arrayBufferToBase64(buffer: ArrayBuffer): string {
    let binary = '';
    const bytes = new Uint8Array(buffer);
    const len = bytes.byteLength;
    for (let i = 0; i < len; i++) {
      binary += String.fromCharCode(bytes[i]);
    }
    return window.btoa(binary);
  }
}
