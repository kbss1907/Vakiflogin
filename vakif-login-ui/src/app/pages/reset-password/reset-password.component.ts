import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { AuthService } from '../../services/auth';
import { Router, RouterLink, ActivatedRoute } from '@angular/router';
import { ToastService } from '../../services/toast.service';
import { CommonModule } from '@angular/common';

@Component({
    selector: 'app-reset-password',
    standalone: true,
    imports: [ReactiveFormsModule, RouterLink, CommonModule],
    templateUrl: './reset-password.component.html'
})
export class ResetPasswordComponent implements OnInit {
    resetForm: FormGroup;
    isSubmitting = false;

    constructor(
        private fb: FormBuilder,
        private authService: AuthService,
        private router: Router,
        private route: ActivatedRoute,
        private toastService: ToastService
    ) {
        this.resetForm = this.fb.group({
            token: ['', [Validators.required, Validators.minLength(8)]],
            newPassword: ['', [Validators.required, Validators.minLength(6)]],
            confirmPassword: ['', [Validators.required, Validators.minLength(6)]]
        }, { validator: this.passwordMatchValidator });
    }

    ngOnInit(): void {
        // Eğer linkten ?token=ABCDEFGH şeklinde gelirse otomatik dolsun diye
        this.route.queryParams.subscribe(params => {
            if (params['token']) {
                this.resetForm.patchValue({
                    token: params['token']
                });
            }
        });
    }

    passwordMatchValidator(g: FormGroup) {
        return g.get('newPassword')?.value === g.get('confirmPassword')?.value
            ? null : { 'mismatch': true };
    }

    onSubmit() {
        if (this.resetForm.valid) {
            this.isSubmitting = true;
            const payload = this.resetForm.value;

            this.authService.resetPassword(payload).subscribe({
                next: (response: any) => {
                    this.toastService.show(response.message || 'Şifreniz başarıyla sıfırlandı. Giriş yapabilirsiniz.', 'success');
                    this.router.navigate(['/login']);
                    this.isSubmitting = false;
                },
                error: (err: any) => {
                    console.error('Şifre Sıfırlama Hatası:', err);
                    this.toastService.show(err.error?.message || 'Şifre sıfırlama işlemi başarısız oldu.', 'error');
                    this.isSubmitting = false;
                }
            });
        } else {
            if (this.resetForm.errors?.['mismatch']) {
                this.toastService.show('Şifreler birbiriyle eşleşmiyor.', 'error');
            } else {
                this.toastService.show('Lütfen tüm alanları geçerli şekilde doldurun.', 'error');
            }
        }
    }
}
