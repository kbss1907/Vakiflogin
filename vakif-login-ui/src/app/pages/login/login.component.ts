import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { AuthService } from '../../services/auth';
import { Router, RouterLink } from '@angular/router';
import { ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './login.html'
})
export class LoginComponent {
  loginForm: FormGroup;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private toastService: ToastService
  ) {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.pattern('^.+@.+\\..+$')]],
      password: ['', [Validators.required]],
      rememberMe: [false]
    });
  }

  onLogin() {
    if (this.loginForm.valid) {
      const payload = {
        email: this.loginForm.value.email,
        password: this.loginForm.value.password,
        rememberMe: this.loginForm.value.rememberMe
      };

      this.authService.login(payload).subscribe({
        next: (response: any) => {
          this.toastService.show('Başarıyla giriş yapıldı! Yönlendiriliyorsunuz...', 'success');
          // Dashboard veya Home sayfasına yönlendir (Şimdilik / e gönderiyoruz)
          this.router.navigate(['/']);
        },
        error: (err: any) => {
          console.error('Giriş Hatası:', err);
          this.toastService.show(err.error?.message || 'Giriş yapılamadı.', 'error');
        }
      });
    } else {
      this.toastService.show('Lütfen e-posta ve şifrenizi kontrol edin.', 'error');
    }
  }
}