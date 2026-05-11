import { Component } from '@angular/core';
import { ToastService } from '../../services/toast.service';
import { CommonModule } from '@angular/common';

@Component({
    selector: 'app-toast',
    standalone: true,
    imports: [CommonModule],
    template: `
    <!-- Toast Container: Sabit sağ üst köşe -->
    <div class="fixed top-5 right-5 z-50 flex flex-col gap-3">
      @for (msg of toastService.messages(); track msg.id) {
        <div 
          class="min-w-[250px] px-4 py-3 rounded-lg shadow-lg border-l-4 transform transition-all duration-300 animate-slide-in flex justify-between items-center"
          [ngClass]="{
            'bg-green-100 border-green-500 text-green-800': msg.type === 'success',
            'bg-red-100 border-red-500 text-red-800': msg.type === 'error',
            'bg-blue-100 border-blue-500 text-blue-800': msg.type === 'info'
          }">
            
          <span class="font-medium mr-4">{{ msg.text }}</span>
          
          <button (click)="toastService.remove(msg.id)" class="text-gray-500 hover:text-gray-800 focus:outline-none">
            &times;
          </button>
        </div>
      }
    </div>
  `,
    styles: [`
    @keyframes slideIn {
      from { transform: translateX(100%); opacity: 0; }
      to { transform: translateX(0); opacity: 1; }
    }
    .animate-slide-in {
      animation: slideIn 0.3s ease-out forwards;
    }
  `]
})
export class ToastComponent {
    constructor(public toastService: ToastService) { }
}
