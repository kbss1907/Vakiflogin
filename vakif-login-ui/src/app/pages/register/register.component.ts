import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../services/auth';
import { ToastService } from '../../services/toast.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [ReactiveFormsModule, RouterLink],
  templateUrl: './register.html'
})
export class RegisterComponent {
  registerForm: FormGroup;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router,
    private toastService: ToastService
  ) {
    this.registerForm = this.fb.group({
      firstName: ['', [Validators.required]],
      lastName: ['', [Validators.required]],
      phone: [''],
      departmentId: [1, [Validators.required, Validators.min(1)]],
      positionId: [1, [Validators.required, Validators.min(1)]],
      // Angular's built-in email validator rejects non-ASCII (Turkish characters).
      // Replaced with a more permissive regex.
      email: ['', [Validators.required, Validators.pattern('^.+@.+\\..+$')]],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  onRegister() {
    if (this.registerForm.valid) {
      // Formda istenen alanları hazırlıyoruz
      const payload = {
        firstName: this.registerForm.value.firstName,
        lastName: this.registerForm.value.lastName,
        phone: this.registerForm.value.phone,
        departmentId: Number(this.registerForm.value.departmentId) || 0,
        positionId: Number(this.registerForm.value.positionId) || 0,
        email: this.registerForm.value.email,
        password: this.registerForm.value.password,
        managerUserId: 0,
        creatorAdminId: 0,
        createdAt: new Date().toISOString(),
        isActive: true
      };

      this.authService.register(payload).subscribe({
        next: (res: any) => {
          this.toastService.show('Kayıt Başarılı! Lütfen giriş yapın.', 'success');
          this.router.navigate(['/login']);
        },
        error: (err: any) => {
          this.toastService.show('Kayıt sırasında bir hata oluştu veya e-posta adresi kullanımda olabilir.', 'error');
        }
      });
    } else {
      this.toastService.show('Lütfen formu eksiksiz ve geçerli şekilde doldurun.', 'error');
    }
  }
}
