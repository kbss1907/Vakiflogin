import { Component } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../services/auth';
import { Router } from '@angular/router';
import { ToastService } from '../../services/toast.service';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterModule],
  template: `
    <div class="min-h-screen bg-gray-50 flex items-center justify-center p-4">
      <div class="max-w-2xl w-full bg-white rounded-2xl shadow-xl overflow-hidden text-center p-10">
        
        <!-- Header -->
        <h1 class="text-3xl font-extrabold text-gray-900 mb-2">Hoş Geldiniz</h1>
        <p class="text-gray-500 mb-8">Burası korumalı alandır. Sadece yetkili kullanıcılar erişebilir.</p>

        <!-- API Status Display -->
        <div class="mb-8 p-6 rounded-xl border-2 transition-all duration-300"
             [ngClass]="apiResponse ? 'border-green-100 bg-green-50' : 'border-dashed border-gray-200 bg-gray-50'">
           
           @if (apiResponse) {
             <div class="font-mono text-sm text-gray-800 break-words">
                <span class="block text-green-600 font-bold mb-2">API Başarılı Yanıt Döndü:</span>
                {{ apiResponse }}
             </div>
           } @else {
             <div class="text-gray-400 flex flex-col items-center">
                <svg class="w-12 h-12 mb-2 opacity-50" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 16H6a2 2 0 01-2-2V6a2 2 0 012-2h8a2 2 0 012 2v2m-6 12h8a2 2 0 002-2v-8a2 2 0 00-2-2h-8a2 2 0 00-2 2v8a2 2 0 002 2z"></path>
                </svg>
                Henüz API'den veri çekilmedi
             </div>
           }
        </div>

        <!-- Action Buttons -->
        <div class="flex flex-col sm:flex-row gap-4 justify-center mb-6">
          <button (click)="checkApi()" 
                  class="flex-1 py-3 px-6 rounded-lg bg-blue-600 hover:bg-blue-700 text-white font-medium shadow-md transition duration-200">
            Backend'den Veri Çek
          </button>
          
          <button (click)="logout()" 
                  class="flex-1 py-3 px-6 rounded-lg bg-red-50 hover:bg-red-100 text-red-600 font-medium transition duration-200">
            Sistemden Çıkış Yap
          </button>
        </div>

        <!-- İzin Yönetimi Yönlendirme Kartları -->
        <div class="border-t border-gray-100 pt-8 mt-4">
          <h2 class="text-xl font-bold text-gray-800 mb-6">İzin Yönetimi Modülü</h2>
          <div class="grid grid-cols-1 sm:grid-cols-2 gap-4">
            
            <a routerLink="/leave/my-requests" 
               class="group relative flex flex-col items-center p-6 bg-white border-2 border-indigo-50 rounded-2xl hover:border-indigo-500 hover:shadow-lg transition-all duration-300">
               <div class="w-12 h-12 bg-indigo-50 rounded-full flex items-center justify-center mb-4 group-hover:bg-indigo-500 group-hover:text-white transition-colors duration-300">
                 <svg class="w-6 h-6 text-indigo-500 group-hover:text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"></path></svg>
               </div>
               <h3 class="text-lg font-semibold text-gray-900">İzinlerim</h3>
               <p class="text-sm text-gray-500 mt-2">Geçmiş izinleri gör ve yeni talep oluştur</p>
            </a>

            <a routerLink="/leave/pending" 
               class="group relative flex flex-col items-center p-6 bg-white border-2 border-emerald-50 rounded-2xl hover:border-emerald-500 hover:shadow-lg transition-all duration-300">
               <div class="w-12 h-12 bg-emerald-50 rounded-full flex items-center justify-center mb-4 group-hover:bg-emerald-500 group-hover:text-white transition-colors duration-300">
                 <svg class="w-6 h-6 text-emerald-500 group-hover:text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"></path></svg>
               </div>
               <h3 class="text-lg font-semibold text-gray-900">Bekleyen Onaylar</h3>
               <p class="text-sm text-gray-500 mt-2">Personel izin taleplerini incele ve yönet</p>
            </a>

          </div>
        </div>

        <!-- İK Personel Modülü Yönlendirme Kartları -->
        <div class="border-t border-gray-100 pt-8 mt-4">
          <h2 class="text-xl font-bold text-gray-800 mb-6">İnsan Kaynakları Modülü</h2>
          <div class="grid grid-cols-1 md:grid-cols-2 gap-4 justify-center">
            
            <a routerLink="/employee/list" 
               class="group relative flex flex-col items-center p-6 bg-white border-2 border-blue-50 rounded-2xl hover:border-blue-500 hover:shadow-lg transition-all duration-300">
               <div class="w-12 h-12 bg-blue-50 rounded-full flex items-center justify-center mb-4 group-hover:bg-blue-500 group-hover:text-white transition-colors duration-300">
                 <svg class="w-6 h-6 text-blue-500 group-hover:text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 20h5v-2a3 3 0 00-5.356-1.857M17 20H7m10 0v-2c0-.656-.126-1.283-.356-1.857M7 20H2v-2a3 3 0 015.356-1.857M7 20v-2c0-.656.126-1.283.356-1.857m0 0a5.002 5.002 0 019.288 0M15 7a3 3 0 11-6 0 3 3 0 016 0zm6 3a2 2 0 11-4 0 2 2 0 014 0zM7 10a2 2 0 11-4 0 2 2 0 014 0z"></path></svg>
               </div>
               <h3 class="text-lg font-semibold text-gray-900">Tüm Personeller</h3>
               <p class="text-sm text-gray-500 mt-2">Şirketteki tüm çalışanları görüntüleyin</p>
            </a>

          </div>
        </div>

      </div>
    </div>
  `
})
export class HomeComponent {
  apiResponse: string = '';

  constructor(
    private authService: AuthService,
    private http: HttpClient,
    private router: Router,
    private toastService: ToastService
  ) { }

  checkApi() {
    this.http.get('https://localhost:7178/api/Auth/me', { responseType: 'text' }).subscribe({
      next: (res) => {
        this.apiResponse = res;
        this.toastService.show('API Bağlantısı Başarılı 🚀', 'success');
      },
      error: (err) => {
        this.apiResponse = 'HATA: ' + err.message;
        this.toastService.show('Yetki reddedildi! Lütfen tekrar giriş yapın.', 'error');
      }
    });
  }

  logout() {
    this.authService.logout();
    this.toastService.show('Sistemden çıkış yapıldı.', 'info');
    this.router.navigate(['/login']);
  }
}
