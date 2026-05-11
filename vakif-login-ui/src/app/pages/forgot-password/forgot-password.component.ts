import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { AuthService } from '../../services/auth';
import { Router, RouterLink } from '@angular/router';
import { ToastService } from '../../services/toast.service';
import { CommonModule } from '@angular/common';

@Component({
    selector: 'app-forgot-password',
    standalone: true,
    imports: [ReactiveFormsModule, RouterLink, CommonModule],
    templateUrl: './forgot-password.component.html'
})
export class ForgotPasswordComponent {
    forgotForm: FormGroup;
    isSubmitting = false;

    constructor(
        private fb: FormBuilder,
        private authService: AuthService,
        private router: Router,
        private toastService: ToastService
    ) {
        this.forgotForm = this.fb.group({
            email: ['', [Validators.required, Validators.pattern('^.+@.+\\..+$')]]
        });
    }

    onSubmit() {
        if (this.forgotForm.valid) {
            this.isSubmitting = true;
            const email = this.forgotForm.value.email;

            this.authService.forgotPassword(email).subscribe({
                next: (response: any) => {
                    this.toastService.show(response.message || 'Şifre sıfırlama kodu email adresinize gönderildi.', 'success');
                    // Kullanıcıyı token girebilmesi için reset password sayfasına yönlendirebiliriz.
                    // Kod mailine gittiğinde (veya konsola düştüğünde), ordan alıp diğer sayfada kullanacak.
                    this.router.navigate(['/reset-password']);
                    this.isSubmitting = false;
                },
                error: (err: any) => {
                    console.error('Şifre Sıfırlama İsteği Hatası:', err);
                    this.toastService.show(err.error?.message || 'Bir hata oluştu.', 'error');
                    this.isSubmitting = false;
                }
            });
        } else {
            this.toastService.show('Lütfen geçerli bir e-posta adresi girin.', 'error');
        }
    }
}
