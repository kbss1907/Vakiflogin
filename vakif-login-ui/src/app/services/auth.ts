import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, from, of, BehaviorSubject } from 'rxjs';
import { switchMap, tap, catchError, map } from 'rxjs/operators';
import { EncryptionService } from './encryption.service';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private apiUrl = '/api/Auth';
  private currentUserSubject = new BehaviorSubject<any>(null);
  public currentUser$ = this.currentUserSubject.asObservable();

  constructor(
    private http: HttpClient,
    private encryptionService: EncryptionService,
    private router: Router
  ) { }

  // Uygulama ayağa kalkarken ilk CSRF pulunu ve kullanıcıyı al
  public initStore(): Observable<boolean> {
    return this.http.get<any>(`${this.apiUrl}/me`, { withCredentials: true }).pipe(
      map(user => {
        if (user) {
          this.currentUserSubject.next(user);
        }
        return true;
      }),
      catchError(() => {
        this.currentUserSubject.next(null);
        return of(true);
      })
    );
  }

  login(loginData: any): Observable<any> {
    return from(this.encryptionService.encrypt(loginData.password)).pipe(
      switchMap(encryptedPassword => {
        const payload = { ...loginData, password: encryptedPassword };
        return this.http.post<any>(`${this.apiUrl}/login`, payload, { withCredentials: true });
      }),
      tap(res => {
        if (res && res.userInfo) {
          this.currentUserSubject.next(res.userInfo);
        }
      })
    );
  }

  register(userData: any): Observable<any> {
    return from(this.encryptionService.encrypt(userData.password)).pipe(
      switchMap(encryptedPassword => {
        const payload = { ...userData, password: encryptedPassword };
        return this.http.post(`${this.apiUrl}/register`, payload, { responseType: 'text', withCredentials: true });
      })
    );
  }

  forgotPassword(email: string): Observable<any> {
    return this.http.post(`${this.apiUrl}/forgot-password`, { email });
  }

  resetPassword(data: any): Observable<any> {
    return from(
      Promise.all([
        this.encryptionService.encrypt(data.newPassword),
        this.encryptionService.encrypt(data.confirmPassword)
      ])
    ).pipe(
      switchMap(([encryptedNewPassword, encryptedConfirmPassword]) => {
        const payload = { 
          ...data, 
          newPassword: encryptedNewPassword,
          confirmPassword: encryptedConfirmPassword
        };
        return this.http.post(`${this.apiUrl}/reset-password`, payload);
      })
    );
  }

  public refreshToken(): Observable<any> {
    return this.http.post<any>(`${this.apiUrl}/refresh-token`, {}).pipe(
      switchMap(() => this.initStore())
    );
  }

  logout(): void {
    this.http.post(`${this.apiUrl}/logout`, {}).pipe(
      catchError(() => of(null))
    ).subscribe(() => {
      this.currentUserSubject.next(null);
      this.router.navigate(['/login']);
    });
  }

  isLoggedIn(): boolean {
    return !!this.currentUserSubject.value;
  }

  getUserId(): number | null {
    return this.currentUserSubject.value?.id || null;
  }
}